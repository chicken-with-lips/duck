# Voxel System Design

## Overview

The game uses a voxel-based world representation where each voxel is **25cm × 25cm × 25cm** in world scale. This applies to all geometry including ships, interiors, and any destructible prefabs. Ships range from small single-seat fighters to large capital ships with fully walkable interiors.

The renderer is Vulkan. All geometry construction happens on the GPU.

---

## Data Storage

### Chunk Structure

The world is subdivided into **32³ voxel chunks** (8m³ at 25cm scale). Each ship exists in its own local coordinate space — world-space placement is handled via transform matrices at the draw/TLAS level, not baked into voxel positions. Only chunks containing at least one occupied voxel are allocated on the GPU.

Each chunk carries:

| Field | Size | Notes |
|---|---|---|
| World chunk position | 3 × int32 (12 bytes) | In voxel units |
| Voxel count | uint32 (4 bytes) | Number of occupied voxels |
| State flags | uint32 (4 bytes) | Dirty, loaded, queued for rebuild, etc. |
| Buffer offset | uint32 (4 bytes) | Offset into the ship's main voxel pool |

Chunk metadata per ship totals roughly **24 bytes × active chunk count**, which is negligible (a large capital ship has ~750 non-empty chunks).

### Per-Voxel Layout

Each voxel is stored as a single **32-bit packed integer**:

```
bits  0– 4 : Local X within chunk    (5 bits, 0–31)
bits  5– 9 : Local Y within chunk    (5 bits, 0–31)
bits 10–14 : Local Z within chunk    (5 bits, 0–31)
bits 15–22 : Material / voxel type   (8 bits, 256 types)
bits 23–30 : Damage amount           (8 bits, 0–255)
bit  31    : Flags                   (active, queued for removal, etc.)
```

This packs all simulation-relevant state into 4 bytes with no wasted bits. The full position within the ship is reconstructed from the chunk's world position plus the local XYZ.

#### Expanding to 64-bit

If more than 256 material types or richer per-voxel flags are needed, expand to 8 bytes:

```
bits  0–14 : Local XYZ               (5 bits each)
bits 15–30 : Material ID             (16 bits, 65536 types)
bits 31–38 : Damage amount           (8 bits)
bits 39–46 : Flags                   (8 bits: emissive, transparent, shield-active, etc.)
bits 47–63 : Reserved                (temperature, faction tint, phase state, etc.)
```

The 4-byte layout is recommended as the starting point.

### Damage State for Multiple Ships

When several ships of the same class exist simultaneously, base voxel data is **shared as a read-only buffer**. Each ship instance stores only a **sparse damage diff**:

```
// 8 bytes per damaged voxel entry
struct DamageEntry {
    uint32_t packedPosition;   // chunk index (16 bits) + local XYZ (15 bits) + 1 spare
    uint8_t  damage;
    uint8_t  flags;            // destroyed, breached, on fire, etc.
    uint16_t padding;
};
```

For ships of different classes, each ship holds its own base hull buffer with no sharing.

---

## Hull Extraction

For exterior rendering, the full interior voxel data is not required. A **hull-only buffer** is pre-extracted per ship class as a one-time offline process:

- Flood fill outward from the bounding volume to identify exterior-facing voxels
- Store as a separate point list
- Hull voxels typically represent ~20% of total ship voxel count

This buffer is what the GPU renders during exterior/top-down gameplay. The full voxel buffer remains the authoritative simulation state and is used when the player enters a ship interior.

### Representative Memory — Capital Ships

| Ship Class | Hull Voxels | Hull Point List (4B/voxel) |
|---|---|---|
| Light Frigate (150m) | ~0.6M | ~2.4 MB |
| Destroyer (250m) | ~1.4M | ~5.6 MB |
| Cruiser (350m) | ~2.6M | ~10.4 MB |
| Battleship (500m) | ~5.1M | ~20.4 MB |
| Carrier (550m) | ~5.8M | ~23.2 MB |
| Dreadnought (700m) | ~8.4M | ~33.6 MB |

8 different capital ship classes: **~117 MB** total hull point list data.

Full interior simulation data (one player ship loaded): **~72 MB**.

---

## GPU Geometry Pipeline

Voxels are **blocky by design** — no smooth surface interpolation. Geometry is constructed entirely on the GPU using the following pipeline.

### Greedy Meshing

Because all faces are axis-aligned and perfectly flat, greedy meshing can merge any coplanar faces of the same material into a single quad with **zero visual difference**. A large flat hull panel of consistent material becomes one quad regardless of size. Merge efficiency on ship hulls typically reaches 95–99%.

After greedy meshing, expected quad counts per ship hull:

| Ship Class | Quads (post-merge) | Geometry Buffer |
|---|---|---|
| Light Frigate | 30K–80K | 1–2.5 MB |
| Destroyer | 50K–150K | 1.5–5 MB |
| Cruiser | 90K–250K | 3–8 MB |
| Battleship | 150K–450K | 5–14 MB |
| Dreadnought | 220K–650K | 7–21 MB |
| **8 ships total** | | **~28–83 MB** |

Actual quad count is driven by the number of distinct material regions on the hull surface, not by voxel count. High material variety increases quads; large uniform panels reduce them.

### Vertex Format

Blocky voxels allow a compact vertex format:

```
bits  0–15 : X position   (uint16, voxel-space units)
bits 16–31 : Y position   (uint16)
bits 32–47 : Z position   (uint16)
bits 48–50 : Face normal  (3 bits — one of ±X, ±Y, ±Z)
bits 51–58 : Material ID  (8 bits)
bits 59–60 : AO value     (2 bits, baked at mesh build time)
bits 61–63 : Spare
```

