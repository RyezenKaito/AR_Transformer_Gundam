# PROMPT TRIỂN KHAI — SUNKEN HARBOR · THALASSA

Bản thiết kế môi trường dưới nước cho Unity project `E:/My project`.
Đây là prompt chuẩn bị triển khai, chưa phải thông báo scene đã được dựng. Tọa độ, số lượng và thông số hình ảnh là thiết kế đề xuất; phải đo theo robot, asset và ảnh render thực tế khi thực hiện.

## 1. Ý tưởng hình ảnh đã chọn

Hãy xây dựng một chiến trường Gundam dưới nước tại một cảng quân sự bỏ hoang bị chìm giữa quần đảo đá. Sân chiến đấu nằm trên nền cát xen đá và bê tông bến cảng, xung quanh là nhà điều hành nghiêng, kho hàng sập, container phủ sinh vật biển, bồn chứa và cần cẩu gãy. Địa hình đáy biển đi lên thành các đảo đá có cây trên mặt nước.

Hình ảnh phải đẹp cả ở góc chiến đấu và khi xoay nhìn lên mặt biển: ánh sáng xanh ngọc lọc qua nước, bóng mặt sóng dao động nhẹ, hạt phù du nhỏ, các dải rong đung đưa và hậu cảnh mờ dần theo khoảng cách. Công trình có màu, texture và cấu trúc đọc được ở gần; môi trường có chiều sâu nhờ hấp thụ ánh sáng và nhiều lớp địa hình.

Không đạt mục tiêu bằng cách đặt một lớp màu xanh và fog lên scene trên cạn. Người xem phải nhận biết được mặt nước ở phía trên, hướng sáng qua nước, vật liệu ngâm nước, sự bồi lắng ở đáy, dòng chảy và chuyển động hạt.

Ưu tiên ảnh đẹp trong gameplay thực tế, có chuyển động và camera xoay được; không chỉ dựng một khung hình cố định.

## 2. Phân tích bốn ảnh tham khảo

### Ảnh 1 — Quần đảo nổi trên biển

- Dùng làm tham chiếu cho lớp bối cảnh trên mặt nước: đảo đá có đường bờ bất quy tắc, cây xanh tập trung trên những vùng đất có thể mọc, các cột đá tách khỏi đảo và nhiều lớp đảo xa.
- Nước sát bờ xanh ngọc đậm hơn vùng phản chiếu trời; bọt sóng xuất hiện quanh đá và bờ, không phủ đều toàn mặt biển.
- Đường chân trời có haze và lớp núi xa. Trời có mây, ánh nắng có hướng; đảo tạo silhouette rõ.
- Pháo đài/bê tông trên đảo là gợi ý mốc kiến trúc, không phải yêu cầu dựng một chiến trường chính trên đảo.
- Không sao chép logo, watermark, thương hiệu, pháo hoặc UI trong ảnh vào map.

### Ảnh 2 — Công trình đổ nát dưới nước

- Đây là tham chiếu chính cho trải nghiệm gameplay: các khối bê tông và công nghiệp bị nghiêng, dầm kim loại lộ ra, tầm nhìn bị nước làm giảm, mặt biển nhìn từ dưới có gợn.
- Giữ cảm giác bí ẩn và quy mô lớn, nhưng làm khu chiến đấu rõ hơn ảnh: vẫn đọc được robot, mục tiêu và kỹ năng.
- Nhà chìm không chỉ là nhà trên cạn đổi màu: phải có cát phủ bậc thấp, bám rong ở mép sáng, bê tông tối ở hốc, kim loại bị ăn mòn và chi tiết gãy có điểm tựa.
- Không nhân bản toàn bộ công trình với cùng góc nghiêng. Có nhà đứng, nhà lún, nhà nghiêng vừa và một mảng công trình sập mạnh.

### Ảnh 3 — Danh sách package

- Đây là danh sách nguồn tài sản, không phải bằng chứng mọi package đều có prefab hoặc shader dùng được ngay trong pipeline hiện tại.
- Xác minh asset thực tế trước khi tạo layout; dùng material riêng của môi trường mới để chuyển đổi khi cần.

### Ảnh 4 — File GLB cần cẩu

- Dùng `harbor_crane.glb` làm mốc hình ảnh lớn tại cảng chìm.
- Hai file `harbor_crane.glb` và `harbor_crane (1).glb` đã kiểm tra SHA-256 và có nội dung giống nhau. Chỉ import một nguồn; biến thể cần cẩu được tạo từ các instance của nguồn đó.
- Chưa kiểm tra mesh, texture, rig và bounds nội bộ GLB. Không khẳng định model có thể tách từng chi tiết trước khi import và xem hierarchy.

## 3. Kiểm kê asset đã đọc trong project

### WaterWorks

- Có `Assets/WaterWorks/Water_Plane.prefab`.
- Material prefab dùng `Shader Graphs/SSR_Water`.
- Có `SSR_Water.shadergraph`, `Volumetric_Water.shader`, `Water_Volume.cs`, `Water_Settings.cs`.
- `Water_Volume.cs` có các API cũ như RenderTargetHandle, cameraColorTarget, Blit và một Execute pass; không được suy luận renderer feature này sẽ hoạt động đúng trong mọi chế độ Render Graph của project.
- `Water_Settings.cs` có ExecuteAlways và ghi vào material lấy từ Resources/renderer.sharedMaterial trong Update. Không dùng trực tiếp cách này nếu nó làm thay đổi material dùng chung hoặc ảnh hưởng map khác.
- Ưu tiên tận dụng texture, normal sóng và thành phần tương thích. Nếu phải thay phần render pass, viết adapter/feature thuộc thư mục map mới, giữ nguyên package gốc.

### Free Island Collection

- Có 5 scene `Assets/Free Island Collection/Scenes/Scene 1.unity` đến `Scene 5.unity`.
- Có TerrainData `Assets/Free Island Collection/Environment/Terrain/Terrains/Terrain 1.asset` đến `Terrain 5.asset`.
- Lượt kiểm kê hiện tại không tìm thấy prefab trong thư mục package.
- Khi xem đảo, mở scene nguồn theo cách bảo toàn scene đang làm hoặc dùng additive read-only, đọc cấu trúc Terrain, terrain layers, trees và props. Tạo bản TerrainData riêng cho map mới nếu cần chỉnh heightmap/splatmap; không sửa TerrainData gốc.

