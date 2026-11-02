# Bàn giao — Gundam theo lượt, 29/10/2026

Đã triển khai bộ prompt vào scene `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity`, sau đó áp dụng yêu cầu bổ sung: kiếm ánh sáng; đòn súng/tầm xa bắn tại chỗ; zoom/orbit mượt; thanh máu trên đầu địch. Không cần tái tạo scene để dùng các thay đổi.

## Hành vi hiện tại

| Loại hành động | Di chuyển | Vũ khí / hiệu ứng |
|---|---|---|
| Attack | Bay áp sát, chém, trở về | Rút chuôi, bật kiếm ánh sáng, tắt/cất |
| Skill / liên xạ của địch | Giữ vị trí đội hình; chỉ xoay ngắm và phản lực thân | Rút súng, hai tay giữ, ngắm nòng, bắn, cất |
| Earth Prison | Bay tới CastAnchor ngoài lồng, thi triển, bay về | Tụ cầu điện, tia truyền, impact, vòm, sáu trụ, lồng sét, dissolve |
| Guard / Dodge | Tại chỗ | Phòng thủ hoặc né đúng nhịp |

Camera mặc định sau Gundam, nhìn chếch xuống; orbit giữ yaw người dùng qua action. Zoom nhận cả wheel chuẩn hóa ±1 và wheel thô ±120; kéo bắt đầu trên nút UI không xoay camera. Thanh máu/damage text theo đầu mục tiêu và camera hiện hành.

## Kết quả kiểm chứng

26/26 trường hợp trong `Acceptance_20261029.md` đã qua sau khi tải lại Play Mode. Có kiểm tra chuỗi lượt, cost/cooldown, né theo timestamp, không giữ/spam, 1v1/1v3, sát thương ultimate năm hit, return/cleanup, camera 720°, va chạm camera, input chuột thực qua Input System, thanh máu theo đầu, súng đứng tại chỗ và hướng nòng, hình học kiếm sau khi lưu trạng thái tắt.

Ảnh cuối `Review_01…13` đã kiểm tra trực quan; `Review_11_BeamSaber.png` cho thấy lưỡi đầy đủ, `Review_12_RangedStationary.png` là bắn từ đội hình, `Review_13_ZoomOrbit.png` là zoom gần và xoay. Clip Game View xuất ở thư mục gốc `ReviewMedia/CombatReview_20261029.webm` gồm Attack, Skill và Earth Prison từ bắt đầu đến trở về; không có âm thanh, không dùng clip này làm số đo FPS.

Đo riêng trong Editor trên i5-12500H / RTX 3060 Laptop, Game View 1233×607: idle 1v1 trung bình 8.85ms, idle 1v3 8.46ms, đoạn ultimate 1v3 9.48ms; p95 tương ứng 10.46 / 10.22 / 11.11ms. Đây là mẫu ngắn ở Editor, không phải cam kết hiệu năng Android. Counter batches không khả dụng ở phiên này (giá trị -1 trong log gốc); không suy diễn thành số draw call.

## Tài sản và giới hạn

Prefab nhân vật, Sentinel, Earth Prison; config, animation, material, shader, runtime scripts, editor builder và bộ kiểm tra đều trong `Assets/TurnBasedBattle`. Bản sao trước triển khai: `Backups/TurnBattle_Before_20261029_060820.zip`. Bộ prompt gộp đã được bổ sung quy tắc mới nhất để tránh AI triển khai lại súng bay áp sát.

AR mới có cơ chế giao camera và hệ tọa độ bàn đấu. Chưa tích hợp AR Foundation hay kiểm chứng trên thiết bị Android/AR thật. Các thử nghiệm 30/60/120Hz là bước mô phỏng logic, không phải FPS đo được. Chuyển động rig cứng đã có blend, IK, marker rút/cất và pose phối hợp; chưa thay bằng motion capture hay bộ animation thương mại.
