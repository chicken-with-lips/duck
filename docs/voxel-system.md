# Voxel System Design

## Overview

The game uses a voxel-based world representation where each voxel is **25cm × 25cm × 25cm** in world scale. This applies to all geometry including ships, interiors, and any destructible prefabs. Ships range from small single-seat fighters to large capital ships with fully walkable interiors.

The renderer is Vulkan 1.3, implemented in C# on .NET 10 via Silk.NET. Geometry for rasterization is constructed on the GPU. Secondary rays (shadows, GI) are traced by **DDA-marching the voxel data directly** rather than against a hardware acceleration structure.

The host language matters in two places and nowhere else: there is no Vulkan memory allocator available (see Phase 0.2), and shader compilation is an MSBuild step invoking a native compiler (Phase 0.7).

---

## Design Revision — Traversal Strategy

The earlier version of this document specified a BLAS-per-chunk built from meshed triangles, with `VK_KHR_ray_query` for shadows, AO and GI. That has been reversed.

**The BLAS had exactly one consumer — ray queries.** It was not used for rasterization, culling, or physics. Removing it resolves several problems at once:

| Problem | Resolution |
|---|---|
| BLAS memory of ~50–310 MB across 8 ships (the original 15–45 MB estimate was low by 3.3–6.9×) | Gone entirely |
| Per-frame TLAS rebuild plus BLAS rebuild for every dirty chunk — cost peaks exactly during heavy combat, which is backwards | Gone; DDA reads live voxel data, destruction is a bitmask edit |
| Same-class ships cannot share BLASes once damage diverges, so the "shared base geometry" saving was illusory | Voxel data with a sparse damage diff shares cleanly |
| "No ray tracing support" hardware tier needed a separate baked-GI or LPV fallback | One path everywhere; scale ray count as a quality setting |
| Greedy meshing was mandatory to keep BLAS triangle counts survivable | Now an optimization, traded against LOD |
| Mesh shader path claimed "no intermediate vertex buffer" while the BLAS required materialized triangles | No longer contradictory — nothing needs persistent triangles |

**This is a bet, not a proof.** Hardware RT cores beat DDA for long incoherent rays through sparse space, which is exactly what a shadow ray toward a light 300m away is. The ray budget is also larger than the first pass of this document assumed — see *Ray Traversal*, where the corrected figures put shadows and GI at a combined ~4.4–8.4M rays/frame rather than the ~2–6M originally stated. Whether DDA holds that at frame rate is **an open question that Phase 4 of the implementation plan exists to answer**. The BLAS path is retained here as a documented fallback, and the geometry pipeline is deliberately structured so reinstating it is additive rather than a rewrite.

---

## Sizing Assumptions

Every memory and geometry figure in this document derives from three numbers. They are stated here once so they can be re-derived from a real ship model rather than re-guessed per section.

| Assumption | Value | Basis |
|---|---|---|
| Ship proportions | length : beam : height = 8 : 1.6 : 1 | From `lighting-model.md`'s "400m × 80m × 50m capital ship interior" |
| Hull shell thickness | 1.5 voxels average | Estimate — armour plus structure, thin relative to a 25cm voxel |
| Surface non-planarity | 1.15 × flat chunk-face area | Estimate — allowance for greebles, recesses, non-axis-aligned hull |

From these: shell chunk count is the one-chunk-thick boundary layer of the ship's chunk bounding box, hull voxel count is `chunks × 1024 × thickness`, and outward-facing quad count is `chunks × 1024 × non-planarity`.

**The previous revision's hull voxel counts (0.6M–8.4M) have been discarded.** They were not derived from anything and implied a shell that got *thinner* as ships got larger — a 150m frigate carrying 1.9× the hull thickness of a 700m dreadnought. The figures below are internally consistent by construction; they are less flattering in places, and that is the point.

| Ship Class | Chunk bbox | Shell chunks | Hull voxels | Fill | Outward quads |
|---|---|---|---|---|---|
| Light Frigate (150m) | 19×4×3 | 194 | 0.30M | 4.7% | 0.23M |
| Destroyer (250m) | 32×7×4 | 596 | 0.92M | 4.7% | 0.70M |
| Cruiser (350m) | 44×9×6 | 1,200 | 1.84M | 4.7% | 1.41M |
| Battleship (500m) | 63×13×8 | 2,526 | 3.88M | 4.7% | 2.97M |
| Carrier (550m) | 69×14×9 | 3,066 | 4.71M | 4.7% | 3.61M |
| Dreadnought (700m) | 88×18×11 | 5,040 | 7.74M | 4.7% | 5.94M |

Note that shell chunks are a **hard upper bound** — a one-chunk boundary layer of the bounding box. Inscribed convex bodies come in well under it (an ellipsoid at 0.69×, a bicone at 0.47×), so a real hull will need fewer. Re-derive from geometry when a model exists; every figure below scales linearly.

---

## Data Storage

### Chunk Structure

The world is subdivided into **32³ voxel chunks** (8m³ at 25cm scale). Each ship exists in its own local coordinate space — world-space placement is handled via transform matrices at the draw and ray-entry level, not baked into voxel positions. Only chunks containing at least one occupied voxel are allocated on the GPU.

| Field | Size | Notes |
|---|---|---|
| Chunk position in ship space | 3 × int16 (6 bytes) | In chunk units, not voxels |
| Voxel count | uint32 (4 bytes) | Number of occupied voxels |
| State flags | uint32 (4 bytes) | Dirty, loaded, queued for rebuild |
| Occupancy offset | uint32 (4 bytes) | Offset into the ship's occupancy pool |
| Attribute offset | uint32 (4 bytes) | Offset into the ship's packed attribute arrays |
| Padding | 2 bytes | Align to 24 |

