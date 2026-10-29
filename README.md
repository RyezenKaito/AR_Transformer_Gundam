# AR Transformer Gundam

Unity project thử nghiệm game AR Gundam. Bản hiện tại tập trung vào Unicorn Gundam: đặt model, điều khiển, animation và hiệu ứng trong sân ảo; trên thiết bị hỗ trợ, có thể thử đặt AR. Story, PvP, kho/shop hoàn chỉnh, AI học sâu và nhận diện tay thật chưa được tích hợp.

## Cài và chạy nhanh

### Chỉ cài bản thử Android

1. Chép `Builds/Android/UnicornDemo/UnicornGundamDemo.apk` sang điện thoại Android.
2. Mở APK, cho phép cài ứng dụng từ nguồn này nếu Android hỏi, rồi mở **Unicorn Gundam Demo**.
3. Nếu thiết bị không hỗ trợ ARCore, game tự chạy ở **sân ảo**. Đây là chế độ đã kiểm tra trên Oppo F9.

### Mở project để phát triển

1. Cài Unity Hub và Unity Editor **6000.5.8f1**, bao gồm Android Build Support, Android SDK, NDK và OpenJDK.
2. Mở chính thư mục `Gundam_AR_Test` bằng Unity Hub.
3. Đợi Unity tải package/import asset lần đầu, sau đó mở `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity`.
4. Nhấn Play để thử trên máy tính; muốn xuất APK thì chuyển Build Platform sang Android.

Trên Windows, có thể dùng Visual Studio hoặc Rider để viết C#. `.vsconfig` có sẵn workload **Managed Game Development** cho Visual Studio.

## Môi trường chạy

| Hạng mục | Thiết lập hiện tại |
|---|---|
| Unity Editor | 6000.5.8f1 |
| Render pipeline | Universal Render Pipeline 17.5.0 |
| Input | Unity Input System 1.20.0 |
| Android tối thiểu | API 26, Android 8.0 |
| Android build | ARM64, IL2CPP, OpenGL ES 3 |
| Ứng dụng Android | `com.kaito.unicorndemo`, phiên bản 1.0.0 |
| AR | AR Foundation 6.5.0, ARCore 6.5.0, ARKit 6.5.0 |
| Khung hình mục tiêu của Unicorn Demo | 30 FPS |

Oppo F9 CPH1825 chạy Android 8.1 đã cài và kiểm tra Unicorn Demo ở sân ảo. Máy không hỗ trợ ARCore nên kết quả này không xác nhận AR thật. AR placement cần Android có Google Play Services for AR hoặc iPhone/iPad hỗ trợ ARKit.

## Package và extension

Project không có file `.vscode/extensions.json`, nên không bắt buộc cài extension VS Code riêng. Các package Unity trực tiếp phục vụ project là:

| Package | Phiên bản | Dùng để làm gì |
|---|---:|---|
| AI Assistant | 2.21.0-pre.1 | Hỗ trợ thao tác AI trong Unity Editor |
| AI Inference | 2.6.1 | Nền tảng chạy mô hình AI cục bộ khi cần về sau |
| glTFast | 6.20.0 | Import model GLB/glTF |
| Input System | 1.20.0 | Bàn phím, chuột, cảm ứng và UI |
| Universal Render Pipeline | 17.5.0 | Render mobile và PC |
| AR Foundation | 6.5.0 | Lớp AR chung cho Android và iOS/iPad |
| ARCore XR Plugin | 6.5.0 | AR trên Android |
| ARKit XR Plugin | 6.5.0 | AR trên iPhone/iPad |
| XR Interaction Toolkit | 3.5.1 | Tương tác XR và sample AR |
| XR Plug-in Management | 4.6.0 | Quản lý loader XR theo nền tảng |
| Android Logcat | 1.4.7 | Xem log điện thoại trong Unity |
| Rider | 3.0.40 | Kết nối Unity với JetBrains Rider |

Unity còn có các module engine như animation, audio, physics, UI, video và UnityWebRequest; đây là thành phần nền tảng, không phải extension cần cài riêng.

## Các Gundam hiện có

`Assets/Models` đang giữ nguyên cấu trúc. Project có 10 Gundam:

| Nhóm | Gundam |
|---|---|
| Tạo trong Unity | BlueFlame, HaoLing, SkyDefender, Kamiki Burning Gundam |
| Model tải về | EXtreme Application ON EXtreme Gundam, MBF-02 Strike Rouge Ootori, RX-78-2 Gundam, RX-78-2 Gundam Part 2, Unicorn Gundam, ZGMF-X09A Justice Gundam |

`Download Source` chỉ lưu gói nguồn đã tải và `Shaders` chỉ lưu shader dùng chung; chúng không phải Gundam.

## Chạy thử ở đâu

| Mục đích | Scene hoặc nơi chạy | Trạng thái |
|---|---|---|
| Unicorn trên điện thoại Android | `Assets/Models/Unicorn Gundam/Demo/Scenes/UnicornPhoneDemo.unity` | Đã kiểm tra trên Oppo F9 bằng sân ảo |
| Unicorn trên PC | Cùng scene Unicorn Demo, bấm Play | Điều khiển chuột, bàn phím và UI |
| BlueFlame | `Assets/Models/BlueFlame/Scenes/BlueFlame_Atelier.unity` | Scene trình diễn model |
| HaoLing | `Assets/Models/HaoLing/Scenes/HaoLing_Phoenix_Atelier.unity` | Scene trình diễn model |
| SkyDefender | `Assets/Models/SkyDefender/Scenes/SkyDefender_Atelier.unity` | Scene trình diễn model |
| Scene thử cũ | `Assets/Sandbox/Legacy/Scenes/SampleScene.unity` | Không nằm trong build hiện tại |

Unicorn Demo: joystick trái để di chuyển; kéo vùng trống để xoay camera; dùng nút để gọi xuống, đánh thử, tách/ráp, bật hiệu ứng lõi và đặt lại. Trên PC dùng WASD hoặc phím mũi tên để di chuyển, Space để đánh, R để gọi xuống, E để tách/ráp, giữ chuột phải để xoay camera.

## Build lại APK Unicorn

1. Mở Unicorn Demo và lưu thay đổi đang làm.
2. Chuyển Unity sang Android.
3. Chọn **Unicorn Demo > 2 Build Android APK**.
4. APK xuất tại `Builds/Android/UnicornDemo/UnicornGundamDemo.apk`.

Menu tạo lại demo tạo asset demo từ model Unicorn gốc. Nó không phải công cụ unpack hoặc thay thế model nguồn.
