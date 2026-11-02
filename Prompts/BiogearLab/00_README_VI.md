# BỘ PROMPT DỰNG BIOGEAR LAB VÀ MỞ RỘNG NHIỀU MÔI TRƯỜNG

Đã khảo sát 10 ảnh và 25 khung hình video. Unity phản hồi qua MCP; project hiện tại dùng Unity 6000.5.8f1 và URP 17.5.0.

Bộ prompt yêu cầu map riêng scene, còn menu chọn và catalog môi trường dùng chung. Để thêm map sau này: tạo scene, tạo `EnvironmentDefinition`, đăng ký vào `EnvironmentCatalog` và thêm scene vào Build Profile. Selector không cần sửa.

## Dùng ngay

1. Mở [prompt tổng](01_MASTER_PROMPT_VI.md), sao chép toàn bộ khối prompt và gửi kèm 10 ảnh/video gốc cho AI có thể thao tác Unity project.
2. Dùng [prompt theo giai đoạn](02_PHASE_PROMPTS_VI.md) lần lượt từ khảo sát, blockout, mesh, vật liệu, ánh sáng, nghiệm thu đến giai đoạn 9 dựng selector. File có 8 brief tiếng Anh cho các asset.
3. Dùng [phân tích reference](03_REFERENCE_ANALYSIS_VI.md) để tra bằng chứng, phần suy luận, asset manifest và cách so ảnh.

```text
Đọc toàn bộ tài liệu trong E:/My project/Prompts/BiogearLab/, xem file reference gốc được liệt kê trong 03_REFERENCE_ANALYSIS_VI.md và triển khai giai đoạn 0–9 theo brief. Tạo scene riêng Assets/BiogearLab/Scenes/BiogearLab_Reconstruction.unity và EnvironmentHub/catalog dùng chung trong Assets/EnvironmentSystem/. Sau mỗi checkpoint, xuất ảnh, sửa sai lệch lớn rồi tiếp tục. Phân biệt chi tiết có bằng chứng với geometry suy luận; hiệu chỉnh kích thước seed bằng đối chiếu nhiều góc. Kiểm tra luồng Hub → BiogearLab → Hub → TestEnvironment. Viết hướng dẫn để thêm map bằng scene và catalog mà không sửa selector.
```

Đích là dựng sát những góc có reference. Khi thiếu mesh, texture, layout và camera gốc, không thể xác nhận giống 100% mọi góc. Các kích thước và màu seed là giá trị khởi đầu cần hiệu chỉnh.

Đã triển khai EnvironmentHub, catalog/selector, ba lựa chọn battle, hai scene mẫu và scene Biogear Lab. Card Biogear Lab đã được mở, có camera tự do và các góc review. Tài liệu bản dựng: `Assets/BiogearLab/Documentation/README_VI.md`. Đây là bản tái dựng procedural có các landmark chính; phần tỷ lệ/chi tiết còn cần đối chiếu, không xác nhận giống 100%. Báo cáo chuyển scene hiện tại nằm trong `Assets/EnvironmentSystem/Documentation/Verification.md`.
