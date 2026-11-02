# PHÂN TÍCH REFERENCE VÀ TIÊU CHÍ ĐỐI CHIẾU

## Phạm vi đã xem

Đã xem 10 ảnh đính kèm và 25 khung hình lấy mẫu mỗi 6 giây, từ 00:03 đến 02:27, của video dài 154,36 giây, 1920×1080, 25 fps. Đã mở riêng bản lớn tại 00:09, 01:09, 01:45 và 02:09. Đây là khảo sát hình ảnh lấy mẫu, chưa phải phân tích mọi frame hoặc phiên âm lời nói trong video.

Unity MCP đã thực thi thành công một truy vấn chỉ đọc: Unity 6000.5.8f1, URP, project `E:/My project`, active scene `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity`, không ở Play Mode và không compiling tại thời điểm truy vấn. Manifest ghi URP 17.5.0, AI Navigation 2.0.14, Input System 1.20.0.

Trong lần làm việc này chỉ tạo bộ tài liệu/prompt và ảnh trích video; chưa dựng hoặc sửa scene chiến trường.

## Danh mục 10 ảnh

Tất cả file ảnh gốc nằm tại `D:/Downloads/`. ID dưới đây khớp thứ tự ảnh trong tin nhắn người dùng.

| ID | Tên file | Dùng để xác định | Chưa thể suy ra chắc chắn |
|---|---|---|---|
| R01 | vadym-krukovets-locationplariummecharenatropical06.webp | Tiết diện hành lang, ramp, rãnh sàn, sọc vàng, đèn trần và fixture nghiêng | Chiều dài toàn hành lang, cao/rộng thực, đầu còn lại nối chính xác tới đâu |
| R02 | vadym-krukovets-locationplariummecharenatropical04.webp | Nội thất vòm, đỉnh hở, bệ giữa, sườn mái, bể kính, nhãn GATE03 | Toàn bộ cửa vòng quanh và mặt khuất phía camera |
| R03 | vadym-krukovets-07-v3.webp | Phong cách texture ocean/sand/grass/metal panel | File texture gốc, map PBR, kích thước tile và cách pack channel |
| R04 | vadym-krukovets-locationplariummecharenatropical02.webp | Vòm ngoài, nước, hình bệ vuông, vị trí cửa, vật chắn và đá nền | Toàn tuyến bệ và cao độ tuyệt đối chỉ từ ảnh này |
| R05 | vadym-krukovets-locationplariummecharena-005.webp | Tháp, reactor chân tháp, liên hệ đĩa/cầu với đá, cổng có số | Topology mặt sau và đường bên trong tháp |
| R06 | vadym-krukovets-04.webp | Lòng chảo, vòng đi bộ, pod nghiêng, cover, mặt dưới đĩa và cửa tầng dưới | Số pod toàn vòng, toàn layout ngoài khung |
| R07 | vadym-krukovets-locationplariummecharenatropical.webp | Toàn cảnh, tỷ trọng sky/kiến trúc/đá, tháp, vòm, hub, biển, vòm đá | Bản đồ orthographic và những tuyến bị che |
| R08 | vadym-krukovets-locationplariummecharena-003.webp | Model sheet tháp, reactor, cầu đôi, trụ, đá kèm và wireframe | Kích thước mét, vật liệu gốc và hidden topology đầy đủ |
| R09 | vadym-krukovets-locationplariummecharenatropical03.webp | Hub nhìn gần, đĩa phía trên, tháp sau, nẹp và các loại cover | Tỷ lệ đĩa/hub thật nếu chưa giải camera perspective |
| R10 | vadym-krukovets-locationplariummecharena-002 (1).webp | Model sheet đĩa, lỗ, lan can, cánh nghiêng, hai nhánh cầu, chữ EVERLIFE | Transform lắp trong scene và mặt dưới hoàn chỉnh |

Lưu ý: R08/R10 có ký hiệu tam giác kèm số 2037/1364, nhiều khả năng là thống kê mesh được tác giả trình bày. Không dùng chúng làm tổng polygon budget cho map hoặc khẳng định mọi instance có đúng số tam giác đó.

## Các mốc video hữu ích