### Photoscanned MountainsRocks PBR

- Root thực tế: `Assets/TheTalesFactory/Photoscanned MoutainsRocks PBR/` — giữ đúng cách viết tên thư mục thực tế.
- Có 9 prefab đá/núi, ví dụ `MountainRocks01.prefab`, các biến thể 01_A/01_B/01_C/01_D, 02/02_A/02_B và `MountainsRocks03.prefab`.
- Material mẫu đã xem dùng shader Standard. Tạo bản URP giữ texture và normal gốc; kiểm tra cách đóng gói roughness/metallic/smoothness trước khi chuyển.
- Dùng bộ đá này cho mốc đá gần camera; thống nhất tông đá xám nâu lạnh và lớp sinh vật bám. Không dùng vân sa thạch vàng của map sa mạc cho toàn bộ đáy biển.

### Industrial Tanks

- Có FBX tại `Assets/Industrial Tanks/Round Tanks/Round_tanks.fbx`, `Long Tanks/Long_Tanks.fbx` và `Large Tank/Large Tank.fbx`.
- Lượt kiểm kê hiện tại không tìm thấy prefab trong package. Import/instantiate đúng model, kiểm tra các bồn có nằm chung một FBX hay không và chọn phần phù hợp.
- Dùng bồn đứng, bồn ngang, cầu thang, ống và chi tiết có sẵn; không mặc định mỗi file là một bồn tách biệt.

### Free Shipping Containers

- Root thực tế: `Assets/Shipping Container Environment/`.
- Có 35 prefab, gồm `container_1` đến `container_4` và các bản `_open`, `warehouse_1`, tấm bê tông, sàn đi bộ, lan can, dốc, pallet, thang và đèn.
- Container mẫu đang dùng `Legacy Shaders/Bumped Diffuse`; cần material URP riêng và bảo toàn atlas/normal.
- Ưu tiên container, tấm bê tông, khung kho và kim loại. Không rải hộp carton còn nguyên vẹn khắp đáy biển như đồ trang trí bình thường.

### Modular Sci-Fi Pack, Abandoned Factory và Abandoned Buildings

- Dùng modular sci-fi cho cấu kiện cơ sở dưới biển: vách, cửa, sàn, cửa sổ, khung cầu nối; cần giảm độ sạch và thống nhất độ chi tiết với các asset khác.
- Dùng Factory Lite cho motor, quạt, tủ điện, thùng phuy, cột, dầm và phế liệu.
- Dùng Abandoned Buildings có chọn lọc cho phần nhà hỏng; tránh biến cảnh thành một ngôi làng sa mạc ngập nước còn nguyên cỏ khô và xương rồng.
- Flora-Form chỉ tận dụng nếu hình dáng thực sự phù hợp. Cây bụi trên cạn không được đổi màu rồi gọi là rong biển.

### Tài sản chưa tìm thấy trong lượt kiểm kê

- Chưa tìm thấy asset chuyên dụng rõ ràng cho san hô, rong biển, cá, xác tàu, caustics hoặc bong bóng.
- Việc tìm theo tên chưa chứng minh tuyệt đối rằng mọi dạng asset này đều không tồn tại; khi triển khai phải kiểm tra thêm thư mục/model và nội dung các scene nguồn.
- Nếu vẫn thiếu, tạo mesh/shader/VFX bổ sung phù hợp, ghi rõ nguồn và cách tạo. Không ghi là đã dùng package san hô hoặc xác tàu khi chưa có.
- Không tự tải/mua thêm package hoặc thay render pipeline toàn project.

## 4. Quy mô, trục tọa độ và tương thích gameplay

- Project đang được kiểm tra bằng Unity 6000.5.8f1, dùng URP của project hiện có; xác nhận lại khi bắt đầu triển khai.
- Tạo scene riêng: `Assets/Environments/SunkenHarbor/Scenes/SunkenHarbor_Battle.unity`.
- Catalog ID: `sunken-harbor`; tên hiển thị `SUNKEN HARBOR · THALASSA`.
- Giữ scene Desert và các map khác; tạo bản sao template chiến đấu phù hợp, chỉ thay phần môi trường trong scene mới.
- Layout origin O đặt tại tâm hai anchor chiến đấu thực tế; bố cục hiện có có tâm gần world X=4.85, nhưng phải đọc dữ liệu để xác nhận.
- X chạy dọc đường giao tranh; Z sang hai bên; Y hướng lên. Tất cả tọa độ bên dưới là local offset so với O.
- Nền trung tâm tại Y=0 để giữ tương thích anchor/hiệu ứng mặt đất hiện có. Thế giới dưới nước được tạo bằng việc đặt mặt biển phía trên, không tùy tiện dịch toàn bộ combat rig xuống một cao độ mới.
- Mặt biển đề xuất tại Y=+42 m. Đo độ cao thực tế của Attack/Skill/Earth Prison và camera trước khi chốt; giữ ít nhất 8–12 m nước phía trên đỉnh động tác cần ở dưới nước.
- Đây là độ sâu cảng ven đảo, không phải hệ sinh thái vực sâu hàng nghìn mét. Sinh vật, màu nước và ánh sáng phải tương ứng.
- Sân chiến đấu trung tâm khoảng 46 × 30 m, viền chuyển tiếp mềm; vùng chơi và cảnh gần khoảng 120 × 100 m.
- Không dựng sàn kim loại hình chữ nhật trơn nổi trên đáy biển. Nền cát, đá và bê tông cũ phải kết nối thành địa hình liên tục.
- Camera được giữ xoay 360°, nhìn lên mặt nước và zoom xa theo yêu cầu đã có. Khi camera vượt mặt biển, xử lý chuyển trạng thái nước/không khí đúng.
- Giữ gameplay theo lượt; không tự thêm cơ chế bơi 6 trục, buoyancy cho robot hay thay luật chiến đấu.

## 5. Bố cục tổng thể và các tuyến nhìn

Tạo ba lớp có chức năng rõ:

1. Lớp gần 0–25 m: Gundam, cát có hạt, nền bê tông gãy, đá nhỏ, rong thấp, hạt phù du rất thưa. Đảm bảo đọc animation và chân robot.
2. Lớp giữa 25–90 m: khu nhà chìm, container, bồn chứa, cần cẩu, chân vách đảo và bãi đổ nát. Tạo mốc và hướng nhìn.
3. Lớp xa trên 90 m: các sườn đá lớn, địa hình đáy biển và silhouette công trình, mờ dần trong nước; phía trên nước có quần đảo nhiều lớp.