**Correction to the previous revision:** that version stated a large capital ship has "~750 non-empty chunks." Not arithmetically possible — even at the old 8.4M hull voxel figure, 750 chunks would need 11,200 voxels each against a capacity of 32,768. The correct figure for a dreadnought is **5,040**, and per-chunk costs were understated by ~6.7×.

### Per-Voxel Storage

The previous revision stored voxels as a packed 32-bit point list with local XYZ in the low 15 bits. **DDA requires O(1) answers to "is there a voxel at (x, y, z)?"**, which a point list cannot give without a search. Storage is restructured around an occupancy bitmask, which also makes position implicit and frees the bits that encoded it.

**Layout A — flat bitmask. Build this first.**

```
Occupancy bitmask   : 32^3 bits as 512 x uint64  = 4096 B per chunk
Prefix-sum table    : 512 x uint16               = 1024 B per chunk
                      EXCLUSIVE running popcount: entry[w] = set bits in words [0, w)
Material array      : 1 byte per occupied voxel, packed, no gaps
Face mask array     : 1 byte per occupied voxel (6 bits used) -- see below
```

Voxel lookup:

```glsl
#extension GL_EXT_shader_explicit_arithmetic_types_int64 : require
#extension GL_EXT_shader_explicit_arithmetic_types_int16 : require
#extension GL_EXT_shader_explicit_arithmetic_types_int8  : require

layout(...) readonly buffer Occupancy { uint64_t occupancy[]; };   // MUST be 64-bit
layout(...) readonly buffer Prefix    { uint16_t prefix[];    };
layout(...) readonly buffer Material  { uint8_t  material[];  };

uint      word  = linearIndex >> 6;              // 64 voxels per word
uint      bit   = linearIndex & 63u;
uint64_t  w     = occupancy[word];
bool      solid = (w & (1ul << bit)) != 0ul;

// only if solid:
uint idx = uint(prefix[word]) + uint(bitCount(w & ((1ul << bit) - 1ul)));
uint mat = uint(material[idx]);
```

Four notes that will each cost you a day if missed:

- All three `#extension` lines are required. Omitting the `_int16`/`_int8` ones is a syntax error on the buffer declarations, which at least fails loudly.
- `occupancy[]` **must** be declared `uint64_t`. Declaring it `uint` compiles clean — glslang silently inserts a zero-extending `OpUConvert` — and every voxel in bits 32–63 of every word reads as empty. Half the world disappears with no diagnostic.
- `bitCount()` on a 64-bit operand returns `int64_t` in GLSL, so the `uint(...)` casts are required, not stylistic.
- The prefix table is **exclusive**. An inclusive table produces a plausible-looking image with every material off by one voxel.

Linear ordering is `x + 32y + 1024z`.

**The face mask.** Hull extraction discards the interior, so the shell's *inner* surface is also adjacent to empty space and naive face culling emits both sides. Recording which of the 6 directions face outward at extraction time (6 bits, stored as 1 byte/voxel) suppresses the inner half. This is why the quad counts above are ~0.77 per hull voxel rather than ~1.5 — outward quad count tracks **surface area**, not shell thickness, so a thicker hull costs voxels without costing geometry. Interior data, where both sides are genuinely visible, does not use the mask.

**Layout B — two-level bricks. Optimization, once A is correct.**

A hull chunk is only ~5% occupied, so a flat 4096-byte mask is mostly zeros. Subdividing into 8³ = 512 bricks of 4³ voxels and allocating voxel masks only for non-empty bricks cuts storage substantially *and* lets DDA skip 1m at a time inside a chunk:

```
Brick occupancy mask : 8^3 bits = 64 B per chunk
Per non-empty brick  : 64-bit voxel mask (8 B) + uint16 exclusive prefix (2 B)
Attribute arrays     : unchanged
```

| Ship Class | Layout A + mask | Layout B + mask | (old point list, re-derived) |
|---|---|---|---|
| Light Frigate | 1.6 MB | 1.0 MB | 1.2 MB |
| Destroyer | 4.9 MB | 3.0 MB | 3.7 MB |
| Cruiser | 9.8 MB | 6.1 MB | 7.4 MB |
| Battleship | 20.7 MB | 12.8 MB | 15.5 MB |
| Carrier | 25.1 MB | 15.5 MB | 18.8 MB |
| Dreadnought | 41.3 MB | 25.5 MB | 31.0 MB |
| **These 6 classes** | **103.4 MB** | **63.9 MB** | 77.6 MB |
| *+2 further frigate-class* | *~107 MB* | *~66 MB* | *~80 MB* |

**Layout A costs 1.33× the old point list.** That is the honest price of O(1) lookup plus the face mask, and it is worth paying — the point list cannot serve DDA at all. Layout B brings it to 0.82×, i.e. cheaper than the original design while still supporting O(1) traversal.

### Top-Level Chunk Grid

For empty-space skipping, each ship carries a dense structure over its chunk bounding box. Two arrays, not one:

- **Occupancy bitmask**, 1 bit per chunk slot — 2.1 KB for a dreadnought at 88×18×11
- **Chunk index map**, `uint32` per chunk slot, mapping (cx, cy, cz) → index into chunk metadata — ~70 KB/ship

The index map is what lets a DDA step actually *fetch* the chunk it just found. A prefix sum over the bitmask would save the 70 KB at the cost of a scan per lookup; at this size, store the array.

### Damage State for Multiple Ships

Base voxel data for a ship class is a **read-only shared buffer**; each instance stores a **sparse damage diff**:

