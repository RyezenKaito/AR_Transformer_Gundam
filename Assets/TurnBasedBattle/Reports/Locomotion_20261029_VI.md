# Chuyển động theo từng chiêu — bản mới nhất

Đã sửa trực tiếp scene, prefab, config và animation; tài liệu Delivery_20261029_VI.md mô tả bản trước và được bổ sung bởi báo cáo này.

- **Kiếm / cận chiến:** chạy bộ áp sát, có nhịp chân theo quãng đường, chân trụ, chuyển động ngược pha thân/tay, tăng tốc–giữ tốc–hãm. Root không nhô lên khỏi mặt đất; không dùng booster. Có bước chuyển chân khi quay trở về.
- **Earth Prison:** nâng lên, lướt giữ độ cao, hạ xuống tại điểm thi triển; tư thế bay và booster riêng. Giữ đủ timeline VFX và sát thương đã xác nhận.
- **Súng / tầm xa:** giữ vị trí đội hình, ngắm/bắn/cất như bản trước.

Attack/EnemyStrike kéo dài 6.90s để có thời gian chạy đúng nhịp: chạy đi 0.65–2.65, hit 3.05, chạy về 3.65–5.95, hoàn tất cất kiếm 6.90. Né đòn kiếm của địch trong 2.80–3.00; damage/cost/cooldown không đổi. Các mốc ultimate không đổi.

**29/29 kiểm tra đạt**, bao gồm hai chân đổi nhịp, chân trụ hạn chế trượt, tốc độ chạy có giới hạn, chuyển góc root/khớp chân không bật đột ngột, đường bay giữ độ cao, giữ nguyên toàn bộ kiểm tra chiến đấu/camera/vũ khí. Các bài này kiểm chứng hành vi và tính liên tục; cảm giác animation được xem thêm bằng ảnh/clip, không suy ra chỉ từ số test.

Ảnh: Review_14_Run.png, Review_15_Flight.png. Clip ở gốc project: `ReviewMedia/LocomotionReview_20261029.webm`, gồm chạy/chém/quay/chạy về, bắn tại chỗ và bay ultimate. Clip xuất 30Hz từ Game View, không có âm thanh; không phải phép đo FPS thời gian thực. Bản CombatReview cũ được giữ để đối chiếu.

`BattleActionDefinition.travelStyle` chọn Stationary/Run/Fly. Nhịp chân/IK ở BattleLocomotionPose; quỹ đạo và vận tốc ở BattleActor; pose gốc ở BattleClipBuilder. Không bổ sung di chuyển tự do; nhân vật vẫn chỉ rời đội hình trong action hợp lệ.
