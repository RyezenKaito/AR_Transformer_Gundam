# Strike Rouge Mecha HUD

Mở scene `Assets/Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity`, chọn tab Game và nhấn Play. Đây là sân ảo trong Editor; không cần điện thoại hoặc camera thật.

## Điều khiển

- Trong lượt của bạn, bấm trực tiếp vào thân bất kỳ Gundam địch còn sống để chọn mục tiêu, hoặc dùng ba nút tên và Tab. Viền đỏ, thanh HP/giáp và dòng xem trước skill chuyển theo lựa chọn. Mục tiêu được giữ qua lượt kế tiếp nếu còn sống; không bắt buộc đánh từ trái sang phải. Kéo để xoay sân không đổi mục tiêu. Chiêu 3 vẫn đánh toàn bộ địch; ulti dùng mục tiêu đã chọn làm mục tiêu chính và gây lan.
- Bấm một trong bốn ô vũ khí, hoặc phím 1 đến 4, để xem trước. Chọn skill chưa tung đòn.
- Bấm EXECUTE hoặc Enter để xác nhận. Tài nguyên được kiểm tra lại ngay khi thực hiện.
- Khi địch chuẩn bị đánh, hai nút Dodge và Parry thay thế cụm vũ khí. Q là Parry, E là Dodge. Mỗi đòn chỉ được dùng một phản ứng.
- Giữ chuột phải và kéo để xoay sân ảo; cuộn lên để zoom gần, cuộn xuống để zoom xa. Mặc định mỗi nấc cuộn giảm khoảng 20% khoảng cách; chỉnh Mouse Wheel Zoom Speed trên RougeArenaCamera nếu muốn đổi độ nhạy. Zoom luôn bị giới hạn khoảng cách an toàn, không hoạt động khi trỏ trên nút UI hoặc trong cinematic. F đưa camera về góc ban đầu. Trong AR, đổi góc bằng cách di chuyển thiết bị.
- Bấm chân dung nhân vật để xem thuộc tính. Bấm SYSTEM / ARENA để mở Reset, nạp Burst thử, chuyển sân ảo hoặc AR, đặt lại sân và reset camera.

## Các luật được nối vào HUD

Burst có năm ô, bắt đầu với hai ô. Chi phí bốn chiêu là 0, 1, 2 và 5. Chiêu 1 hồi một ô sau khi hoàn thành; mỗi lượt hành động của người chơi hoàn tất hồi thêm một ô. Full Burst tiêu hao năm ô rồi hồi một ô cuối lượt. Burst không hồi theo lượt của địch. Bản mới không dùng thanh Energy phần trăm hoặc cộng Burst khi bị đánh như demo cũ.

Mỗi Gundam có MaxHP, HP hiện tại, MaxArmor, Armor hiện tại, Attack, Def, Speed và Break Dmg. Giá trị trong scene là dữ liệu thử nghiệm. HP và Armor trên HUD đọc từ nhân vật thật, không dùng số minh họa của ảnh concept.

Lịch hành động dùng action value 10000 / Speed. Nhân vật nhanh có lượt thường xuyên hơn; hòa thời điểm được xử lý theo thứ tự đăng ký, người chơi trước. Timeline hiển thị tác nhân hiện tại cùng các hành động kế tiếp. Gundam bị hạ bị bỏ khỏi lịch.

Đúng timing thì Dodge chắc chắn thành công khi Speed lớn hơn người đánh; Speed bằng hoặc thấp hơn có 50% xác suất. Parry so Def nhân 1,25 với Attack: lớn hơn thì chặn toàn bộ, còn lại nhận 50%. Sai timing không được giảm sát thương từ phản ứng. Không còn phản đòn Perfect Parry của demo cũ.

Sát thương giáp cơ sở bằng 0,5 Attack cộng 0,5 Break Dmg. Tổng lượng phá giáp của một skill được chia cho các hit của animation, tránh nhân lượng phá giáp theo số hit. Còn giáp thì sát thương HP giảm 50%; hết giáp thì mất lớp giảm đó. Đòn vừa làm vỡ giáp vẫn dùng trạng thái giáp lúc bắt đầu hit; hiệu ứng phá giáp chỉ được ghi nhận một lần cho lần chuyển trạng thái.

**Giới hạn thiết kế:** công thức sát thương HP theo Attack và Def chưa được chốt trong tài liệu combat. Demo giữ các tổng sát thương HP của skill cũ làm dữ liệu tuning; Def hiện dùng cho ngưỡng Parry. Các hệ số phá giáp của skill đang bằng 1. Ulti chỉ dùng trong lượt của chính nhân vật; quyền chen lượt chưa được thêm. Không áp dụng cooldown hai lượt cũ cho chiêu 3 trong mô hình tài nguyên mới.

## Cấu trúc UI

- Prefab: `Assets/Game/Features/Combat/UI/StrikeRouge/Prefabs/StrikeRougeMechaHUD.prefab`.
- Cấu hình art: `Assets/Game/Features/Combat/UI/StrikeRouge/StrikeRougeHudArt.asset`.
- Chân dung và icon: `Assets/Game/Features/Combat/UI/StrikeRouge/Art`.
- `MechaBattleHUD` quản lý lựa chọn và hiển thị, còn `RougeBattleDemo` sở hữu luật combat và tài nguyên.
- `MechaPlateGraphic` vẽ khung cắt góc bằng mesh UI, không dùng ảnh chụp nguyên giao diện làm nút.
- `RougeTurnOrder` tính lịch hành động và dự báo timeline.

Chân dung được render từ bản sao model trong Editor. Bộ icon mặc định lấy từ atlas minh họa sinh bằng công cụ Imagegen tích hợp, gồm rifle, đại kiếm, tên lửa và đầu Gundam; các ảnh vũ khí bake từ rig cũng được giữ làm tài nguyên thay thế. Không sửa hoặc unpack file trong Models. HUD hiển thị trên Canvas theo safe area, còn camera và Gundam là scene thật. Trong ulti, camera cinematic riêng ẩn HUD combat và giữ góc nhìn trước đó. Mất tracking thì tạm dừng combat, animation và timing.

Atlas nằm tại `Assets/Game/Features/Combat/UI/StrikeRouge/Art/MechaWeaponsAtlas.png`. Prompt và nguồn tài nguyên được ghi trong `Docs/Design/StrikeRougeHUDArt.md`.

## Công cụ Editor

- **Gundam > Strike Rouge > Upgrade Mecha HUD**: tạo art còn thiếu, dựng prefab và gắn vào scene; lưu bản sao scene trước thay đổi.
- **Gundam > Strike Rouge > Validate Mecha HUD**: chạy Play Mode, kiểm tra luật, tương tác nút, cinematic và bố cục ở các độ phân giải mô phỏng.
- Báo cáo và ảnh kiểm tra: `Docs/Reports/StrikeRougeHUD`.
- Code trước thay đổi: `Docs/Reports/StrikeRougeHUD/Before`. Scene trước thay đổi nằm trong `Assets/Sandbox/StrikeRougeCombat/PreservedScenes`.

Kiểm tra bằng Game View không thay thế kiểm tra Android/iOS hoặc tracking AR trên thiết bị. Bản này không build APK hoặc cài lên điện thoại.