```c
// 8 bytes per damaged voxel entry
struct DamageEntry {
    uint32_t packedPosition;   // chunk index (16 bits) + local index (15 bits) + 1 spare
    uint8_t  damage;
    uint8_t  flags;            // destroyed, breached, on fire
    uint16_t padding;
};
```

Plus a per-instance **occupancy override**: when a chunk first takes damage, clone its 4096-byte mask into instance-local storage and clear destroyed bits there. Undamaged chunks read the shared mask. A ship that has lost 500 chunks' worth of integrity costs 500 × 4096 B ≈ 2.05 MB.

**Critical: resolve the attribute index against the shared mask, not the override.** Clearing a bit shifts every subsequent popcount index down by one, so resolving `idx` against a damaged mask fetches the wrong material for every voxel after the first hole in that chunk. Two mask reads per hit:

```glsl
bool solid = testBit(instanceMask, linearIndex);        // has it been destroyed?
uint idx   = prefixIndex(sharedMask, linearIndex);      // where are its attributes?
```

Cloning mask, prefix table and attribute arrays together instead costs 2.56 MB per 500 chunks plus attributes, and buys nothing. Use the two-read form.

---

## Hull Extraction

For exterior rendering and exterior ray traversal, full interior voxel data is not required. A **hull-only dataset** is pre-extracted per ship class offline:

- Flood fill inward from the bounding volume to identify the exterior shell
- Build the occupancy bitmask, exclusive prefix table, material array and outward face mask from the shell only
- Hull voxels are ~20% of total ship voxel count

The full dataset remains authoritative simulation state and is resident only for the ship whose interior the player currently occupies.

**Open issue — breaches on hull-only ships.** A hull-only ship is a sealed shell around empty space, so blowing a hole in one exposes nothing. This conflicts with the Phase 6 verification goal and with *Procedural Damage Geometry*. Options, in increasing cost: thicken the extracted shell so breaches show wall thickness; generate procedural interior geometry at breach boundaries; or promote a ship to full interior data when it first takes structural damage. Not resolved — decide before Phase 6.

---

## Ray Traversal

### Two-Level DDA

A standard Amanatides–Woo grid march, at two levels:

1. **World → ship.** Transform the ray into each ship's local space by its inverse transform, slab-test against the ship AABB. With a handful of ships this is a linear loop; no TLAS equivalent needed at this count.
2. **Chunk level.** March the top-level chunk bitmask in 8m steps until an occupied chunk is entered; resolve via the chunk index map.
3. **Voxel level.** March the 32³ occupancy mask in 25cm steps (Layout A), or brick-then-voxel (Layout B). On a hit, resolve attributes via the exclusive prefix table.

| Ray length | Chunk steps, axis-aligned | Chunk steps, worst-case diagonal |
|---|---|---|
| 50m | 6 | ~19 |
| 100m | 12 | ~38 |
| 300m | 38 | ~112 |

A 3D DDA crosses up to ~3× the axis-aligned cell count on a diagonal ray, so budget against the second column. Total worst-case step count for a 300m shadow ray:

```
112 chunk steps  +  3 occupied chunks entered x ~55 fine steps  =  ~280 steps
   (a diagonal crossing of a 32^3 chunk is 32 x sqrt(3) ~ 55 fine steps)
```

At ~6M shadow rays that is **~1.7B steps/frame, or ~101 G steps/s at 60Hz**. That is not obviously comfortable and is not asserted to be. It is the number Phase 4 must measure.

### Consumers

| Pass | Rays/frame | Notes |
|---|---|---|
| Shadow rays | ~2–6M at 4K | Only pixels receiving direct light |
| GI probes, 8 ships exterior | 0.83M | 8 × 13K probes × 8 rays |
| GI probes, player interior | 1.60M | 200K probes at 2m spacing × 8 rays |
| **Total at 8 rays/probe** | **~4.4–8.4M** | ~6.9–10.9M at 16 rays/probe |

**Correction:** the first pass of this document quoted 104K–208K GI rays and called them negligible. That is `lighting-model.md`'s single-ship exterior case; the scene budgeted here is 8 ships plus a walkable interior, ~23× more. GI is a real load, comparable to shadows.

`lighting-model.md`'s own probe arithmetic is also loose: "50 × 20 × 13 = 13,000" for a 400m ship at 4m spacing, where 400/4 = 100 gives 26,000. Re-derive alongside this.

### Material and Emissive Lookup

A DDA hit returns a voxel index directly, so **material ID is a table lookup** — no attribute interpolation, no barycentrics, no hit shaders. Emissive colour comes from the material palette indexed by that ID, which makes *Emissive Voxels as GI Sources* in `lighting-model.md` cheap.

Per-voxel **damage and flags are not O(1)** — they live in the sparse `DamageEntry` list and require a search. Only the fully-destroyed bit is O(1), via the occupancy override. Shading paths needing live damage state (scorching, breach glow) should read it from a per-chunk dense damage array promoted on first damage, or accept a bounded search. This is the one place the old point-list objection still applies.

### Fallback: Hardware Ray Queries

If Phase 4 misses budget, reinstate the BLAS for shadows only, leaving GI on DDA:

- `VK_KHR_acceleration_structure` + `VK_KHR_ray_query`
- One BLAS per non-empty chunk from the meshing pass output, rebuilt on dirty
- TLAS rebuilt per frame from per-ship transforms
- Makes greedy meshing mandatory again, and requires a persistent triangle buffer
- Plus per-instance BLAS duplication once damage diverges

---

## GPU Geometry Pipeline

