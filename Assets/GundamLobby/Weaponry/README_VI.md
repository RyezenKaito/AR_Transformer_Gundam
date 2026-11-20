# Home Dashboard + Weaponry

Thanh điều hướng dùng chung prefab `Weaponry/Prefabs/CommandNavigationBar.prefab`, gồm bốn mục: PHÒNG NGHIÊN CỨU, CHIẾN ĐẤU, KHO ĐỒ, CỬA HÀNG. PHÒNG NGHIÊN CỨU là cảnh hangar/lobby. Bấm mục này từ Kho đồ hoặc Vũ khí sẽ đóng màn đang mở và khôi phục camera, Gundam ở lobby. Mục hiện tại có nền cyan. Nhãn vật phẩm, nhóm lọc, sắp xếp, giá, thao tác, vai trò Gundam và thông báo đã Việt hóa. Tên riêng Gundam và mã kỹ thuật được giữ nguyên.

Mở Assets/GundamLobby/Scenes/HomeLobby.unity và Play. Home theo sơ đồ: Hồ sơ trái trên, bốn menu phía trên, sự kiện trái, Gundam giữa, thông tin và Xuất kích bên phải. Nút VŨ KHÍ trong khối thông tin Gundam mở màn kỹ năng. Đổi Gundam mở danh sách model thật. CHIẾN ĐẤU hoặc XUẤT KÍCH ở lobby mở màn chuẩn bị với bàn bản đồ hologram và 5 chế độ tiếng Việt. Nút Xuất kích bên trong màn chuẩn bị dùng phản hồi mẫu, chưa khởi chạy chiến đấu. CỬA HÀNG mở chợ vật phẩm với tìm kiếm tiếng Việt, danh mục, lọc độ hiếm, sắp xếp, phân trang và popup chi tiết. Mua/Đặt giá dùng phản hồi mẫu, không trừ tiền; sự kiện vẫn dùng phản hồi mẫu.

Weaponry: 5 thẻ đổi nội dung, kéo vùng giữa xoay model, cuộn zoom; Xem minh họa nháy sơ đồ năng lượng; Esc/Quay lại hoặc Phòng nghiên cứu trả camera và model về lobby. Preview dùng Gundam đang trang bị. Nút Đổi Gundam mở danh sách model; Kho đồ mở inventory. Chưa có chiến đấu hoặc tiêu hao tài nguyên.

Dữ liệu mẫu ở Weaponry/Data/WeaponryCatalog.asset. Các chỉ số và kỹ năng dùng cho prototype theo lượt, chưa phải dữ liệu cân bằng. Home lấy HP/ATK/DEF/SPD từ StrikerCatalog.asset.

Chọn Home Dashboard and Weaponry Systems để đổi Player Avatar, Commander Level, Commander Progress, Resource Amounts, sprite sự kiện và tiêu đề sự kiện. Avatar hồ sơ độc lập với Gundam đang trang bị; mặc định dùng ảnh chân dung vuông có sẵn của project. Ba ô tài nguyên dùng icon vector riêng, nhãn và số lượng; nút cộng có phản hồi mẫu. Cấp, tiến độ và số dư là dữ liệu hiển thị mẫu.

Prefabs đang dùng: CommandNavigationBar, HomeEventCard, HomeGundamStats, WeaponrySkillCard, WeaponryStatRow. Chữ, nút, panel, icon là uGUI/TMP/vector có thể sửa riêng. Các bản prefab/header cũ được giữ làm lịch sử; scene dùng CommandNavigationBar mới. Bản scene trước sửa ở GundamLobby/Backups. Không đổi pipeline, chất lượng render hoặc môi trường.
