# Bộ phân tích và prompt Earth Prison

Được soạn ngày 27/10/2026 từ video MP4 bạn gửi. Bộ này gồm tài liệu và ảnh tham khảo; chưa triển khai skill Earth Prison mới trong Unity.

## Mở theo thứ tự

1. `References/KEYFRAMES.jpg`: xem nhanh tám pha của chiêu.
2. `PHAN_TICH_VIDEO_EARTH_PRISON_VI.md`: phân tích chi tiết các lớp, timeline và những điều chưa thể suy ra.
3. `EARTH_PRISON_MASTER_PROMPT_VI.md` hoặc `.txt`: đặc tả đầy đủ 22 mục để giao cho chat làm Unity. Hai bản có cùng nội dung.

## Câu lệnh để bắt đầu triển khai

Đính kèm master prompt và ảnh KEYFRAMES vào chat có quyền làm việc với Unity, hoặc cho chat đọc đường dẫn trong project, rồi gửi:

> Hãy thực hiện EARTH_PRISON_MASTER_PROMPT_VI trong project E:/My project. Đọc ảnh tham khảo và bản phân tích, bắt đầu bằng scene VFXLab để tái dựng đúng timeline, vòm xoáy vàng và sáu trụ đá nối sét. Làm lần lượt các mốc A–E, chạy thử và so ảnh thật. Sau khi hiệu ứng đạt mới tích hợp thành chiêu riêng cho Gundam; giữ nguyên Q/E/R hiện có. Báo rõ phần đã hoàn thành và phần còn thiếu.

Nếu chat ở máy khác, đường dẫn tuyệt đối trong prompt sẽ không truy cập được. Khi đó gửi kèm video hoặc các ảnh tham khảo và chỉ rõ project đích.

## Khi hiệu ứng chưa giống video

> Giữ camera, seed và scale cố định. So ảnh ở 3,75 / 4,50 / 5,50 / 6,75 giây với References. Sửa hình vòm rách, khối đá, sáu điểm sáng, đường sét chu vi và cách tan rã trước. Chụp lại từng mốc sau chỉnh sửa; không tăng bloom để che hình khối chưa đúng.

## Cách đọc thông số

- Timestamp phân tích là quan sát từ khung hình, gần đúng theo bước 0,25 giây.
- Số trụ và hình thức các pha được nhìn thấy trong video.
- Mét, particle counts, shader parameters, damage, cooldown, phím T và SFX là đề xuất triển khai.
- Video không đủ để kết luận damage, stun, collision hoặc nguyên nhân mục tiêu biến mất.
- Kỹ thuật mesh/ribbon/Particle System trong prompt là cách tái dựng phù hợp project, không phải mã nguồn được lấy từ tác giả video.

Ảnh trong `References` là ảnh phân tích video gốc, không phải ảnh của skill đã được tạo mới. `SOURCE_METADATA.json` lưu thông tin nguồn và phương pháp lấy mẫu.