Voxels are **blocky by design** — no smooth surface interpolation.

### Face Culling — Mandatory

Emit a face only where a solid voxel borders an empty one. Along the X axis, where a 32-voxel column is contiguous in the bitmask, this is bitwise. Note the direction convention carefully — with bit *i* holding the voxel at coordinate *i*:

```glsl
// nbrPlus / nbrMinus are the adjacent chunks' boundary columns (0 if none)
uint plus  = (column >> 1) | ((nbrPlus & 1u) << 31);
uint minus = (column << 1) | ((nbrMinus >> 31) & 1u);

uint faces_pos = column & ~plus;    // solid here, empty at i+1  -> face toward +axis
uint faces_neg = column & ~minus;   // solid here, empty at i-1  -> face toward -axis
```

**The neighbour terms are not optional.** A bare `column & ~(column >> 1)` shifts in a zero, so bit 31 always reports "+neighbour empty" and bit 0 always reports "-neighbour empty". Every chunk boundary through solid material then emits a double-sided wall of quads — at ~5,000 shell chunks per capital ship, a lot of coplanar Z-fighting geometry.

**Y and Z are not free.** With linear order `x + 32y + 1024z` over 64-bit words, an X-column is 32 contiguous bits, a Y-column is 2 bits from each of 16 words, and a Z-column is 1 bit from each of 32. Those axes need a bit-transpose into per-axis masks (or gathers). Binary greedy meshing implementations build three transposed masks for exactly this reason. The transpose is per-dispatch work, not extra persistent storage — but it is real and should be priced into the Phase 2 measurement.

### Greedy Meshing — Optional, Measured

Without the BLAS, merging no longer gates memory. It trades against LOD, and the trade is range-dependent:

| Situation | Quads, face-masked | Verdict |
|---|---|---|
| Interior, player walking | a few thousand | Merging pointless |
| One dreadnought filling the screen | 5.94M | Merging pays |
| Six different capital classes in frame | 14.9M | LOD/impostors, not merging |

The pixel-coverage ceiling is the honest guide. A 700m × 140m hull spanning 1,200 px along its length projects to ~288K pixels, so **~95%** of its 5.94M quads are sub-pixel. At that range the correct answer is a pre-baked LOD mesh, which is already planned. **Merging matters in the mid-range band only** — close enough that LOD would be visible, far enough that the whole ship is on screen.

Recommendation: ship face culling first, measure, add merging when the mid-range band demonstrably misses budget.

When added, use **binary greedy meshing**, not the classic scan-based algorithm — a 32-voxel X-column is one `uint32`, making the merge a sequence of mask operations rather than a 2D scan over a visited array. The commonly quoted order-of-magnitude speedup assumes the transposed masks above are already built.

Merge on **material ID alone**. Realistic efficiency with material-only matching on detailed hull surfaces is **85–95%**, giving 0.3–0.9M quads for a dreadnought. Per-vertex AO matching would drop this to 70–90%, which is the main reason it is removed below.

### Vertex Format

```
bits  0-15 : X position   (uint16, voxel-space units)
bits 16-31 : Y position   (uint16)
bits 32-47 : Z position   (uint16)
bits 48-50 : Face normal  (3 bits - one of +-X, +-Y, +-Z)
bits 51-58 : Material ID  (8 bits)
bits 59-63 : Spare
```

**8 bytes per vertex**, 32 bytes per quad. With mesh shaders a quad can be a single 16-byte record (corner + extent + material) with vertices generated procedurally.

**Baked per-vertex AO has been removed.** It is redundant with the SSAO pass `lighting-model.md` specifies as the primary vertical-darkening mechanism, and it is the largest practical limiter on merge efficiency — two faces can only merge if their corner AO values match, which fragments merging across detailed surfaces.

### Geometry Path

The implementation plan builds the **compute + indirect draw** path first and treats mesh shaders as a later, measured alternative. Neither is labelled "primary" ahead of measurement.

**Compute + indirect draw (Phase 2, the committed path)**

1. Compute shader reads occupancy from SSBO, culls faces, optionally merges, writes vertex data, bumps an atomic count.
2. `vkCmdDrawIndirectCount` consumes it — **non-indexed**. The indexed variant requires a bound index buffer, which nothing here produces. If indexed drawing is wanted for vertex reuse, bind one shared static quad index buffer (`0,1,2, 0,2,3` repeated) rather than emitting indices per quad.
3. Barriers separate compute and graphics — see Phase 2 for the exact form.

Scratch buffer sizing is for the **visible set** after culling, not the fleet. Because quad density per chunk is uniform by construction (~1,178 outward quads per shell chunk), this is one number regardless of class: **~75 MB at 2,000 visible chunks, ~188 MB at 5,000**, at 32 B/quad. Allocate a fixed ring and clamp; overflow degrades to dropped chunks rather than corruption. At wide zoom the clamp will fire routinely, which is another argument for LOD.

**Mesh shaders (`VK_EXT_mesh_shader`, Phase 7)**

Needs **no persistent geometry memory at all**, which is the single biggest argument for it.

- **Task shader**: one workgroup per chunk. Frustum, distance and horizontal-slice culling (a top-down camera only needs chunks in the visible Y range). Dispatches mesh workgroups for survivors only.
- **Mesh shader**: reads occupancy and attributes, culls faces, optionally merges, emits quads.
- **Draw**: `vkCmdDrawMeshTasksIndirectEXT`. One entry per ship means `drawCount` > 1, requiring the `multiDrawIndirect` feature — check it, or issue one call per ship.

Because geometry is regenerated per frame, per-instance damage masking composes correctly with merging; there is no cached merged quad to invalidate.

