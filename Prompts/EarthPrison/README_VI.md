> ĐẶC TẢ MỚI NHẤT — 29/10/2026
> Đọc [master mới](../2026-10-29-Camera-Flight-Weapons/00_MASTER_PROMPT_VI.md) và các prompt 01–05 cùng thư mục. Camera mặc định sau lưng nhưng xoay 360°; Attack, Skill và Ultimate đều bay tới phía địch rồi trở về; khớp chuyển động mượt và vũ khí chỉ rút khi dùng.
> Phần bên dưới là tài liệu ngày trước được giữ để tham chiếu. Các ràng buộc camera fixed, root tuyệt đối đứng yên khi đánh, skill/ultimate đứng tại chỗ hoặc súng cầm sẵn đã được thay thế.

---

# Earth Prison — ultimate cho game theo lượt

Cập nhật28/10/2026 theo yêu cầu mới: hai phía cố định, camera fixed, đánh luân phiên, né tại chỗ bằng timing. Bộ này là phân tích/prompt; chưa tạo skill mới trong Unity.

## Đọc theo thứ tự

1. `../PHAN_TICH_CO_CHE_DANH_THEO_LUOT_VI.md`: giải thích thiết kế.
2. `../GUNDAM_TURN_BASED_MASTER_PROMPT_VI.md`: luật trận, command, né, gauge và thứ tự triển khai.
3. `EARTH_PRISON_MASTER_PROMPT_VI.md` hoặc `.txt`: 22 mục chi tiết ultimate.
4. `References/KEYFRAMES.jpg` và `PHAN_TICH_VIDEO_EARTH_PRISON_VI.md`: hình ảnh/timeline quan sát từ video.

## Giao việc

> Dựng Earth Prison theo master hiện hành trong hệ thống đánh theo lượt của project. Đây là Ultimate của nhân vật, dùng một lượt và gauge 100; nhân vật và quái giữ anchor, camera giữ hai phía xuyên suốt. Tạo lồng sáu trụ quanh phía quái, phát damage theo marker và chỉ chuyển sang lượt quái sau khi hoàn tất chiêu. Tái sử dụng rig/asset phù hợp; kiểm thử không có controller realtime chạy nền.

Nếu chưa có turn battle, làm các mốc nền của master tổng trước. VFXLab dùng preview hình ảnh riêng, không tự trừ HP hoặc chuyển lượt.

## So hình ảnh

So tại 0,75/2,25/3,00/3,75/4,50/5,50/6,75/7,50s với ảnh nguồn. Ưu tiên vòm xoáy rách, sáu trụ có khối, sáu đỉnh và sét chu vi. Camera battle fixed; góc top-down/chính diện chỉ dùng kiểm trong lab.

Timestamp video là gần đúng theo bước 0,25 s; thông số damage/gauge/particle/metre/shader là đề xuất. Các ảnh References lấy từ video gốc, không phải ảnh kết quả skill mới. Luật né/turn-based do người dùng xác định ngày 28/10, không suy ra từ video.

Bản ngày 27/10 ở `../Archive/2026-10-27-Realtime/EarthPrison/` chỉ để tham khảo lịch sử.
