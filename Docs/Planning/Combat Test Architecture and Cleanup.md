# Kiến trúc dữ liệu và kế hoạch dọn project

## Kết quả đợt dọn đã duyệt

- Đã chuyển 99 mục theo sơ đồ. HomeLobby ở `Assets/Game/Scenes`, nội dung lobby ở `Assets/Game/Features/MainMenu/Lobby`, runtime và art riêng ở thư mục từng nhân vật. Các scene showcase ở `Assets/Sandbox/Showcases`.
- Đã đưa 39 file nhóm A ra khỏi Assets và SourceArt. Bản rollback gồm các file gốc và .meta ở `Archive/CombatCleanup/Approved-rollback.zip`.
- Menu mở demo combat của Unicorn và Rouge trỏ đến Combat Test. Hai scene nguồn cũ còn phục vụ công cụ authoring/validation riêng và luồng AR của Rouge, nên giữ lại cho tới khi chuyển các công cụ đó. Các mục cần đối chiếu trong nhóm C vẫn được giữ.
- Unity đã mở HomeLobby, Combat Test, bốn showcase và scene build điện thoại mà không báo missing script. Hai profile, prefab và clip đều nạp được.

## Quy trình Combat Test

**Gundam → Combat → Open Test Scene → Play.** Bấm **F2** hoặc nút tên nhân vật để đổi giữa RX-0 Unicorn và Strike Rouge Ootori. Mỗi lần đổi sẽ khởi tạo trận mới với cùng sân ảo và ba địch thử. Danh mục ngừng nhận lệnh đổi khi đang trong cinematic.

Unity Play Mode đã chạy Unicorn → Rouge → Unicorn trong cùng một scene. Mỗi lần đổi chỉ còn một camera sân và một EventSystem. AssetDatabase xác nhận scene không phụ thuộc trực tiếp vào profile hoặc prefab gói nhân vật. [Kết quả kiểm thử](../Reports/CombatTestValidation.txt).

| Thành phần | Dữ liệu và trách nhiệm |
|---|---|
| Combat Test Catalog | Tên hiển thị và khóa tải của mỗi nhân vật. Scene không giữ tham chiếu trực tiếp tới toàn bộ prefab trong roster. |
| CharacterCombatProfile | Tên, HP, Armor, ATK, DEF, SPD, Break DMG và bốn slot chiêu với Burst cost, clip Unity và hệ số. Unicorn đọc thời lượng action từ profile. Rouge dùng hệ số trên sát thương gốc của từng chiêu, giữ nhịp hit và VFX trong runtime hiện có. |
| Combat Package | Prefab có model, controller, animation rig, socket, HUD và tham chiếu VFX từng nhân vật. Gói Unicorn dùng prefab chuẩn `Unicorn_Base` dạng nested. |
| Shared fixture | Sân, ánh sáng và ba đối thủ được tái sử dụng. Máu, giáp, Burst, buff và trạng thái lượt sống trong instance của trận. |

Mỗi profile là ScriptableObject để có thể chỉnh trong Inspector và giữ tham chiếu prefab/clip bằng Unity GUID. Loader dùng `Resources.LoadAsync` theo khóa. Khi đổi nhân vật, instance cũ được hủy và asset Resources có thể còn trong bộ nhớ. Unity đóng toàn bộ asset trong thư mục Resources vào build. Khi roster lớn hoặc cần tải theo nội dung phát hành, chuyển sang Addressables để quản lý bundle và giải phóng handle theo gói. JSON phù hợp cho save hoặc dữ liệu trao đổi dựa trên ID.

### Thêm nhân vật

1. Tạo prefab gồm model, bộ điều khiển combat và các dây nối socket, animation, HUD. Gắn CombatTestPackage để nhận ba đối thủ chung. Hai runtime đang có là Unicorn và Rouge. Runtime khác cần bổ sung cách nối vào CombatTestPackage.
2. Tạo CharacterCombatProfile cho nhân vật trong một thư mục Resources và điền bốn slot, gồm chi phí cùng clip tương ứng.
3. Thêm một entry vào Combat Test Catalog bằng đường dẫn tương đối từ Resources, bỏ phần đuôi .asset.
4. Vào Combat Test và kiểm tra đổi sang nhân vật mới, bốn chiêu, hiệu ứng, nhánh phòng thủ và đổi lại nhân vật trước.

## Sơ đồ thư mục sau khi duyệt