Video gốc: `D:/Downloads/YTSave.to_YouTube_Media__iGI-b0dEvY_Making-of-Biogear-Lab-Map-Mech-Arena_001_1080p.mp4`.

| Mốc lấy mẫu | Nội dung nhìn thấy | Giá trị cho quá trình dựng |
|---|---|---|
| 00:09, 00:15 | Toàn map từ trên cao nằm trong khung chương trình, cạnh người thuyết trình | Đọc đường vòng ngoài, cầu về hub, vị trí vòm/nước/tháp, khoảng mở nhìn ra biển |
| 00:33 | Cận cụm máy/bình dưới kết cấu | Hình dáng props và ánh sáng vùng thấp |
| 00:39, 00:45, 00:51 | Gameplay quanh hub, cover và mép nước | Tương quan mech/cửa/cover; xác nhận không gian chơi |
| 00:57, 01:03 | Nhiều concept art | Phân biệt ý đồ hình khối với kết quả cuối; không lấy chi tiết khác bản cuối làm chuẩn |
| 01:09 | Concept lòng chảo trung tâm và sketch phụ | Đọc tổ chức các mảng dốc, hub và đường xuyên, nhưng số lượng/chi tiết cần kiểm với bản cuối |
| 01:15 | Concept đĩa và tháp | Bổ sung cách đọc silhouette và liên kết giữa công trình |
| 01:21, 01:27 | Gameplay các hướng quanh map | Cổng, đá và sightline trên đường chính |
| 01:33 | Gameplay hành lang | Tỷ lệ mech, vách, ramp, nhịp portal |
| 01:39 | Mesh/blockout vòm có màu đánh dấu | Hình học vòm hở; không lấy màu debug làm vật liệu cuối |
| 01:45 | Vòm nhìn chính diện từ phía nước, thấy 5 bệ | Đếm bệ, đọc tuyến cong và vị trí bệ giữa trước cửa |
| 01:51, 01:57, 02:03 | Các góc bờ nước/nhà vòm | Quan hệ bệ, bờ đường, cửa và cách người chơi tiếp cận |
| 02:09 | Gameplay nhìn xuống nội thất qua đỉnh hở | Xác nhận lỗ đỉnh thực, bệ mục tiêu, các cửa vòng quanh, có nhãn GATE04 |
| 02:15 | Gameplay nhìn vào phòng vòm từ hành lang/cửa | Liên hệ corridor → nội thất và bể kính phía trong |
| 02:21 | Texture atlas/trình bày asset | Dấu hiệu workflow atlas/trim, không phải texture gốc có thể dùng trực tiếp |
| 02:27 | Cổng số 02 và đường dưới đá | Hình dạng cổng, đường quanh đá và props máy |

Khung lớn của video có presenter/HUD viền ngoài. Khi đo landmark phải crop đúng viewport map, không dùng toàn 1920×1080 như một camera render map.

## Các kết luận có bằng chứng mạnh

1. Trung tâm là không gian nhiều lớp: lòng chảo thấp, vòng sàn phía trên, các lối/cửa bên dưới, cầu/ramp nối nhiều hướng.
2. Đĩa phía trên có lỗ thực ở giữa và mặt dưới phát cyan; model sheet thể hiện hai nhánh cầu ra phía công trình tháp.
3. Tháp có các nhánh thuôn cao, lõi tối và cụm máy tròn cam/cyan lớn dưới chân. Model sheet giúp đọc hình tốt hơn ảnh phối cảnh.
4. Nhà vòm có lỗ tròn ở đỉnh, cả cấu trúc nội thất, cửa rộng và bệ giữa; video có góc nhìn xuyên từ trên xuống.
5. Có 5 bệ vuông nhìn được trong khung 01:45, bố trí thành tuyến cong nông phía trước vòm. Dấu giữa bệ là dạng thập bốn cánh, không phải ngôi sao năm cánh.
6. Vật liệu chính thiên về stylized: panel lớn, bevel rõ, đá gãy mảng rộng, cỏ xanh và biển turquoise.
7. Cảnh ngoài trời phụ thuộc mạnh vào mây tích khối lớn và độ sáng bầu trời, không chỉ geometry.
8. HUD gameplay có các mục tiêu A–E. Điều này hỗ trợ việc phân tích luật/map sau này, chưa xác định toàn bộ vị trí và nhãn chỉ bằng một frame.

