# Strike Rouge Ootori combat demo

Mở **Gundam > Strike Rouge > Open Combat Demo**, chọn tab **Game** rồi nhấn **Play**. Scene thử nghiệm là `Assets/Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity`.

HUD hiện dùng prefab Mecha với chân dung, HP và Armor, năm ô Burst, timeline theo Speed, bốn ô vũ khí và nút EXECUTE. Chọn chiêu hoặc phím 1 đến 4 chỉ xem trước; bấm EXECUTE hoặc Enter mới thực hiện. Q là Parry, E là Dodge. Các thao tác chỉ có hiệu lực trong đúng giai đoạn của trận.

**Hướng dẫn hiện tại:** [Strike Rouge Mecha HUD](StrikeRougeMechaHUD.md). Tài liệu này mô tả chi phí 0, 1, 2, 5; hồi Burst; giáp; luật Dodge và Parry; camera; các công cụ dựng prefab và kiểm thử.

## Bộ skill và animation

| Chiêu | Animation và hiệu ứng | Chi phí Burst |
|---|---|---|
| Beam Rifle Burst | Ngắm mục tiêu, ba tia beam và hiệu ứng đầu nòng | 0 và hồi 1 ô khi hoàn thành |
| Anti Ship Blade | Lao tới, hai nhịp chém, vệt kiếm rồi trở về vị trí | 1 ô |
| Ootori Missile Barrage | Lộn người và phóng chín tên lửa theo quỹ đạo cong vào các địch | 2 ô |
| Full Burst | Tích năng lượng, triển khai pháo, railgun và beam launcher; cinematic tập trung vào nhân vật | 5 ô |

Mỗi lượt của người chơi hoàn tất hồi thêm một ô Burst, tối đa năm. Bắt đầu trận với hai ô. Không còn thanh Energy 100 hoặc thanh Burst phần trăm của bản demo trước nâng cấp HUD.

Model có sáu clip nhúng: O-00-POSE, O-01-APPER, O-02-IDLE, O-03-LAUNCHER, O-04-SEPARATION, O-99-POSE2. Bản demo sử dụng trực tiếp APPER, IDLE và LAUNCHER. Năm clip Generic bổ sung cho rifle, đại kiếm, barrage, guard và dodge nằm trong `Assets/Game/Features/Combat/Animations/StrikeRouge`. Clip SEPARATION được giữ để nghiên cứu sau.

RougeAnimationDriver dùng PlayableGraph để phát và trộn clip trên rig gốc. Khi mất tracking, đồng hồ combat, animation và các hiệu ứng tạm dừng. Trong sân ảo, giữ chuột phải để xoay, cuộn để zoom và F để trả góc ban đầu. Ulti dùng camera cinematic riêng và khôi phục góc đang xem sau khi kết thúc hoặc Reset.

## Kiểm thử và tài nguyên

- Công cụ: **Gundam > Strike Rouge > Validate Mecha HUD**.
- Kiểm tra bố cục riêng: **Gundam > Strike Rouge > Validate HUD Layouts**.
- Báo cáo hiện tại và ảnh: `Docs/Reports/StrikeRougeHUD`.
- Tài nguyên UI và prefab: `Assets/Game/Features/Combat/UI/StrikeRouge`.
- Code runtime: `Assets/Game/Features/Combat/Scripts/StrikeRougeDemo`.
- Công cụ Editor: `Assets/Tools/Editor/StrikeRougeDemo`.
- Shader skill: `Assets/Game/Features/Combat/Shaders/ARSkillGlow.shader`.

Các báo cáo trong `Docs/Reports/StrikeRougeSkills` và file StrikeRougeCombatValidation cũ phản ánh phiên bản trước; không dùng kết quả đó để suy ra chi phí hoặc luật phòng thủ hiện tại. Các model nguồn không được sửa hoặc unpack trong lần nâng cấp UI này.

Kiểm tra Editor sử dụng sân ảo và mô phỏng độ phân giải, safe area, tạm dừng tracking. Chưa thay thế được kiểm tra tracking và hiệu năng Android hoặc iOS trên thiết bị thật.