```text
AR_Transformer_Gundam/
├── Assets/
│   ├── Game/
│   │   ├── Scenes/                       # Scene game và HomeLobby
│   │   ├── Features/
│   │   │   ├── Characters/
│   │   │   │   ├── Scripts/             # Schema profile và logic nhân vật dùng chung
│   │   │   │   ├── Unicorn/
│   │   │   │   │   ├── Art/, Animations/, Materials/, Prefabs/
│   │   │   │   │   ├── Runtime/, UI/, Data/ # Hành vi, art HUD, cấu hình buff
│   │   │   │   │   └── Resources/CombatProfiles/Unicorn/ # Profile + Combat Package
│   │   │   │   └── StrikeRouge/
│   │   │   │       ├── Animations/, Runtime/, UI/
│   │   │   │       └── Resources/CombatProfiles/StrikeRouge/
│   │   │   ├── Combat/                  # Luật lượt, camera và HUD dùng chung
│   │   │   ├── AR/, Assembly/, Inventory/
│   │   │   └── MainMenu/                # UI, prefab, hangar và dữ liệu lobby
│   │   └── Shared/                      # Art, âm thanh, VFX được dùng chung
│   ├── Models/                          # Nguồn model theo gói, giữ cấu trúc import
│   ├── Sandbox/
│   │   ├── CombatTest/                  # Một scene, catalog, fixture, runner
│   │   └── Showcases/                    # Scene duyệt art/model và nguyên mẫu
│   ├── Tools/Editor/                    # Authoring, kiểm tra, làm lại prefab
│   └── Settings/, XR/, XRI/, Samples/, TextMesh Pro/, MobileARTemplateAssets/
├── SourceArt/                           # Blender gốc, file nguồn, render và reference
├── Docs/                                # Hướng dẫn, GDD và kết quả kiểm thử
├── Tools/                               # Script hỗ trợ ngoài Unity
├── Packages/, ProjectSettings/          # Cấu hình Unity
└── Archive/, Builds/, Clone/            # Lưu trữ, bản build và checkout cục bộ
```

### Bản đồ di chuyển dự kiến

| Nguồn | Đích | Điều kiện |
|---|---|---|
| Assets/Sandbox/UnicornCombat/Runtime | Assets/Game/Features/Characters/Unicorn/Runtime | Giữ GUID của MonoBehaviour. |
| Assets/Sandbox/UnicornCombat/Unicorn Psycho-Frame Buff.asset | Assets/Game/Features/Characters/Unicorn/Data | Cập nhật đường dẫn trong công cụ authoring. |
| Assets/Game/Features/Combat/UI/Unicorn | Assets/Game/Features/Characters/Unicorn/UI | Cập nhật đường dẫn authoring. |
| Assets/Game/Features/Combat/Animations/StrikeRouge | Assets/Game/Features/Characters/StrikeRouge/Animations | Cập nhật đường dẫn của trình tạo clip. |
| Assets/Game/Features/Combat/Scripts/StrikeRougeDemo | Combat/Scripts và Characters/StrikeRouge/Runtime | Chia theo lớp. DemoFighter, RougeTurnOrder, MechaBattleHUD, MechaPlateGraphic, RougeHudArt và camera dùng chung ở Combat. Battle/presentation/animation driver/cinematic ở StrikeRouge. RougeArenaPlacement về Features/AR. |
| Assets/Game/Features/Combat/UI/StrikeRouge | Combat/UI/Shared cho prefab MechaBattleHUD. Character UI cho art riêng | Giữ GUID prefab HUD và các Sprite. Ba ảnh địch được dùng chung trong fixture. |
| Assets/Sandbox/UnicornCombat/Materials | Assets/Sandbox/CombatTest/Materials | Giữ GUID cho sân ảo và URP Volume đang được Test Scene dùng. |
| Assets/GundamLobby | Assets/Game/Scenes và Assets/Game/Features/MainMenu | Di chuyển theo nhóm Scene, UI, hangar, Editor và vật liệu. Rà toàn bộ đường dẫn dạng chuỗi trước khi chuyển. |
| Assets/Models/{BlueFlame,HaoLing,SkyDefender}/Scenes và UnicornMotionPrototype | Assets/Sandbox/Showcases/{Tên} | Cập nhật menu của từng công cụ dựng model. Giữ model gốc và các cảnh showroom có giá trị kiểm duyệt. |
| Assets/GundamLobby/Previews | Docs/Reports/UIIntegration hoặc SourceArt/Reviews/Lobby | Chuyển ảnh kết quả duyệt được giữ lại. |

## Danh sách xin duyệt xóa

Rà soát theo GUID trong YAML của Unity, đường dẫn dùng trong code và SHA-256 theo nội dung. Kết quả tham chiếu tĩnh không bao quát dữ liệu trong worker cache hoặc tham chiếu nội bộ của file nhị phân. Danh sách dưới đây ghi các đường dẫn trước đợt dọn.

### Nhóm A — 39 bản sao lưu và ảnh trùng

Nếu bạn còn dùng các bản rollback này, có thể lưu bản zip ngoài Assets trước khi xóa. Xóa asset Unity qua AssetDatabase để xử lý luôn file .meta.