---

## Multi-Ship Memory Budget

### 8 Ships, Different Classes

| Component | Memory |
|---|---|
| Voxel data, Layout A incl. face mask (Layout B) | ~107 MB (~66 MB) |
| Per-ship occupancy overrides (8 × 2.05 MB) + damage diffs | ~20–30 MB |
| Geometry scratch | 0 MB (mesh shader) — ~188 MB (compute path, 5,000 visible chunks) |
| Acceleration structures | 0 MB |
| Full interior, player ship only | ~100 MB |
| **Total** | **~227 MB — ~425 MB** |

Layout B with the mesh shader path brings the low end to ~186 MB. On the committed Phase 2 compute path expect the upper end until either LOD or mesh shaders land.

Not included: render targets. A 4K G-buffer carrying albedo, normal, material ID, emissive and depth is on the order of 170 MB and is charged to the lighting system, not here.

### 8 Ships, Same Class

Base voxel data collapses to one shared read-only copy (~41 MB for a dreadnought, Layout A) plus per-instance overrides. Unlike the BLAS design this sharing is real — nothing forks per instance except the damage diff and the occupancy override.

---

## Vulkan Feature Requirements

Targeting **Vulkan 1.3**, most of what this design needs is core and enabled as feature bits, not extension strings.

**Required device features** (the actual hardware gates):

| Feature | Struct | Needed for |
|---|---|---|
| `shaderInt64` | `VkPhysicalDeviceFeatures` | 64-bit occupancy words and popcount |
| `storageBuffer16BitAccess` | `VkPhysicalDeviceVulkan11Features` | uint16 prefix tables |
| `storageBuffer8BitAccess` | `VkPhysicalDeviceVulkan12Features` | 1 B/voxel material and face-mask arrays |
| `bufferDeviceAddress` | `VkPhysicalDeviceVulkan12Features` | Per-ship buffer pointers |
| `drawIndirectCount` | `VkPhysicalDeviceVulkan12Features` | GPU-driven draw counts |
| `descriptorIndexing` | `VkPhysicalDeviceVulkan12Features` | Bindless material textures |
| `synchronization2` | `VkPhysicalDeviceVulkan13Features` | Barrier form used throughout |
| `dynamicRendering` | `VkPhysicalDeviceVulkan13Features` | G-buffer pass without renderpass objects |
| `multiDrawIndirect` | `VkPhysicalDeviceFeatures` | Only if batching ships into one indirect call |

Note the split: `storageBuffer16BitAccess` is in **Vulkan11**Features while `storageBuffer8BitAccess` is in **Vulkan12**Features. Subgroup ballot and arithmetic are core 1.1, reported through `VkPhysicalDeviceSubgroupProperties::supportedOperations` — query, don't assume. `VK_KHR_shader_subgroup_extended_types` is **not** needed: it governs subgroup ops taking 8/16/64-bit operands, and `subgroupBallot()` returns a `uvec4` regardless.

**Optional extensions:**

| Extension | Purpose |
|---|---|
| `VK_EXT_memory_budget` | Per-heap usage instrumentation (Phase 0.5). Not core; requires 1.1+ |
| `VK_EXT_mesh_shader` | Alternative geometry path (Phase 7) |
| `VK_KHR_acceleration_structure` | Only if the BLAS shadow fallback is reinstated |
| `VK_KHR_ray_query` | Only if the BLAS shadow fallback is reinstated |
| `VK_KHR_ray_tracing_pipeline` | Path-traced reference mode only |

Ray tracing has moved from required to optional, lowering the minimum hardware target. Note that `lighting-model.md`'s Hardware Targets table tiers all four rows on ray-query throughput; DDA is bandwidth- and ALU-bound rather than RT-core-bound, so that table needs re-deriving in full, not just its bottom row.

**Shader extensions:** `GL_EXT_shader_explicit_arithmetic_types_int64` / `_int16` / `_int8`, `GL_KHR_shader_subgroup_ballot`, `GL_KHR_shader_subgroup_arithmetic`.

---

## Required Edits to `lighting-model.md`

Removing the acceleration structure invalidates that document in more places than the AO term:

| Location | Change |
|---|---|
| §Overview (line 5) | "hardware ray queries for shadow rays and ambient occlusion" → DDA traversal; AO is screen-space |
| §Overview (line 7) | "BLAS/TLAS acceleration structures maintained by the voxel system" — no longer maintained |
| §Frame Pipeline steps 2–3 | Remove BLAS and TLAS rebuild |
| §Frame Pipeline step 6 | AO ray queries → SSAO |
| §Direct Lighting (line 31) | `rayQueryEXT` inline → DDA compute traversal |
| §Ambient Occlusion (line 47) | Remove the baked per-vertex AO term |
| §GI (line 55) | Ray queries → DDA; re-derive the probe count arithmetic (50 vs 100 across a 400m ship) |
| §Hardware Targets (line 121) | Re-derive all four tiers against DDA cost, not RT throughput |
| §Summary table (lines 132–135) | "Ray query (per light)" → DDA; remove the baked AO row |
| §BLAS Compaction (lines 147–149) | Delete — the whole Future Improvement is predicated on a structure that no longer exists |
| §Path-Traced Reference (line 165) | "The same BLAS/TLAS structures are reused… No additional geometry preparation is needed" is now false |

**`lighting-model.md` is also internally inconsistent on AO independently of this change:** its overview and frame pipeline specify ray-query AO, while §Ambient Occlusion and the Summary table specify SSAO plus baked per-vertex. The previous revision of *this* document read only the latter and wrongly asserted ray-query AO "does not exist in the lighting design." It does, in two places. The resolution is SSAO, but the edit needs making explicitly rather than assuming.