Không đặt công trình lớn, vách đá hoặc rong cao xuyên vùng trung tâm. Các hướng chính phải khác nhau: cần cẩu ở tây bắc, tháp điều hành ở bắc, bồn chứa phía đông, nhà sập phía nam, khe đá mở về phía tây.

Giữ ít nhất một vista nhìn lên từ sân thấy mặt biển và một phần đảo/đá chắn sáng; một vista ngang thấy silhouette cần cẩu qua lớp nước; một vista thấp thấy robot đứng trên cát có caustics nhẹ.

## 6. Công trình chìm — thiết kế từng cụm

Các bounds là mục tiêu tham khảo, được phép hiệu chỉnh theo model. Độ nghiêng ghi theo cảm giác so với phương đứng; thực hiện rotation theo pivot/mặt tựa thực tế thay vì áp ngẫu nhiên từng Euler axis.

### U01 — Nhà điều hành cảng, mốc phía bắc

- Tâm `(−8,0,+33)`, footprint khoảng 15 × 12 m, cao 12–15 m.
- Kết cấu bê tông kết hợp khung kim loại, 2–3 tầng; mặt tiền hướng sân, cửa sổ lớn đã mất kính phần lớn.
- Nghiêng khoảng 7–10° theo một hướng, có một phía móng lún vào cát. Chân công trình phải được địa hình bồi lấp hợp lý.
- Một thang ngoài, lan can gãy và anten cong trên mái; không có cỏ khô hay đồ nội thất sáng sạch.
- Cát phủ sàn tầng dưới; lớp rong ngắn trên mép nhận sáng, lớp bám xám xanh ở chân, gỉ quanh bu lông và mối nối.
- Cửa tối có chiều sâu nhưng không đen tuyệt đối; ưu tiên dựng phần nội thất nhìn thấy từ sân.

### U02 — Kho hàng sập phía tây bắc

- Tâm `(−32,0,+29)`, footprint 18 × 12 m, cao phần còn lại 8–10 m.
- Tận dụng `warehouse_1`/Factory hoặc modular phù hợp. Phần mái hỏng khoảng 35–45%, dầm lớn còn điểm tựa.
- Một mảng tường nghiêng ra sau; tôn rách và sàn bê tông gãy thành các mảng lớn.
- Đống đổ nát gồm vài cấu kiện lớn, một số mảnh trung và sỏi nhỏ; tránh rải phế liệu thành vòng tròn đều.
- Bên cạnh là container U05; giữ khoảng hở để đọc silhouette hai công trình.

### U03 — Trạm nghiên cứu/phụ trợ nhỏ

- Tâm `(+16,0,+31)`, footprint 12 × 10 m, cao 7–9 m.
- Dùng modular sci-fi ít chi tiết, màu sơn xanh xám/trắng ngà đã bạc, các panel không phát sáng đồng loạt.
- Một cửa áp lực lớn hoặc cửa công nghiệp, cửa sổ nhỏ, 2 ống nối và 1 tủ điện ngoài.
- Có 2–3 đèn báo cyan rất yếu, xem là nguồn năng lượng còn sót; không biến thành căn cứ neon sáng rực.
- Vật liệu và mức chi tiết phải hòa với nhà bê tông/cần cẩu, tránh một căn nhà low poly quá sạch nằm giữa cảnh thực.

### U04 — Công trình gãy nghiêng lớn phía nam

- Tâm `(+3,0,−34)`, footprint đống kết cấu khoảng 22 × 15 m, cao 10–13 m.
- Mượn cảm giác ảnh công trình chìm: một khối tầng lật 20–28°, dầm/trụ nhìn được qua chỗ mất vách.
- Tạo pivot tại điểm gãy/mặt tựa, chôn một phần dưới cát, có đá và dầm đỡ dưới cạnh thấp; không treo nhà lơ lửng.
- Kết cấu sập tạo một khoảng hốc tối ở phía sau, không che kín camera và sân trung tâm.

### U05 — Bãi container phía tây

- Tâm `(−37,0,+6)`, trải khoảng 20 × 18 m.
- Dùng 7 container: 3 nằm tương đối thẳng ở nền, 2 nghiêng 10–18°, 1 bản open và 1 container lệch nằm trên một container khác có điểm tiếp xúc hợp lý.
- Màu sơn gồm xanh bạc, đỏ gỉ trầm và xám; dưới nước màu đỏ bị suy giảm theo khoảng cách, không đỏ neon.
- Cửa open không bị collider hình hộp bịt kín nếu camera/robot có thể đi vào. Nếu chỉ làm cảnh nền, giới hạn đường tiếp cận bằng bố cục rõ ràng.
- Cát lấp góc thấp 0.2–0.6 m; bám sinh vật không đều, đặc biệt ở gân và mép. Không đặt hai container cùng model, màu và rotation cạnh nhau.

### U06 — Container và vật tư phía đông

- Tâm `(+39,0,−10)`, gồm 5 container bố trí thành 2 cụm lệch nhau.
- 1 container mở có các kiện nhựa/kim loại bên trong; 1 nằm nghiêng gần đá; 3 cái tạo nhịp lùi dần về bồn chứa.
- Chừa tuyến nhìn qua giữa hai cụm về khe đá xa. Vật tư nhỏ không rải đều thành bãi rác phủ hết sân.

### U07 — Trạm bơm và tủ điều khiển

- Tâm `(+35,0,+28)`, footprint 9 × 8 m, cao 5–7 m.
- Một khung công nghiệp, motor, 3 đường ống khác đường kính, một van và tủ điện.
- Một đoạn ống gãy nhô lên có bong bóng nhỏ, không tạo tia khói xám như trên cạn.
- Ống nối tới bồn U08 phải chạm khớp và đi men địa hình; hạn chế chạy cắt ngang sân chính.

### U08 — Cụm bồn đứng

- Tâm `(+46,0,+34)`, 3 bồn cao 8–12 m, có chênh lệch kích thước và một bồn nghiêng khoảng 5–8°.
- Dùng model Round Tanks thực tế; giữ cầu thang/ống có sẵn nếu chất lượng phù hợp.
- Kim loại gỉ nâu xám dưới lớp bám nước xanh olive, các bệ tiếp đất chìm một phần dưới cát.
- Tạo đường ống và giằng đủ hợp lý; không xếp bồn đứng lơ lửng trên nền.

