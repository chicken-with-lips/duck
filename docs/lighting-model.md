# Lighting Model Design

## Overview

The game uses a **deferred rendering pipeline** with a hybrid lighting model: direct lighting via shadow-mapped point/spot lights, hardware ray queries for shadow rays and ambient occlusion, and probe-based dynamic global illumination (GI) for indirect bounce light. The camera is top-down perspective with rotation and zoom but no pitch.

The renderer is Vulkan. Ray queries are performed inline in compute shaders via `VK_KHR_ray_query` using the BLAS/TLAS acceleration structures maintained by the voxel system.

---

## Frame Pipeline

```
1. CPU: mark dirty chunks, compute visible chunk list
2. Compute: rebuild dirty chunk BLASes
3. Compute: rebuild TLAS (all ships, with per-ship transforms)
4. Task/Mesh: geometry generation → G-buffer pass
   - Albedo, normal, material ID, emissive, depth
5. Compute: shadow ray queries (direct lighting)
6. Compute: ambient occlusion ray queries
7. Compute: GI probe update (DDGI ray queries per probe)
8. Compute: deferred lighting resolve
   - Direct light + shadows + AO + indirect GI
9. Graphics: post-process (bloom on emissives, tone mapping)
```

---

## Direct Lighting

Standard clustered point lights covering ship interior lights, engine glows, and weapon systems. Shadow rays are traced per pixel per significant light source using `rayQueryEXT` inline in the deferred lighting compute pass.

Because the game is set in space with a dark background and bounded light sources, the number of lights affecting any given pixel is small. Most of the scene outside ship hulls receives zero direct illumination beyond emissive contributions.

Dominant direct light sources:
- Interior ship lights (point/spot, per-deck)
- Engine exhaust glow (emissive voxels, large area)
- Weapon fire and explosions (short-lived point lights)
- Distant star (optional directional, negligible at combat distances)

---

## Ambient Occlusion

Screen-space ambient occlusion (SSAO) is computed in a dedicated compute pass sampling the depth buffer. This is the primary mechanism for the vertical darkening that the GI system does not capture — the base of tall objects (consoles, crew, machinery) will correctly appear darker than the top due to proximity to the floor geometry.

Additionally, per-vertex AO is **baked into the voxel mesh at build time** (2 bits per vertex, packed into the vertex format). This captures static occlusion at corners, recessed panels, and floor-adjacent surfaces for zero runtime cost. The two AO terms are multiplied at shading time.

---

## Global Illumination

### Chosen Approach: DDGI-Style Probe Grid + Ray Queries

Rather than a screen-space technique (which would require rebuilding on camera rotation) or a 2D approximation (which cannot correctly light tall 3D objects), GI is computed using a **sparse 3D probe grid updated via ray queries**.

Probes are placed in a 3D grid over each ship's interior and the surrounding exterior volume. Each frame, a small number of rays are traced per probe to sample incoming radiance. Probe irradiance is stored as spherical harmonics (SH) and interpolated at shading time. This approach:

- **Handles camera rotation and zoom for free** — probes are in world space and unaffected by view changes
- **Correctly lights tall objects** — a probe samples the full hemisphere, so the bottom of a fridge near a dark floor receives less indirect light than the top near a lit ceiling
- **Is low cost** — ray count is per probe, not per pixel

### Probe Layout

Probes are placed on a 3D grid at roughly **one probe per 2–4 metres**. For a 400m × 80m × 50m capital ship interior this gives approximately:

```
200 × 40 × 25 = 200,000 probes at 2m spacing
50  × 20 × 13 = 13,000 probes  at 4m spacing (exterior shell)
```

In practice only probes inside or immediately adjacent to ship geometry are active. Empty space probes are culled.

### Per-Probe Ray Budget

Each active probe traces **8–16 rays per frame** randomly distributed over the hemisphere. Results are accumulated temporally — a probe converges to a stable irradiance estimate over ~8–16 frames. At 8 rays per probe and 13,000 active exterior probes:

```
13,000 probes × 8 rays = 104,000 GI rays per frame
```

At 60fps this is roughly 6.2 million GI rays per second — a small fraction of available ray throughput on modern hardware.

### GI Resolution and 4K

GI is inherently low-frequency (no sharp edges in bounce light). The probe grid captures GI at metre-scale resolution. At shading time, each pixel looks up and interpolates between its 8 nearest probes. This is correct and visually indistinguishable from per-pixel GI regardless of output resolution, making the technique resolution-agnostic — it costs the same at 4K as at 1080p.

### Temporal Accumulation

Probe irradiance blends toward the current frame's ray result with a small weight (~0.1–0.2 per frame), creating a stable running average. For static or slow-changing scenes (ships not under fire) probes are effectively converged at all times.

For dynamic events (explosions, hull breaches, new emissive sources), probes in the affected region converge within 4–8 frames (~67–133ms at 60fps). This is imperceptible in practice. Probes whose rays hit geometry that no longer exists (destroyed voxels) are marked dirty and reset on the next update cycle.

