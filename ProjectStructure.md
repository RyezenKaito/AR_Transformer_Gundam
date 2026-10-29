# Cấu trúc project dành cho nhóm phát triển

Tài liệu này dành cho ba thành viên phát triển AR Transformer Gundam. Nó giải thích nơi đặt file, thư mục nào Unity tự quản lý và những gì không được di chuyển tùy ý. Đợt tổ chức hiện tại giữ nguyên `Assets/Models`; việc unpack và chia lại model tải về sẽ thực hiện sau khi nhóm thống nhất cách xử lý.

## Cây thư mục tổng quan

```text
Gundam_AR_Test/
├── Assets/                 Tài nguyên Unity import khi chạy game
├── Packages/               Danh sách package Unity
├── ProjectSettings/        Cấu hình project và nền tảng build
├── Docs/                   Tài liệu nội bộ
├── SourceArt/              Ảnh tham khảo, render và nguyên liệu tạo hình
├── Tools/                  Công cụ chạy ngoài Unity
├── Builds/                 APK và bản build đã xuất
├── Archive/                Bản cũ và mốc khôi phục cục bộ
├── GeneratedAssets/        Dữ liệu tạm do công cụ tạo asset sinh ra
├── Library/ Logs/ Temp/    Cache, log và file tạm Unity
├── UserSettings/ .vs/ .utmp/  Thiết lập/cached theo máy
├── .git/ .gitignore        Lịch sử và quy tắc Git
├── .vsconfig               Workload Visual Studio đề xuất
├── README.md               Hướng dẫn cài và chạy
├── Gundam_AR_Test.slnx     Solution cho IDE
└── *.csproj                Project C# do Unity/IDE sinh lại
```

## Các thư mục ở gốc project

| Thư mục hoặc file | Chứa gì | Khi thêm file | Tác dụng và lưu ý |
|---|---|---|---|
| `Assets` | Scene, script, prefab, model, material, texture, audio và cấu hình Unity | Khi Unity cần import/use nội dung đó | Unity dùng `.meta` để giữ liên kết. Di chuyển asset bằng cửa sổ Project của Unity hoặc AssetDatabase. |
| `Packages` | `manifest.json`, `packages-lock.json` | Khi cài/gỡ package | Giữ thư viện và phiên bản đồng nhất cho cả nhóm. |
| `ProjectSettings` | Android, iOS, input, graphics, XR, Build Settings | Khi đổi cấu hình chung | Giúp build nhất quán giữa các máy; cần xem xét kỹ trước khi sửa. |
| `Docs` | Kế hoạch, thiết kế, hướng dẫn, hồ sơ nhân vật, báo cáo | Khi viết nội dung cho nhóm | Thành viên mới hiểu project mà không phải mở từng scene. |
| `SourceArt` | Ảnh tham khảo, render review, nguyên liệu art | Khi có ảnh nguồn/ảnh đánh giá | Không phải asset chạy game, không làm nặng APK. |
| `Tools` | Script PowerShell, công cụ build, xử lý ảnh | Khi công cụ chạy ngoài Unity | Tách tool khỏi gameplay và quá trình import Unity. |
| `Builds` | APK, trạng thái build, dữ liệu gỡ lỗi | Khi xuất cho tester | Phân theo nền tảng để không nhầm bản cài. |
| `Archive` | Backup, lịch sử authoring, mốc trước khi tổ chức | Khi cần giữ bản cũ | Không phải nguồn đang chạy; `.gitignore` bỏ qua vì có thể lớn/cục bộ. |
| `GeneratedAssets` | Dữ liệu do plugin/công cụ sinh ra | Không tự thêm file thủ công | Chỉ dọn khi biết rõ công cụ tạo ra và không còn cần. |
| `Library`, `Logs`, `Temp`, `UserSettings`, `.vs`, `.utmp` | Cache/thông tin theo máy | Không thêm file thủ công | Unity/IDE tự tạo lại. Không commit, không dời chỉ để “làm sạch”. |
| `.git`, `.gitignore` | Lịch sử và quy tắc Git | Dùng khi làm việc với Git | Commit nguồn hiện hành; loại cache/build local/archive cục bộ. |
| `.vsconfig`, `.slnx`, `.csproj` | Thiết lập/cấu trúc IDE | `.csproj` và `.slnx` do Unity/IDE sinh | `.vsconfig` hỗ trợ cài đúng workload; không chỉnh `.csproj` trực tiếp. |

## Bên trong Assets

