# Modular SciFi MegaKit — môi trường đã tích hợp

Nguồn: thư mục người dùng cung cấp `D:/Downloads/Modular SciFi MegaKit[Standard]/Modular SciFi MegaKit[Standard]`. Bản Standard của Quaternius, kèm giấy phép CC0 gốc.

## Tài sản

- `Assets/ThirdParty/Quaternius/ModularSciFiMegaKit/Models`: 189 model FBX bản Unity, đủ các nhóm Aliens / Columns / Decals / Platforms / Props / Walls.
- `Textures`: 21 texture gốc; `Textures/URP`: bản chuyển ORM sang Metallic(R)/Smoothness(A) và Occlusion dùng kênh G.
- `Assets/TurnBasedBattle/Environment/Materials`: 25 material URP, đã remap vào FBX. Model kéo vào scene dùng được ngay; không cần shader Built-in.
- `MegaKitHangar.prefab`: môi trường lắp từ các module, đã gắn dưới `BattleContent/MegaKitHangar` trong TurnBattleDemo.

## Bố trí

Sàn 44×40 đơn vị, khoang cao khoảng 8 đơn vị, không đặt trần kín để camera trên cao có thể orbit. Vách/cột/console/crate ở biên; vùng đội hình, đường chạy, vòng Earth Prison và khoảng bay R để trống. Mặt sàn tại y≈0, giữ nguyên anchor nhân vật.

Các mesh sàn/lan can prototype cũ được tắt renderer, vẫn giữ dữ liệu để phục hồi. Vòng đánh dấu đội hình vẫn hiển thị. Collider đơn giản trên sàn và vách phục vụ tránh xuyên camera; không gắn hàng trăm MeshCollider lên đồ trang trí. Tất cả nội dung nằm dưới BattleContent để đi theo bàn đấu khi đổi vị trí/quay/scale.

Có reflection probe realtime 128px cập nhật lúc mở scene. Chưa kiểm chứng hiệu năng Android; có thể giảm probe/texture/decor khi tối ưu thiết bị. Các model ngoài scene vẫn được nhập để dùng tiếp.

## Chỉnh và dựng lại

- Menu **Turn Battle > Environment > Apply MegaKit Hangar** tạo lại riêng môi trường, không thay config/animation/nhân vật. Thao tác này thay đối tượng MegaKitHangar hiện có; lưu prefab riêng nếu đã sửa thủ công.
- Menu **Prepare MegaKit URP Materials** dựng lại material và remap từ MaterialImport.json.
- **Build Turn Battle Demo** tự gắn lại MegaKit khi material đã có; môi trường không mất sau khi tái tạo scene.
- `MegaKitEnvironmentBuilder.cs` chứa layout, ánh sáng môi trường, import và hook dựng scene.

Giữ `License_Standard.txt` cùng asset. Các định dạng OBJ/glTF/FBX thường không được chép lặp vào Assets; project đã dùng bộ FBX (Unity) đầy đủ và texture gốc. File gốc trong Downloads không bị sửa.
