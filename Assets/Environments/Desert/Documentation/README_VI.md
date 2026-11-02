# Desert Outpost · Red Canyon

Vân núi đã cập nhật sang texture sa thạch phong hóa dùng chung cho các núi và đá: bỏ sọc tuần hoàn trong shader/hình học, đồng nhất tỷ lệ vân theo mét và lọc độ gồ để tránh hạt đen. Nguồn texture và prompt: `SandstoneTexture_Source.md`; kiểm tra: `RockSurfaceVerification.md`.

Scene: `Assets/Environments/Desert/Scenes/Desert_Battle.unity`.
Catalog ID: `desert-outpost`. Mở từ Environment Hub bằng thẻ **DESERT OUTPOST · RED CANYON**.

## Đã dựng

- 12 công trình được đặt ID H01–H12, gồm nhà sa mạc, nhà hỏng, lều tôn, kho, trạm bơm và tháp canh.
- Import hai GLB từ Downloads, lưu mesh và texture trong thư mục Desert; chuẩn hóa kích thước theo geometry bounds. Source GLB được giữ tại `Sources/Desert` ngoài Assets.
- Material URP riêng cho Desert Buildings, Abandoned Buildings, Factory Lite, Flora-Form và Mountains; giữ nguyên material gốc của package.
- 16 xương rồng có thân gân và nhánh cong, 8 cây sa mạc, các vùng cỏ khô và bụi cây. Xương rồng/cành cây là mesh procedural; foliage dùng Flora-Form. Không tìm thấy Raft on the Desert trong project khi dựng.
- Các lớp canyon từ package Mountains, vòm đá có lỗ mở thật, đồi thấp và đá ven đường. Phần vòm, chân vòm, đồi và đá nhỏ được dựng procedural.
- Thùng phuy, thùng gỗ, motor, tủ thiết bị, bồn chứa, mái che, cột điện và dây chùng.
- Nền cát liên tục với sắc độ đất nén, bầu trời có mây mỏng chuyển động và 6 vùng bụi sát đất chạy trong Play Mode.
- Giữ hệ thống Gundam, HUD, chiến đấu và chuyển môi trường hiện có.

## Chỉnh sửa

Hierarchy: `Desert_Battlefield/Desert_LayoutOrigin`. Tìm theo H01–H12, C01–C16, T01–T08 và M01–M07 để chỉnh từng cụm.

Menu:

- **Tools → Desert Outpost → Capture Review**: chụp 7 camera review vào `Documentation/Review` và cập nhật thumbnail catalog.
- **Tools → Desert Outpost → Verify Battle and Scene Transitions**: chạy trong Play Mode tại scene Desert; cuối kiểm tra trở về Desert.
- **Tools → Desert Outpost → Build Detailed Battlefield**: tạo lại phần môi trường theo layout trong builder. Thao tác này thay root môi trường đã quản lý và có backup scene trong `Backups/Desert`; các sửa tay trên root cũ cần được đưa vào builder nếu muốn giữ khi tạo lại.

## Xác minh

Đợt cuối: **66 PASS, 0 FAIL** trong Unity Editor Play Mode. Kiểm tra mặt đất, vùng chiến đấu, shader, vertex hữu hạn, Attack/Skill/Earth Prison, camera và luồng Desert → Hub → City → Desert → Hub → Desert.

Kiểm tra cuối cũng ghi nhận 0 lỗi shader và Console không có lỗi. Chi tiết ở `Verification.md`. Chưa chạy standalone/Android hoặc benchmark FPS. Hình ảnh là bản dựng từ các asset hiện có và mesh bổ sung, không phải bản sao 100% ảnh tham khảo.

Preview: `Review/Desert_Gameplay.png`, `Desert_Overview.png`, `Desert_StreetReverse.png`, `Desert_H01_H02.png`, `Desert_H04_H10.png`, `Desert_Flora.png`, `Desert_Arch.png`.

## Cập nhật núi sa thạch
Vòm dạng vòng cũ đã được thay bằng khối sa thạch liền vách, silhouette lệch, chân mở rộng, khe xuyên thật và bề mặt xói mòn. Dùng Tools → Desert Outpost → Replace Artificial Arch With Canyon để cập nhật riêng phần này. Kết quả kiểm tra tập trung ở CanyonVerification.md; preview ở Review/Desert_Arch.png.


## Tăng mật độ khu dân cư và cây cỏ
Đã bổ sung 10 nhà (22 tổng), 24 cây (32 tổng), 12 xương rồng (28 tổng), 1.500 bụi cỏ bổ sung. Cỏ được gom thành patch mesh, không có collider; cỏ thấp trong sân chừa vùng spawn. Xem DensityExpansion.md và DensityVerification.md. Menu cập nhật riêng: Tools → Desert Outpost → Increase Houses Trees And Grass.

