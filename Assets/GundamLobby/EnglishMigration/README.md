# My project English lobby update

Scene: `Assets/GundamLobby/Scenes/HomeLobby.unity`.

The existing My project hangar, header positions, right-side unit card, navigation pages and footer remain in place.

- All current UI labels, catalogs and runtime messages use English.
- Laboratory opens a four-item dropdown: Lobby, Weaponry, Strike Design, Update.
- Lobby returns to the home hangar. Weaponry opens the existing weaponry page. Strike Design opens Gundam selection. Update displays a development notice; it does not implement an upgrade system.
- The playable selection catalog contains Unicorn and Strike Rouge only. Other source models remain on disk.
- `EnglishMigration/PlayableRoster.asset` holds their source combat statistics and four skill names/costs, transferred from the AR project's profiles.
- `ARPresentation` contains copies of the two completed animated presentation prefabs and their dependencies. Imported copies have remapped GUIDs to avoid replacing existing assets.
- The former left event card is inactive; its replacement shows the unit name, model grade, six core stats and a six-axis radar. Both left and right information follow the equipped Gundam.
- The selection specifications now contain 6 core rows, 4 skills, and 4 turn-economy rows.

glTFast 6.20.0 was added for the Strike Rouge GLB source. It declares Unity 6000.0 support. The render pipeline remains URP 17.0.4.

Backup scene: `Assets/GundamLobby/Backups/HomeLobby_BeforeEnglishLab_20261119.unity`.

Verification: 31 Play Mode checks passed, including all Lab routes, both roster buttons, source HP values, English text on inactive pages, and missing/error materials. Checked Game View at 1920x1080 and 1366x768. No files in AR_Transformer_Gundam were changed.

This transfer covers the requested lobby presentation and navigation. It does not copy the Story/combat implementation into My project.
