> Bản hiện hành: xem [thông số chạy chậm và R bay cao](Reports/HighCast_20261029_VI.md). **31/31 kiểm tra đạt.** Clip mới: `ReviewMedia/HighCastReview_20261029.webm` tại gốc project. Các phần timeline 6.90s/10.90s bên dưới mô tả bản trước; bản mới dùng Attack 12.45s, R 13.10s và VfxStart 2.55s. Cách điều khiển không đổi.

# Gundam theo lượt — bản cập nhật 29/10/2026

Mở `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity` rồi Play. Scene và prefab đã lưu; không cần chạy builder để chơi.

## Cách chơi

- `1`: kiếm ánh sáng, chạy áp sát/chém/chạy trở về/cất kiếm.
- `2`: rút súng, ngắm và bắn **tại vị trí đội hình**, rồi cất súng. Đòn liên xạ của địch cũng đứng tại chỗ.
- `3`: phòng thủ, giảm 50% sát thương trong pha địch kế tiếp.
- `R`: chọn Earth Prison khi đủ 100 thanh; `Enter` hoặc nút xác nhận để thi triển. Ultimate vẫn bay tới vùng địch, thi triển rồi bay về.
- `Tab` hoặc bấm thanh máu trên đầu địch: chọn mục tiêu. Không có di chuyển tự do bằng WASD.
- `Space` hoặc nút NÉ: bấm một lần trong vùng vàng của từng hit. Giữ phím hoặc spam không tạo lần né mới cho cùng hit.
- Giữ chuột phải và kéo: xoay 360°. Lăn chuột: zoom gần/xa. `Q/E`: xoay. `Home`: khôi phục góc sau lưng, chếch xuống 18°.
- Trên màn hình cảm ứng: vuốt một ngón để xoay, chụm hai ngón để zoom. Cử chỉ bắt đầu trên nút UI không điều khiển camera.
- `P`: tạm dừng; vẫn xoay camera để xem. `F1`: tùy chọn, tốc độ 0.5×/1×/2×. `F2`: nạp ultimate để thử. `F5/F6`: reset 1v1/1v3.

Thanh máu và số sát thương chiếu theo camera đang hoạt động, bám đầu mục tiêu. Kiếm có lõi trắng, quầng xanh, ánh sáng cục bộ và vệt chém; chỉ bật lưỡi khi dùng, tắt trước khi cất. Vũ khí được tái sử dụng, không tạo mới mỗi đòn.

## Dữ liệu và nơi chỉnh

- `Config/BasicAttack.asset`, `EnergyCannon.asset`, `EnemyStrike.asset`, `EnemyCombo.asset`: weapon, approachTarget, grip/draw/ignite/sheathe marker, thời lượng, damage. Súng dùng `approachTarget=false` nhưng vẫn chạy chuỗi rút/ngắm/bắn/cất.
- `Config/EarthPrison.asset`: toàn action 10.90s; VFX bắt đầu 1.55s và kéo dài 8s; damage tại ActionTime **4.90 / 6.65 / 7.05 / 7.45 / 7.85**, tương ứng **120 + 60 × 4 = 360** mỗi mục tiêu sống đủ chuỗi. Return 9.55–10.60s; hoàn tất 10.90s rồi mới lượt địch.
- `Prefabs/Unicorn_BattlePlayer.prefab`, `Sentinel.prefab`: rig, hai holster, hai hand grip, muzzle, offhand grip, booster và kiếm. `BattleWeaponRig` điều phối IK/attachment bằng action clock.
- `Prefabs/EarthPrison_VFX.prefab`: trụ, vòm rách, sét, particle. Các lớp dùng tọa độ local/custom dưới `BattleContent`.
- `Scripts/Runtime/BattleOrbitCamera.cs`: orbit/zoom/follow và giới hạn. Camera không tự reset yaw khi đánh.
- `Scripts/Editor/BattleClipBuilder.cs`: pose/animation được tạo từ marker; Animator nguồn tắt để tránh hai hệ điều khiển cùng một khớp.

Menu **Turn Battle/Build Turn Battle Demo** tái tạo scene, config, material, animation và prefab từ builder. Nó sẽ ghi đè các chỉnh sửa thủ công trong tài sản được tạo; chỉ dùng khi muốn tái tạo. Bản gốc trước triển khai nằm ở `Backups/TurnBattle_Before_20261029_060820.zip` tại gốc project.

## Kiểm tra và bằng chứng

Trong Play Mode, menu **Turn Battle/Run Latest Acceptance Tests** chạy kiểm tra thực tế lên scene rồi trả về trận 1v1. Đọc `Reports/Acceptance_20261029.md` để xem kết quả mới nhất, không dùng báo cáo Verification.md cũ làm kết luận cho bản này.

Ảnh `Reports/Review_01…15` gồm góc mặc định, các pha ultimate, nhiều yaw, kiếm, bắn tại chỗ và zoom. Clip tại `ReviewMedia/LocomotionReview_20261029.webm` ở gốc project được xuất từ Game View theo bước 30Hz, không có âm thanh và không phải số đo FPS thời gian thực. `BattleReviewMovie.Capture` xuất frame; `Tools/encode-battle-review.cjs` mã hóa WebM bằng Edge cục bộ. Số liệu hiệu năng Editor riêng ở `Reports/Performance_20261029.md`.

## AR và giới hạn kiểm chứng

`UseExternalCamera(camera)` giao camera cho hệ ngoài, không sửa pose/projection/trạng thái bật của camera tracking. HUD/VFX đổi sang camera đó; `RestoreDesktopCamera()` khôi phục orbit. Đã kiểm tra bàn đấu đổi vị trí, quay và scale đồng đều; chưa tích hợp AR Foundation, build Android hay kiểm tra tracking/gesture/hiệu năng trên thiết bị thật. Chuyển động hiện dùng rig cứng và animation sinh từ pose; có thể tiếp tục tinh chỉnh mỹ thuật trong clip builder.

## Cập nhật chạy bộ / bay theo từng chiêu

`BattleActionDefinition.travelStyle`: Stationary, Run, Fly. `approachTarget` cho phép tiếp cận; Stationary đi cùng approachTarget=false. `runStride` là quãng đường mỗi chu kỳ chân, mặc định 1.65 đơn vị bàn đấu. Nhịp chân dựa trên quãng đường thực, không chạy animation cố định trong khi root đi với tốc độ khác. `BattleLocomotionPose` ghép chuyển động thân/tay và IK chân vào action pose, trước bước gắn vũ khí.

Attack / EnemyStrike mới: chuẩn bị+rút 0–0.65s; chạy đi 0.65–2.65; hit 3.05; quay/chạy về 3.65–5.95; cất và ổn định đến 6.90s. Blade bật 2.48, tắt 3.35. Cửa sổ né đòn cận chiến địch 2.80–3.00s. Sát thương, thanh ultimate và cooldown không đổi. Earth Prison giữ các marker damage/VFX cũ; đường bay mới có đoạn lướt giữ độ cao thay vì một cung nhảy.

Ảnh `Review_14_Run.png` và `Review_15_Flight.png` minh họa hai loại di chuyển. Clip LocomotionReview là bản mới; CombatReview lưu bản trước để đối chiếu. Chất lượng chuyển động còn có thể tinh chỉnh theo cảm nhận khi chơi, nhất là khi thay model/tỉ lệ chân khác.
