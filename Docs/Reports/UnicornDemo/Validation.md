# Unicorn Gundam bản chạy thử đầu tiên

Đã build, cài và mở trên Oppo F9 CPH1825 chạy Android 8.1. Người dùng đã xác nhận thử tất cả chức năng và không gặp lỗi ở bản cuối ngày 20/10/2026.

## Trên điện thoại

- Mở ứng dụng **Unicorn Gundam Demo**. Unicorn tự hạ cánh xuống sân ảo.
- Joystick bên trái để di chuyển; camera đi theo nhân vật.
- Kéo vùng trống để đổi góc nhìn.
- **Gọi xuống** để phát lại chuyển động hạ cánh.
- **Đánh thử** để xem chuyển động tấn công và xung sáng.
- **Tách / ráp** để tách các cụm lớn rồi hoàn nguyên.
- **Hiệu ứng lõi** để xem vòng sáng thử nghiệm trong ba giây.
- **Đặt lại** để đưa nhân vật và góc nhìn về vị trí ban đầu.
- Oppo F9 đã được kiểm tra thực tế và báo không hỗ trợ ARCore. Nút **Thử camera AR** sẽ thông báo và giữ sân ảo. AR thật cần được kiểm tra trên điện thoại có hỗ trợ.

Đây là bản thử chuyển động và điều khiển. Tách/ráp hiện là chế độ xem các cụm, chưa phải nhận diện tay hoặc bài lắp theo socket. Đánh thử và hiệu ứng lõi chưa có hệ thống sát thương, mana hoặc Burst chiến đấu hoàn chỉnh.

## Trong Unity

Project: `G:\MotChutVibe\Unity\Gundam_AR_Test`.

Scene: `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity`.

Scene đã được mở và chạy ở Game view để thử. Nếu dừng, bấm Play để chạy lại.

- WASD hoặc phím mũi tên để di chuyển.
- Giữ chuột phải và kéo để xoay góc nhìn; cuộn để thay đổi khoảng cách.
- Space để đánh; R để gọi xuống; E để tách/ráp.
- Bấm lại nút Play để thoát chế độ chạy và chỉnh sửa scene.

## Kết quả kiểm tra

- APK cuối build thành công, báo cáo build không có lỗi; dung lượng APK 80.038.825 byte.
- Cài và khởi chạy thành công trên Oppo F9, ARM64, Android API 27.
- Bản cuối có cấu hình Android tối thiểu API 26, OpenGL ES 3, IL2CPP, AR Optional.
- Bộ đếm hiển thị khoảng 30 FPS ở ảnh kiểm tra đứng chờ trên điện thoại; chưa phải đo hiệu năng dài hạn mọi tình huống.
- Lượt khởi động kiểm tra bản cuối không còn exception InputUpdateType.None đã được tái hiện ở bản đầu.
- Trong Editor, di chuyển và camera đi theo đã được xác nhận; reset 5 lần vẫn chỉ có 1 Unicorn; nút UI không làm dịch sân.
- Người dùng xác nhận đã thử tất cả và không gặp lỗi ở bản cuối.
- Scene SampleScene gốc giữ nguyên, đã đối chiếu mã SHA256 với bản sao trước khi chỉnh sửa. Prefab/mesh/vật liệu Unicorn nguồn được giữ riêng; bản demo dùng prefab đã gom cụm và vật liệu.

Prefab thử có 11 cụm hiển thị và 21 vật liệu dùng chung, từ nguồn 375 MeshRenderer và 243 vật liệu. Animation là chuyển động cơ học bằng code, chưa có rig humanoid hoặc bộ clip animation sản xuất hoàn chỉnh.

## Build lại

Chọn Android trong Unity rồi dùng menu **Unicorn Demo > 2 Build Android APK**.

APK trong project: `Builds/Android/UnicornDemo/UnicornGundamDemo.apk`.

Báo cáo build: `Builds/Android/UnicornDemo/build-status.json`.

Mã SHA256 APK đã cài và bản gửi kèm: `B8A3153CED102192EC80F64242D6D4846FA2D7D29A24B27DCEBD61C6A1D715B2`.

Git và .gitignore đã được khởi tạo trong project; chưa tạo commit hoặc kết nối remote. Cấu hình gốc được sao lưu tại workspace tài nguyên ban đầu trong `work/unicorn_demo/backup_before_demo`.
