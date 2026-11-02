# WaterWorks / URP compatibility

Checked locally in Unity 6000.5.8f1 with the installed URP 17.5 package.

The original WaterWorks `Water_Volume.cs` failed compilation after import: obsolete RenderTargetHandle (error) and removed Configure/Execute overrides. The original source and meta were copied to `Backups/SunkenHarbor/20261102_142126_WaterWorksOriginal` before changing the script.

`Assets/WaterWorks/Scripts/Water_Volume.cs` now retains the same class and script GUID, wrapping the installed URP FullScreenPassRendererFeature. It only binds a material using `SunkenHarbor/Underwater Optics`. The legacy volume material expects a legacy blit input and is intentionally not activated by the adapter. Old WaterWorks demo-volume behavior has not been reimplemented/tested; assign the modern optics material if using this adapter explicitly.

The new battlefield uses its own renderer data `Lighting/HarborRenderer.asset`, a standard URP FullScreenPassRendererFeature before post-processing, and `Materials/Camera water optics.mat`. It does not depend on activating the legacy WaterWorks feature or on Water_Settings/Resources material mutation.

The active PC pipeline renderer list gained this renderer as an additional entry; its default renderer index is unchanged. The prior pipeline asset was backed up in `Backups/SunkenHarbor` before adding the entry. The new renderer is selected on map/review cameras only. Desktop Editor is the tested configuration; other quality pipeline assets/platforms may need the renderer registered separately.

The custom weathered-surface shader includes a DepthNormals pass. This is required by the existing SSAO/depth prepass so underwater reconstruction sees environment geometry, rather than only built-in robot materials. The material uses a single optical-distance pass; RenderSettings fog is disabled in this scene to avoid double fogging. Overlay UI remains unaffected.

Compatibility checks include GPU image differences with attenuation enabled/disabled, above-water sky bypass, zero shader compile errors and a transition to Desert that retains its original renderer.

The old island TerrainData files can report serialized-version deprecation warnings on read. They are sampled into new independent mesh assets; the original data was not re-saved or altered. Raw tank meshes use very large source coordinates: gameplay collision for the tank groups is simplified to bounds colliders instead of cooking those raw triangle meshes.

During the last Play Mode test, Unity reported `Cannot import package in play mode` from a GUI import action. This is separate from the scene verification; Unity was returned to Edit Mode to permit asset import.
