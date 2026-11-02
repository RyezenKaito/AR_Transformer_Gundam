# Thêm và chọn môi trường

Mở menu **Tools → Environment System → Open Environment Hub**, rồi nhấn Play.
Trong scene battle và các map đã đăng ký có nút **CHỌN MÔI TRƯỜNG** ở góc trên phải để về hub.

Catalog hiện có 10 môi trường: Sàn tập, Sci-fi Hangar, Sphere City, Biogear Lab, Warehouse, Street City, Volcanic Caldera, Desert Outpost, Sunken Harbor và Orbital Frontier. Hai map blockout Đảo biển/Sa mạc — MẪU đã được xóa. Ba lựa chọn đầu dùng các variant của BattleEnvironmentSwitcher; các chiến trường khác có scene riêng.

## Thêm map mới

1. Tạo scene trong `Assets/Environments/<ID>/Scenes/`. Có một camera và một AudioListener hoạt động; thêm ánh sáng, geometry, collider và controller nếu cần.
2. Create → Environments → Environment Definition. Đặt ID duy nhất, tên và mô tả. Gán SceneAsset vào trường Scene, đặt Available=true và Battle Variant=-1 cho map độc lập.
3. Có thể gán thumbnail; nếu để trống UI dùng màu Accent. Thumbnail không bắt buộc.
4. Mở `Assets/EnvironmentSystem/Resources/EnvironmentCatalog.asset`, thêm definition vào Environments. Thứ tự array là thứ tự card.
5. Chạy **Tools → Environment System → Register Catalog Scenes in Build**, rồi **Validate Catalog**. Khi dùng Build Profile có override danh sách scene, thêm hub và các scene catalog vào override đó. Không cần sửa selector.
6. Mở hub, Play và chọn map mới. Navigator tự hiện nút quay về hub ở scene được đăng ký, không cần thêm navigator thủ công.

Khi rename/move scene bằng Unity Editor, tham chiếu SceneAsset giữ GUID và editor postprocessor cập nhật đường dẫn runtime. Sau khi đổi, chạy Register Catalog Scenes in Build và Validate Catalog trước khi build.

## Luồng hoạt động

Navigator được tạo tự động trong Play Mode nếu scene khởi đầu là hub hoặc một scene trong catalog. Nó tồn tại qua scene change, dùng LoadSceneAsync chế độ Single và chặn tải lặp. Không tạo thêm Camera, AudioListener hoặc EventSystem. UI dùng IMGUI để độc lập với EventSystem của scene battle.

Đổi scene sẽ bắt đầu lại battle. Nút **ĐỔI MÔI TRƯỜNG** sẵn có trong battle vẫn đổi phông nền tại chỗ và giữ HP/lượt/camera. Hai thao tác phục vụ hai nhu cầu khác nhau, được ghi rõ trong hub.

Last selected ID lưu ở PlayerPrefs `EnvironmentSystem.LastSelection`; ID không tồn tại thì không card nào đánh dấu Mở lại, các map hợp lệ vẫn dùng được. Đây chỉ là sở thích lựa chọn, không lưu trận đấu.

## Khởi tạo Hub

Lệnh Tools → Environment System → Install Environment Hub tạo Hub nếu cần và giữ các definition đã có; không tạo lại hai scene mẫu đã xóa. Muốn thêm chiến trường, tạo scene/definition mới theo hướng dẫn phía trên.

## Kiểm tra

Trong hub Play Mode: Tools → Environment System → Verify Switching in Play Mode. Báo cáo ghi trong `Documentation/Verification.md`. Bài kiểm tra đọc catalog và mở mọi lựa chọn available, quay về hub, kiểm tra đúng variant, chống tải lặp, một scene/một navigator/một camera/một audio listener, ID không hợp lệ và scene thiếu. Nó khởi tạo lại battle trong lúc kiểm tra.

Đã chọn workflow tải bất đồng bộ theo [Unity SceneManager.LoadSceneAsync](https://docs.unity3d.com/jp/current/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html). Chưa xác nhận standalone build hoặc thiết bị Android; kiểm tra build profile và input trên thiết bị đích trước khi phát hành.