```text
Assets/
├── Models/                         Giữ nguyên trong đợt tổ chức hiện tại
├── Game/                           Phần game dùng chung
│   ├── Scenes/                     Scene game chính
│   ├── Core/                       Khởi động, chuyển scene, lưu dữ liệu
│   ├── Features/
│   │   ├── AR/                     AR placement, anchor, raycast
│   │   ├── Characters/             Điều khiển, animation, dữ liệu nhân vật
│   │   ├── Assembly/               Tháo/lắp bộ phận và socket
│   │   ├── Combat/                 Máu, sát thương, năng lượng, Burst
│   │   ├── Inventory/              Gundam, vũ khí, core, currency
│   │   └── MainMenu/               Màn hình đầu và chọn chế độ
│   └── Shared/                     UI, VFX, Audio dùng chung
├── Sandbox/Legacy/                 Scene thử cũ
├── Tools/Editor/                   Tool chỉ chạy trong Unity Editor
├── Settings/Rendering/             Cấu hình rendering
├── XR/ và XRI/                     Cấu hình XR/XR Interaction Toolkit
├── Samples/                        Sample import từ package
├── MobileARTemplateAssets/         Asset của Mobile AR Template
└── TextMesh Pro/                   Font, shader, resource chữ
```

### Models

`Assets/Models` chứa 10 Gundam, model tải về, prefab, material, texture, scene Atelier, Unicorn Demo, script riêng từng model và một số backup. Trong đợt tổ chức, 741 file tại đây đã được kiểm tra SHA-256 và giữ nguyên.

Không chuyển, đổi tên, unpack hoặc xóa gì trong `Models` khi làm tính năng chung. Sau khi nhóm chốt cách xử lý model tải về, mỗi Gundam có thể dần thống nhất `Meshes`, `Materials`, `Textures`, `Animations`, `Prefabs`; chỉ làm khi đã kiểm tra scene/prefab tham chiếu.

### Game

`Game` biến model thành game. Không đặt model nguồn từng Gundam vào đây. Khi một feature đủ lớn, tạo `Scripts`, `Prefabs`, `Data`, `UI` bên trong feature đó, thay vì tạo thư mục rỗng trước.

| Thư mục | File nên đặt | Giúp gì cho project |
|---|---|---|
| `Game/Scenes` | MainMenu, Inventory, Arena, Story | Phân biệt scene game thật với test/sample. |
| `Game/Core` | Bootstrap, scene loader, save/load, service dùng xuyên game | Không để code nền tảng lẫn feature. |
| `Game/Features/AR` | `ARPlacement`, AR anchor/plane/raycast, UI trạng thái AR | Tái sử dụng AR cho mọi Gundam. |
| `Game/Features/Characters` | Movement, animator, Character Definition, prefab playable | Các Gundam dùng chung khung điều khiển. |
| `Game/Features/Assembly` | Socket, Part Definition, lắp/tháo, UI part | Nơi dành cho gameplay lắp ráp. |
| `Game/Features/Combat` | Damage, health, skill, mana, Burst, combat AI | Gom luật chiến đấu. |
| `Game/Features/Inventory` | Dữ liệu sở hữu, vũ khí, core, trang bị, coin | Phục vụ kho và shop. |
| `Game/Features/MainMenu` | UI/controller màn hình đầu, chọn mode, ghép trận | Tách luồng vào game. |
| `Game/Shared/UI` | Button, dialog, HUD, thanh máu, joystick | Không duplicate component UI. |
| `Game/Shared/VFX` | Shockwave, hit, glow, landing dùng chung | Tái sử dụng hiệu ứng. |
| `Game/Shared/Audio` | Nhạc và âm thanh chung | Tránh nhiều bản audio giống nhau. |

Hiện có `ARPlacement.cs` trong `Game/Features/AR/Scripts`, còn movement và animation controller ở `Game/Features/Characters/Scripts`. Đây là code đã có trước khi tổ chức, chỉ đổi vị trí chứ không đổi tên class hay hành vi.

### Sandbox và Tools Editor

`Sandbox/Legacy/Scenes/SampleScene.unity` là SampleScene cũ. `Sandbox/Legacy/Models copy` là thư mục rỗng đã tồn tại trước khi tổ chức và được giữ lại để không tự xóa dữ liệu nhóm. Các scene demo riêng của BlueFlame, HaoLing, SkyDefender và Unicorn vẫn ở `Models` vì chúng thuộc từng model.

`Assets/Tools/Editor` hiện có `GundamAnimTester.cs`. Tên thư mục `Editor` có ý nghĩa đặc biệt với Unity: code bên trong không được đưa vào APK. Chỉ đặt menu, builder, validator và cửa sổ test Editor ở đây; logic người chơi dùng trên điện thoại phải ở `Game` hoặc model tương ứng.

`Settings/Rendering` chứa asset render như `DefaultVolumeProfile.asset`. `XR`, `XRI`, `Samples`, `MobileARTemplateAssets` và `TextMesh Pro` đang do package/mẫu quản lý; giữ vị trí hiện có, không dời khi chưa hiểu tham chiếu.

## Docs SourceArt Tools Builds Archive

