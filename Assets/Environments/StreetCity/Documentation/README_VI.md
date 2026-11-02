# City Street — Gundam giữa đường

Scene: `Assets/Environments/StreetCity/Scenes/StreetCity_Battle.unity`.

Vào EnvironmentHub → **CITY STREET · GUNDAM** (thẻ đầu danh sách), hoặc mở scene trực tiếp rồi nhấn Play.

## Nội dung

Đã nhập file `D:/Downloads/street_city_buildings_8.glb`: 73 phần mesh, 127.321 vertex, 77.066 tam giác, 9 material và 8 ảnh texture. Bản gốc được sao chép tại `Sources/StreetCity/street_city_buildings_8.glb`; các mesh/material Unity nằm trong thư mục StreetCity này.

- `Prefabs/StreetCity_Original.prefab`: toàn bộ mô hình ở bố cục nguồn, có đủ 73 mesh. Geometry và transform glTF được chuyển sang hệ trục Unity, UV và chiều tam giác tương ứng.
- `Prefabs/StreetCity_Battlefield.prefab`: các khối nhà nguồn đã được đặt dọc hai bên đại lộ. Mặt nền phẳng của GLB được tắt trong bố cục chiến trường và thay bằng đường nhựa.
- Đường chính rộng 18 m, có collider liên tục, tim đường, vạch mép, vạch qua đường, vỉa hè và đèn đường. Khối nhà giữ mesh/texture nguồn, phóng tỷ lệ 4 lần để phù hợp không gian trận đấu.
- Gundam đứng đúng tại **world (0, 0, 0)** trên tim đường. Đối thủ đứng phía trước dọc đường. BattleContent được dịch vị trí; dữ liệu action/tuning và các scene battle cũ được giữ nguyên.
- Có robot, HUD, camera orbit và hệ đánh theo lượt 1v1/1v3. Đây là chiến trường của game hiện có, không phải một scene chỉ có camera xem mô hình.
- Mặt nạ metallic/roughness glTF được chuyển sang metallic/smoothness cho URP; ảnh mặt tiền được giữ nguyên. Đèn, nền đường và vỉa hè là các phần bổ sung cho map.

## Điều khiển

1: Attack · 2: Skill · 3: Guard · R: Ultimate · Enter: xác nhận · Tab: đổi mục tiêu · Space: né đúng thời điểm.

F2: nạp Ultimate · F5: 1v1 · F6: 1v3 · P: tạm dừng.

Giữ chuột phải để xoay ngang đủ 360° và ngẩng lên bầu trời/hạ xuống. Cuộn để zoom từ 2,2 đến 80 m (vật cản có thể đẩy camera lại gần). Q/E xoay ngang, phím mũi tên lên/xuống để ngẩng/hạ camera, Home về góc mặc định. Góc dọc từ -85° đến 89°; camera giữ phía trên mặt đất khi ngẩng lên. Nút **CHỌN MÔI TRƯỜNG** trở lại hub; chuyển scene sẽ khởi tạo lại trận đấu.

## Chỉnh và dựng lại

- **Tools → Street City → Open Battlefield**: mở scene.
- **Tools → Street City → Capture Review**: chụp hai góc tổng thể/đường phố vào `Documentation/Review/`.
- **Tools → Street City → Build Gundam Battlefield**: dựng lại từ scene battle đã lưu và dữ liệu GLB; scene trước đó được lưu ở `Backups/StreetCity/`. Generated mesh/material được cập nhật tại chỗ, do đó muốn giữ nguyên một phiên bản thì sao lưu cả thư mục hoặc dùng version control.
- Script chuyển GLB thành dữ liệu nhập: `Tools/import_street_city_glb.py`, chỉ dùng Python/numpy có sẵn. `ImportData/manifest.json` mô tả mesh/group/material; dữ liệu nguồn là static mesh, không phải importer glTF tổng quát cho rig/animation.
- Registry: `StreetCityDefinition.asset`, ID `street-city`, trong EnvironmentCatalog chung.

## Kiểm tra

Trong Play Mode của map: **Tools → Street City Arena → Verify Battle in Play Mode**. Báo cáo: `Documentation/Verification.md`.

Các kiểm tra gồm vị trí Gundam, bảo toàn số mesh/vertex nguồn, vật liệu URP, collider đường, khoảng trống chiến đấu, Attack/Skill/Ultimate 1v3, sát thương/đường về/cất vũ khí và camera orbit. Luồng chọn tất cả map được kiểm tra qua Environment System, báo cáo ở `Assets/EnvironmentSystem/Documentation/Verification.md`.

Chưa có chế độ đi bộ tự do, traffic simulation hoặc phá hủy tòa nhà. Chưa benchmark hiệu năng, build standalone hay kiểm tra Android.

## Kết quả ngày 31/10/2026

- 45 kiểm tra chiến đấu đạt: Gundam ở tâm đường, số mesh/vertex nguồn đúng, vật liệu hợp lệ, collider đường, khoảng trống, Attack/Skill/Ultimate 1v3, sát thương, cất vũ khí, trở về anchor, đối thủ phản công và camera orbit.
- 118 kiểm tra luồng môi trường đạt với 8 lựa chọn available, bao gồm map mới và các map trước đó. Báo cáo nằm ở `Assets/EnvironmentSystem/Documentation/Verification.md`.
- Console sau lượt kiểm tra không có error. Các kiểm tra được chạy trong Unity Editor Play Mode.
- Ảnh tổng thể/đường: `Documentation/Review/StreetCity_Overview.png`, `StreetCity_Road.png`. Màn chơi có HUD: `StreetCity_GameView.png`.