### U09 — Bồn ngang đổ và dầm gãy

- Tâm `(+34,0,−37)`, footprint khoảng 16 × 10 m.
- Một bồn ngang bị tuột khỏi bệ, trục lệch 10–20°, có 2 bệ đỡ cũ lộ một phần.
- Bên cạnh 3–5 đoạn ống và vài tấm kim loại; dây/cáp nặng nằm sát đất, chỉ phần tự do chuyển động nhẹ nếu cần.

### U10 — Cần cẩu cảng, điểm nhấn chính

- Chân tại `(−39,0,+45)`, cao mục tiêu 22–30 m sau khi kiểm tra model và quỹ đạo kỹ năng.
- Import một nguồn `D:/Downloads/harbor_crane.glb`, giữ source và manifest; chuẩn hóa theo geometry bounds và kiểm tra trục hướng.
- Chân cần cẩu lún một bên, nghiêng tổng thể 5–8° nếu cấu trúc cho phép. Cần vươn về khoảng không phía trên bãi container, không chĩa xuyên sân/đảo.
- Nếu model tách được bộ phận, tạo một đoạn lan can hỏng hoặc phần cần gãy có điểm tựa. Nếu model liền mesh, không tuyên bố đã tách/gãy chi tiết khi chưa thực hiện.
- Móc/cáp nặng có độ chùng hợp lý. Không làm cáp thép đung đưa như rong mềm trong nước.
- Phần trên gần mặt biển nhận thêm ánh sáng, phần chân mờ và phủ bám; không áp một màu xanh đen lên toàn bộ model.

### U11 — Cầu cảng gãy

- Dọc phía bắc, từ X≈−48 đến +35, quanh Z=+43; chia 3 đoạn với khe gãy 3–8 m.
- Các trụ/cọc có độ cao khác nhau, phần thấp phủ trầm tích. Một đoạn dốc đi từ đáy tới sàn cũ nhưng không chắn sân.
- Có lan can cong, thang, gân sàn, vết nứt và các khối bê tông sứt cạnh. Dùng chi tiết đủ lớn để đọc qua nước.

### U12 — Đống phế liệu thấp phía tây nam

- Tâm `(−39,0,−31)`, footprint 12 × 9 m, cao tối đa khoảng 3–4 m.
- Motor, thùng phuy, tấm sàn, dầm và vài mảnh vỏ; 1–2 silhouette lớn trước, đồ nhỏ sau.
- Không đặt xác tàu lớn giả bằng các cube. Nếu không có asset xác tàu, dùng đúng chủ đề cảng/công trình chìm và ghi rõ không có tàu chuyên dụng.

## 7. Nền đáy biển — từng khu địa hình

### S01 — Lòng sân chiến đấu

- Khoảng 46 × 30 m quanh O, phẳng tương đối tại Y=0, sai khác cao độ nhỏ theo yêu cầu gameplay.
- 60–70% cảm giác bề mặt là cát biển nhạt, phần còn lại là đá thấp và mảng bê tông cũ chìm dưới cát.
- Gợn cát nhỏ có hướng dòng chảy, không phải những sóng địa hình lớn làm chân robot lơ lửng.
- Có vài vệt trầm tích, đá vụn nhỏ và rong rất thấp ở mép. Chừa vùng quanh anchor cho chân robot và hiệu ứng.

### S02 — Viền cát bồi quanh công trình

- Tạo dune/bồi lắng cỡ 0.2–1 m ở chân nhà, cạnh container, phía khuất dòng.
- Giao tuyến phải mềm và hợp lý theo trọng lực; không dùng vòng cát giống nhau quanh mọi object.
- Có góc công trình bị cát lấp, các mép bậc thấp mất nét theo thời gian ngâm.

### S03 — Rãnh đáy phía tây

- Tâm khoảng `(−66,−6,0)`, dài 70 m, rộng 18–25 m; bắt đầu ngoài vùng chơi.
- Sườn đá có hốc, đá vụn và rong thưa. Sâu dần theo tuyến nhìn, không thành một vách dựng hình hộp.
- Dùng optical attenuation tạo cảm giác sâu; không đặt tấm phẳng màu đen để giả vực.

### S04 — Sườn chân đảo phía bắc

- Bắt đầu quanh Z=+68, đi từ đáy biển lên cao vượt mặt nước thành đảo chính.
- Dùng đá photoscan có normal/roughness thống nhất, ghép theo khối lớn–vừa–nhỏ; không phóng một viên đá nhỏ thành núi làm vân to bằng căn nhà.
- Cảnh chìm và cảnh nổi phải chung một địa hình liên tục, không có đảo nổi trên không hoặc mép terrain xuyên nước rõ rệt.

### S05 — Khe đá phía đông

- Quanh X=+75, hai vách lệch cao 18–30 m, có lối vista nhìn ra nước xanh xa.
- Đặt 1–2 cột đá lẻ, có chân nối vào đáy và sườn đá. Không tạo vòm tròn đều hoặc dãy nón giống nhau.

### S06 — Nền xa và đường chân trời dưới nước

- Địa hình tiếp tục ít nhất 250–350 m quanh map; độ chi tiết giảm theo khoảng cách.
- Khi zoom camera xa không lộ đáy plane, cạnh terrain, backface hoặc khoảng trống.
- Không cần cố làm mọi chi tiết xa rõ như cảnh gần; dùng lớp silhouette hợp lý trong nước.

## 8. Quần đảo phía trên mặt nước

Đây là bối cảnh phụ khi camera lên cao/qua mặt nước và là vật chắn sáng nhìn từ đáy biển, không thay thế sân chiến đấu dưới nước.

