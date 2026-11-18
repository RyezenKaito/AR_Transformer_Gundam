# Giao diện lobby inventory và chọn Gundam

Mở `Assets/Game/Scenes/HomeLobby.unity`, chọn Game và nhấn Play. HomeLobby được đưa lên đầu Build Settings; các scene demo trước đó vẫn được giữ.

UI được tích hợp từ branch `duy`, commit `e74c18a`, tại `Clone/UI`. Đây là project Unity có lịch sử riêng. Merge giữ nguồn gốc commit và nhập cây `Assets/GundamLobby` cùng phụ thuộc giao diện, không nhập các bài lab, cấu hình AR hay package cũ của project nguồn. Nhánh `backup/pre-ui-merge-20261102` lưu trạng thái trước tích hợp. `Clone` được loại khỏi Git để không thêm repo lồng nhau khi commit.

## Model và animation

Catalog UI vẫn có mười mục. Các mục có chuyển động được thay prefab bake tĩnh bằng prefab giữ cấu trúc rig. Chân dung và bố cục của thành viên làm UI được giữ.

| Mục chọn | Chuyển động dùng trong preview |
|---|---|
| Unicorn | UnicornMotion có sẵn, trạng thái idle; không gọi hiệu ứng hạ cánh hoặc chiến đấu |
| Justice | Clip nguồn 00-IDLE |
| Extreme | Clip nguồn Idle |
| Strike Rouge | Clip nguồn O-02-IDLE |
| RX-78 rifle | Clip nguồn G01-RIFLE, phát chậm để trình diễn vũ khí |
| RX-78 saber | Clip nguồn G02-SABER, phát chậm để trình diễn vũ khí |

BlueFlame, HaoLing, SkyDefender và Kamiki giữ model trình bày đang có. Chưa tạo mới bộ animation cho các model không có clip phù hợp trong lần tích hợp này.

Prefab animated nằm trong `Assets/Game/Features/MainMenu/Presentation/Prefabs`. `LobbyPreviewAnimation` dùng PlayableGraph của rig gốc, tắt root motion và tự giải phóng graph khi preview bị tắt hoặc thay. Model nguồn trong `Assets/Models` không bị chỉnh sửa. Các asset trùng GUID được dùng lại từ thư viện chính.

Trong màn chọn Gundam, nhấn card để xem, CONFIRM STRIKER để áp dụng, RETURN để hủy lựa chọn đang xem. Gundam đã xác nhận được dùng cho cả lobby và inventory. Preview inventory tiếp tục dùng RenderTexture và layer 30; kéo để xoay, cuộn để zoom. Các nút USE/mua bán vẫn là phản hồi mẫu của bản UI.

## Phần dữ liệu để thảo luận tiếp

Các chỉ số catalog và giao diện hiện là dữ liệu trình bày của thành viên thiết kế UI, chưa phải nguồn chỉ số combat. Chưa thêm database, cloud, nâng cấp hoặc thay công thức chiến đấu. Bước tiếp theo cần thống nhất nơi lưu chỉ số gốc, chỉ số sau trang bị và trạng thái trận trước khi nối vào biểu đồ mạng nhện.

## Kiểm tra và công cụ

- `Gundam > Lobby > Integrate Animated Previews`: cập nhật prefab animated và các tham chiếu preview trong scene hiện tại.
- `Gundam > Lobby > Validate UI Integration`: chạy Play Mode để kiểm tra lobby, inventory, roster, chuyển động của sáu mục chọn và confirm/cancel.
- Kết quả, mapping và ảnh thực nghiệm nằm tại `Docs/Reports/UIIntegration`. Kiểm thử Editor không thay thế kiểm thử mobile/AR trên điện thoại.
