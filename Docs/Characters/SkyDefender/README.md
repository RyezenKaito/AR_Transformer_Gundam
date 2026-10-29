# Sky Defender — Crimson Sovereign

Model thứ hai được dựng trong Unity 6000.5.8f1 / URP theo ảnh `SourceArt/References/SkyDefender/overview.png` người dùng cung cấp. Toàn bộ tài nguyên dùng trong Unity nằm trong `Assets/Models/SkyDefender/`.

## Xem model

Mở scene `Assets/Models/SkyDefender/Scenes/SkyDefender_Atelier.unity`. Nhấn Play, kéo chuột để xoay, cuộn để thu/phóng. Phím 1 hoặc R: toàn cảnh; 2: mặt trước; 3: mặt sau; 4: đầu; W: bật/tắt bộ cánh. Hỗ trợ kéo bằng một ngón tay qua Input System; thao tác trên thiết bị thật chưa được kiểm thử.

## Prefab

| Prefab trong Assets/Models/SkyDefender/Prefabs | Nội dung |
| --- | --- |
| SkyDefender_Detailed.prefab | Toàn bộ robot, 12 cụm cánh và hai vũ khí; 1.037 mesh riêng để chỉnh sửa |
| SkyDefender_Body.prefab | Thân robot và vũ khí, 417 mesh |
| SkyDefender_Wings.prefab | Bộ cánh và backpack, 620 mesh |
| SkyDefender_Combined.prefab | Toàn bộ model ghép theo 10 vật liệu, 10 mesh renderer |
| SkyDefender_AR_25cm.prefab | Bản combined thu nhỏ để chiều cao robot tính cả mào khoảng 25 cm |

Tổng 61.490 tam giác cho toàn bộ model. Bản combined giữ nguyên số tam giác, giảm số renderer; đây không phải LOD và không phải phép đo draw call trên thiết bị. Pivot của bản thân/toàn bộ nằm ở mặt đặt chân; prefab cánh đặt gốc ở tâm cụm đeo lưng để gắn lại.

## Thiết kế

- Dáng cao, đầu nhỏ, chân dài, giáp vát nhọn và eo hẹp; cổ được rút ngắn sau khi kiểm tra render.
- Đầu có mặt nạ trắng, mắt xanh, V-fin vàng, mào đỏ/trắng, cảm biến và nắp giáp sau.
- Ngực, bụng, giáp váy, vai, tay, ngón tay, đùi, đầu gối, ống chân, cổ chân và bàn chân là các nhóm riêng.
- Khớp có vòng ổ trục, nắp kim loại, piston và bu lông; các cụm cánh có thanh đỡ, giáp đỏ và đầu lưỡi trắng ở cả hai mặt.
- Hai vũ khí gồm trường đao và cụm súng/khiên theo hình tham khảo.
- Vật liệu URP gồm đỏ kim loại trầm, đỏ sáng, trắng ngà, vàng, gunmetal, titan, khe tối, mắt xanh và ký hiệu kỹ thuật. Texture gồm normal bề mặt, albedo vi mô, các map metallic/smoothness riêng và một atlas ký hiệu 1024×1024.

Đây là bản tái dựng bằng hình học có phong cách góc cạnh theo ảnh tổng quan, không phải model gốc của hãng hay bản sao chính xác mọi chi tiết. Hoa văn, kết cấu khuất, tỷ lệ cánh và cơ cấu vũ khí có phần suy dựng; chi tiết đúc và decal trong ảnh chưa được tái tạo đầy đủ. Các khớp là Transform để chỉnh tư thế; chưa có rig Humanoid, skinning hay animation chiến đấu.

## Studio và kiểm tra

Scene có đế, phông, phản chiếu, bộ ánh sáng và renderer riêng có SSAO. Component SkyDefenderStudio áp dụng pipeline trưng bày khi mở scene và khôi phục pipeline trước đó khi đóng. Pipeline này phục vụ xem trong Editor; prefab model không mang theo studio.

- Đã lưu và mở lại scene, xác nhận cả ba thành phần tone mapping/bloom/color grading được lưu trong asset.
- Đã kiểm tra cả 5 prefab: không thiếu mesh, vật liệu, shader hoặc script.
- Đã chạy Play Mode hơn 6.000 frame, kiểm tra camera xoay và bật/tắt cánh bằng các hàm điều khiển. Không ghi nhận lỗi trong Console ở lần kiểm tra.
- Chưa xuất APK/iOS, đo hiệu năng điện thoại hoặc tích hợp vào luồng đặt AR của Unicorn.

Ảnh render ở `SourceArt/Renders/SkyDefender/`: Hero, Front, Rear, Head. Báo cáo tại `Docs/Reports/SkyDefender/BuildReport.txt` và `ValidationReport.txt`.

## Dựng lại

Mã nằm trong `Assets/Models/SkyDefender/Editor/`; menu **Sky Defender → Build model and atelier** dựng lại prefab và scene do bộ dựng quản lý. Sao chép prefab trước khi chỉnh thủ công nếu muốn giữ riêng các sửa đổi. Dựng lại tạo mesh asset phiên bản mới; các mesh cũ chỉ nên đưa ra khỏi Assets sau khi kiểm tra không còn scene/prefab tham chiếu.

Mã nguồn authoring dự phòng ở `Archive/Characters/SkyDefender/Authoring/`; script tạo atlas ở `Tools/Art/SkyDefender/MakeDecalAtlas.ps1`. Các mesh từ lượt thử không còn dùng đã được chuyển vào thư mục `Iterations/` bên trong đó; model hiện tại chỉ dùng hai tệp mesh container trong Assets.