---

# Implementation Plan

Ordered so each phase produces something runnable and measurable, and so no phase depends on a decision a later measurement might reverse.

## Phase 0 — Instrumentation and Buffer Plumbing

Nothing here is voxel-specific, but every later decision depends on being able to measure.

**Starting point.** `Source/VulkanRenderSystem` already has: a Vulkan 1.3 instance with validation layers and debug messenger, physical device selection, queue families, logical device, swapchain and image views, command pool/buffers, semaphores and fences at 2 frames in flight, and dynamic rendering working. It has **no** memory allocation, buffers, descriptors, shader modules, query pools, or buffer device address — that is the whole of Phase 0.

### 0.1 — Feature detection and enablement

Do this first. It is a go/no-go on the hardware and costs nothing to find out.

Extend `IsDeviceSuitable` to query the full feature chain via `GetPhysicalDeviceFeatures2`, and verify every entry in *Vulkan Feature Requirements* above before accepting a device. Then enable the same chain at device creation: `PhysicalDeviceFeatures2` → `Vulkan11Features` → `Vulkan12Features` → `Vulkan13Features`.

Two conversion hazards in the existing code:

- `DeviceCreateInfo` currently sets `PEnabledFeatures`. Chaining `PhysicalDeviceFeatures2` while `PEnabledFeatures` is non-null is a spec violation — it must become null.
- The standalone `PhysicalDeviceDynamicRenderingFeatures` struct is redundant on a 1.3 target; `Vulkan13Features` carries both `DynamicRendering` and `Synchronization2`.

Also note that `DescriptorIndexing` in `Vulkan12Features` is a separate bool from the individual capability bools — enabling it does *not* enable `DescriptorBindingPartiallyBound`. Set each one actually used.

While here, capture `VkPhysicalDeviceLimits.timestampPeriod`, the queue family's `timestampValidBits`, `nonCoherentAtomSize`, and `maxMemoryAllocationCount`.

**Done when:** every required feature logs as present and the device still creates.

### 0.2 — Allocator and buffer abstraction

**Not VMA.** The engine is C# on .NET 10 with Silk.NET, which has no memory allocator — the port was [closed as not planned](https://github.com/dotnet/Silk.NET/issues/605), and the maintained option (`Vortice.VulkanMemoryAllocator`) pulls in a second complete Vulkan binding set as a dependency.

More importantly it is not needed. This design allocates roughly 100 large, long-lived buffers against a spec floor of 4096 `vkAllocateMemory` calls. VMA solves thousands-of-small-allocations, aliasing, and defragmentation; none of those occur here. **One `VkDeviceMemory` per buffer** removes offset arithmetic, alignment math, free lists and defrag entirely — about 150 lines.

- `GpuBuffer`: handle, memory, size, device address, mapped pointer
- `FindMemoryType(typeBits, requiredProperties)` over `GetPhysicalDeviceMemoryProperties`
- Device-local pool for voxel data; staging ring for uploads
- **Prefer a `DEVICE_LOCAL | HOST_VISIBLE` memory type for staging when one exists** (ReBAR, integrated GPUs) and fall back to plain `HOST_VISIBLE` — it skips a copy on the upload path for three lines of selection logic
- Assert against `maxMemoryAllocationCount`; warn at 50%

Keep everything behind `GpuBuffer` so a sub-allocator can slot in later without touching call sites.

### 0.3 — Upload and readback

Upload is a staging write plus `vkCmdCopyBuffer2` on a one-shot command buffer.

Readback matters more than it looks: Phase 1's CPU-reference comparison depends on it, and that comparison is the only defence against the silent-wrong-data failures this design is prone to. Build it properly now.

Use `HOST_VISIBLE | HOST_CACHED` for the readback buffer — coherent-only memory is uncached and reads back slowly. If the type is not also `HOST_COHERENT`, flush and invalidate ranges must be aligned to `nonCoherentAtomSize`; unaligned ranges are a validation error and on some drivers a silent wrong read.

### 0.4 — Timestamps

- `VkQueryPool` of `VK_QUERY_TYPE_TIMESTAMP`, sized `framesInFlight × 2 × passCount`
- `vkCmdResetQueryPool` at the top of each frame's recording, or enable `hostQueryReset` and reset from the CPU
- `vkCmdWriteTimestamp2` bracketing each pass
- Read the *previous* frame's results with `vkGetQueryPoolResults` so the frame never stalls

Convert with `deltaTicks × timestampPeriod` for nanoseconds, and mask raw values to `timestampValidBits` first — some queues report fewer than 64 valid bits, and the garbage in the high bits produces wild readings.

### 0.5 — Memory budget instrumentation

Enable `VK_EXT_memory_budget` (still a standalone extension, requires 1.1+) and surface `VkPhysicalDeviceMemoryBudgetPropertiesEXT::heapBudget` / `heapUsage` in the same overlay as the timestamps.

This is worth the half hour specifically because the memory figures in this document rest on the estimates in *Sizing Assumptions*. Having real per-heap usage on screen from Phase 0 means Phase 1's re-derivation works from measurement rather than from those estimates.

### 0.6 — Descriptors and buffer device address

- Single bindless set: `VK_DESCRIPTOR_BINDING_PARTIALLY_BOUND_BIT` + update-after-bind for material textures
- `vkGetBufferDeviceAddress` for per-ship buffer pointers

Put the **addresses in a per-ship instance SSBO** and pass only its address plus an instance index in push constants — `maxPushConstantsSize` is only guaranteed to be 128 bytes, and 8 ships × 7 buffers × 8 bytes is 448.

