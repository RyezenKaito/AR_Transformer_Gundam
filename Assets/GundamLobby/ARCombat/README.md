# Battle scene integration

Open `Assets/GundamLobby/Scenes/HomeLobby.unity`, enter Play, select Battle and click Deploy.
The battle opens `Assets/GundamLobby/ARCombat/Game/Scenes/Story/Stage_1_1.unity`.
The currently equipped Unicorn or Strike Rouge is passed through the shared
`GundamLobby.ActiveStriker` preference. Both original combat packages, the Story HUD,
enemy profiles and battlefield assets are imported selectively from AR_Transformer_Gundam.

The combat scene path is editable on the BattleController component. Existing Battle
mode/map selection is still prototype metadata; Deploy currently starts Stage 1-1.
The original Story victory flow can continue to Stage 1-2. Pause/result menu actions
return to this project's HomeLobby, retry the current stage or resume the fight.

Combat controls: select target, choose skill 1–4 and confirm with Space or tap again.
During defense timing, Q/D dodges and E/A parries. P opens the pause menu.

Existing project scenes remain in Build Settings; HomeLobby and the two Story stages
are added and enabled. The Unity version, rendering pipeline and package manifest
are retained. The source AR project is read-only throughout this integration.

Backups: `Assets/GundamLobby/Backups/HomeLobby_BeforePlayableBattle_20261119.unity`,
plus BattleController and Build Settings snapshots in the workspace migration folder.
Imported GUIDs and file paths are recorded in `guid-remap.json` and `imported-files.json`.
