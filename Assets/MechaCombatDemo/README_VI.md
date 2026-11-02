# Gundam Combat Demo — cách chạy

Đã triển khai trực tiếp trong project `E:/My project`, giữ nguyên OBJ, prefab và scene gốc.

## Mở và điều khiển

Mở `E:/My project/Assets/MechaCombatDemo/Scenes/MechaCombatTest.unity`, nhấn **Play** rồi click Game View.

| Phím | Chức năng |
| --- | --- |
| WASD | Chạy theo camera |
| Alt giữ | Đi bộ |
| Shift giữ | Boost |
| Ctrl | Dash |
| Space | Nhảy; giữ trên không để hover |
| C | Hạ độ cao |
| Chuột | Xoay camera |
| Chuột trái | Combo kiếm; nhấn nối trong cửa sổ combo |
| Chuột phải giữ | Bắn năng lượng |
| Chuột giữa | Khóa/bỏ khóa mục tiêu |
| Q | Lao chém |
| E | Charge rồi bắn beam |
| R | Celestial Breaker, cần target hợp lệ trong 10 m và đủ 80 energy |
| Esc | Pause/resume |
| F1 | Hiện/ẩn hướng dẫn |
| F5 | Reset HP, energy, cooldown và sân thử |

Dummy ở phía trước để thử hit. Đối thủ proxy nằm bên phải phía xa; tiến gần để kích hoạt. Trên sân có tường, góc hẹp, bậc, dốc và trần thấp để kiểm va chạm.

## Đã có trong project

- Rig cơ khí 26 joint điều khiển đủ 375 chi tiết, robot thử cao 2,6 m; không làm mềm tấm giáp.
- Mapping part, rest pose, giáp vai/hông độc lập, socket tay/kiếm/súng/nozzle.
- 37 `.anim`, AnimatorController, mask thân trên, clip sheets và công cụ dựng lại.
- Di chuyển, start/stop/turn/boost/dash, nhảy/hover/rơi/land.
- Combo ba đòn, hit sweep theo đường kiếm, chống hit lặp qua nhiều collider.
- Súng hitscan, Q lao chém, E beam, R điện ảnh 4 giây.
- Energy, cooldown, damage, hit stop, target lock, pause/reset, camera collision.
- Native particle/trail/ribbon/beam/ring và âm thanh tổng hợp.
- Dummy, AI sparring đơn giản, HUD thử nghiệm.

## File chỉnh sửa chính

Các đường dẫn dưới nằm trong `E:/My project/Assets/MechaCombatDemo/`:

- `CombatTuning.asset`: tốc độ, năng lượng, cooldown, damage và các cửa sổ action.
- `Animations/`: 37 clip keyframe; `MechaDemo.controller`, `UpperBody.mask` và `AnimationClipSheets.csv` nằm ngay thư mục gốc demo.
- `Scripts/Runtime/`: gameplay, rig data, camera, VFX, health và AI.
- `Scripts/Editor/`: rig builder, clip builder, scene builder, validator.
- `Reports/`: mapping, nghiệm thu rig/animation/gameplay và kết quả đo.
- `Unicorn_CombatRig.prefab`: bản rig nền.
- `Unicorn_CombatPlayer.prefab`: bản người chơi có vũ khí/controller.
- `Scenes/MechaRigValidation.unity`: scene kiểm riêng khớp.

Menu **Mecha Demo → Build Combat Test Scene** tạo lại scene từ các công cụ. Công cụ cập nhật asset do demo quản lý; hãy sao lưu hoặc duplicate clip đã chỉnh tay trước khi build lại. Scene đang sửa được lưu bản snapshot riêng nếu có thay đổi chưa lưu.

## Mức hoàn thiện và giới hạn

Đây là bản demo chơi được đầu tiên với animation keyframe tạo bằng công cụ và đã kiểm tra trong Unity. Bộ này chưa phải motion capture hay animation thương mại được animator chỉnh từng frame.

Vũ khí, dummy, đối thủ và âm thanh là asset prototype. Ngón tay giữ nguyên geometry cứng ở pose gốc; chưa có rig ngón riêng. Súng dùng hitscan có kiểm vật cản từ muzzle. AI dùng collision và wall-slide cục bộ, chưa có NavMesh tìm đường toàn sân. HUD dùng IMGUI phục vụ demo nên còn allocation chuỗi; không tuyên bố zero-GC.

Pose rộng, tiếp đất trên địa hình phức tạp và các góc camera cực trị vẫn cần thêm biên tập mỹ thuật. Các số liệu kiểm tra cụ thể nằm trong báo cáo; những bài chưa đo không được coi là đã đạt.

