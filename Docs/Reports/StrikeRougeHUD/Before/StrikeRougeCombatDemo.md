# Strike Rouge Ootori — animation và bộ skill

Mở **Gundam → Strike Rouge → Open Combat Demo**, chọn tab **Game**, nhấn **Play**. Scene là `Assets/Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity`.

Khi vào Play, Strike Rouge xuất hiện bằng clip gốc `O-01-APPER`, hạ xuống sân rồi chuyển sang `O-02-IDLE`. Trong một lượt, chọn một trong ba đối thủ bằng thẻ tên hoặc Tab, rồi bấm nút chiêu hoặc phím 1–4.

| Phím | Chiêu | Animation và hiệu ứng | Tài nguyên / sát thương |
|---|---|---|---|
| 1 | Beam Rifle Burst | Rifle ngắm mục tiêu, 3 beam xanh lá từ đầu nòng thật, chớp nòng và va chạm | Miễn phí; tổng 35 lên một mục tiêu; +22 Energy, +18 Burst |
| 2 | Anti-Ship Blade Assault | Lao tới, hai nhịp chém bằng đại kiếm trên rig; vệt sáng theo đầu kiếm; trở về ô đứng | 25 Energy; tổng 65 lên một mục tiêu; +24 Burst |
| 3 | Ootori Missile Barrage | Lộn người, phóng 3 đợt tên lửa; đạn có thân, đường khói và quỹ đạo cong tới từng mục tiêu | 40 Energy; tổng 43 lên mỗi địch còn sống; +30 Burst; chờ hai lượt mình tiếp theo |
| 4 | Full Armament Burst | Tích năng lượng ở core; clip gốc `O-03-LAUNCHER` triển khai pháo; railgun vàng, tên lửa và beam launcher hồng có lõi trắng | 100% Burst; tổng 120 lên mục tiêu chính, 60 lên mỗi địch khác |

Các thông số là cấu hình thử nghiệm. Ulti sử dụng một khung cinematic toàn màn hình: cận Strike Rouge khi nạp năng lượng, góc bắn ngang, rồi cảnh rộng khi mục tiêu trúng đòn. Trong phần cận nhân vật, sân, UI combat và enemy được tạm ẩn để không che nhân vật. Các trạng thái hiển thị được phục hồi khi chuyển cảnh, kết thúc hoặc reset. Đây là cinematic dựng bằng code, chưa phải một Timeline asset riêng.

## Xoay và zoom góc nhìn

Trong **sân ảo**:

- **Giữ chuột phải và kéo:** xoay quanh sân.
- **Cuộn chuột:** tiến gần hoặc lùi xa.
- **F:** trả về góc nhìn ban đầu, không reset trận.
- Trên điện thoại: **kéo một ngón** để xoay, **véo hai ngón** để zoom.
- Cử chỉ bắt đầu trên nút/UI sẽ không điều khiển camera. Camera có giới hạn độ nghiêng và khoảng cách.

Trong **AR**, di chuyển điện thoại để đổi góc nhìn; các cử chỉ camera sân ảo được tắt. Ulti dùng một camera phụ xuất ra khung cinematic, trong khi camera AR và tracking vẫn giữ nguyên. Sân không bị chuyển chỗ để diễn ulti.

Trong lúc ulti chạy, camera tự do tạm khóa. Kết thúc hoặc Reset sẽ phục hồi đúng góc người chơi đang xem trước ulti; không ép về góc mặc định.

## Phòng thủ và nút thử nghiệm

- **Q / Parry:** phản ứng trong 0,20 giây cuối; chặn 85% sát thương. Trong 0,09 giây cuối là Perfect Parry: không mất HP, phản 18 sát thương.
- **E / Dodge:** phản ứng trong 0,40 giây cuối; nhân vật né sang bên và không mất HP.
- Mỗi đòn địch chỉ có một lượt phản ứng. Bấm sớm sẽ tiêu lượt đó.
- **TEST Charge Ult:** nạp Burst lên 100% trong lượt mình để xem ulti ngay.
- **RESET:** dừng chiêu đang chạy, thu hồi VFX, trả nhân vật về idle và phục hồi trận. Có nút RESET riêng trên overlay cinematic.
- Có thể tắt âm thanh ở `Battle Controller → Rouge Skill Presentation → Sound Enabled` trước khi Play.

Mất tracking AR sẽ tạm dừng đồng hồ combat, animation và VFX. Tọa độ đường lao chém, đạn và hiệu ứng đều theo sân; độ dày đường sáng được điều chỉnh theo tỷ lệ sân AR.

## Animation thực sự được sử dụng

Model có sáu clip nhúng trong GLB: `O-00-POSE`, `O-01-APPER`, `O-02-IDLE`, `O-03-LAUNCHER`, `O-04-SEPARATION`, `O-99-POSE2`.