## Những điểm phải để mở

| Nội dung | Vì sao chưa biết | Cách xử lý hợp lý |
|---|---|---|
| Kích thước map theo mét | Không có thước đo hoặc asset gốc | Dựng theo tỷ lệ R; dùng mech/cửa làm kiểm tra scale, ghi là ước lượng |
| Tọa độ chính xác mọi tuyến | Góc tổng thể là phối cảnh, có khuất | Solve nhiều view; vẽ adjacency trước rồi tinh chỉnh tọa độ |
| Số cửa, pod và props toàn vòng | Nhiều phần nằm ngoài khung | Cho số lượng/vị trí chỉnh được; chỉ chốt phần đã nhìn thấy |
| FOV và camera pose gốc | Không có metadata scene | Khớp landmark và horizon, lưu config camera |
| Số cụm vòm/cấu trúc vòm phụ toàn map | Một số góc có cấu trúc cong bị khuất | Dựng cụm vòm chính xác nhận được; kiểm tra cấu trúc phụ trước khi nhân đôi |
| Layout mặt sau tháp, tầng dưới xa camera | Thiếu góc quan sát | Dựng tối giản nhất quán, lưu riêng InferredGeometry |
| Chữ/số trên mọi cổng | Không phải mọi mặt đều đọc rõ | Dùng placeholder chỉnh được, không random số và báo là suy luận |
| Pipeline, shader, lightmap của game gốc | Ảnh không chứa thiết lập engine | Tái dựng kết quả bằng URP hiện có, không khẳng định là shader gốc |
| Màu albedo/roughness chính xác | Ảnh đã qua ánh sáng, tone mapping, nén | Khởi đầu bằng palette đề xuất rồi cân dưới lighting cố định |
| Hiệu ứng đỏ/hồng dưới bệ nước | Chỉ hiện ở một số khung video | Xem là biến thể chưa xác định, tắt trong bản render mặc định |
| FPS mục tiêu | Chưa biết thiết bị và độ phân giải đích | Làm cấu trúc có LOD/instancing, profile sau khi chốt thiết bị |

## Sơ đồ quan hệ cần tái dựng

Đây là sơ đồ quan hệ, không phải bản đồ gốc có tỷ lệ. Những đoạn đường cụ thể vẫn cần được solve từ reference.

```text
                      THÁP / REACTOR
                             |
                   GATE01 / TRỤ ĐỠ
                      ||           hai nhánh cầu
                      ||
                 ĐĨA TRÒN TRÊN CAO
                      :            lỗ/cột sáng đồng trục
                  LÒNG CHẢO HUB
                /      |       \
       VÒNG SÀN TRÊN — CẦU/RAMP — NHÁNH BÊN
                \      |       /          \
                 CỬA / ĐƯỜNG DƯỚI          BỜ NƯỚC
                        |                   |
                 ĐƯỜNG VÒNG NGOÀI      5 BỆ THEO CUNG
                   /         \              |
              VÒM ĐÁ       CỔNG ĐÁ       NHÀ VÒM
                                             |
                                      HÀNH LANG / CỬA
```

Chỉ dùng đường chấm ở đây để chỉ quan hệ cao độ/cùng trục giữa đĩa và lòng chảo, không biểu diễn một tuyến đi bộ thẳng đứng. Không suy thêm một đường đi từ sơ đồ này nếu reference không chứng minh.

## Bảng asset để theo dõi tiến độ

