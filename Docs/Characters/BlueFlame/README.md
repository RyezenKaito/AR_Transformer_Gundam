# BlueFlame CH-01 — bản dựng trong Unity

Tham chiếu chính: `SourceArt/References/BlueFlame/overview.png`. Bảng `structure.png` dùng bổ sung cho phân chia bộ phận; tiêu đề Sky Defender trên bảng không được xem là tên chính thức của BlueFlame.

Đây là bản diễn giải hình khối từ ảnh, không phải dữ liệu CAD hay mesh gốc của ZZA. Những mặt khuất và cơ cấu bên trong được thiết kế thêm. Unicorn trong project là mẫu đối chiếu về độ dày giáp, khe lắp ráp, cơ cấu khớp và độ rõ của các bộ phận.

## Thư viện

Mọi asset dùng trong Unity nằm dưới `Assets/Models/BlueFlame/`:

- `Prefabs/BlueFlame_Detailed.prefab`: từng bộ phận riêng theo cụm đầu, thân, hông, tay, chân và cánh.
- `Prefabs/BlueFlame_Combined.prefab`: gộp theo vật liệu cho trưng bày; không giảm số tam giác.
- `Prefabs/BlueFlame_AR_25cm.prefab`: bản gộp có chiều cao 25 cm, đáy model ở y=0.
- `Prefabs/BlueFlame_Body.prefab`: thân robot và vũ khí, không có cánh.
- `Prefabs/BlueFlame_FlightUnit.prefab`: backpack và cánh.
- `Prefabs/BlueFlame_EnergyFX.prefab`: lửa xanh dạng shader có chuyển động, tách riêng.
- `Scenes/BlueFlame_Atelier.unity`: scene trưng bày có camera, đèn, đế và bộ điều khiển.
- `Meshes`, `Materials`, `Textures`, `Shaders`, `Settings`: dữ liệu riêng của BlueFlame.
- `Editor`: mã dựng và công cụ kiểm tra, không đưa vào bản game.
- `Runtime`: điều khiển camera và thiết lập render riêng cho scene trưng bày.

## Kiểm tra trong Play Mode

Kéo chuột để xoay, cuộn để zoom. Phím 1 về góc tổng quan, 2 chính diện, 3 mặt sau, 4 cận đầu–ngực. W bật/tắt cánh, F bật/tắt lửa xanh. Mặc định tắt lửa để xem rõ bề mặt giáp.

## Chi tiết được dựng

Vỏ giáp kín có chiều dày và nhiều bề mặt vát mép; mặt chính được chiếu về một mặt phẳng để tránh nếp gãy tam giác không chủ ý. Mỗi cụm có phân chia lớp giáp ngoài và phần khung bên trong. Các bộ phận xoay dùng vòng bi, vòng chặn, trục và nắp bắt vít; piston gồm xy-lanh, ty kim loại và đai giữ. Cánh có hộp chịu lực, khớp gốc và các tấm vỏ nối tiếp. Súng có nòng đôi, áo tản nhiệt, vòng làm mát và miệng nòng rỗng. Bàn tay có lòng bàn tay, đốt ngón và trục khớp.

Vật liệu tách lớp sơn trắng, xanh cobalt, xanh đậm, khung gunmetal, titanium, vàng, đỏ và thấu kính. Texture metallic/smoothness có biến thiên nhẹ. Atlas service markings được vẽ riêng; đây là decal diễn giải, không phải bộ decal gốc hãng.

## Phạm vi

Model phục vụ trưng bày và chỉnh từng cụm. Chưa có skeleton, skinning, animation chiến đấu, LOD hoặc kiểm thử hiệu năng trên điện thoại. Các khớp là chi tiết hình học và hierarchy, chưa mô phỏng chuyển động cơ khí. Prefab 25 cm chỉ chuẩn hóa kích thước, chưa tích hợp vào luồng AR placement.

Menu `BlueFlame > Build full model and atelier` tái dựng asset và scene của BlueFlame từ mã. Thao tác này sẽ thay nội dung prefab/scene BlueFlame do công cụ sinh; cần lưu bản riêng trước khi sửa tay rồi tái dựng. Mã dùng đường dẫn riêng, không sửa geometry của Unicorn, Hạo Linh hoặc Sky Defender.

Ảnh kiểm tra nằm tại `SourceArt/Renders/BlueFlame/`; số liệu lần dựng và kiểm tra tại `Docs/Reports/BlueFlame/`.

Tham khảo nhận diện sản phẩm: [JFC – CH-01 Blue Flame](https://www.jfc.sg/products/blue-flame). Hình dáng bản dựng ưu tiên hai ảnh người dùng cung cấp.