- A01 — Đảo chính phía bắc, tâm khoảng `(0,+42,+120)`, footprint 170 × 120 m, đỉnh cao hơn mặt nước 30–55 m. Có đá xám nâu, thảm đất, cây xanh theo vùng và một công trình bê tông cũ trên mỏm thấp.
- A02 — Đảo đá phía đông, tâm khoảng `(+180,+42,+30)`, footprint 100 × 75 m; mép dốc và vài khoảng đá trơ, cây ít hơn A01.
- A03 — Đảo thấp phía tây, tâm khoảng `(−190,+42,−20)`, footprint 90 × 60 m; bờ đá thấp xen đất có cây bụi.
- A04/A05 — Hai cột đá biển lệch nhau gần A01, cao hơn mặt nước 15–28 m, chân rộng và hình dáng khác nhau. Chọn mesh đá tốt thay vì làm nón trơn.
- A06–A09 — 4 silhouette đảo xa ở khoảng 350–700 m nếu far clip/hiệu năng cho phép; giảm tương phản, tạo lớp khí quyển thay vì copy nguyên đảo gần.
- Các đảo không xếp thành vòng đều. Giữ khoảng biển mở ở ít nhất hai hướng để có đường chân trời rộng.
- Chọn TerrainData và vegetation của Free Island Collection sau khi kiểm tra nội dung thực tế; không tạo giả prefab chỉ vì không tìm thấy prefab package.
- Dải bờ có foam và ẩm theo mực nước; cây trên đảo không mọc từ mặt đá dựng hoặc chìm ngập toàn thân dưới biển trừ khi đó là cây chết có chủ đích.

## 9. Thực vật và sinh vật biển

Không cần biến cảng chìm thành rạn san hô nhiệt đới sặc sỡ. Chọn hệ thẩm mỹ nhất quán: nước ven đảo trong vừa phải, rong xanh olive/nâu, sinh vật bám xám ngà, san hô điểm nhấn tiết chế nếu có thể bổ sung phù hợp.

- V01–V04: 4 cụm rong cao 3–6 m ở viền Z≈±24 đến ±40, ngoài vùng di chuyển/kỹ năng. Mỗi cụm có 8–16 thân/tán, dáng và pha chuyển động khác nhau.
- V05–V12: 8 cụm rong vừa 0.8–2.5 m quanh đá, chân nhà và container; có khoảng hở, không dựng thành hàng rào xanh kín.
- V13–V20: 8 vùng cỏ biển thấp 0.15–0.5 m ở mép cát, bắt đầu khoảng 250–450 tuft cho toàn map rồi điều chỉnh theo overdraw/ảnh thực tế.
- V21–V26: 6 cụm sinh vật bám/sea fan/coral thấp tại những đá nhận sáng; màu olive, nâu tím trầm, xanh xám và ngà. Không dùng màu neon đồng loạt.
- Tổng phân bố: tập trung ở đá và cấu trúc có bề mặt bám; thưa ở nền cát thường xuyên bị khuấy; rất thưa ở tâm sân.
- Rong có gốc đứng yên, biên độ uốn tăng theo chiều cao. Dùng vertex mask từ UV/vertex color và luồng nước có hướng; không làm cả bụi cây tịnh tiến qua lại.
- Chuyển động chính chậm, lệch pha giữa các cụm; không lắc đồng bộ như quạt và không xuyên đá/tường quá rõ.
- Nếu thêm cá: chỉ 2–3 đàn nhỏ làm bối cảnh, tổng khoảng 20–40 cá silhouette, chạy theo vòng spline có tránh khu giao tranh. Không bơi xuyên camera hay nhà; tắt/giảm ở chất lượng thấp.
- Nếu chưa có mesh phù hợp, tạo bổ sung hoặc giữ ít loại hơn nhưng đúng hình; không lấy xương rồng/cỏ sa mạc từ map trước rồi đổi màu.

## 10. Hình ảnh nước — yêu cầu bắt buộc

### 10.1 Mặt nước nhìn từ trên

- Biển có hai thang sóng: độ phồng lớn chậm và gợn nhỏ nhanh hơn. Biên độ đề xuất sóng mặt khoảng 0.15–0.4 m trong vùng cảng, điều chỉnh theo khoảng cách và chất lượng.
- Normal có chuyển động theo nhiều hướng không song song; tránh một normal map trôi thẳng đều làm nước giống tấm kính kéo qua.
- Fresnel, reflection và refraction có độ mạnh vừa phải. Nếu SSR không hỗ trợ hoặc vật thể ra ngoài màn hình, có fallback phản xạ trời/probe; không để vùng nước đen hoặc mất phản chiếu đột ngột.
- Shore foam có độ dày thay đổi và chuyển động, bám theo tiếp giáp đá/nước; không là viền trắng liên tục cố định quanh mọi đảo.
- Không có foam trên toàn mặt biển hoặc dưới đáy biển chỉ vì sử dụng chung texture noise.

### 10.2 Mặt nước nhìn từ dưới

- Render underside có normal/chuyển động đúng, không cull mất mặt nước khi ngẩng camera.
- Có vùng sáng theo hướng trời/mặt trời và sự biến dạng bề mặt; nếu làm hiệu ứng cửa sổ sáng nhìn lên trời, dùng chuyển tiếp/Fresnel hợp lý, không đặt một vòng sáng tròn cứng.
- Bề mặt không phải trần phẳng màu cyan. Không thể nhìn xuyên rõ cả đảo ở xa qua nước như không có môi trường khúc xạ.
- Phản xạ và khúc xạ ổn định khi camera xoay nhanh, zoom xa, nhìn xiên hoặc gần mực nước.

### 10.3 Trạng thái camera ở trong nước

- Dùng một cơ chế chủ đạo tính quãng đường ánh sáng trong nước từ camera đến bề mặt opaque hoặc giới hạn water volume; tái dựng world position từ depth nếu cần.
- Độ truyền màu có thể dựa trên dạng `T = exp(-absorption * waterDistance)`; màu tán xạ được thêm theo phần ánh sáng mất đi. Đây là mô hình hình ảnh đơn giản, phải hiệu chỉnh trong scene thay vì coi là mô phỏng đầy đủ.
- Trong nước ven đảo xanh: màu đỏ suy giảm nhanh hơn xanh lục/xanh lam; vật gần vẫn giữ được màu sơn và độ tương phản. Không làm mọi thứ chỉ còn một màu xanh xám phẳng.
- Tầm nhìn đề xuất: chủ thể rõ ở 0–20 m; cấu trúc đọc được ở 20–55 m; silhouette mờ dần ở 55–110 m. Điều chỉnh cho gameplay và độ sâu thực tế.
- Không áp distance fog hai lần bằng cả RenderSettings fog dày và fullscreen underwater pass cùng một độ suy giảm.
- Xử lý riêng sky pixel/không có depth, nước nông, chỗ nhìn ra ngoài volume và camera ngoài nước. Không dùng depth vô hạn làm toàn màn hình đen.
- UI/HUD và chữ mục tiêu phải giữ sắc nét, không bị refraction hoặc độ mờ dưới nước phủ lên theo cách khó đọc.