| Nhóm | Prefab/mesh cần có | Tiêu chí hình học chính |
|---|---|---|
| Hub | MainRing, BowlSectors, ObjectivePedestal, OuterWall, LowerPortal | Lòng chảo rỗng, sàn có bề dày, cửa/lối dưới mở |
| Đĩa | AnnularHull, DarkDeck, Fins, RailSegments, UndersideRing | Lỗ giữa, thân dẹt vát, cánh nghiêng và mặt dưới đọc rõ |
| Liên kết tháp | TwinBridge_L/R, SupportColumns, Gate01 | Cầu đôi có khoảng trống, nối đúng cao độ và đầu trụ |
| Tháp | Blade_A/B/C, DarkCore, ReactorFace, Base | Tỷ lệ cao/mảnh, chân cong/loe, reactor nghiêng đúng |
| Vòm | DomeShell, MeridianRibs, DiagonalBraces, BaseRing, Portals | Đỉnh hở, vỏ trong/ngoài, panel lime/trắng đúng vùng |
| Nội thất | CircularFloor, InnerPedestal, Aquarium, Console, GateSigns | Bể cong có chiều sâu, cửa thật và bệ đồng trục |
| Hành lang | Portal, Straight, Ramp, Transition, Fixtures | Tiết diện khớp, rãnh sàn và dải vàng/cyan liên tục |
| Bệ nước | WaterPlatform, Connector, Markings | 5 instance, dấu thập, tuyến cong trước vòm |
| Đường | StraightDeck, CurvedDeck, RampDeck, SideWall, HazardTrim | Nối được các tầng, mép sàn và nhịp nẹp nhất quán |
| Props | IvoryCover, OrangeDoubleCover, Crate, Pod, TubeRack, GateMarker | Silhouette đúng, chân chạm mặt sàn, không che sai cửa |
| Thiên nhiên | CliffChunks, RockArch, GrassCaps, SparsePalms, DistantMountains | Khối stylized, arch có lỗ, cỏ trên gờ/mặt nằm ngang |
| Rendering | WaterMaterial, GlassMaterial, BeamVFX, SkyProfile, LightingProfile | Chạy đúng URP, không sai sorting, nhìn giống từ nhiều camera |

## Camera và bằng chứng nghiệm thu

| Camera | Reference chính | Landmark/đặc điểm cần kiểm |
|---|---|---|
| Overview | R07 và video 00:09 cho camera phụ | Vị trí tháp/hub/vòm, khung vòm đá, khoảng biển, tỷ trọng sky |
| CentralFront | R09 | Tâm beam, mép hub, lỗ đĩa, tháp phía sau, cổng/cover |
| UnderDisk | R06 | Độ dày đĩa, cyan mặt dưới, khoảng hở đĩa–hub, đường dưới |
| DomeApproach | R04 và video 01:45 | Tỷ lệ vòm, cửa, 5 bệ nước, mép đường hai bên |
| DomeInterior | R02 | Lỗ mái, truss, bệ/ring, GATE03, bể kính, cửa hành lang |
| Corridor | R01 | Điểm tụ, nhịp khung, ramp, rãnh sàn, đèn cyan, sọc vàng |
| TowerSide | R05 | Ngọn tháp, reactor, cầu, trụ, quan hệ công trình–đá |
| AerialLayout | Video 00:09 | Sự nối nhau của tuyến ngoài, hub, tháp và cụm vòm/nước |

Mỗi ảnh gốc có thể khác crop, điểm nhìn và profile ánh sáng. Không ép một camera khớp hai reference có góc nhìn khác nhau. Một profile cảnh nhất quán là mặc định; nếu phát hiện reference thuộc biến thể khác thì lưu variant riêng có chú thích.

### Cách đánh giá định lượng vừa đủ

1. Crop vùng scene của reference; loại footer đen/HUD/presenter khỏi vùng đo. Với video, crop theo viewport map ở chính frame đó.
2. Render cùng aspect và khớp horizon/FOV/pose. Không resize méo một chiều để giả khớp.
3. Chọn landmark nhìn được ở cả hai ảnh, lưu (u,v) = (x/W,y/H). Tính khoảng cách chuẩn hóa bằng đường chéo ảnh.
4. Dùng median sai lệch landmark ≤2% đường chéo và sai lệch từng landmark quan trọng ≤3% làm ngưỡng nội bộ ban đầu. Chúng là mục tiêu công việc đề xuất, không phải đánh giá phần trăm giống map.
5. Overlay hoặc đặt hai ảnh cạnh nhau để xem silhouette, độ dày, khoảng âm, bóng đổ và màu lớn. Một ảnh khớp chưa đủ: cần kiểm lại các góc khác.
6. Lập danh sách sai lệch kèm mức ảnh hưởng và hướng sửa. Sửa camera trước nếu lỗi có tính hệ thống trên toàn ảnh; sửa asset nếu cùng lỗi xuất hiện ở nhiều góc.

