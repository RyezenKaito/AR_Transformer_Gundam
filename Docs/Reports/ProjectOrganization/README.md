# Kết quả tổ chức project — 2026-10-29

Project: Gundam_AR_Test, Unity 6000.5.8f1.

## Đã thực hiện

- Tổ chức thư mục gốc: Docs, SourceArt, Tools, Builds, Archive.
- Di chuyển 17 nhóm nội dung bên ngoài Assets; giữ bản build và các dữ liệu hỗ trợ đi cùng.
- Di chuyển 8 nhóm asset bằng AssetDatabase.MoveAsset; giữ nguyên GUID và metadata.
- Code AR dùng chung ở Assets/Game/Features/AR/Scripts.
- Code nhân vật dùng chung ở Assets/Game/Features/Characters/Scripts.
- Gundam Animation Tester ở Assets/Tools/Editor.
- SampleScene ban đầu ở Assets/Sandbox/Legacy/Scenes/SampleScene.unity.
- DefaultVolumeProfile ở Assets/Settings/Rendering.
- Models copy vốn rỗng được giữ tại Assets/Sandbox/Legacy/Models copy.
- Chỉ loại thư mục Assets/Scripts sau khi xác nhận đã rỗng; metadata cũ có trong bản sao khôi phục.
- Lưu thay đổi chưa lưu của SampleScene trước khi di chuyển. Bản trên đĩa trước thao tác ở Before/Assets/Scenes; bản đã lưu thay đổi ở SavedOpenScenes trong hồ sơ di chuyển.
- Cập nhật đường dẫn tài liệu và hai công cụ PowerShell xử lý ảnh. Giữ nguyên tên class, namespace và nội dung C# được di chuyển.
- Cập nhật đường dẫn SampleScene trong danh sách build; giữ nguyên trạng thái bật/tắt của các scene.

## Models được bảo toàn

Đối chiếu SHA-256 của toàn bộ 741 file trong Assets/Models (bao gồm metadata):

- Trước: 741 file.
- Sau: 741 file.
- File bị thêm, xóa, đổi tên hoặc đổi nội dung: 0.
- Không Unpack; không thay đổi cấu trúc, code, prefab hay scene bên trong Models.

## Kiểm tra Unity

- 836 GUID gốc được giữ đầy đủ; không mất GUID cần giữ. Một GUID của thư mục Scripts rỗng được loại có chủ đích.
- 5 script được di chuyển đều phân giải được class.
- 9 scene: 0 Missing Script.
- 97 prefab: 0 Missing Script.
- Unity biên dịch thành công.
- Chạy UnicornPhoneDemo trong Play Mode: controller có mặt, đúng 1 UnicornMotion, 33 renderer và 1 camera; không ghi nhận Error/Exception trong Console khi kiểm tra.
- Kết thúc chạy thử và trả Editor về SampleScene đã chuyển, không còn thay đổi chưa lưu.
- Không build/cài APK mới trong lần tổ chức này. APK cũ được giữ nguyên nội dung; SHA-256 B8A3153CED102192EC80F64242D6D4846FA2D7D29A24B27DCEBD61C6A1D715B2.

Xem chi tiết tại UnityValidation.txt và PlayModeValidation.txt cùng thư mục.

## Tương thích công cụ hiện có

Các builder trong Models có đường dẫn xuất cố định. Để giữ nguyên Models mà vẫn hoạt động trên máy Windows hiện tại, đã tạo 5 junction ngoài Assets: Docs/Art, Docs/BlueFlame, Docs/SkyDefender, Docs/SceneBackups, Builds/UnicornDemo. Đây là liên kết tới nơi lưu mới, không phải dữ liệu trùng lặp.

Công cụ Tools/Build/Setup-LegacyPaths.ps1 đã được chạy kiểm tra và hỗ trợ thiết lập lại junction khi đổi vị trí project. Ba công cụ PowerShell đều phân tích cú pháp thành công; các công cụ tạo hình không được chạy để tránh ghi vào Models.

Các junction có tính cục bộ, không được lưu trong Git. Khi chuyển máy hoặc đổi đường dẫn project, chạy lại công cụ thiết lập trên Windows. macOS/Linux cần liên kết tương đương hoặc một đợt cập nhật builder được cho phép sửa Models.

## Bản khôi phục

Archive/ProjectOrganization/2026-10-29_ProjectOrganization chứa Before, SavedOpenScenes, root-moves.json, asset-moves.tsv, các bảng GUID và models-before.json. Archive được loại khỏi Git vì chứa bản sao cục bộ và tài nguyên lịch sử dung lượng lớn.