### 10.4 Khi camera đi qua mực nước

- Có trạng thái Above / Crossing / Underwater dựa trên cao độ mặt nước thực tế; thêm vùng chuyển tiếp khoảng 0.25–0.6 m để tránh nhấp nháy khi sóng đi qua camera.
- Trong trạng thái crossing, hiệu ứng không bật/tắt thô toàn màn hình từng frame. Nếu xử lý waterline theo pixel, phải khớp mặt nước và có fallback ổn định.
- Khi camera ra khỏi nước, giảm đúng underwater attenuation và chuyển sang khí quyển trên mặt biển; không giữ toàn cảnh màu xanh dưới nước.
- Khi về map khác/Hub, feature, global shader parameter, audio filter và fog của map nước phải được reset hoặc cô lập đúng.

## 11. Ánh sáng dưới nước và caustics

- Một mặt trời ấm phía trên nước, góc đủ xiên để tạo hướng ánh sáng và silhouette cần cẩu; ánh sáng dưới nước thiên xanh trung tính.
- Màu chủ đạo dưới nước: xanh lam–lục trung bình, cyan dịu ở vùng sáng, đá xám nâu lạnh, kim loại gỉ trầm, cát ngà/xám. Dùng màu cyan yếu cho vài đèn thiết bị để định hướng.
- Nền cát sáng hơn hốc bê tông và container; không sáng trắng như tự phát quang.
- Caustics là vệt ánh sáng do mặt nước, chủ yếu trên mặt hướng lên, có hai lớp chuyển động lệch pha và fade theo độ sâu/khoảng cách.
- Với sân sâu khoảng 40 m, caustics ở đáy phải mềm, rộng và có cường độ tiết chế trong nước trong; không đặt hoa văn trắng nét sắc như hồ bơi nông trên mọi bề mặt.
- Tránh áp caustics đồng đều lên vách đứng, mặt dưới dầm, bên trong nhà kín và HUD. Dùng normal/occlusion/shadow phù hợp trong giới hạn render path.
- 3–5 dải sáng thể tích nhẹ ở những khe mở chính, phát từ trên mặt nước và có fade mềm, noise rất thấp; không tạo các tấm tam giác sáng nhìn thấy rõ khi xoay camera.
- Nếu không có true volumetrics phù hợp URP hiện tại, tạo giải pháp nhẹ có kiểm soát và ghi rõ đó là mô phỏng, không gọi là volumetric vật lý đầy đủ.
- Cửa sổ và hốc công trình tối hơn phía ngoài nhưng không đen đặc. Dùng ambient/bounce phù hợp trước khi thêm nhiều point light.
- Bloom nhẹ chỉ tác động ánh sáng mạnh; không dùng bloom/fog để che chất lượng model hoặc texture lỗi.

## 12. Hạt nước, bong bóng và trầm tích

- Suspended particles: hạt nhỏ, thưa gần camera, nhiều lớp xa, màu xám xanh/cát nhạt, chuyển động chậm theo dòng. Không giống tuyết rơi dày trong nước.
- Tránh spawn trong vùng quá sát camera khiến hạt phóng lớn che màn hình; có near fade và soft depth intersection nếu pipeline hỗ trợ.
- B01 tại U07: bong bóng nhỏ không liên tục từ ống gãy; có kích thước thay đổi, trôi lên và lệch dòng.
- B02 gần chân U10: cụm bong bóng rất thưa từ một khe kết cấu, không phát như vòi sục công viên.
- B03 phía bồn U09: vài bong bóng theo chu kỳ dài. Không mặc định mỗi vật thể đều rò khí.
- Bong bóng nhẹ tăng kích thước theo độ cao nếu mô phỏng, tan/mất tại mặt nước; không bay lên trời mãi.
- Dấu chân/tiếp đất mạnh tạo đám trầm tích thấp, cát mịn bung ra rồi lắng; opacity thấp và thời gian vừa phải để không che hit VFX.
- Không thêm khói đen lửa cháy bình thường trong nước. Các hiệu ứng phản lực/kỹ năng hiện có được giữ logic, chỉ bổ sung phản ứng môi trường nếu làm được mà không phá timing.
- Particle phải được giới hạn, cull theo khoảng cách và có chế độ thấp. Không tạo hàng chục nghìn transparent quad phủ toàn màn hình.

## 13. Vật liệu và sự đồng bộ giữa package

- Tạo một thư viện material riêng của SunkenHarbor, giữ nguồn asset gốc. Quy định palette chung trước khi đặt hàng trăm object.
- Đá: giữ albedo/normal PBR của photoscan, giảm khác biệt sắc độ giữa các mẫu; trộn cát ở chân và biofilm ở vùng phù hợp. Ưu tiên chi tiết đá thật, không sinh các dải sin lặp như vân gỗ.
- Bê tông: xám ngà lạnh, lỗ rỗ, nứt, vùng vỡ có chiều sâu; màu loang theo mức bám và lắng cát. Không dùng một texture phóng to để những hạt bê tông to bằng robot.
- Kim loại: rỉ ở mép, bu lông, khe và phần mất sơn; giữ độ tương phản vật liệu dưới lớp bám nước. Không phủ gỉ cam sáng đồng đều toàn bộ.
- Biofilm/sinh vật bám: mask thay đổi theo world height, normal, vị trí và noise, không là một viền xanh ngang giống nhau trên tất cả object.
- Vùng cát bồi: phủ ưu tiên mặt hướng lên và chân thấp. Không để cát bám ngược dưới trần nhà như phủ tuyết sai hướng.
- World-space/triplanar dùng cho đá/tài sản cần tránh UV giãn; giữ atlas gốc cho container và cấu kiện có nhãn/gân đặc trưng.
- Dùng cùng tỷ lệ chi tiết theo mét cho các khối đá; chia độ mạnh normal theo khoảng cách nếu cần. Không sửa màu đá bằng cách nhân một lớp cyan tối lên mọi texture.
- Kiểm tra grain/normal ở góc xiên; lọc mip và giảm cường độ khi bị chấm đen, shimmering hoặc gợn bóng giả. Không lấy albedo làm normal mạnh mà không kiểm chứng hình ảnh.

## 14. Tích hợp URP và giới hạn WaterWorks

