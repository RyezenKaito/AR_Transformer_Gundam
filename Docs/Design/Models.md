# Thư viện model

Tất cả bộ model được quản lý trong `Assets/Models/`, theo tên từng bộ. Di chuyển bằng Unity AssetDatabase để giữ nguyên GUID và liên kết trong scene, prefab, material.

| Thư mục | Nội dung |
| --- | --- |
| `Assets/Models/HaoLing/` | Hạo Linh, Feng, Huang; prefab, mesh, texture, material, scene trưng bày và mã dựng |
| `Assets/Models/SkyDefender/` | Sky Defender đỏ/trắng/vàng; robot, 12 cụm cánh, vũ khí, prefab chi tiết/combined/AR và scene trưng bày |
| `Assets/Models/BlueFlame/` | BlueFlame CH-01 trắng/xanh; robot, cánh, súng, khiên, hiệu ứng riêng và scene trưng bày |
| `Assets/Models/Unicorn Gundam/` | Model và prefab Unicorn gốc; vật liệu trong Materials, texture trong Textures |
| `Assets/Models/Unicorn Gundam/Demo/` | Prefab UnicornAnimated, mesh tối ưu, mã điều khiển và scene demo điện thoại |
| `Assets/Models/Kamiki Burning Gundam/` | FBX, prefab và vật liệu Kamiki |
| `Assets/Models/Shaders/` | Shader dùng chung cho model |

## Lấy model vào scene

- Hạo Linh để chỉnh từng chi tiết: `Assets/Models/HaoLing/Prefabs/HaoLing_Trio_Detailed.prefab`.
- Hạo Linh ít renderer hơn: `Assets/Models/HaoLing/Prefabs/HaoLing_Trio_Combined.prefab`.
- Hạo Linh tỉ lệ AR khoảng 25 cm: `Assets/Models/HaoLing/Prefabs/HaoLing_Trio_AR_25cm.prefab`.
- Robot và hai chim riêng: các prefab EMP02_HaoLing, MC02F_Feng, MC02H_Huang trong cùng thư mục Prefabs.
- Unicorn gốc: `Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab`.
- Unicorn có điều khiển chuyển động demo: `Assets/Models/Unicorn Gundam/Demo/Prefabs/UnicornAnimated.prefab`.
- Kamiki: `Assets/Models/Kamiki Burning Gundam/Kamiki Burning Gundam.prefab`.
- Sky Defender: `Assets/Models/SkyDefender/Prefabs/SkyDefender_Detailed.prefab`; bản ít renderer hơn là `SkyDefender_Combined.prefab` cùng thư mục.
- BlueFlame: `Assets/Models/BlueFlame/Prefabs/BlueFlame_Detailed.prefab`; bản gộp là `BlueFlame_Combined.prefab`, bản chuẩn hóa 25 cm là `BlueFlame_AR_25cm.prefab` cùng thư mục.

Kéo prefab cần dùng từ Project vào Hierarchy hoặc gán prefab vào trường model của bộ điều khiển. Không cần mở scene trưng bày để dùng model. Đây là cách tổ chức thư viện; không bổ sung cơ chế tải model động trong game.

## Scene để xem và thử

- Hạo Linh: `Assets/Models/HaoLing/Scenes/HaoLing_Phoenix_Atelier.unity`.
- Unicorn: `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity`.
- Sky Defender: `Assets/Models/SkyDefender/Scenes/SkyDefender_Atelier.unity`.
- BlueFlame: `Assets/Models/BlueFlame/Scenes/BlueFlame_Atelier.unity`.

Script dùng chung đã tách khỏi Models: `Assets/Game/Features/Characters/Scripts/Movement/` và `Assets/Game/Features/AR/Scripts/`. Script phục vụ riêng từng model được giữ cùng bộ tương ứng; mã chỉ chạy trong Editor nằm trong thư mục Editor.

## Quy ước cho model mới

Tạo một thư mục theo tên model dưới `Assets/Models/`. Gom prefab, mesh nguồn, Materials, Textures và nội dung riêng của model vào đó. Lấy prefab từ thư viện khi xây scene. Di chuyển asset trong cửa sổ Project của Unity để giữ các tham chiếu.

Các đường dẫn trong tài liệu bàn giao và mã dựng đã được cập nhật. Danh sách SupersededAssets.txt và file ZIP trong Archive/Characters/HaoLing/Authoring là hồ sơ lưu trữ lịch sử, vẫn giữ đường dẫn tại thời điểm sao lưu.
