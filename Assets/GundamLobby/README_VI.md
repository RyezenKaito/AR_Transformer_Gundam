# Gundam Home Lobby

Mở `Assets/GundamLobby/Scenes/HomeLobby.unity`, chọn Game rồi nhấn Play. Đây chỉ là giao diện trang chủ; các nút hiện thông báo mẫu. Kéo chuột tại vùng trống ở giữa để xoay Gundam.

## Thay asset
Chọn `Lobby Systems` và component `HomeLobbyController`:
- `Gundam Prefab`: kéo prefab Gundam thật vào. Prefab dùng đơn vị mét, chân ở y=0, cao khoảng 8.8 đơn vị, mặt hướng -Z. Placeholder tự ẩn khi Play.
- `Hangar Prefab`: thay nhà chứa nếu muốn; placeholder tự ẩn khi Play.
- `Avatar Sprite`, `Banner Sprite`, `Skill Sprites`: kéo sprite tương ứng vào các trường Inspector.
- Chỉnh tên, cấp và bốn chỉ số ở Sample Data.
- Thumbnail mẫu ở Textures và các RawImage trong prefab Team Strip, Header, Activities có thể thay trực tiếp. Ba thumbnail đội hình hiện dùng cùng một model placeholder, không đại diện ba asset Gundam riêng.

## Cấu trúc
Runtime chứa controller, hover và hình học HUD. Editor chứa công cụ dựng scene (Tools > Gundam Lobby > Create Home Lobby, chỉ dùng khi chưa có scene). Prefabs chứa Header, Footer, Gundam Information, Activities, Team Strip, PlaceholderGundam và PlaceholderHangar. UI là uGUI/TextMeshPro có thể sửa trực tiếp, không phải ảnh nền chụp màn hình.

## Giới hạn
Gundam, avatar, banner và hangar là placeholder hình học gốc dựng trong project; cần model và artwork thật để đạt chi tiết như ảnh tham chiếu. Chưa có gameplay, inventory, nâng cấp hay scene phụ. Pipeline URP hiện có không bị đổi. Scene AR cũ có thay đổi chưa lưu được chụp bản sao vào Backups trước khi tạo lobby; file scene AR gốc không bị ghi đè. Build Settings của project không bị thay đổi.