1. Đọc renderer/pipeline asset đang dùng và xác nhận Render Graph/Compatibility Mode thực tế. Không tắt Render Graph toàn project chỉ để làm package cũ chạy.
2. Thử WaterWorks trong scene kiểm tra nhỏ trước, ghi nhận phần nào hoạt động: mặt nước, normal, refraction, SSR, volume và camera dưới nước.
3. Nếu feature cũ không hoạt động, viết pass/adapter tương thích API hiện có, có xử lý lifecycle tài nguyên và camera filtering. Dùng API đúng package được cài, không đoán từ code cũ.
4. Không bật một underwater pass toàn cục cho mọi camera/map. Có thể dùng renderer riêng cho camera của map hoặc một feature có điều kiện theo water volume/camera rõ ràng.
5. Nếu thêm renderer data vào pipeline, thực hiện theo cách bổ sung có kiểm soát, giữ nguyên default renderer của các map cũ. Không ghi đè setting dùng chung không liên quan.
6. Scene View, camera review, reflection camera và gameplay camera có thể cần xử lý khác nhau. Tránh render pass đệ quy hoặc tạo render texture mới mỗi frame không giải phóng.
7. Bật depth/opaque texture cho camera cần đến, xác minh timing copy color/depth để water và underwater pass không đọc texture sai thời điểm.
8. Hiệu ứng nên thực hiện trước lớp UI không bị ảnh hưởng. Kiểm tra transparent objects, particles, water underside và weapon VFX; không coi opaque depth là đủ cho mọi đối tượng.
9. Nếu tận dụng Water_Settings, tách instance material và dữ liệu volume của map; không để ExecuteAlways ghi vào shared Resources material của project.
10. Không áp shader nước trong suốt lên tất cả vật thể để giả cảnh chìm. Opaque geometry vẫn có vật liệu opaque bình thường, attenuation do đường đi trong nước quyết định.

## 15. Camera, va chạm, gameplay và âm thanh

- Robot đứng đúng mặt đáy Y≈0, bóng/hiệu ứng mặt đất không chìm dưới collider. Giữ vị trí anchor, HUD, target và cơ chế chiến đấu hiện có.
- Dọn cả renderer và collider của môi trường template cũ trong scene mới; không để sàn vô hình xuyên qua đáy biển.
- Cửa/mảng tường/cần cẩu không xuyên vùng bay của kỹ năng. Đo quỹ đạo thật trước khi đặt cấu kiện phía trên sân.
- Collider của nhà, đá và container khớp hình ở mức hợp lý; rong/cá/caustics/particles thường không có collider.
- Mặt nước không là tường collider chặn camera hoặc kỹ năng nếu không có gameplay yêu cầu.
- Camera xoay 360° không đi xuyên công trình; ngẩng nhìn mặt biển phải có hình ảnh đẹp, không chỉ thấy mặt dưới plane đen.
- Zoom xa/qua mặt nước phải hoạt động với chuyển trạng thái nước–không khí. Giữ camera orbit và input đã có, không thêm một controller chồng lên.
- Một camera gameplay, một AudioListener và tối đa một EventSystem đang hoạt động. Camera review lưu disabled, không có listener.
- Âm thanh dưới nước: ambience trầm nhẹ, chuyển động nước xa, tiếng bong bóng thưa. Nếu dùng low-pass, chỉ áp trong scene/trạng thái camera phù hợp và trả lại khi rời map.
- Không tự làm mất tiếng đòn đánh/HUD để tạo cảm giác nước. Mức ambience thấp hơn thông tin âm thanh chiến đấu.

## 16. Catalog, cấu hình và tổ chức scene

Đăng ký `sunken-harbor` vào catalog hiện có; không dựng một màn hình chọn môi trường thứ hai.

```text
SunkenHarbor_Battlefield
  SunkenHarbor_LayoutOrigin
    01_Seabed_S01_S06
    02_SunkenStructures_U01_U04
    03_Containers_U05_U06
    04_IndustrialTanksAndPumps_U07_U09
    05_HarborCrane_U10
    06_PierAndDebris_U11_U12
    07_PhotoscanRocksAndCliffs
    08_SeaPlants_V01_V26
    09_FishAndBackgroundLife
    10_OceanSurfaceAndVolume
    11_SurfaceIslands_A01_A09
    12_CausticsAndLightShafts
    13_ParticlesAndBubbles_B01_B03
    14_LightingAndAudio
    15_ReviewCameras
```

- Thư mục: `Assets/Environments/SunkenHarbor/` gồm Scenes, Prefabs, Models/ImportData, Materials, Textures, Shaders, Scripts, Lighting, Settings, Audio và Documentation/Review.
- Giữ source GLB ngoài Assets nếu không có importer phù hợp, lưu manifest và asset Unity đã chuyển trong thư mục map. Không import hai file crane giống nhau thành hai bộ mesh/texture.
- Cấu hình gợi ý `SunkenHarborConfig`: seed, waterLevel, visibility/absorption, waterTint, surfaceWaveScale/speed, causticStrength/speed, plantCurrentStrength, particleDensity, bubbleRate, fishDensity, qualityLevel và audioVolume.
- Có thao tác editor riêng cho layout, water/atmosphere, materials, vegetation và chụp review. Thay nước không được dựng lại toàn bộ nhà, đảo và crane.
- Các cụm có ID để chỉnh riêng; scatter dùng seed cố định, không đổi ngẫu nhiên mỗi lần vào Play Mode.
- Persist mesh/material/texture và cấu hình. Không để scene chỉ hoạt động trong session hiện tại vì mesh procedural chưa được lưu.
- Khi cập nhật mesh asset, dùng cách sao chép dữ liệu mesh thực sự cập nhật vertices/indices/normals/bounds và làm mới collider; không mặc định một thao tác copy serialized sẽ đổi toàn bộ native mesh data.
- Backup cả scene lẫn asset managed sẽ bị ghi đè; backup scene đơn lẻ không đủ nếu scene cũ trỏ cùng mesh/material đang được sửa.

## 17. Tối ưu có ưu tiên hình ảnh

