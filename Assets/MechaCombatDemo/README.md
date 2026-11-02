# Unicorn Combat Demo

Open Scenes/MechaCombatTest.unity then Play.

WASD run; Alt walk; Shift boost; Ctrl dash; Space jump/hold hover; C descend; LMB combo; RMB gun; MMB lock; Q slash; E charge beam; R ultimate; F1 help; F5 reset; Esc pause.

375 source meshes use a mechanical rigid-part skeleton with named joints, no soft skin deformation. Original OBJ/prefab/scene preserved. RigMapping and clip sheets document authored paths and timing.

Weapons, target/enemy and synthesized sound are prototype assets. Gun is hitscan with visible beam bolt. Native particle/line/trail effects target URP. Tune CombatTuning.asset. Camera is native custom controller, no Cinemachine dependency. AI is a simple collision-aware sparring proxy, not full pathfinding. Final visual animation polish still requires reviewing clips and extreme armor clearance.
