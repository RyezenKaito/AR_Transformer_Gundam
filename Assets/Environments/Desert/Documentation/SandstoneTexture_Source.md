# Weathered sandstone texture

Mode: built-in `image_gen.imagegen`, new raster material asset. Generated on 2026-11-02 for this Unity project; no external texture downloaded.

Saved texture: `Assets/Environments/Desert/Textures/Sandstone_Weathered_Albedo.png`.
Original: `C:/Users/LENOVO/.codex/generated_images/01a18fec-d19f-7953-834e-9891f1f3960b/exec-9fbccf77-2ad5-4bc9-b0cd-a93e3e37478e.png`.

## Generation prompt

Use case: photorealistic-natural. Asset type: production game PBR BASE COLOR ALBEDO texture, a single seamless tile of natural weathered desert sandstone rock face. Create a square high resolution flat orthographic surface scan representing approximately 3 by 3 meters of warm beige reddish sandstone, fine sandy mineral grains, irregular chipped edges of shallow flakes, subtle sedimentary laminae with irregular thickness, a few fine branching fractures, tiny pores and granular rough weathered areas. Geologically credible natural rock, photographic surface detail, moderate warm muted ochre / beige / dusty reddish brown, consistent neutral exposure. Texture fills entire image edge to edge, perfectly tileable horizontally and vertically, evenly lit shadowless diffuse color only, no baked directional shadows or shiny highlights, no ambient occlusion black crevices, no perspective, no horizon, no landscape, no rocks as separate objects. Avoid regular horizontal stripes, sinusoidal wavy bands, wood grain, polished wood, stacked pancakes, orange oversaturation, large black streaks, dramatic lighting, text and watermarks. This is a material texture sheet to wrap all mountains in a Unity desert scene, not a rendered scene.

## Unity integration

- Texture imported as sRGB albedo, Repeat wrapping, mipmaps, anisotropy 8.
- Shared world-space triplanar mapping at 0.22 repeats/metre; same scale on the main massif, package canyon meshes, distant mountains and rock clusters.
- 103 cliff/rock renderers use three distance variants of the same surface, retaining one common texture and palette.
- Removed periodic sine bands from both the mountain face geometry and its material.
- Fine surface normals are approximated from filtered albedo luminance in the shader, with constrained strength and distance fading; this is not a scanned normal/height map. Roughness is varied gently from the material texture.
- Captured close and gameplay views after reducing the original relief speckling. Validation: `RockSurfaceVerification.md`, 7 PASS. No performance benchmark claimed.
- Review: `Review/Desert_RockDetail.png`, `Review/Desert_Gameplay.png`, `Review/Desert_Overview.png`.

Backup of the prior shader, canyon source, mesh, material and scene: `Backups/Desert/20261102_132131_BeforeRockTexture`.