- Ưu tiên geometry/material chất lượng cao cho đá và cấu kiện gần camera; giảm LOD của đảo/núi xa, props nhỏ và cá.
- Rong, caustics và particles là nguồn overdraw; kiểm tra bằng công cụ phù hợp trước khi tăng số lượng lớn.
- Dùng chung mesh/material và gom patch cỏ biển khi hợp lý; không tạo một material riêng cho từng thân rong.
- Hạn chế đèn realtime có shadow và reflection camera; SSR/planar reflection phải có quality settings rõ ràng.
- Dùng noise/caustics có độ phân giải và mip phù hợp để tránh shimmering; kiểm tra khi quay camera, không chỉ ảnh đứng.
- Low quality giảm hạt, đàn cá, rong xa, rays và reflection cost trước; giữ màu nước, silhouette công trình và mặt biển hoạt động.
- Không khẳng định đạt 60 FPS, Android hay một cấu hình phần cứng khi chưa profile/chạy build thật.

## 18. Các bước triển khai khi được yêu cầu bắt đầu dựng

1. Backup và kiểm kê model/material thực tế; xem 5 scene đảo, bounds bồn và cấu trúc crane.
2. Tạo scene mới từ template chiến đấu, chốt scale Gundam và giữ anchor trên đáy Y=0.
3. Dựng seabed + bối cảnh đảo nối liền; đặt U01–U12 dưới dạng bố cục có ID.
4. Làm một bản thử water/underwater nhỏ để xác nhận URP trước khi hoàn thiện hàng trăm props.
5. Chốt góc nhìn gameplay, nhìn lên mặt biển và trên mặt nước; xử lý camera crossing trước khi thêm VFX dày.
6. Chuyển material riêng, thống nhất palette và cỡ vân đá; thêm cát bồi và biofilm.
7. Thêm rong, sinh vật bám, cá và props theo cụm có chủ đích; kiểm tra chồng xuyên.
8. Chỉnh ánh sáng, absorption, caustics, rays, particles và âm thanh qua các góc render.
9. Chạy chiến đấu thực tế, chuyển map và kiểm tra tài nguyên render; sửa lỗi trước khi tăng chất lượng tiếp.
10. Lưu scene/prefab/cấu hình, tạo thumbnail từ chính scene và cập nhật catalog.

Không dừng ở việc tạo plane xanh, vài nhà nghiêng và báo hoàn thiện. Không dùng số lượng renderer hoặc Console sạch để thay thế đánh giá hình ảnh.

## 19. Bộ kiểm tra cần có khi triển khai

- Scene load được và có đúng `sunken-harbor` trong catalog/build list; ID không trùng.
- Gundam đứng đúng đáy, Attack/Skill/Earth Prison hoàn tất và trở về anchor; camera không bị kẹt trong nhà/cần cẩu.
- Camera xoay đủ hướng, nhìn lên, zoom xa và đi qua mặt biển; không có cull sai mặt nước, màn hình đen, fog bật/tắt liên tục hoặc hiệu ứng mắc lại sau khi lên khỏi nước.
- Mặt nước nhìn từ trên có normal/reflection/foam đúng; nhìn từ dưới có chuyển động và vùng sáng hợp lý.
- Caustics không phủ HUD hoặc mặt dưới nhà vô lý; particles và bubble không cắt cứng qua geometry.
- Material không hồng, không mất normal/alpha, không kéo giãn atlas; mesh hữu hạn, bounds/collider đúng và không có z-fighting rõ.
- Reload scene và restart Play Mode vẫn giữ mesh/material/texture/config giống ảnh đã review.
- Hub → SunkenHarbor → Hub → Desert → SunkenHarbor: không nhân đôi camera/listener/EventSystem; nước, âm thanh và shader global không rò sang map khác.
- Kiểm tra camera review/reflection không kích hoạt pass dưới nước sai, không leak RenderTexture hoặc cảnh báo tài nguyên render.
- Ghi rõ các kiểm tra đã chạy, lỗi còn lại, thay thế asset và giới hạn chất lượng/hiệu năng thực tế.

## 20. Ảnh và tài liệu bàn giao

Chụp cùng một trạng thái cấu hình, không dùng các ảnh có chất lượng khác nhau mà không ghi nhãn:

- `SunkenHarbor_Gameplay.png`: robot, nền cát và công trình đọc rõ trong nước.
- `SunkenHarbor_OverviewUnderwater.png`: bố cục toàn cảng chìm và nhiều lớp địa hình.
- `SunkenHarbor_SurfaceLookUp.png`: góc ngẩng từ sân lên mặt sóng.
- `SunkenHarbor_Crane.png`: cần cẩu, bãi container và ánh sáng lọc qua nước.
- `SunkenHarbor_Ruins.png`: nhà sập/nghiêng với tiếp đất và vật liệu ngâm nước.
- `SunkenHarbor_SeabedDetail.png`: cát, đá, rong và caustics ở khoảng cách chơi.
- `SunkenHarbor_SurfaceIslands.png`: đảo nổi, mặt biển, bọt bờ và mây.
- Một đoạn review ngắn nếu công cụ cho phép: camera orbit dưới nước, nhìn lên, qua mặt nước và quay trở lại. Ghi FPS capture/quality thực tế; không gắn nhãn benchmark nếu chỉ là clip trình diễn.

Tài liệu: AssetManifest, WaterCompatibilityNotes, README_VI, Verification và AssetSources. Mọi texture sinh thêm phải có nguồn/prompt hoặc cách tạo; không nhận texture giả lập là scan thật.

## 21. Tiêu chí hình ảnh cuối cùng

Từ góc nhìn chiến đấu, trong vài giây người xem phải nhận ra: robot đang đứng dưới nước; phía trên là biển có ánh sáng và gợn; xung quanh là một cảng đã chìm có lịch sử sử dụng, hư hỏng và bị thiên nhiên biển xâm chiếm. Nước tạo chiều sâu nhưng không che mất gameplay. Đá, bê tông, kim loại và rong cùng thuộc một cảnh; các package không trông như được đặt cạnh nhau chưa chỉnh sửa.

Tránh các lỗi đã gặp ở map trước: núi/vòm dạng khối trơn lặp dáng, vân sin như gỗ, vật liệu kéo giãn, gồ bề mặt gây hạt đen, nhiều object nhưng cảnh vẫn trống do scale sai, mái/props lơ lửng và thay đổi chỉ tồn tại trong Editor session.

Mục tiêu là một chiến trường cảng chìm có chất lượng hình ảnh nhất quán, hoạt động trong game hiện tại và có cấu trúc dễ chỉnh tiếp; không cam kết giống 100% ảnh tham khảo hay chất lượng của một engine/package khác chưa được sử dụng.
