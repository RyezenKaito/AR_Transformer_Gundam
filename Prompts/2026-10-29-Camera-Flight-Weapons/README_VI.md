# Bộ prompt ngày 29/10/2026

Đọc `00_MASTER_PROMPT_VI.md` để chốt phạm vi và thứ tự thực hiện. Sau đó gửi từng prompt 01–05, hoặc gửi toàn bộ `TAT_CA_PROMPT_VI.txt` trong một lần.

| Tệp | Vấn đề giải quyết |
| --- | --- |
| 01_CAMERA_THIRD_PERSON_360_VI.md | Góc sau lưng theo ảnh, orbit 360°, follow mềm, zoom và camera ngoài/AR |
| 02_BAY_AP_SAT_THEO_LUOT_VI.md | Attack, Skill, Ultimate đều bay tới phía địch, ra đòn, quay về rồi mới đổi lượt |
| 03_CHUYEN_DONG_KHOP_MUOT_VI.md | Rig, vai/eo/tay/chân phối hợp, blend pose, IK, trọng lượng và nhịp động tác |
| 04_RUT_CAT_VU_KHI_VI.md | Idle tay không, rút/nắm/kích hoạt/đánh/tắt/cất đúng marker và socket |
| 05_EARTH_PRISON_BAY_AP_SAT_VI.md | Ultimate áp sát, timeline VFX riêng, sát thương đồng bộ và cleanup |

Mỗi prompt có bản `.txt` cùng nội dung để copy. Các con số là cấu hình khởi đầu để tinh chỉnh, không phải yêu cầu số đo chính xác từ video.

**Câu mở đầu khi giao cho AI triển khai:**

> Hãy thực hiện bộ prompt ngày 29/10/2026 trong project hiện tại. Bộ này thay thế yêu cầu cũ về camera fixed, nhân vật luôn đứng yên khi đánh và vũ khí luôn cầm sẵn. Đọc master và 5 phần kèm theo, kiểm tra phần code/scene còn dang dở rồi hoàn thiện theo thứ tự, có kiểm thử và lưu scene trước khi báo xong.

Ảnh camera: `References/DefaultCamera.png`. Video mới được trích thành `References/CONTACT.jpg` và `References/DrawSequence/CONTACT.jpg`; metadata có trong mỗi thư mục. Video nguồn vẫn ở đường dẫn Downloads người dùng cung cấp, không được chép nguyên video vào project.

Bộ này là tài liệu giao việc. Tạo prompt không đồng nghĩa đã thực hiện các thay đổi vào game hoặc đã kiểm chứng AR.
