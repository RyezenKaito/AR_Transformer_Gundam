# Chạy chậm, sải dài và R thi triển trên cao — bản hiện hành

> Cập nhật hiệu ứng phản lực sau lần quay clip: đã bỏ hai cylinder đen thêm vào lưng và sphere xanh kéo dài, thay bằng flash/jet additive trong suốt ở socket gốc. Scene, prefab và ảnh Review mới đã cập nhật; clip HighCastReview quay trước sửa hình thức phản lực, vẫn dùng được để đối chiếu nhịp chạy/độ cao R. Xem ThrusterFlashCheck.md cho kiểm tra bổ sung. Regression vẫn 31/31.

Đã thực hiện prompt `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt`. Báo cáo này thay thế các thông số locomotion và thời điểm ultimate trong Delivery/Locomotion trước đó.

## Thay đổi đã lưu

- Sải hai bước tăng từ 1.65 lên **2.10 đơn vị bàn đấu**. Thời gian chạy đi tăng từ 2.0 lên **4.70s**, chạy về **4.90s**; trên đường chuẩn, tốc độ giữa quãng khoảng **2.28 đơn vị/s**, chu kỳ hai bước khoảng **0.92s**. PlaybackSpeed toàn trận vẫn là 1×.
- Vai trái vung khoảng ±23°, vai tay cầm chuôi khoảng ±17°, khuỷu khoảng 69–83° và 69–79°. Vai vung ngược chân cùng bên, có chuyển trọng lượng pelvis, chân trụ và bước quay. Tay được blend về pose chạy riêng trước attachment, tránh cộng động tác lên pose rút kiếm cũ.
- R: khoảng hở bàn chân so với mặt bàn được điều khiển ở **1.15 × chiều cao rig**, khoảng **2.99 đơn vị** với Gundam 2.6. Bù offset chân theo pose hover; không lấy bounds của kiếm/booster làm chiều cao model.
- Caster ở ngoài bán kính vùng trụ, giữ độ cao trong cả 8s VFX, booster mức thấp; các lớp bụi/vòm/trụ/lồng vẫn ở mặt bàn. VFX chỉ mở khi vị trí ngang, độ cao và pha ổn định hợp lệ.
- Camera blend focus giữa caster trên cao và mục tiêu dưới đất, nới boom tối đa 2.2 đơn vị và pitch thêm tối đa 5° theo độ cao; yaw và mức zoom người dùng vẫn được giữ. AR camera ngoài không bị ghi pose.

## Timeline hiện hành

| Action | Các mốc |
|---|---|
| Kiếm Attack / EnemyStrike | Chạy đi 0.75–5.45; hit 5.90; quay/chạy về 6.60–11.50; cất 11.68–12.10; complete 12.45s |
| Né đòn kiếm địch | Cue 5.00; cửa sổ 5.65–5.85; impact 5.90s |
| Earth Prison | Bay lên/tiến 0.45–2.20; ổn định đến 2.55; VFX 2.55–10.55; return/hạ 10.55–12.50; complete 13.10s |
| Damage R | ActionTime 5.90 / 7.65 / 8.05 / 8.45 / 8.85; 120 + 4×60 = 360 mỗi mục tiêu sống đủ chuỗi |

Súng vẫn đứng tại chỗ, damage/cost/cooldown và nguyên tắc đánh theo lượt không đổi. Lượt địch chờ nhân vật về và hoàn tất cleanup.

## Kiểm chứng

**31/31 kiểm tra đạt** trong Acceptance_20261029.md: chân trụ, bước đổi chân, tốc độ/cadence, tay ngược pha, độ liên tục root/khớp, R giữ độ cao suốt các pha, camera thấy caster và vùng đất ở góc mặc định, marker damage mới, cùng toàn bộ hồi quy turn/cooldown/dodge/weapon/board transform.

Bài kiểm tra độ trượt chân đã được sửa để đo lúc bàn chân tiếp đất. Chỉ xét vận tốc đứng bằng 0 sẽ nhận nhầm cả đỉnh quỹ đạo vung chân là chân trụ; không dùng tiêu chí sai đó để ép animation đứng cứng.

Ảnh Review_02_Charge, Review_04_Cage, Review_14_Run, Review_15_Flight là ảnh mới. Clip `ReviewMedia/HighCastReview_20261029.webm` ở gốc project xuất Game View 30Hz, không có âm thanh, không phải số đo FPS thời gian thực. Clip `LocomotionReview_20261029.webm` giữ bản trước, với cùng góc 35° cho đoạn kiếm để đối chiếu. Cảm giác chuyển động vẫn cần người chơi đánh giá trực tiếp; test chỉ xác nhận các thuộc tính cụ thể, không chứng minh chất lượng animation tuyệt đối.

Scene: Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity. Các prefab/config/clip mới đã lưu. Chưa kiểm tra trên thiết bị Android/AR thật. Báo cáo Performance trước là phép đo của bản trước, không phải benchmark cho bản này.
