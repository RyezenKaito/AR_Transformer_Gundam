# Volcanic Caldera · Emberfall

Scene: `Assets/Environments/Volcano/Scenes/Volcano_Battle.unity`.

Vào EnvironmentHub → **VOLCANIC CALDERA · EMBERFALL**, hoặc **Tools → Volcano → Open Battlefield** rồi Play.

## Nội dung đã dựng

- Sàn basalt có mặt trên liền, vách dày khoảng 9 m và footprint khoảng 54 × 40 m (thu nhỏ từ 64 × 48 m); mép bất quy tắc và đá vỡ. Gundam ở world (0,0,0), enemy phía trước; BattleContent dịch đồng bộ 5,4 m theo X, không sửa tuning hay animation.
- Hồ lava dưới mặt sàn 1,4 m, sát mép đá hơn; có các nhánh inlet, dòng từ núi chính, miệng lava, hai thác lava và hai khe nóng trang trí sát rìa sàn. Khe trang trí bám theo kích thước sàn và không tạo hố thật hoặc sát thương.
- Núi chính có 11 dòng lava quanh sườn. Khoảng 80% núi/đá bờ và núi hậu cảnh có dòng lava từ đỉnh xuống, với một số nhánh phụ. Các đỉnh và dòng chảy được chiếu ray vào collider của chính mesh đá để bám bề mặt.
- Lava có nếp cuộn và vệt nóng chuyển động rõ hơn: tốc độ cơ sở 0,22 trong shader, các dòng trên sườn/thác chạy nhanh gấp 2,2 lần hồ. Vỏ nguội và các khe nóng đi theo UV của dòng.
- Có 50 emitter khói quanh mặt lava, sát mép sàn và chân thác (tăng từ 34). Lưu lượng mỗi emitter tăng từ 1,95 lên 5,4 hạt/giây; kích thước cơ sở từ 3,8 lên 5,2 m và thời gian sống từ 7 lên 9 giây. Các làn khói lớn dần khi bốc lên, lệch nhẹ theo gió rồi tan; shader trong suốt có soft-depth fade và fade gần camera. Nguồn khói nằm ngoài vùng chiến đấu trung tâm. Mật độ/độ mờ có thể chỉnh riêng với cột khói núi chính.
- Có thêm 21 cột khói từ đỉnh núi/đá gần và núi hậu cảnh; núi lửa chính có cột khói than tối, kích thước lớn hơn và bốc cao hơn. Có 10 cụm mây tro trôi ở cao độ khoảng 78–114 m, kết hợp skybox nhiều lớp mây tro chuyển động để có bầu trời đầy đủ khi quay 360° hoặc nhìn thẳng lên.
- Shader basalt có texture mới, mapping theo trục thế giới, relief và macro variation; lava có hướng UV theo dòng, hai pha advection, crust texture, khe nóng bị warp và emission có thể chỉnh.
- Núi chính có crater và các sườn rãnh, các dãy đá bao quanh, sky tro, khói phun, ejecta theo đợt, tro và embers thưa, vent smoke, hai vùng heat haze cục bộ dùng camera opaque texture.
- Ánh sáng directional trung tính/lạnh, bốn point light cam không shadow, reflection probe và Volume riêng. Các vật liệu/shader dùng URP; không đổi pipeline của dự án.
- Ambience rumble tự tổng hợp, loop 12 giây. Lava, khói và ambience tiếp tục khi game tạm dừng bằng P; P giữ hành vi pause combat hiện tại, không thay Time.timeScale.
- Robot, HUD và combat 1v1/1v3 dùng hệ TurnBasedBattle hiện có. Map đã đăng ký vào catalog và build scene list với ID `volcanic-caldera`.

## Chơi và camera

1 Attack · 2 Skill · 3 Guard · R EarthPrison · Enter xác nhận · Tab đổi mục tiêu · Space né đúng nhịp.

F2 nạp Ultimate · F5 1v1 · F6 1v3 · P pause.

Giữ chuột phải để xoay ngang 360° và ngẩng/hạ. Lăn chuột zoom 2,2–80 m. Q/E xoay ngang, ↑/↓ ngẩng/hạ, Home về góc mặc định. Pitch -85° đến 89°. Vật cản có thể rút boom về gần. Nút **CHỌN MÔI TRƯỜNG** trở về hub.

## Chỉnh sửa và dựng lại

