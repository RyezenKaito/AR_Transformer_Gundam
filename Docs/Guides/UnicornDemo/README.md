# Unicorn Gundam bản thử trên điện thoại

Scene: `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity`.

Bản thử mặc định mở sân ảo, tự gọi Unicorn từ trên cao xuống. Oppo F9 CPH1825 được kiểm tra qua USB đang chạy Android 8.1 API 27, hỗ trợ ARM64 và OpenGL ES 3.2. Máy không có trong danh sách ARCore của Google; sân ảo là đường kiểm thử chính trên thiết bị này.

## Cách sử dụng

- Joystick bên trái: di chuyển theo hướng camera.
- Kéo vùng trống phía trên: xoay góc nhìn trong sân ảo.
- Gọi xuống: phát lại chuyển động đáp xuống tại vị trí ban đầu.
- Đánh thử: animation cơ học và vòng xung, chưa có hệ thống sát thương.
- Tách / ráp: tách các cụm lớn để xem cấu trúc, bấm lần nữa để hoàn nguyên. Đây là exploded view, chưa phải nhận diện tay thật hoặc bài lắp linh kiện theo socket.
- Hiệu ứng lõi: vòng sáng trong ba giây; chưa phải hệ thống mana/Burst chiến đấu hoàn chỉnh.
- Đặt lại: đặt lại góc nhìn/vị trí trong sân ảo; ở AR, quét và chọn vị trí mới.
- Thử camera AR: khởi tạo AR thủ công trên thiết bị tương thích. Nếu không hỗ trợ thì giữ sân ảo. Máy cần Google Play Services for AR sẽ nhận thông báo.
- Đặt tại đây: trong chế độ AR, xác nhận vị trí vòng ngắm trên mặt phẳng và tạo anchor. Bấm các nút UI không dời sân.

Trên PC: WASD hoặc phím mũi tên, Space để đánh, R để gọi xuống, E để tách/ráp; giữ chuột phải và kéo để xoay, cuộn để đổi khoảng cách.

## Những phần được tạo

- Prefab Unicorn riêng, chuẩn hóa chiều cao 2 đơn vị trong sân ảo; 0,24 m khi đặt ở AR với tỷ lệ sân 0,12.
- 11 cụm mesh và các pivot đầu/vai/khuỷu/hông/gối; 21 vật liệu chung, 56 lượt vật liệu trước tối ưu theo pipeline. Các số này không phải số draw call đã đo trên GPU.
- Chuyển động đứng chờ, bước đi, đánh thử, hạ cánh và ánh sáng động cơ bằng code. Không cần tải hay sinh asset bên ngoài.
- Scene thử và script trong `Assets/Models/Unicorn Gundam/Demo`, giữ nguyên prefab/mesh/vật liệu Unicorn gốc.
- Android tối thiểu API 26, ARM64, IL2CPP, OpenGL ES 3, AR Optional và khởi tạo XR thủ công.
- Target 30 FPS, tắt bóng đổ trên model và pipeline render 0,85 để thử trên điện thoại cũ.
- Bản sao cấu hình cũ ở thư mục `work/unicorn_demo/backup_before_demo` bên workspace tài nguyên ban đầu.
- Git được khởi tạo, có `.gitignore` cho cache/build; chưa tạo commit hoặc kết nối remote.

## Build lại

1. Mở đúng Unity 6000.5.8f1 và scene demo.
2. Chạy Play trong Editor để thử sân ảo.
3. Chuyển nền tảng sang Android.
4. Dùng menu `Unicorn Demo > 2 Build Android APK`.
5. APK được ghi vào `Builds/Android/UnicornDemo/UnicornGundamDemo.apk`; trạng thái build ở `build-status.json` cùng thư mục.

Menu `Unicorn Demo > 1 Build demo scene and animated prefab` tạo lại các asset demo từ prefab gốc. Chỉ dùng khi đã thoát Play và lưu scene hiện tại. Không dùng prefab demo đã gộp mesh để thay bản model nguồn chi tiết.

## Phạm vi bản thử

Đây là bản kiểm tra hình ảnh, chuyển động, input, đặt sân và khả năng cài/chạy. Chưa bao gồm Story, PvP, AI học sâu, kho/shop hay nhận diện bàn tay. Các tính năng AR camera/anchor phải được kiểm tra riêng trên thiết bị ARCore/ARKit được hỗ trợ; kết quả sân ảo trên F9 không xác nhận AR thật đã đạt.

Tài liệu hỗ trợ thiết bị: https://developers.google.com/ar/devices
