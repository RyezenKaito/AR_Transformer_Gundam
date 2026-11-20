# Chiến đấu — giao diện chọn chế độ

Mở HomeLobby và bấm CHIẾN ĐẤU hoặc XUẤT KÍCH. Cột phải có 5 chế độ tiếng Việt, không có danh sách Missions. Chọn thẻ để đổi nội dung; chọn nhãn trên bản đồ để đổi địa điểm; kéo vùng bản đồ để xoay hologram. Đổi mức độ Tiêu chuẩn/Nâng cao và bấm Xuất kích để xem phản hồi mẫu. Chưa tạo màn chiến đấu, tiêu hao tài nguyên, ghép trận hoặc phòng mạng.

Phòng nghiên cứu, Esc và Quay lại trả camera về lobby. Kho đồ mở màn kho hiện có. Đổi Gundam mở roster và tự quay lại màn Chiến đấu khi chọn xong.

Battle/Data/BattleModeCatalog.asset: chỉnh tên, mô tả, cấp khuyến nghị, năng lượng và địa điểm. Các dữ liệu này là mẫu thiết kế theo lượt. Battle Preparation Systems: các liên kết UI, stage và model mount. Model xem trước lấy Gundam đang trang bị. Prefab BattleModeCard tách riêng; header dùng CommandNavigationBar hiện có. Địa hình hologram là mesh 3D và shader Unlit riêng, UI chữ/nút/panel tách rời có thể sửa. Không đổi render pipeline hay cài plugin trả phí.