- Dùng trực tiếp ba clip gốc: `APPER`, `IDLE`, `LAUNCHER`.
- Giữ nguyên `SEPARATION` để tham khảo/tái sử dụng; chưa buộc animation tách backpack vào chuỗi chiêu lặp.
- Tạo thêm năm clip Generic trên cùng rig: `Rouge_RifleBurst`, `Rouge_AntiShipBlade`, `Rouge_MissileBackflip`, `Rouge_Guard`, `Rouge_Dodge`.
- Các clip bổ sung nằm tại `Assets/Game/Features/Combat/Animations/StrikeRouge`. Những đường cong không đổi được rút về hai keyframe.
- `RougeAnimationDriver` dùng PlayableGraph để chuyển giữa clip gốc và clip bổ sung, đồng thời dừng đúng thời gian khi tracking bị gián đoạn.
- Giữ nguyên `.glb`, `.fbx` và `New Animator Controller.controller` trong thư mục Models. Bản sao scene trước nâng cấp nằm trong `Assets/Sandbox/StrikeRougeCombat/PreservedScenes`.

## Code

| File | Trách nhiệm |
|---|---|
| `RougeBattleDemo.cs` | Lượt, tài nguyên, sát thương, parry/dodge và HUD |
| `RougeAnimationDriver.cs` | Phát và trộn animation trên rig gốc |
| `RougeSkillPresentation.cs` | Các chuỗi skill, muzzle/kiếm, VFX, âm thanh và camera ulti |
| `RougeArenaCamera.cs` | Xoay, zoom, reset góc nhìn sân ảo; chặn cử chỉ trên UI và trong cinematic/AR |
| `RougeCinematicDirector.cs` | Camera cinematic riêng, khung hình toàn nhân vật, ẩn/khôi phục enemy và HUD |
| `RougeArenaPlacement.cs` | Đặt sân AR, chuyển sân ảo và thông báo tracking |
| `RougeCombatAuthoring.cs` | Tạo clip bổ sung, gắn socket và nâng cấp scene trong Editor |
| `RougeCombatValidation.cs` | Kiểm thử tích hợp bằng các hành động công khai giống HUD |
| `RougeSkillSetup.cs` | Audit clip gốc và tạo ảnh xem tư thế |

Các script runtime nằm trong `Assets/Game/Features/Combat/Scripts/StrikeRougeDemo`; công cụ Editor nằm trong `Assets/Tools/Editor/StrikeRougeDemo`. Shader VFX là `Assets/Game/Features/Combat/Shaders/ARSkillGlow.shader`.

## Kiểm thử

Menu **Gundam → Strike Rouge → Validate Combat Demo** tự vào Play, chạy thử ba chiêu và ulti, phòng thủ, cooldown, thắng/thua, sân bị xoay/thu nhỏ/di chuyển, tạm dừng tracking và reset trong ulti. Phần camera kiểm tra thêm khung hình nhân vật, enemy/UI phục hồi, góc nhìn trước ulti, kéo chuột, wheel, kéo một ngón và pinch bằng thiết bị Input System mô phỏng. Công cụ dừng Play sau khi hoàn thành.

- Kết quả: `Docs/Reports/StrikeRougeCombatValidation.txt`.
- Ảnh bốn chiêu, audit clip và bảng ghép animation: `Docs/Reports/StrikeRougeSkills`.
- Ảnh mở đầu ulti tập trung vào nhân vật: `Docs/Reports/StrikeRougeSkills/05_UltimateCharacterFocus.png`.
- Ảnh sân sau reset: `Docs/Reports/StrikeRougeCombatDemo.png`.

Kiểm thử Editor sử dụng sân ảo và mô phỏng việc tạm dừng tracking bằng giao diện điều khiển combat. Cần thử thêm trên thiết bị AR tương thích để xác nhận camera/tracking và hiệu năng thật. Bản nâng cấp này không tự thay đổi Build Settings hoặc cài APK lên điện thoại.

## Video tham khảo thiết kế

- Strike Rouge SEED: https://www.youtube.com/watch?v=4K9XZ_6IOUw
- Ootori trong SEED DESTINY HD Remaster tập 39: https://www.youtube.com/watch?v=gMJaDX0P18w&t=1040s
- Demo IWSP và Ootori trong Gundam Breaker 3; phần Ootori khoảng 3:20: https://www.youtube.com/watch?v=f1dKAN6_cLc&t=200s

Tên chiêu và việc chia thành ba chiêu + ulti là cách chuyển thể riêng của project. Animation game tham khảo không được tải về hoặc đưa vào project; các clip combat bổ sung được tạo cho chính skeleton hiện có.