```text
Docs/
├── Planning/       Kế hoạch, phân công và Word cho nhóm
├── Design/         Thiết kế và ghi chú kỹ thuật
├── Characters/     Mô tả từng Gundam
├── Guides/         Hướng dẫn cài, chạy và phát triển
└── Reports/        Build report, validation, đánh giá

SourceArt/
├── References/     Ảnh tham khảo
├── Renders/        Ảnh render trình diễn
└── Reviews/        Ảnh review

Tools/
├── Art/            Script decal, texture và ảnh
└── Build/          Script build và đường dẫn tương thích

Builds/
└── Android/        APK theo demo/phiên bản

Archive/
├── Characters/     Authoring source cũ
├── Experiments/    Kết quả thử nghiệm/scene backup
└── ProjectOrganization/  Mốc khôi phục khi tổ chức lại
```

`Docs/Planning` đặt kế hoạch 8 tuần, phân công, biên bản và file Word nhóm đọc. `Docs/Design` chỉ chứa thiết kế, còn `Docs/Reports` chỉ ghi kết quả đã kiểm tra. Ảnh nguồn không cần chạy game thì nằm ở `SourceArt`; khi một ảnh trở thành texture game, đưa bản được duyệt vào `Assets/Models/<Gundam>/Textures` hoặc feature tương ứng.

`Tools/Art` và `Tools/Build` có thể ghi file nên đọc nội dung trước khi chạy. `Builds` dành cho bản cài gửi tester; không đặt source code vào đó. `Archive` chỉ là lịch sử, không lấy file trong đó thành “bản chính” nếu chưa đối chiếu asset đang dùng.

## Junction tương thích trên Windows

Một số builder trong `Assets/Models` còn dùng đường dẫn cũ. Để không sửa `Models`, project dùng các junction sau:

| Đường dẫn cũ | Đường dẫn chính |
|---|---|
| `Docs/Art` | `SourceArt` |
| `Docs/BlueFlame` | `Docs/Reports/BlueFlame` |
| `Docs/SkyDefender` | `Docs/Reports/SkyDefender` |
| `Docs/SceneBackups` | `Archive/Experiments/SceneBackups` |
| `Builds/UnicornDemo` | `Builds/Android/UnicornDemo` |

Junction không tạo bản sao dữ liệu. Khi copy project sang máy Windows khác hoặc đổi vị trí project, chạy `Tools/Build/Setup-LegacyPaths.ps1`. Trên macOS/Linux cần symbolic link tương đương hoặc một đợt sửa builder trong `Models` được nhóm chấp thuận.

## Quy tắc làm việc chung

1. Trước khi di chuyển asset trong `Assets`, kiểm tra scene/prefab tham chiếu nó và dùng Unity để di chuyển.
2. Không kéo asset ra/vào `Assets` bằng Windows Explorer khi Unity đang mở.
3. Không đặt APK, ZIP tải về, screenshot review hoặc backup vào `Assets` trừ khi game thực sự dùng khi chạy.
4. Code mới đặt theo feature. Nếu chưa rõ vị trí, thảo luận trước khi tạo `Misc`, `Temp` hoặc `New Folder`.
5. Commit file nguồn hiện hành; `.gitignore` loại cache, build local và archive cục bộ.
6. Khi unpack một Gundam, ghi nguồn model, prefab gameplay, scene đã kiểm tra và người thực hiện trong `Docs/Characters/<Tên Gundam>`.

## Quy trình thêm một tính năng

1. Chọn feature sở hữu chức năng trong `Assets/Game/Features`; nếu dùng xuyên game, cân nhắc `Game/Core` hoặc `Game/Shared`.
2. Tạo hoặc cập nhật scene test, prefab và dữ liệu cần thiết ngay trong feature đó; chỉ dùng `Sandbox` cho thử nghiệm tạm thời.
3. Chạy thử trong Editor, sau đó kiểm tra trên thiết bị Android nếu chức năng có camera, cảm ứng, hiệu năng hoặc UI mobile.
4. Ghi scene đã kiểm tra và các giới hạn còn lại vào `Docs/Reports` hoặc tài liệu Gundam liên quan.
5. Chỉ đưa asset, script và tài liệu đang được dùng vào commit; không đưa cache Unity, APK tạm hay ảnh tham khảo chưa duyệt vào `Assets`.

## Checklist trước khi bàn giao hoặc merge

| Kiểm tra | Kết quả cần có |
|---|---|
| Scene và prefab | Mở được, không missing script/material/reference. |
| Thiết bị mục tiêu | Đã thử Editor và Android khi chức năng có liên quan mobile/AR. |
| Vị trí file | Asset trong `Assets` khi game cần dùng; tài liệu, ảnh nguồn và APK ở đúng thư mục ngoài `Assets`. |
| Model tải về | Chưa unpack hoặc dời cấu trúc nếu nhóm chưa duyệt. |
| Tài liệu | Cập nhật báo cáo/ghi chú đủ để hai thành viên còn lại chạy thử được. |
