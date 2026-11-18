# Chapter 1 — Hai phương án map chờ duyệt

GDD mục 4.1 đặt ba địch thường ở cả hai màn. Màn 1-1 tập trung lượt, chọn mục tiêu và chiêu cơ bản. Màn 1-2 dạy quản lý Armor/Burst. GDD để chủ đề cốt truyện và tên màn cho nhóm chốt. Các cấu hình bên dưới là đề xuất, chưa dựng level.

## A — Desert Outpost → Street City

| Màn | Asset đã clone | Bố cục và luồng |
| --- | --- | --- |
| 1-1 | `Clone/MAP/Assets/Environments/Desert/Scenes/Desert_Battle.unity` | Khoảng mở giữa các thùng tiếp tế và cụm đá. Bố trí ba Part 1 với góc bắn thoáng. Người chơi lần lượt nhận cue khóa mục tiêu và chọn địch. Dùng một điểm đặt sân, không cần lái Gundam. |
| 1-2 | `Clone/MAP/Assets/Environments/StreetCity/Scenes/StreetCity_Battle.unity` | Dùng mặt đường và hai lối vòng mép phố. Hai Part 1 giữ tuyến xa, Part 2 áp sát qua lối bên. Từ checkpoint triển khai đến điểm kết trận tập trung vào phá Armor và ưu tiên mục tiêu. |

Nhịp mở từ trạm tiếp tế tới phố. Cần giữ lane bên đủ rộng cho Part 2 và đặt vật cản ngoài vùng nhìn chính.

## B — Sci-Fi Warehouse → Biogear Lab

| Màn | Asset đã clone | Bố cục và luồng |
| --- | --- | --- |
| 1-1 | `Clone/MAP/Assets/SciFi Warehouse Kit/Demo/Scene/SciFi_Warehouse.unity` | Dùng sàn trung tâm làm khu huấn luyện, đưa kệ và thùng ra mép. Ba Part 1 có line-of-sight dễ đọc. Người chơi triển khai ở cửa kho và chọn mục tiêu từ một vị trí. |
| 1-2 | `Clone/MAP/Assets/BiogearLab/Prefabs/BiogearLab_Environment.prefab` và scene `BiogearLab_Reconstruction.unity` | Dùng hành lang và các cụm cover để tách lane. Đề xuất một Part 1 ở cuối trục nhìn, hai Part 2 tại hai cửa bên. Đặt đội ở đầu phòng, chọn mục tiêu rồi qua checkpoint kết trận. |

Nhịp mở từ kho tới cơ sở nghiên cứu. Hành lang có nhiều vật cản nên cần kiểm thử corner camera, khoảng thoát hướng đâm và NavMesh sau khi chọn map.

Cả hai giữ 1-1 và 1-2 là màn quái thường. Thành phần và bố trí đề xuất có thể được cân lại sau test. Hai màn dùng ba địch theo GDD, điều kiện mở màn tiếp theo là thắng màn trước. Chờ chốt phương án trước khi import môi trường và dựng level.
