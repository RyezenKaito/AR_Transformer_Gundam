> ĐẶC TẢ MỚI NHẤT — 29/10/2026
> Đọc [master mới](2026-10-29-Camera-Flight-Weapons/00_MASTER_PROMPT_VI.md) và các prompt 01–05 cùng thư mục. Camera mặc định sau lưng nhưng xoay 360°; Attack, Skill và Ultimate đều bay tới phía địch rồi trở về; khớp chuyển động mượt và vũ khí chỉ rút khi dùng.
> Phần bên dưới là tài liệu ngày trước được giữ để tham chiếu. Các ràng buộc camera fixed, root tuyệt đối đứng yên khi đánh, skill/ultimate đứng tại chỗ hoặc súng cầm sẵn đã được thay thế.

---

# Cách dùng bộ prompt Gundam theo lượt — 28/10/2026

Thiết kế hiện hành: hai phe đứng hai phía cố định, camera fixed, đánh luân phiên, bấm đúng thời điểm để né tại chỗ; Earth Prison là ultimate. Lượt sửa này cập nhật tài liệu, chưa triển khai gameplay mới.

## Đọc và gửi những gì

1. Đọc `PHAN_TICH_CO_CHE_DANH_THEO_LUOT_VI.md` để hiểu luật và các giá trị đề xuất.
2. Gửi `GUNDAM_TURN_BASED_MASTER_PROMPT_VI.md` hoặc `.txt` làm đặc tả chính.
3. Gửi thêm `EarthPrison/EARTH_PRISON_MASTER_PROMPT_VI.md` và `EarthPrison/References/KEYFRAMES.jpg` để làm ultimate.

`GUNDAM_MASTER_PROMPT_VI.md/.txt` tại tên cũ đã được đồng bộ đầy đủ sang thiết kế mới. Không cần gửi cả hai tên master. Bản realtime cũ lưu trong Archive/2026-10-27-Realtime, không dùng làm yêu cầu hiện hành.

## Câu lệnh bắt đầu triển khai

> Hãy thực hiện GUNDAM_TURN_BASED_MASTER_PROMPT_VI và EARTH_PRISON_MASTER_PROMPT_VI trong project E:/My project. Game đánh theo lượt, nhân vật trái/quái phải đứng cố định, camera giữ nguyên cả hai phía. Làm trận 1v1 chạy được và né bằng Space đúng thời điểm trước; sau đó tích hợp Earth Prison làm Ultimate trong lượt của nhân vật, rồi thử 1v3. Đọc rig/prefab hiện có để tái sử dụng, tách controller realtime khỏi scene battle, thực hiện từng mốc và kiểm thử thật. Không dừng ở kế hoạch nếu có thể triển khai.

## Nếu tiếp tục ở một chat khác

Cho chat đọc master và hướng dẫn hiện hành; ghi rõ đã triển khai tới mốc nào và báo cáo mới nhất. Nếu máy khác không đọc được E:/My project, đính kèm các tệp và ảnh; không mặc định đường dẫn tuyệt đối còn dùng được.

## Những quyết định đã chốt và thông số được chỉnh

Đã chốt: turn-based, hai phía/camera fixed, Earth Prison ultimate, né theo timing tại chỗ. Các số damage, gauge, cửa sổ 0,20 s, phím gán, cấu hình1v3 và cooldown là đề xuất thiết kế, nằm trong config để tinh chỉnh.

Khi cần sửa, ưu tiên cập nhật cả master tổng và phần gameplay Earth Prison; các quan sát video gốc giữ riêng. Không đưa lại WASD/chase/dash/camera cinematic từ bản Archive vào trận.