- `Assets/GundamLobby/Backups/CinematicVolume_BeforeSharpLobby_20261029_111541.asset`
- `Assets/GundamLobby/Backups/Hangar_BeforeHeavyDock_20261029_110852.prefab`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeDetailedHangar_20261029_090421.unity`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeHeavyDock_20261029_110852.unity`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeInventory_20261102_083559.unity`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeLowCamera_20261029_114947.unity`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeStriker_20261102_134012.unity`
- `Assets/GundamLobby/Backups/HomeLobby_BeforeUnicorn_20261029_083901.unity`
- `Assets/GundamLobby/Backups/HomeLobby_before_screenshot_match.unity`
- `Assets/GundamLobby/Backups/LobbyShape.cs.bak`
- `Assets/GundamLobby/Backups/Mobile_RPAsset_BeforeSharp_20261029_112902.asset`
- `Assets/GundamLobby/Backups/Mobile_RPAsset_BeforeSpeckleFix_20261102_093322.asset`
- `Assets/GundamLobby/Backups/PC_RPAsset_BeforeLobbyForward.asset`
- `Assets/GundamLobby/Backups/PC_RPAsset_BeforeSharpAA_20261029_112348.asset`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/Activities.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/Footer.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/Gundam Information.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/Header.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/HomeLobby_before_repair.unity`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/PlaceholderGundam.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/PlaceholderHangar.prefab.bak`
- `Assets/GundamLobby/Backups/CanvasRepair_20261028_203928/Team Strip.prefab.bak`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/BeforeCombat_20261030_015533_0.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/BeforeValidation_20261030_130824.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeCamera_20261030_211259.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_033319.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_033642.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_033802.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_034006.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_034919.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeHud_20261031_035457.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeUpgrade_20261030_184936.unity`
- `Assets/Sandbox/StrikeRougeCombat/PreservedScenes/CombatBeforeUpgrade_20261030_185254.unity`
- `Assets/Models/HaoLing/Backups/UnicornPhoneDemo_20261027_095504.unity`
- `SourceArt/Blender/Unicorn/Unicorn_Import.blend1`
- `SourceArt/Blender/Unicorn/Unicorn_MotionPrototype_v01.blend1`
- `Assets/GundamLobby/Previews/Inventory_PreviewTest.png`
- `Assets/GundamLobby/Previews/Inventory_RT_Debug.png`
- `Assets/GundamLobby/Previews/Inventory_RT_Verified.png`

### Nhóm B — Hai combat scene đã có đường thay thế

- `Assets/Sandbox/UnicornCombat/Scenes/Unicorn Demo.unity`.
- `Assets/Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity`.

Chỉ loại hai scene cũ sau khi chuyển menu và validation của bản cũ sang Combat Test. Kiểm lại luồng AR gốc của Rouge nếu cần test AR trên thiết bị, vì Combat Test hiện là sân ảo. Scene `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity` là entry đang bật trong Build Settings và được lệnh build APK gọi trực tiếp. Đề xuất giữ scene đó ở đợt này.

### Nhóm C — Xác minh trước khi gom bản trùng đang dùng

- Hai bản `UNICORN_GUNDAM_OBJ.obj` ở gốc Unicorn và trong Textures trùng nội dung. Bản ở gốc có tham chiếu. Chỉ đề xuất xóa bản trong Textures sau khi kiểm tra .mtl và luồng import lại.
- Kamiki FBX ở FigmaArtSource trùng byte với bản trong Models, nhưng StrikerModelBaker đang mở nó bằng đường dẫn cứng. Chuyển baker về bản chuẩn và kiểm texture/import settings trước khi duyệt loại bản Figma.
- Texture RX-78-2 trùng byte ở hai bộ model. Giữ cùng GLB/FBX/MTL cho đến khi xác minh tham chiếu ngoài Unity. Các mẫu XR, StudioReflection của BlueFlame/SkyDefender và ảnh portrait trùng đang có GUID được dùng độc lập.
- `Assets/Sandbox/Legacy/Scenes/SampleScene.unity` vẫn nằm trong Build Settings ở trạng thái tắt. Cần kiểm tra nội dung riêng trước khi đề xuất xóa. `Assets/Settings/Project Configuration/SampleScene.unity` thuộc cấu hình/template. Giữ cả hai trong đợt đầu.
- Thư mục cache `Library`, `Temp`, `Logs`, `.vs` và checkout `Clone` nằm ngoài đợt dọn nội dung Unity. Không dọn các junction tương thích trong Docs.

### Kiểm tra sau khi được duyệt

1. Chụp danh sách GUID và dependencies của Combat Test, HomeLobby, prefab chuẩn và build scene.
2. Dùng AssetDatabase.MoveAsset để di chuyển. Kiểm tra GUID cũ, cập nhật menu, build scripts, hard-coded paths và tài liệu liên quan.
3. Quét missing scripts/references, mở HomeLobby, đổi Unicorn ↔ Rouge và chạy các chiêu trong Combat Test. Kiểm tra lại build profile và các scene showcase.
4. Xóa theo đúng danh sách được duyệt sau khi các kiểm tra trên đạt.
