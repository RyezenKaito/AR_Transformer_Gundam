# Hạo Linh Bạch Đế — bộ ba trưng bày

Đã dựng trực tiếp trong project **Gundam_AR_Test**, Unity 6000.5.8f1 / URP.

## Mở và xem

- Scene: `Assets/Models/HaoLing/Scenes/HaoLing_Phoenix_Atelier.unity`.
- Nhấn Play, kéo chuột để xoay, cuộn để phóng to/thu nhỏ.
- Phím 1: robot; 2: Phụng; 3: Hoàng; 4 hoặc R: trở lại toàn cảnh; Space: bật/tắt chuyển động lơ lửng của hai chim.
- Có thể chọn từng khớp và từng mảnh giáp trong Hierarchy khi ở Edit Mode.

## Các asset bàn giao

| Asset trong `Assets/Models/HaoLing/Prefabs/` | Nội dung |
| --- | --- |
| `HaoLing_Trio_Detailed.prefab` | Robot và hai chim, 2.007 mesh riêng, giữ các nhóm khớp để chỉnh sửa |
| `HaoLing_Trio_Combined.prefab` | Bộ ba ghép theo vật liệu, 26 mesh renderer, giữ riêng ba đơn vị |
| `HaoLing_Trio_AR_25cm.prefab` | Bản ghép, tỉ lệ 0,075, robot cao khoảng 25 cm |
| `EMP02_HaoLing_Detailed.prefab` | Robot riêng, gốc tọa độ đặt ở lòng bàn chân |
| `MC02F_Feng_Detailed.prefab` | Phụng riêng, hai đuôi uốn xuống |
| `MC02H_Huang_Detailed.prefab` | Hoàng riêng, đuôi vươn lên và mào vòng vàng |

Hình học của cả bộ: **117.972 tam giác**, 9 vật liệu chính. Bản combined giữ nguyên số tam giác, chỉ ghép các phần cùng vật liệu. Chưa có LOD hoặc phép đo tốc độ trên điện thoại thật. Prefab AR có sẵn kích thước để tích hợp; luồng đặt mô hình AR hiện có của Unicorn chưa được thay thế.

Các phần gồm đầu, mắt, vương miện, giáp ngực/vai/váy/chân, khung khớp, ngón tay, thương/kiếm, khiên, backpack/dock, đầu chim, cánh nhiều lớp, móng vuốt, khớp cánh, hai đuôi nhiều đốt trên mỗi chim và các đầu đuôi ruby. Các khớp là Transform để tạo dáng; chưa có skeletal skinning hoặc hoạt ảnh chiến đấu.

Vật liệu gồm trắng ngọc, trắng ánh ngà, navy, titan, vàng champagne, khe tối, đỏ carmine, ruby trong và ngọc xanh phát sáng. Texture 512×512 được vẽ bằng thuật toán cho men giáp, kim loại xước, tinh thể và bề mặt vi mô. Các đường viền, đinh, rãnh và lớp giáp chủ yếu được dựng bằng hình học.

## Ánh sáng và bảo toàn công việc trước đó

Scene trưng bày có camera, đế bát giác, phông cong, ánh sáng và cubemap phản chiếu riêng. `HaoLingStudioLighting` bật pipeline trưng bày khi scene mở và khôi phục pipeline trước đó khi scene đóng. Asset pipeline Unicorn gốc không bị sửa.

Scene Unicorn đang có thay đổi chưa lưu đã được sao lưu tại `Assets/Models/HaoLing/Backups/UnicornPhoneDemo_20261027_095504.unity` trước khi mở scene mới. Các bản mesh trung gian không còn được scene/prefab dùng đã được lưu vào `Archive/Characters/HaoLing/Authoring/HaoLing_Iteration_Backup.zip` để tránh làm nặng thư mục Assets.

## Tham khảo và mức độ chính xác

Đây là **bản tái dựng thủ công bằng mesh theo ảnh**, có phong cách góc cạnh; không phải mesh gốc của GS-TOYS và không bảo đảm sao chép chính xác từng chi tiết. Hình dáng tổng thể, cách phân lớp và vật liệu được suy dựng từ hai ảnh người dùng gửi và các ảnh sản phẩm công khai. Các bề mặt sau lưng và kết cấu bị che được tự thiết kế. Không dùng ảnh quảng cáo làm texture phủ trực tiếp lên model.

- [Nhà phân phối Milestone: EMP-02, hai chim Feng/Huang và khả năng ghép cánh](https://b2b.mile-stone.jp/en/products/00065720017).
- [Hobby Search: ảnh sản phẩm và mô tả màu trắng/vàng/navy](https://www.1999.co.jp/11464783).
- [Ảnh toàn bộ ba đơn vị](https://www.1999.co.jp/itbig146/11464783a3.jpg).
- [Ảnh cận thân và đầu](https://www.1999.co.jp/itbig146/11464783a4.jpg).
- [Ảnh hai chim](https://www.1999.co.jp/itbig146/11464783a7.jpg).

## Kiểm tra đã thực hiện

- Các script dựng hình biên dịch và chạy thành công trong Unity.
- Prefab detailed và combined có cùng 117.972 tam giác; không có mesh/material rỗng hoặc script bị thiếu ở lần kiểm tra trước khi chốt.
- Đã render toàn cảnh, cận đầu/thân, robot riêng và góc sau để kiểm tra hình học và bố cục.
- Đã chạy Play Mode hơn 500 frame, xác nhận camera và bộ điều khiển xem mô hình hoạt động; không ghi nhận lỗi runtime trong Unity Console tại thời điểm kiểm tra.
- Chưa xuất Android/iOS, chưa kiểm tra hiệu năng hoặc theo dõi AR trên thiết bị thật.

Mã dựng lại nằm trong `Assets/Models/HaoLing/Editor/`; menu **Hao Ling → Build detailed trio and showcase** tạo lại bộ model. Thao tác dựng lại sẽ thay thế các prefab/scene do bộ dựng này quản lý; nên sao chép asset trước khi chỉnh tay.
