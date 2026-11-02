# Industrial Hangar / Sector 07

Detailed 3D replacement for HomeLobby. Curved structural ribs, two-level service catwalks, pressure lock, pipework, maintenance manipulators, crane, launch plinth, forklift, tool chest, pallets and gas bottles are actual meshes. Lighting uses the existing URP pipeline, shadowed directional key, warm practicals, local reflection probe, height-spanning soft light-shaft planes and dust particles. The light-shaft effect is a translucent visual approximation rather than physically simulated volumetric scattering.

Prefab: Prefabs/IndustrialHangar_Sector07.prefab. Lobby Systems > Hangar Prefab points here. Its scene preview is hidden at Play time and replaced once by the prefab. Original placeholder environment remains inactive. Unicorn and all UI remain separate.

## Edit geometry
Open the prefab. For detailed per-part editing enable 01 Editable Geometry and disable 02 Optimized Render Geometry. The visible optimized version combines static pieces into material batches. Apply the same edits to optimized meshes or rebuild when finished; source edits do not automatically regenerate the batches. Lights, signs, reflection probe and particles stay individually editable. Materials use a generated brushed-metal texture and can be replaced with production PBR texture sets later.

The cinematic VolumeProfile stores every override as a persistent subasset. Old post-processing asset is retained. A scene copy is saved under Assets/GundamLobby/Backups before replacement.
