# SWITCH BREAK STRIKER

Mở HomeLobby, Play, bấm SWITCH BREAK STRIKER. Bấm thẻ để xem model thật, kéo vùng giữa để xoay và cuộn để zoom. Enter hoặc CONFIRM STRIKER xác nhận; Esc/RETURN hủy xem và quay lại. Lựa chọn được lưu cục bộ bằng PlayerPrefs (GundamLobby.ActiveStriker), áp dụng cho robot ở lobby và preview inventory.

Catalog: Assets/GundamLobby/Striker/StrikerCatalog.asset — thay model, portrait, tên, vai trò, cấp và chỉ số trong Inspector. 10 model được chuẩn hóa chiều cao tối đa 8.66 và ngang tối đa 8.8 để vừa hangar. Prefab roster và stat row có thể sửa riêng. Canvas uGUI/TMP 1920x1080; không dùng ảnh toàn giao diện.

Giá trị và vai trò là dữ liệu mẫu cho game theo lượt. 5 GLB được đưa sang prefab hình học ở tư thế animation; chưa chuyển toàn bộ animation chiến đấu. SkyDefender, BlueFlame, HaoLing được gom mesh theo material để giảm draw calls. Model nguồn giữ nguyên ở D:/AR_Transformer_Gundam-main/Assets/Models. Không đổi render pipeline và giữ Mobile URP Soft Shadows để tránh lỗi đốm trắng Intel.
