# Warehouse — Sector 09

Chiến trường mới dựng từ **SciFi Warehouse Kit** có sẵn trong project, tham chiếu video `YTSave.to_YouTube_Media_pjjbWdSblE8_Sci-Fi-Warehouse-Construction-Kit-1-0-1-for-Unity_001_1080p.mp4` dài 28,4 giây. Đã khảo sát 9 frame lấy mẫu, xem bản contact sheet tại `ReviewMedia/Warehouse/contact_sheet.jpg`.

## Mở và chơi

- EnvironmentHub → Play → **WAREHOUSE · SECTOR 09**.
- Hoặc mở `Assets/Environments/Warehouse/Scenes/Warehouse_Battle.unity` rồi nhấn Play.
- Scene đã có robot, đối thủ, HUD, camera orbit và hệ đánh theo lượt từ scene battle hiện có.
- 1: Attack; 2: Skill; 3: Guard; R: Ultimate; Enter: xác nhận; Tab: đổi mục tiêu; Space: né đúng thời điểm.
- F2: nạp Ultimate; F5: trận 1v1; F6: trận 1v3; P: tạm dừng.
- Giữ chuột phải để xoay camera, cuộn để zoom; Q/E xoay ngang; Home về góc ban đầu.
- Nút **CHỌN MÔI TRƯỜNG** quay về hub. Việc đổi scene khởi tạo lại trận đấu.

## Bố cục

Kho 48 × 42 m, cao khoảng 18 m để chừa khoảng trống cho robot bay và Ultimate. Sàn chiến đấu nằm ngay giữa, trong khung vạch vàng khoảng 25,6 × 20 m. Camera và lane chiến đấu giữ được không gian trống; kệ, thùng, xe đẩy, bình chữa cháy và các chi tiết dịch vụ nằm quanh chu vi.

Tường panel xám, cửa kho lớn, cửa sổ cao, trần/skylight, cột, ống gió, cầu đi bộ hai bên và phía sau cùng cầu thang/lan can vàng dùng prefab thực của package. Ánh sáng gồm đèn treo, fill tường, skylight và reflection probe. Bố cục được thiết kế mới dựa trên ngôn ngữ hình ảnh trong video, không phải copy nguyên demo.

## Asset và chỉnh sửa

- Scene: `Scenes/Warehouse_Battle.unity`.
- Prefab môi trường: `Prefabs/Warehouse_Sector09.prefab` (chỉ kiến trúc, props và lighting; battle rig nằm trong scene).
- Material URP: `Materials/`. Các texture màu/normal/metallic/AO/emission và alpha được lấy từ material package. Material/prefab gốc trong `Assets/SciFi Warehouse Kit` được giữ nguyên.
- Scene battle gốc: `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity`, được dùng làm mẫu để giữ đúng các tham chiếu rig/HUD/effect. Các action/tuning dùng chung không bị thay đổi.
- Definition: `WarehouseDefinition.asset`, đăng ký trong EnvironmentCatalog dùng chung; không cần sửa selector.
- 3 camera review tắt mặc định: Warehouse_Overview, Warehouse_Catwalk, Warehouse_Floor. Camera chiến đấu là camera hoạt động duy nhất khi Play.

Menu **Tools → Warehouse Arena → Open Battle Scene** mở scene. **Capture Review** xuất ảnh vào `Documentation/Review/`. **Build Battle Scene** dựng lại từ scene battle đã lưu và prefab kit; scene warehouse cũ được sao lưu dưới `Backups/Warehouse/`. Builder sẽ thay thế scene generated, vì vậy lưu những chỉnh sửa thủ công cần giữ trong một scene sao chép hoặc version control.

## Kiểm tra

Trong Warehouse Play Mode, chạy **Tools → Warehouse Arena → Verify Battle in Play Mode**. Báo cáo thực tế ghi ở `Documentation/Verification.md`: vật liệu URP, collider sàn, khoảng trống chiến đấu, HUD, Attack/Skill/Ultimate 1v3, sát thương, robot trở về vị trí, trần và camera orbit.

Luồng đổi tất cả map được kiểm tra bằng **Tools → Environment System → Verify Switching in Play Mode**, báo cáo ở `Assets/EnvironmentSystem/Documentation/Verification.md`.

Chưa benchmark FPS, build standalone hay kiểm tra Android. Material Standard cũ đã được dựng lại cho URP; hình ảnh có thể khác pipeline/lighting trong video gốc.

## Kết quả kiểm tra 31/10/2026

- **42 kiểm tra Warehouse Battle đạt**: scene có rig/HUD/camera đúng, collider sàn, khoảng giữa 24 × 20 × 9,6 m không có vật cản, Attack/Skill/Ultimate gây sát thương đúng, trở về anchor/cất vũ khí, phản công của đối thủ và 8 bước camera orbit.
- Chiều cao đỉnh thân robot đo trong lượt chạy: Attack 3,82 m, Skill 2,90 m, Ultimate 5,47 m, đều dưới trần.
- **106 kiểm tra luồng môi trường đạt**: tải 7 lựa chọn available, quay về hub, ID/scene thiếu và trạng thái camera/audio/navigator.
- Console sau lượt kiểm tra trả về 0 error. Chưa kiểm tra hiệu năng hoặc bản build thiết bị.

Preview environment: `Documentation/Review/Warehouse_Overview.png`, `Warehouse_Catwalk.png`, `Warehouse_Floor.png`. Ảnh Game View với HUD được lưu riêng ở `Warehouse_GameView.png`.