### Các dấu hiệu chưa đạt

- Chỉ có một sàn tròn, không có lòng chảo, tầng dưới và đường đi thực.
- Đĩa bị dựng như cylinder đặc/torus tròn, lỗ giữa hoặc cầu đôi biến mất.
- Tháp quá thấp, nhánh quá dày, reactor bị thay bằng cửa tròn chung chung.
- Nhà vòm trở thành sphere kính đóng kín, không có bể/ramp/nội thất.
- Năm bệ nước bị biến thành cầu thẳng, dấu giữa sai vành/thập hoặc sidewall thiếu cam/đai đen.
- Đá nhiễu realistic, cỏ thành rừng dày, biển là plane xanh trơn hoặc sky là gradient không mây.
- Mọi bề mặt trắng trở thành chrome hoặc toàn scene có glow cyan quá mạnh.
- Rải nhiều props nhưng không khớp silhouette/cao độ và camera.
- Scene chỉ đẹp ở một góc, các mặt khác thủng normals, không có collider/lối nối.
- Script chạy tạo object tạm nhưng scene/prefab/mesh không được lưu bền vững.

## Thứ tự ưu tiên thực tế

1. Khớp bố cục, camera và tỷ lệ landmark.
2. Khớp hình khối và khoảng rỗng của hub/đĩa/tháp/vòm.
3. Nối đúng các tầng/cầu/hành lang và đường ngoài.
4. Khớp đá, sky, nước và màu lớn.
5. Hoàn thiện props, seams, texture, chữ và VFX.
6. Đối chiếu nhiều góc, lưu asset, kiểm tra collision và tối ưu theo thiết bị.

Thêm chi tiết trước khi hoàn tất ba bước đầu thường làm tăng công sửa mà chưa cải thiện độ giống tổng thể.

## Các file khảo sát đã tạo

- `E:/My project/ReviewMedia/BiogearLab/reference_manifest.json`: thông số video, mốc và đường dẫn frame.
- `E:/My project/ReviewMedia/BiogearLab/contact_sheet_1.jpg`: 00:03–00:51.
- `E:/My project/ReviewMedia/BiogearLab/contact_sheet_2.jpg`: 00:57–01:45.
- `E:/My project/ReviewMedia/BiogearLab/contact_sheet_3.jpg`: 01:51–02:27.
- `E:/My project/ReviewMedia/BiogearLab/frame_009s.jpg`: ảnh lớn góc tổng thể.
- `E:/My project/ReviewMedia/BiogearLab/frame_069s.jpg`: ảnh lớn concept lòng chảo.
- `E:/My project/ReviewMedia/BiogearLab/frame_105s.jpg`: ảnh lớn nhà vòm và 5 bệ.
- `E:/My project/ReviewMedia/BiogearLab/frame_129s.jpg`: ảnh lớn nhìn qua lỗ đỉnh vòm.
- Các frame lấy mẫu khác và script `extract_reference.py` nằm cùng thư mục.

## Tài liệu Unity dùng để kiểm tra workflow

- [URP Lit material Inspector](https://docs.unity.com/en-us/engine/6000.7/manual/materials-and-shaders/built-in/shaders-in-universalrp/reference/lit-shader): cách hiểu material, emission và các texture dữ liệu. Trang tham khảo được lập chỉ mục ở bản 6000.7; khi triển khai phải đối chiếu shader/package URP 17.5 trong project, không mặc định API giống hoàn toàn.
- [Bloom Volume Override — Unity 6000.5](https://docs.unity.com/en-us/engine/6000.5/manual/post-processing-and-full-screen-effects/urp/effect-list/bloom): threshold, intensity và scatter để điều chỉnh quầng sáng.

Các tài liệu này chỉ hỗ trợ phần kỹ thuật Unity. Các nhận định về map xuất phát từ ảnh/video người dùng cung cấp; không có nguồn asset game gốc được truy cập.