Buffers whose address is taken need `SHADER_DEVICE_ADDRESS_BIT` in usage **and** `VkMemoryAllocateFlagsInfo` with `DEVICE_ADDRESS_BIT` chained onto the allocation. Missing the second fails at `vkGetBufferDeviceAddress` rather than at allocation, which makes it awkward to trace.

### 0.7 — Shader build step

Wire this now so Phase 1 is not blocked on it. An MSBuild target over a glob, with `Inputs`/`Outputs` set so incremental builds skip unchanged shaders:

```xml
<Target Name="CompileShaders" BeforeTargets="Build"
        Inputs="@(Shader)" Outputs="@(Shader->'$(OutDir)shaders/%(Filename)%(Extension).spv')">
  <Exec Command="glslc %(Shader.Identity) -o $(OutDir)shaders/%(Shader.Filename)%(Shader.Extension).spv --target-env=vulkan1.3 -I Shaders/include" />
</Target>
```

Prefer **`glslc` over `glslangValidator`** — it has proper `-I` include-path support, which matters because traversal and voxel lookup are shared across four or more passes and belong in includable units from the start. If the language spike selects Slang, this target swaps to `slangc` and nothing else changes.

### 0.8 — Pipeline cache

`VkPipelineCache` serialized to disk on shutdown, loaded at startup. Genuinely the least urgent item here — defer until compile times become annoying.

**Done when**, in order of what it costs to discover late:

1. The device reports every required feature — and if it does not, that is known before a single shader is written
2. A timing overlay shows per-pass GPU milliseconds and per-heap memory usage
3. A buffer can be allocated, uploaded with a known pattern, read back, and asserted equal

## Phase 1 — Voxel Data on the GPU

**Build**

- Offline hull extraction → occupancy bitmask, exclusive prefix table, material array, outward face mask (Layout A)
- Per-ship buffers: chunk metadata, top-level chunk bitmask, chunk index map, occupancy pool, prefix pool, attribute pools
- Ship instance SSBO: model matrix, inverse model matrix, AABB, buffer device addresses
- Upload via `vkCmdCopyBuffer2` from the staging ring, `VkBufferMemoryBarrier2` to shader read

**Verify:** a compute shader samples N random coordinates and writes back occupancy + material; compare against a CPU reference over the same source data. Get this exactly right. Every later phase reads through this path, and the failure modes here — 32-bit occupancy declaration, inclusive prefix table — produce plausible output rather than obvious breakage.

**Also re-derive the sizing assumptions here.** Once a real hull model exists, replace the shell thickness and non-planarity estimates with measured values and re-check the memory tables. Everything in this document scales off them, and the Phase 0.5 heap instrumentation gives the measured side of that comparison directly.

## Phase 2 — Rasterized Geometry, Face Culling Only

Deliberately no merging yet.

**Vulkan primitives**

- Compute pipeline, one workgroup per chunk, `vkCmdDispatchIndirect` from a GPU-built visible list
- Bit-transpose to per-axis masks for the Y and Z passes; time it separately
- Subgroup `subgroupBallot`, `bitCount`, `subgroupExclusiveAdd` for compaction
- Output: vertex SSBO + indirect args + count buffer via `atomicAdd`
- `vkCmdDrawIndirectCount` (non-indexed), or indexed with one shared static quad index buffer
- G-buffer via `VK_KHR_dynamic_rendering`: albedo, normal, material ID, emissive, depth

**Barriers** — three distinct dependencies, not one:

| Producer | Consumer | Stage / access |
|---|---|---|
| Visible-list compute | `vkCmdDispatchIndirect` | `COMPUTE_SHADER \| SHADER_STORAGE_WRITE` → `DRAW_INDIRECT \| INDIRECT_COMMAND_READ` |
| Mesh compute → indirect args + count | `vkCmdDrawIndirectCount` | `COMPUTE_SHADER \| SHADER_STORAGE_WRITE` → `DRAW_INDIRECT \| INDIRECT_COMMAND_READ` |
| Mesh compute → vertex data | Vertex stage | `COMPUTE_SHADER \| SHADER_STORAGE_WRITE` → `VERTEX_ATTRIBUTE_INPUT \| VERTEX_ATTRIBUTE_READ` |

Pairing `VERTEX_ATTRIBUTE_READ` with a `DRAW_INDIRECT` destination stage is invalid (VUID-VkBufferMemoryBarrier2-dstAccessMask-03902) — the validation layers catch it, but it is an easy one to write. If vertex data is consumed as an SSBO in the vertex shader instead of as vertex input, use `VERTEX_SHADER | SHADER_STORAGE_READ`.

**Verify:** one static ship, correct materials, no Z-fighting at chunk boundaries (the tell for a missing neighbour term in face culling). Record the quad count and pass timings — this is the baseline the Phase 7 merging decision is measured against.

## Phase 3 — Two-Level DDA

The core of the revised design.

**Build**

- Compute shader: ray → ship-local, slab test, march chunk bitmask, resolve via chunk index map, march voxel bitmask, return hit index + material + distance
- Start with a debug pass tracing one primary ray per pixel into a storage image, writing hit material. This renders the scene *entirely* by DDA and is by far the cheapest way to find traversal bugs.

**Vulkan primitives**

- `VkImage` with `VK_IMAGE_USAGE_STORAGE_BIT`
- Specialization constants for max step count and chunk dimensions, so tuning doesn't need a shader recompile
- Push constants for the ray basis

**Verify:** the DDA image matches the Phase 2 G-buffer material IDs pixel-for-pixel at the silhouette. Any mismatch is a traversal bug, and finding it here is far cheaper than finding it inside a noisy shadow term.