**8 bytes per vertex.** With 4 vertices per quad this is 32 bytes per quad. With mesh shaders, quads can be encoded as a single 16-byte record (corner + extent + material) and vertices generated procedurally, halving the buffer cost.

### Mesh Shader Pipeline (Primary)

Uses `VK_EXT_mesh_shader`:

- **Task shader**: one workgroup per chunk. Performs frustum culling, distance culling, and horizontal slice culling (top-down camera only needs chunks within the visible Y range). Dispatches mesh shader workgroups for surviving chunks only.
- **Mesh shader**: reads the chunk's voxel point list from an SSBO, performs face visibility tests against neighbouring voxels, executes greedy merge, and emits quads directly. No intermediate vertex buffer.
- **Draw call**: `vkCmdDrawMeshTasksIndirectEXT` with one entry per ship.

Per-instance data in the task shader identifies the ship's damage buffer, so the mesh shader can skip destroyed voxels for that specific instance — enabling shared base geometry with per-ship damage applied at draw time.

### Compute + Indirect Draw (Fallback)

For hardware without mesh shader support:

1. Compute shader reads chunk point list from SSBO, performs face culling and greedy merge, writes to a pre-allocated vertex buffer, increments an atomic draw count.
2. `vkCmdDrawIndexedIndirect` consumes the output.
3. A pipeline barrier separates compute and graphics passes.

### Baked Ambient Occlusion

At mesh build time, the compute/mesh shader samples the 6 neighbouring voxel positions around each face and bakes a 2-bit AO value per vertex. This is free at runtime and produces natural darkening at corners, floor-adjacent surfaces, and recessed areas — compensating for the absence of vertical GI gradients in the lighting model.

---

## Multi-Ship Memory Budget

### 8 Ships, Same Class (Shared Base)

| Component | Memory |
|---|---|
| Hull point list (shared, 1 copy) | 14 MB |
| Per-ship damage maps × 8 | ~10 MB |
| Meshed geometry (shared base) | 30–50 MB |
| BLASes | 20–50 MB |
| Full interior (player ship only) | ~72 MB |
| **Total** | **~150–200 MB** |

### 8 Ships, Different Classes

| Component | Memory |
|---|---|
| Hull point lists × 8 | ~117 MB |
| Per-ship damage maps × 8 | ~10 MB |
| Meshed geometry (no sharing) | ~28–83 MB |
| BLASes × 8 | ~15–45 MB |
| Full interior (player ship only) | ~72 MB |
| **Total** | **~240–330 MB** |

BLAS compaction (`VK_COPY_ACCELERATION_STRUCTURE_MODE_COMPACT_KHR`) typically recovers 30–50% of BLAS memory after the initial build and is recommended for all ships.

---

## Acceleration Structures

A BLAS is maintained per non-empty chunk, built from the triangle output of the meshing pass. BLASes are only rebuilt when a chunk's dirty flag is set (damage, construction). The TLAS is rebuilt every frame from existing BLASes, updated with each ship's current transform — ship movement and rotation are free.

```
Per frame:
  1. Rebuild dirty chunk BLASes       → vkCmdBuildAccelerationStructuresKHR
  2. Rebuild TLAS (all ships)         → vkCmdBuildAccelerationStructuresKHR
  3. Geometry pass                    → vkCmdDrawMeshTasksIndirectEXT
  4. Ray query passes (shadow, AO, GI)
```

---

## Key Vulkan Extensions

| Extension | Purpose |
|---|---|
| `VK_EXT_mesh_shader` | Task/mesh shader geometry generation |
| `VK_KHR_ray_query` | Inline shadow, AO, and GI rays |
| `VK_KHR_acceleration_structure` | BLAS/TLAS for voxel chunks |
| `VK_EXT_descriptor_indexing` | Bindless material texture arrays |
| `VK_KHR_draw_indirect_count` | Variable-count indirect draws from GPU |
| `VK_KHR_synchronization2` | Cleaner pipeline barriers |

---

## Future Improvements

### Streaming and LOD

The current design keeps all ship hull geometry resident on the GPU simultaneously. As ship counts or sizes scale up, a streaming system would load/unload chunk geometry based on distance from the camera or active gameplay zone.

For very distant ships a pre-baked LOD mesh (10K–50K triangles) could replace the greedy-meshed voxel geometry, with damage represented as a texture overlay rather than geometry modification. At the distances involved in a large top-down battle, 25cm voxel detail is below the resolution of the display anyway.

### Sparse Voxel Octree

The current flat chunk array is optimal for dynamic destruction. If there are ship classes that are never destructible (background scenery, derelicts), an SVO representation would reduce memory significantly and could double as the acceleration structure for ray queries, eliminating the need for a separate BLAS.

### Procedural Damage Geometry

Currently destroyed voxels simply disappear from the mesh. A future improvement would generate secondary geometry at breach boundaries — exposed interior geometry, jagged hull edges, particle emitters for fires and venting atmosphere. This would be driven by a secondary compute pass that identifies boundary voxels adjacent to destroyed ones.

### Voxel Physics

The damage system currently tracks per-voxel health but does not simulate structural integrity. A future system could maintain a connectivity graph per ship and trigger hull section detachment when a region becomes structurally isolated — sections floating away as debris with their own voxel buffers and physics transforms.

### Compressed Voxel Storage for Interiors

The full interior voxel buffer (72 MB for a large capital ship) is only needed when the player is inside. A run-length encoded (RLE) compressed representation stored in system memory could be decompressed on-demand into GPU memory as the player moves through the ship, keeping GPU memory usage flat regardless of how many ship interiors exist in the simulation.
