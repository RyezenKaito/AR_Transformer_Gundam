# Sunken Harbor · Thalassa

Scene: `Assets/Environments/SunkenHarbor/Scenes/SunkenHarbor_Battle.unity`.
Catalog ID: `sunken-harbor`; mở từ Environment Hub bằng thẻ **SUNKEN HARBOR · THALASSA**.

## Nội dung đã dựng

- Sân Gundam trên đáy biển tại Y≈0, mặt biển ở Y=42. Nền cát liên tục, bê tông cũ và vành đá photoscan.
- Nhà điều hành có cửa sổ mở, nhà kho, trạm phụ trợ, công trình sập nghiêng, 12 container, cầu cảng gãy, motor/tủ điện/ống và các cụm bồn chứa từ asset có sẵn.
- Cần cẩu GLB gồm 9 mesh, 255.362 tam giác nguồn; bản duplicate download không import lần hai. Source được giữ ở `Sources/SunkenHarbor`.
- Năm đảo là mesh lấy mẫu từ heightmap của Free Island Collection, với địa hình chìm/nổi liên tục. TerrainData nguồn không bị chỉnh sửa.
- Rong/cỏ biển dạng ribbon có sway trong shader; 8 cụm sea fan (24 fan nhỏ), 3 đàn cá với 30 cá, bong bóng và hạt lơ lửng. Đây là mesh/VFX bổ sung procedural, không phải package sinh vật biển chuyên dụng.
- Phản ứng trầm tích khi robot di chuyển sát đáy/tiếp đất và ambience dưới nước do tổng hợp âm thanh tại chỗ.
- Water optics theo quãng đường nhìn trong nước, hấp thụ RGB khác nhau; mặt biển có hai mặt, gợn chuyển động, khúc xạ screen-space và foam tiếp giáp.
- Renderer riêng cho camera của map; default renderer của các map cũ giữ nguyên. Render Graph toàn project không bị tắt.

## Điều chỉnh

`Settings/HarborConfig.asset`: waterLevel, absorption, scatter, visibility, particleDensity, ambienceVolume, lowQuality.

Component `HarborAtmosphere` trên root có menu **Apply water settings**. Trong Play Mode, các tham số quang học chính được cập nhật; một số thay đổi chất lượng cần gọi Apply. Các material được điều khiển là asset riêng của map, không phải material nguồn của package.

Menu **Tools → Sunken Harbor**:

- **Build Battlefield**: dựng lại phần map từ template, backup managed assets khi scene đã tồn tại. Những sửa tay cần được đưa vào builder nếu muốn giữ khi dựng lại.
- **Polish Lighting Water And Marine Life**: cập nhật riêng đảo, ánh sáng, sea fan/trầm tích và ambience.
- **Capture Review**: chụp các camera review và cập nhật thumbnail trong catalog.
- **Verify Battle Optics And Transitions**: chạy trong Play Mode tại Sunken Harbor; kết thúc ở Sunken Harbor.

## Kiểm tra thực tế

Đợt cuối **80 PASS, 0 FAIL** trong Unity Editor. Bao gồm collider sân, mesh hữu hạn, shader, toàn bộ phần crane, Attack/Skill/Earth Prison, đòn phản hồi của enemy, camera orbit/zoom, so sánh ảnh GPU có/không hấp thụ nước, bypass hiệu ứng khi camera nhìn trời phía trên mặt nước, particle sống và chuyển Hub → Desert → Sunken Harbor → Hub → Sunken Harbor.

Renderer của Desert vẫn dùng thiết lập cũ trong lượt chuyển scene. Chưa chạy standalone/Android hoặc benchmark FPS. Xem `Verification.md`.

Ảnh cuối: `Review/SunkenHarbor_Gameplay.png`, `SunkenHarbor_OverviewUnderwater.png`, `SunkenHarbor_SurfaceLookUp.png`, `SunkenHarbor_Crane.png`, `SunkenHarbor_Ruins.png`, `SunkenHarbor_SeabedDetail.png`, `SunkenHarbor_SurfaceIslands.png`.

## Phạm vi kỹ thuật và giới hạn

- Phản chiếu mặt biển dùng fallback trời mô phỏng, kết hợp screen-space refraction; chưa phải SSR/planar reflection đầy đủ của toàn quần đảo.
- Light shafts dùng geometry alpha nhẹ, không phải volumetric ray marching vật lý.
- Caustics là mô phỏng procedural, không phải kết quả mô phỏng truyền sáng từ mặt sóng.
- Rong/cá/sea fan được tạo để phục vụ bối cảnh game; không khẳng định mô phỏng sinh học/thuỷ động lực học chính xác.
- WaterWorks cũ phát sinh lỗi compile trong Unity 6. Chỉ file `Water_Volume.cs` được thay bằng adapter tương thích FullScreenPassRendererFeature sau khi backup. Chi tiết ở `WaterCompatibilityNotes.md`. Art assets nguồn của các package được giữ nguyên.

Unity được đưa về **Edit Mode** sau kiểm tra vì có thao tác import package bị Unity chặn trong Play Mode. Nhấn Play để xem chuyển động nước, rong, cá và bong bóng.