---

## Emissive Voxels as GI Sources

Emissive voxels (engine exhausts, glowing weapon systems, fires) inject directly into the probe system — a probe whose rays intersect an emissive voxel receives the voxel's emitted radiance as its incoming light for that ray direction. No separate light injection pass is required; emissives are just bright surfaces that the probe rays naturally hit.

This means engine glow, fires, and weapon charge-up animations all contribute to GI automatically and correctly without any manual light placement.

---

## Interior vs Exterior

The same probe grid and ray query pipeline covers both camera modes — the exterior top-down combat view and the walkable interior view. No mode switching or pipeline swap is required.

For the exterior view the active probes are concentrated around the ship hull. For the interior view the active probes are the deck-level interior probes. Probe activation is determined by proximity to occupied voxels, which naturally shifts as the player moves from exterior to interior.

If only the player's ship interior is being walked (other ships viewed externally), interior probes for enemy ships can be deactivated to reduce ray count. The exterior hull probes remain active for correct combat lighting.

---

## Hardware Targets

| Hardware | GI Feasibility |
|---|---|
| RTX 4070 and above | Full probe grid, 16 rays/probe, 60fps at 4K comfortable |
| RTX 3060 / RX 6600 | Full probe grid, 8 rays/probe, 60fps at 1440p → upscale to 4K |
| GTX 1660 Ti / RX 5600 XT | Reduced probe count, 4 rays/probe, 60fps at 1080p |
| No ray tracing support | Baked GI only — see Future Improvements |

Ray query performance scales linearly with probe count and rays per probe. Both can be tuned down independently as a graphics quality setting without changing the pipeline.

---

## Summary of Lighting Terms

| Term | Technique | Resolution |
|---|---|---|
| Direct lighting | Clustered deferred | Per pixel |
| Hard shadows | Ray query (per light) | Per pixel |
| Ambient occlusion (static) | Baked per-vertex at mesh build | Per vertex |
| Ambient occlusion (dynamic) | SSAO depth-buffer sampling | Per pixel, sub-native |
| Indirect GI | DDGI probe grid + ray queries | Per probe (~2–4m grid) |
| Emissive contribution | Probe ray intersections | Per probe |
| Post-process bloom | Emissive threshold pass | Per pixel |

---

## Future Improvements

### ReSTIR GI

If per-pixel GI is needed — for example, fine-grained indirect detail in close-up interior sequences — ReSTIR GI replaces the probe lookup with per-pixel reservoir-based spatiotemporal resampling. Each pixel traces one ray per frame and reuses high-quality samples from spatial neighbours and previous frames. Quality approaches multi-ray reference at one ray per pixel. This would complement rather than replace the probe system: probes handle the broad scene and ReSTIR handles close-up detail when the player is walking inside a ship.

### BLAS Compaction for Ray Performance

Compacting BLASes after initial build (`VK_COPY_ACCELERATION_STRUCTURE_MODE_COMPACT_KHR`) reduces memory by 30–50% and can improve ray traversal cache efficiency. Recommended once the BLAS build pipeline is stable.

### Volumetric Lighting

Weapon fire, engine exhaust, and atmospheric venting from breached hull sections would benefit from volumetric lighting — a froxel grid sampled in the lighting pass to accumulate in-scattering along the view ray. This is particularly impactful for capital ship engines viewed from behind and for interior breach events where light shafts are visible through hull holes.

### Spectral Emissives

Currently emissive voxels store a single RGB colour. Extending to a simple two-lobed spectral model (warm/cool separation) would allow more physically plausible light propagation through the probe system — engine exhaust would tint nearby hull panels with a physically correct warm orange rather than a flat RGB multiply.

### Light Propagation Volumes (LPV) as Alternative

LPV stores a low-resolution 3D grid of spherical harmonic coefficients over a ship's interior volume and propagates light through it each frame via a series of compute passes. It requires no ray tracing hardware, making it a viable fallback for players without RT-capable GPUs. For a 400m ship at 1m LPV resolution: 400 × 80 × 50 = 1.6M texels, stored as 3 × R16G16B16A16 images = ~38 MB. Quality is lower than probe-based ray queries but acceptable for interiors.

### Path-Traced Reference Mode

For cinematic capture or a dedicated screenshot mode, a progressive path tracer using the full `VK_KHR_ray_tracing_pipeline` could replace the real-time pipeline entirely. The same BLAS/TLAS structures are reused. The path tracer accumulates samples over multiple frames until convergence, producing reference-quality imagery of the voxel ships. No additional geometry preparation is needed.

### Irradiance Field Caching

For completely static regions of a ship (sections not yet reached by damage), probe irradiance could be baked to a persistent cache on disk and loaded at ship spawn time, eliminating the initial convergence period for those probes. Dirty probes (in or adjacent to damaged chunks) are excluded from the cache and computed at runtime. This would make the first frame of GI correct rather than grey.