- `Settings/VolcanoConfig.asset`: seed, kích thước sàn, cao độ lava, vị trí núi, số dòng lava núi chính (`mainLavaStreams`), tỷ lệ núi xung quanh có lava (`surroundingLavaCoverage`), emission, crust coverage, tốc độ flow, mật độ khói/tro, volume và chu kỳ ejecta. Các trường hình học được dùng khi dựng lại.
- Sửa tham số khí quyển trong config rồi khởi động Play mới, hoặc dùng context menu **Apply runtime atmosphere settings** trên component VolcanoAtmosphere. Tham số được đặt bằng MaterialPropertyBlock, không ghi đè material dùng chung khi chuyển scene.
- `flowSpeed`, `flowWaves`: tốc độ và nếp cuộn dòng chảy. `lavaVapourDensity`, `lavaVapourOpacity`: mật độ và độ rõ của khói mỏng trên lava. **Tools → Volcano → Update Flow And Mist** cập nhật riêng các hiệu ứng trong scene Volcano đang mở, không dựng lại địa hình. Nhóm khói do công cụ quản lý được thay tại chỗ; thư mục được sao lưu trước khi cập nhật.
- `mountainSmokeDensity`, `skySmokeDensity`: mật độ cột khói núi và mây tro tầng cao. `skyAshCoverage` điều khiển độ phủ mây của skybox, được lưu vào material qua **Tools → Volcano → Update Mountain And Sky Smoke**. Menu này cập nhật riêng khói trên núi và bầu trời, giữ địa hình và khói mặt lava; có sao lưu trước khi thay nhóm hiệu ứng được quản lý.
- Low Quality giảm rate khói/tro và số ejecta mỗi đợt; đây là cấu hình giảm hiệu ứng cơ bản, chưa phải một preset đã benchmark trên thiết bị.
- **Tools → Volcano → Build Battlefield** dựng lại từ scene battle gốc đã lưu, cùng seed/config và các texture hiện có. Khi scene Volcano đã có, toàn bộ thư mục được sao lưu vào `Backups/Volcano/<timestamp>/` trước khi ghi đè. Giữ bản sao lưu nếu có chỉnh tay cần khôi phục.
- Generator thay thế scene Volcano và asset được quản lý trong thư mục này; chỉnh sửa tay trong scene cần được lưu bản sao trước khi dựng lại. Root/prefab ngoài scene này không bị thay đổi.
- **Capture Review** chụp ba review camera tắt sẵn và cập nhật thumbnail. Ảnh GameView/1v3/Overhead/SkyView được chụp thêm từ Play Mode.
- Prefab chính: `Prefabs/Volcano_Battlefield.prefab`. Có các module `Volcano_Main`, `Lava_Flow`, `Lava_Fall` và `Volcanic_RockCluster`. Các module giữ tọa độ bố trí của map sinh ra; đặt root ở origin để khớp bố cục nguồn.

## Kiểm chứng

**Tools → Volcano Arena → Verify Battle in Play Mode** chạy kiểm tra trong scene này. Báo cáo `Documentation/Verification.md`.

**Tools → Environment System → Verify Switching in Play Mode** kiểm tra catalog, async scene loading, quay về hub và camera/audio/nav singleton cho toàn bộ môi trường. Báo cáo chung: `Assets/EnvironmentSystem/Documentation/Verification.md`.

Ảnh trong `Documentation/Review/` là capture từ Unity, không phải ảnh concept. Nguồn texture và âm thanh trong `AssetSources.md`.

## Kết quả kiểm tra ngày 31/10/2026

- 62 kiểm tra scene/chiến đấu/camera đạt, 0 FAIL sau lần thu nhỏ sàn và tăng lava: vật liệu URP, vertices hợp lệ, nền liên tục, vùng đánh trống, Attack/Skill/Ultimate 1v3, đường đánh luôn có sàn bên dưới, sát thương, đường về, cất vũ khí, HUD, phản công, Guard đúng hệ số, orbit, sky/overhead 80 m và lava trên phần lớn núi xung quanh.
- 130 kiểm tra Environment System đạt với 9 lựa chọn available trong catalog, bao gồm map mới.
- 24 kiểm tra riêng đạt cho chu trình Volcano → City Street → Warehouse → Volcano: atmosphere, ambience, sky và Volume được nạp/dọn đúng theo scene. Báo cáo `TransitionVerification.md`.
- Kiểm tra compiler của bốn shader Volcano: 0 error. Console sau lượt kiểm tra cuối không có error.
- Các kết quả trên được chạy trong Unity Editor Play Mode, không phải benchmark hoặc build trên thiết bị.
- Kiểm tra riêng dòng chảy/khói: `FlowMistVerification.md`; lần tăng khói mới nhất: `DenseSmokeVerification.md`. Clip `Documentation/Review/Volcano_FlowMist.mp4` ghi giai đoạn khói mỏng trước lần tăng mật độ. Các ảnh `Volcano_FlowMist.png`, `Volcano_Overview.png` và `Volcano_GameView.png` đã được cập nhật với khói dày hơn.
- Khói núi/bầu trời: `MountainSkySmokeVerification.md`. Ảnh mới: `Documentation/Review/Volcano_MountainSky.png` và `Volcano_SkyCanopy.png`. Camera review đều tắt sẵn, không có AudioListener bổ sung.

### Điều chỉnh bố cục theo yêu cầu

Sàn 54 × 40 m nhỏ hơn khoảng 16% mỗi chiều so với bản 64 × 48 m. Mặt lava nâng từ -5,5 lên -1,4 m. Bản dựng hiện có 154 mặt lava, gồm 11 dòng chính và 90 cascade trên các đỉnh/đá xung quanh, cùng các nhánh phụ, hồ và thác. Collider nền và khoảng trống giữa sàn được giữ cho 1v3. Hình học và prefab đã được lưu; ảnh review được cập nhật cho bố cục mới.

## Phạm vi chất lượng

Địa hình, lava shader, VFX và ambience được tự dựng trong dự án, có tham chiếu video L.V.E. Bộ asset L.V.E./shader HDRP gốc không được nhập hoặc sử dụng. Đây là phiên bản procedural có thể chơi và chỉnh sửa; chưa phải bản sao đạt chất lượng hình ảnh tương đương video nguồn.

Không có mô phỏng fluid vật lý, phá hủy nền/tòa nhà hoặc sát thương do lava. Chưa benchmark FPS, build standalone hoặc thử Android. Texture basalt là ảnh AI, relief suy ra từ độ sáng albedo, không phải bộ PBR scan đã đo/calibrate.
