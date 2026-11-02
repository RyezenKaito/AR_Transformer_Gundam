# ORBITAL FRONTIER · EARTHWARD

Chiến trường Gundam ngoài khoảng không, tích hợp vào hệ thống đổi môi trường hiện có.

## Mở và chơi

- Chạy EnvironmentHub rồi chọn **ORBITAL FRONTIER · EARTHWARD**.
- Hoặc mở `Assets/Environments/OrbitalFrontier/Scenes/OrbitalFrontier_Battle.unity` và bấm Play.
- Chuột phải + kéo: xoay. Cuộn chuột: zoom. Q/E: xoay ngang. Mũi tên lên/xuống: đổi góc cao. Home: về góc mặc định.
- Trên màn hình cảm ứng: kéo vùng trống để xoay, chụm hai ngón để zoom. Các thao tác qua code/controller đã được kiểm tra trong Editor; chưa chạy trên thiết bị Android.
- Góc mặc định nhìn từ cao xuống 18° theo ảnh tham khảo; Home đặt lại góc này. Camera yaw 360°, pitch ±89°, khoảng cách 2,5–140 m. Có thể nhìn từ dưới đội hình lên và nhìn từ trên xuống.

## Những gì đã dựng

- Trái Đất từ NASA Blue Marble, lớp mây procedural chuyển động riêng, viền khí quyển mỏng; Mặt Trăng procedural, Sao Mộc có dải mây và xoáy, Sao Thổ có vành, cùng hành tinh giả tưởng Tinniuq ở các hướng khác.
- 7 tàu: tàu lớn GLB, 3 StarSparrow, 2 SF fighter, 1 R35 dropship.
- Đã bỏ trạm hình trụ và vệ tinh thô. Ba cụm xác tàu W01–W03 ghép từ thân/frame/cockpit Omega và core/cánh/động cơ StarSparrow, kèm 36 bộ phận tàu rời có texture từ asset gốc.
- 88 asteroid sử dụng 10 mesh biến thể của GLB; thêm 6 thiên thạch lướt liên tục, có vệt sáng vàng cam đỏ thuôn dài, chạy theo đường cố định ngoài vùng đánh nhau. Tàu lớn ở (132,10,0) tương đối LayoutOrigin, chiều dài chuẩn hóa 56 m, hiện trong góc gameplay mặc định.
- Tư thế không trọng lực và thruster duy trì vị trí. Chuyển tiếp cận từ chạy sang bay trong scene này. Anchor, sát thương và luồng lượt đánh dùng hệ thống hiện có.
- Không có sàn dưới robot. Đây là trận đấu theo lượt trong không gian; chưa có điều khiển bay tự do sáu bậc tự do.

## Tùy chỉnh

Scene root `OrbitalFrontier_Battlefield/LayoutOrigin` chia thành CelestialBodies, Station (nay chứa xác tàu), Ships, AsteroidField, Salvage và Flybys. Mỗi vật thể chuyển động có `OrbitalMotion`: chỉnh tốc độ xoay, driftAmplitude và driftAxis trong Inspector. Không chỉnh root battle actor để tạo drift vì sẽ lệch anchor gameplay.

Earth nằm tại tọa độ local (2200,-400,2100), bán kính 650 m theo bố cục nghệ thuật; mây 652,5 m và khí quyển 660 m. Tỷ lệ và khoảng cách các thiên thể được điều chỉnh cho hình ảnh, không phải mô hình thiên văn. Earth xoay 0,8°/giây, cloud 1,03°/giây, được tăng tốc để quan sát được.

Chế độ space bật qua `BattleActor.zeroGravity` và `BattleOrbitCamera.freeOrbit`; far clip riêng 10.000 m. Các field mặc định tắt ở map cũ. Space sử dụng URP renderer 0; SunkenHarbor giữ renderer nước riêng 1.

`Tools > Orbital Frontier > Build Battlefield` dựng lại scene từ template và asset hiện có; thao tác này thay thế bố cục scene space đã chỉnh tay, có sao lưu scene trước khi dựng. `Capture Review` chụp tám góc. `Verify Battle And Navigation` chạy trong Play Mode và ghi `Verification.md`.

Prompt tổng: `Prompts/Orbital_Frontier_SPACE_MASTER_VI.md`. Chi tiết nguồn asset: `AssetManifest.md` và `V2_Changes.md`. Prompt cập nhật: `Prompts/Orbital_Frontier_V2_Wrecks_Flybys_VI.md`. Ảnh: folder `Review`.

## Kiểm chứng và giới hạn

Đã chạy 107 kiểm tra trong Unity Editor Play Mode: tính toàn vẹn primitive/tam giác GLB, shader, thể tích chiến đấu trống, 6 flyby lặp lại với vệt sáng không nối khi reset, tàu lớn trong góc mặc định, Jupiter/Saturn, chuyển động Earth/mây, camera một vòng/trên/dưới/zoom xa, Attack/Skill/EarthPrison, sát thương, quay về anchor, lượt đáp trả, và Hub → SunkenHarbor → OrbitalFrontier → DesertOutpost → OrbitalFrontier. Các scene giữ một camera, một AudioListener và không trùng EventSystem.

Đã xem và điều chỉnh ảnh render để robot rõ hơn, Earth không chiếm toàn màn hình và có thể nhận dạng lục địa. Chưa build standalone/Android hoặc benchmark FPS; chưa kiểm tra kính AR/XR. Tư thế bay dùng animation hiện có cộng lớp lower-body hover, không phải bộ mocap không trọng lực mới.

## Nguồn hình ảnh Trái Đất

NASA Earth Observatory — Blue Marble: Next Generation, Base Map with Topography, January 2004 global composite.

https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/base-topography/

https://assets.science.nasa.gov/content/dam/science/esd/eo/images/bmng/bmng-topography/january/world.topo.200401.3x5400x2700.jpg

Mây/khí quyển, bề mặt Moon, các dải mây Sao Mộc/Sao Thổ và vành Sao Thổ được mô phỏng bằng shader. Tinniuq và các tàu nhỏ lấy từ package cục bộ. Không suy đoán tác giả/giấy phép GLB từ tên file; giữ file gốc tại `Sources/OrbitalFrontier` và thông tin package đi kèm project.





Góc nền thiên hà được xoay 220° để thấy một phần dải sáng bên phải ngay trong góc mặc định; camera cao 18° và vị trí hành tinh/tàu giữ nguyên.