## Phase 4 — Shadows, and the Traversal Decision Gate

**Build**

- Deferred lighting compute pass; reconstruct world position from depth
- Per pixel, per significant light: one DDA shadow ray, early-out on first hit
- Clustered light assignment to keep the per-pixel light loop short

**Verify:** this is the measurement the whole traversal strategy rests on. Capture the shadow pass timestamp at 4K on a realistic combat scene with several capital ships, and include diagonal rays — the worst case is ~280 steps, ~3× the axis-aligned figure. Budget against the frame target.

**If it misses, reinstate the BLAS for shadows here**, before GI and destruction are layered on top and the measurement gets muddy. That is why this phase precedes them.

## Phase 5 — GI Probes

**Build**

- Probe buffer: SH coefficients (L1, 4 coefficients × RGB) + validity
- Probe activation from proximity to occupied chunks; compacted active list built on GPU
- 8–16 DDA rays per active probe over the hemisphere, per-frame rotated Fibonacci basis
- Temporal blend at 0.1–0.2 per frame; mark probes dirty when their region takes damage
- Trilinear interpolation across the 8 nearest probes at shading time

**Vulkan primitives**

- `vkCmdDispatchIndirect` over the compacted active-probe list, with the same `DRAW_INDIRECT | INDIRECT_COMMAND_READ` barrier as Phase 2
- Ping-pong probe buffers, or in-place with a `VkMemoryBarrier2` between update and sample

**Verify:** a scene lit only by an emissive panel shows plausible bounce falloff, stable across camera rotation and zoom — the property that motivated probes over screen-space. Re-measure total traversal cost with GI on: at 2.4M probe rays this is not a rounding error on the Phase 4 number.

## Phase 6 — Destruction

**Build**

- Per-instance damage diff and occupancy override, per *Damage State for Multiple Ships*
- Copy-on-write cloning the occupancy mask only, with attribute indices resolved against the **shared** mask
- Dirty chunk list, consumed by both the geometry pass and probe invalidation
- Damage application as a compute pass over a queue of impact events

**Resolve first:** the hull-only breach issue in *Hull Extraction*. Non-player ships have no geometry behind the shell.

**Verify:** shoot a hole in a hull; confirm the geometry pass and DDA traversal agree — the hole must cast light through it, not merely look like a hole. Then check materials *around* the breach specifically: getting the shared-vs-instance mask indexing wrong produces correct-looking holes surrounded by wrong-coloured hull, on a ship that is by definition under fire and hard to inspect.

## Phase 7 — Measured Optimizations

Only where Phase 2/4/5 measurements say so.

- **Binary greedy meshing**, if the mid-range band misses budget. Material ID only.
- **Layout B brick subdivision**, if voxel memory is tight — cuts it ~38% and speeds intra-chunk DDA.
- **Mesh shader path**, eliminating the persistent geometry buffer and its ~188 MB ceiling.
- **LOD meshes** for distant ships. Per the pixel-coverage figures this outweighs merging at combat zoom, and it is what stops the Phase 2 scratch buffer from clamping routinely.
- **Interior streaming** with RLE-compressed voxel data in system memory. This is the only phase with churny allocation, and its pattern is a **ring** — chunks load ahead of the player and unload behind, roughly FIFO. A linear/ring sub-allocator inside one large `VkDeviceMemory` block is both simpler and faster than a general-purpose allocator here: O(1) allocation, no fragmentation by construction, bulk reset. Roughly 80 lines behind the existing `GpuBuffer` abstraction. This design likely never needs VMA at any phase.

## Sequencing Notes

Phases 0–3 are the critical path and depend on nothing in the lighting design — build and validate them against a single static ship.

Phase 4 is the decision gate for the whole traversal strategy. Do not let it slip behind Phase 5 or 6; those are much harder to re-baseline if the shadow measurement forces the BLAS back in.

Phases 2 and 3 are worth building in parallel if you have the appetite, since Phase 3's verification compares their outputs directly and each is a check on the other.

---

## Future Improvements

### Streaming and LOD

Stream chunk data by distance from camera or active gameplay zone. For distant ships a pre-baked LOD mesh (10K–50K triangles) replaces voxel geometry, with damage as a texture overlay. At battle distances 25cm detail is far below display resolution — ~95% of a dreadnought's quads are sub-pixel at 1,200 px across.

### Sparse Voxel Octree

Layout B is a fixed two-level brickmap, capturing most of the sparsity win with far less complexity than a full SVO. A true SVO would help only for large never-destructible geometry (background scenery, derelicts) where deep uniform regions are common.

### Procedural Damage Geometry

A secondary compute pass identifying boundary voxels adjacent to destroyed ones could generate exposed interior geometry, jagged hull edges, and emitter positions for fires and venting atmosphere. Also one of the candidate resolutions to the hull-only breach issue above.

### Voxel Physics

The damage system tracks per-voxel health but not structural integrity. A per-ship connectivity graph could trigger hull section detachment when a region becomes isolated — sections floating away with their own voxel buffers and transforms.

### Compressed Voxel Storage for Interiors

An RLE representation in system memory, decompressed on demand as the player moves, would keep GPU usage flat regardless of how many interiors exist.

### Path-Traced Reference Mode

A progressive path tracer for cinematic capture. Without a persistent BLAS this requires either building acceleration structures on demand for that mode, or writing the path tracer against the same DDA traversal — the latter is simpler and shares all existing code. Either way, `lighting-model.md`'s claim that "no additional geometry preparation is needed" no longer holds.
