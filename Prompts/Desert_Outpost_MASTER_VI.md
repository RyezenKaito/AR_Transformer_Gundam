# PROMPT TRIỂN KHAI — DESERT OUTPOST / RED CANYON

Đây là prompt giao việc cho AI làm trực tiếp trong Unity. Các tọa độ, kích thước và số lượng dưới đây là thiết kế đề xuất, không phải dữ liệu đo từ ảnh. Hình tham khảo quyết định ngôn ngữ tạo hình; gameplay và asset thực tế quyết định điều chỉnh cuối cùng.

---

## 1. Vai trò, mục tiêu và phạm vi

Bạn là environment artist, level designer và Unity technical artist. Hãy nâng cấp scene `Assets/Environments/Desert/Scenes/Desert_Battle.unity` trong project `E:/My project` thành chiến trường sa mạc hoàn chỉnh cho Gundam.

Scene hiện tại là bố cục khởi đầu. Phải nâng cấp địa hình, chất liệu, công trình, cây cối và khí quyển; không coi việc đã có renderer, collider và catalog là bằng chứng hoàn thiện hình ảnh.

Mục tiêu hình ảnh: một khu định cư và trạm tiếp vận bị bỏ hoang trong hẻm núi sa thạch. Đường cát rộng chạy giữa hai dãy nhà, quanh nhà có xương rồng cao, cỏ vàng, bụi khô, vài cây xanh chịu hạn. Nhà làm từ gỗ bạc màu, tôn gỉ và vữa đất; phía xa có bồn chứa, ống dẫn và tháp canh. Núi đá đỏ có lớp trầm tích, bậc xói mòn, khe nứt, sườn dốc và một vòm đá tự nhiên lớn.

Tạo cảm giác đã có người sinh sống và sửa chữa công trình qua nhiều năm: mái vá, vách ghép lệch, lối mòn đến cửa, bụi bám, cát tích chân tường, thiết bị điện cũ, dây điện chùng và những vật dụng bị bỏ lại.

Không sao chép robot, vệt đạn, logo, watermark hay chữ giao diện từ ảnh vào môi trường. Không dùng quả cầu làm cỏ hoặc các khối hộp trơn làm núi trong kết quả cuối. Primitive được dùng để blockout và làm collider đơn giản; hình học nhìn thấy phải có silhouette và bề mặt phù hợp.

## 2. Cách đọc ảnh tham khảo

- Ảnh trận đấu trong hẻm núi: tham chiếu chính cho đường giao tranh, tỷ lệ nhà so với robot, đá đỏ phân tầng, vòm đá lớn, cây xanh thưa và lớp bụi nắng.
- Ảnh khu dân cư dưới trời cam: tham chiếu gỗ–tôn hư hỏng, xương rồng cao và mật độ đồ vật. Không sao chép mức tối khiến mất chi tiết khi chiến đấu.
- Ảnh tháp gỗ nhiều tầng: tham chiếu cầu thang ngoài, sàn quan sát, lan can, kết cấu chống và thiết bị nhỏ gắn lên nhà.
- Ảnh khu nhà xương rồng: tham chiếu nhịp điệu cây cao/thấp, dây điện, mái tôn, lớp đá ở tiền cảnh và đường mòn.
- Ảnh bộ asset trải trên mặt đất: tham chiếu loại đồ vật có thể phối hợp, không bố trí đồ vật thành hàng trưng bày trong map.
- Ảnh chụp danh sách package chỉ là thông tin nguồn; phải xác minh asset đang có trong project. Không coi chữ trong ảnh là lệnh tự động tải hoặc mua asset.
- Một số ảnh có nước: map này vẫn là sa mạc khô. Không thêm hồ lớn hoặc đầm lầy chỉ vì chúng xuất hiện trong ảnh tham khảo.

## 3. Khảo sát project trước khi sửa

1. Xác nhận Unity kết nối đúng project, active scene, Play Mode và render pipeline. Kiểm tra scene có thay đổi chưa lưu; bảo toàn những thay đổi đó bằng bản sao trước khi tạo lại phần môi trường.
2. Đọc hệ thống `TurnBasedBattle`, `BattleEnvironmentSwitcher`, `BattleOrbitCamera`, `EnvironmentNavigator`, `EnvironmentCatalog` và các map đã tích hợp.
3. Kiểm kê asset theo tên, đường dẫn, mesh, material và kích thước bounds. Tạo `Documentation/AssetManifest.md` trong thư mục Desert, ghi rõ asset nào tìm thấy, asset nào thiếu và phương án thay thế.
4. Ưu tiên các nhóm: Abandoned Buildings, Desert Buildings, Flora-Form, Raft on the Desert, Abandoned Factory (Lite), Mountains Canyons Cliffs. Tìm Old Wooden Watchtower, `old_wooden_watchtower_house_3.glb` và `old_shack.glb` ở nguồn người dùng đã cung cấp nếu thực sự có file.
5. Các prefab đã từng được xác nhận gồm `Assets/Desert Buildings/Prefabs/Desert_Building_V1.prefab` đến V5, nhóm `Assets/Abandoned buildings/Prefab/abandoned_buildings/` và `Assets/TirgamesAssets/Factory/Prefabs/`. Kiểm tra lại trước khi dùng.
6. `GrassDry_A.asset`, `GrassDry_B.asset`, `BushDry_A.asset`, `BushDry_B.asset` có thể là Mesh thay vì GameObject. Kiểm tra loại asset; nếu là Mesh, tạo GameObject với MeshFilter/MeshRenderer và material phù hợp. Không bỏ qua âm thầm vì không tìm được prefab.
7. Old Wooden Watchtower là tháp gỗ theo tên và ảnh. Chỉ thêm giằng hoặc chi tiết kim loại nếu hợp lý; không tự mô tả nó là tháp kim loại.
8. Giữ nguyên asset gốc. Tạo bản material URP riêng cho map nếu cần chuyển shader; bảo toàn albedo, normal, alpha clipping, cull mode và scale texture. Không đổi shader toàn bộ project.
9. Nếu package có phong cách low poly khác biệt, ưu tiên asset phù hợp nhất ở gần camera; asset đơn giản hơn có thể dùng ở xa. Không trộn các cây có mức độ chi tiết và độ bão hòa chênh lệch rõ rệt cạnh nhau.

## 4. Hệ tọa độ, tỷ lệ và đường đi

Thiết kế dưới đây dùng local layout origin O ở tâm khu giao tranh, Y = mặt đường, X chạy dọc đường chính, Z chạy ngang đường. Hướng +Z là dãy nhà phía bắc; −Z là phía nam.

Đặt `Desert_LayoutOrigin` tại tâm thực tế của hai bên chiến đấu. Với bố cục hiện có, tâm gần world `(4.85, 0, 0)`; phải đọc anchor hiện tại để xác nhận. Không dịch player/enemy chỉ để khớp con số này.

Mọi vị trí trong bảng là offset từ O, không phải world position tuyệt đối. Mặt cửa các nhà chủ yếu quay về đường ở Z = 0; không giả định cùng một yaw cho mọi prefab vì trục forward của mesh có thể khác nhau.

- Footprint map hoạt động: khoảng 110 × 90 m.
- Nền chiến đấu trung tâm: X từ −24 đến +24, Z từ −14 đến +14; giữ thông thoáng.
- Trục đường chính: X từ −55 đến +55, bề rộng 18–22 m, hai mép hơi uốn nhẹ ngoài vùng giao tranh.
- Làn phụ phía bắc: quanh Z = +16 đến +19, bề rộng tối thiểu 5–6 m.
- Làn phụ phía nam: quanh Z = −16 đến −19, bề rộng tối thiểu 5–6 m.
- Các hẻm kết nối giữa nhà: rộng 4–7 m tùy silhouette robot và cơ chế di chuyển hiện tại.
- Vành cảnh trung gian: bán kính khoảng 60–110 m; núi xa khoảng 140–280 m.
- Đơn vị mặc định 1 Unity unit ≈ 1 m. Đo chiều cao Gundam trước khi bố trí. Nếu tỷ lệ robot đòi hỏi thay đổi, áp dụng một hệ số chung cho bố cục; không kéo dãn từng căn nhà tùy tiện.
- Đường chính và khu spawn dùng mặt đất liên tục, dốc nhẹ, không có bậc trang trí làm mắc chân. Nền không phải một tấm hộp có mép cứng lộ ra.

Không thay đổi gameplay theo lượt thành game đi lại tự do trong phạm vi công việc này. Các tuyến đường phục vụ bố cục, di chuyển hiện có và khả năng mở rộng; giữ nguyên logic anchor, kỹ năng, HUD và lượt đánh.

## 5. Công trình — yêu cầu riêng cho từng căn

Các kích thước là bounds mục tiêu X × Z × cao, được phép điều chỉnh nhẹ sau khi xem prefab. Dịch cả cụm khi cần tránh giao nhau; giữ khu trung tâm trống và đường nối liên tục.

### H01 — Nhà gỗ hai tầng có ban công, điểm nhấn phía tây bắc

- Tâm offset `(−31, 0, +29)`, bounds khoảng 13 × 11 × 10 m; mặt tiền quay vào đường.
- Tầng dưới dùng vách gỗ bạc nâu xám, chân nhà có đá kê và cát tụ. Tầng trên lùi vào khoảng 1 m, ban công sâu 2 m chạy qua khoảng hai phần ba mặt tiền.
- Ban công có cột chống, giằng chéo và lan can ghép thanh ngang. Một đoạn lan can hỏng ngắn nhưng sàn phải có kết cấu đỡ nhìn hợp lý.
- Mái tôn dốc nhẹ, tấm sáng/tối xen nhau, mép gỉ đỏ nâu; có 2–3 miếng vá, không phủ gỉ đồng đều toàn mái.
- Một thang gỗ ngoài ở cạnh trái, một cửa tầng dưới tối, hai cửa sổ khác kích thước, một ô bị đóng ván chéo.
- Sân: hai thùng phuy, một pallet dựa tường, một tủ điện nhỏ, một ghế hỏng và 6–10 cụm cỏ khô. Đồ vật ở sát vách, không vươn vào làn phụ.
- Chi tiết kể chuyện: một dây điện nối sang H02; lối mòn sáng hơn dẫn từ đường đến cửa; cát tích phía khuất gió.

### H02 — Nhà vữa đất hai tầng, điểm nhấn kiến trúc sa mạc

- Tâm `(−13, 0, +30)`, bounds 10 × 10 × 9 m; dùng prefab Desert Buildings phù hợp.
- Vữa màu cát vàng nhạt, loang nâu ở chân, cạnh sứt lộ lớp xây bên dưới, một số vết nứt có hướng từ góc cửa.
- Mái bằng có tường chắn thấp; một mái che bằng vải hoặc tôn trước cửa. Vải bạc màu, rũ và rách ít ở mép.
- Cửa chính gỗ sẫm, cửa sổ thụt sâu tạo bóng, khung gỗ cũ. Một mặt bên tương đối trống để tương phản với mặt tiền nhiều chi tiết.
- Trên mái: một bồn nước nhỏ hoặc thùng chứa và một chảo anten nếu asset có; không xếp đầy mọi thiết bị.
- Sân có 3 thùng hàng, một cây xanh chịu hạn T01 và một cụm xương rồng thấp C07. Tránh cây chạm xuyên mái.

### H03 — Căn nhà bị sập một phần

- Tâm `(+4, 0, +30)`, bounds 12 × 10 × 7 m.
- Giữ một tường bên cao và phần mặt tiền còn cửa. Khoảng 35–45% mái hỏng, nhìn thấy xà gỗ và vùng nội thất tối.
- Đống đổ nát gồm 2–3 khối lớn, 5–9 mảnh trung bình và đá vụn nhỏ; phân bố rơi về một phía, không tạo vành tròn đều quanh nhà.
- Một cửa bị kẹt nghiêng; xà mái tựa lên tường hoặc đống gạch, không lơ lửng.
- Lối cạnh phía tây rộng tối thiểu 5 m. Nội thất chỉ dựng phần nhìn thấy: mảnh sàn, một tủ cũ, bụi nền; không dựng phòng kín vô ích.
- Cỏ khô mọc ở khe chân tường, một bụi xanh xám nhỏ ở phía có bóng.

### H04 — Lều sửa máy bằng tôn

- Tâm `(+23, 0, +29)`, bounds 12 × 9 × 6 m; mặt rộng quay ra đường.
- Khung gỗ/kim loại cũ, tường tôn gỉ từng mảng, mái lệch nhẹ nhưng có xà đỡ. Cửa gara mở một phần nhìn thấy máy móc bên trong.
- Một bàn sửa máy, một motor hoặc quạt công nghiệp, ba đoạn ống, hai bánh/lốp nếu có asset và bốn thùng phuy ở sân bên.
- Một biển nhỏ sơn bong, có thể dùng ký hiệu hoặc chữ ngắn tự tạo. Không dán chữ giao diện từ ảnh.
- Vệt dầu khô và đất nén chỉ quanh máy; không làm cả nền sa mạc bóng ướt.

### H05 — Nhà vữa thấp đầu đường phía đông

- Tâm `(+41, 0, +27)`, bounds 10 × 9 × 6 m.
- Mái bằng, khối nhà đơn giản, một bậc vào cửa và mái che hẹp. Màu vữa tối hơn H02, có các mảng vá mới/cũ khác nhẹ về sắc độ.
- Một cạnh nhà bị cát bồi 0.3–0.5 m. Dùng chuyển tiếp mềm, không để mound cắt thẳng qua cửa.
- Sân có ba bao vật tư hoặc kiện hàng, một thùng nước cũ và vài cây gai. Để cửa nhìn rõ từ cuối đường.

### H06 — Old Shack, căn nhà tôn gỗ thấp

- Tâm `(−35, 0, −28)`, bounds 10 × 8 × 5.5 m.
- Ưu tiên `old_shack.glb` nếu có và đã import đúng. Giữ đặc trưng model gốc; không bịa thêm chi tiết có sẵn trong mesh.
- Mái tôn đỏ gỉ không đều, tường gỗ/vữa cũ, cửa kim loại bạc màu. Chân nhà hơi nâng trên đá hoặc gỗ kê.
- Hai thùng phuy trước hiên, một bó ván dựa bên cạnh và lối mòn đi vòng hông. Một góc sân có cát và cỏ khô che mép móng.
- Không nhân bản nguyên căn này nhiều lần với cùng props để lấp map.

### H07 — Quầy trao đổi vật tư bỏ hoang

- Tâm `(−18, 0, −28)`, bounds 12 × 9 × 5 m.
- Nhà thấp có mái hiên sâu khoảng 2.5 m, cột gỗ không đều, vải bạt bạc nâu hoặc xanh xám. Mặt quầy mở tối bên trong.
- Trước quầy có 4–6 thùng gỗ khác cỡ, một bàn gỗ, một ghế đổ và tấm biển nghiêng. Tạo khoảng trống giữa đồ vật để người xem hiểu đây từng là nơi buôn bán.
- Bạt có vài chỗ rách nhỏ; dây bạt nối thật vào cột. Tránh bạt bay xuyên mái hoặc cửa.

### H08 — Nhà gỗ bỏ hoang với cửa đóng ván

- Tâm `(0, 0, −29)`, bounds 11 × 9 × 7 m.
- Khác H01: một tầng cao, mái chữ A, màu gỗ tối hơn, mặt tiền ít đồ hơn, cửa sổ bị đóng ván.
- Một phần vách bên mất tấm gỗ, nhìn thấy khung. Một cầu thang/hiên ngắn bị xô lệch; có nền đỡ bên dưới.
- Đặt một cây chết T05 cạnh nhà và một cụm xương rồng cột C11 ở phía sau.
- Khoảng trống trước cửa sạch hơn vùng mép nhà, tạo hướng nhìn từ đường vào trong.

### H09 — Nhà điều hành trạm tiếp vận

- Tâm `(+19, 0, −29)`, bounds 13 × 10 × 8 m.
- Khối vữa thấp kết hợp phần chắp vá bằng tôn; phía trên có sân mái, lan can và một anten.
- Cầu thang ngoài đặt ở cạnh đông, không quay thẳng chắn đường. Nếu chỉ là cảnh nền, cầu thang vẫn phải nối đúng mặt sàn về hình học.
- Thiết bị gồm một tủ điện sát tường, một máy lạnh cũ nếu có asset, dây điện chạy có điểm bám và hai tấm pin năng lượng nhỏ trên mái.
- Màu nhấn xanh bạc trên cửa/tủ; không dùng đèn neon rực.

### H10 — Kho vật tư hỏng mái

- Tâm `(+39, 0, −29)`, bounds 13 × 11 × 7 m.
- Kết cấu công nghiệp đơn giản từ Factory Lite, mái thiếu một vài tấm, tường tôn gỉ, cửa lớn trượt mở một phần.
- Bên trong thấy bóng máy móc và 2 cụm kiện hàng; khoảng tối vẫn giữ được texture, không thành hình chữ nhật đen đặc.
- Ngoài kho có khu xếp thùng theo cụm, một xe/khung máy cũ chỉ khi có asset phù hợp và ống nối về bồn chứa I01.

### H11 — Tháp canh gỗ, mốc đứng cao

- Tâm `(−47, 0, +29)`, footprint khoảng 8 × 9 m, cao 16–20 m tùy model và tỷ lệ robot.
- Ưu tiên model tháp đã có; tháp có cột đứng, giằng chéo, 2–3 chiếu nghỉ và sàn quan sát trên cùng.
- Cầu thang zigzag hoặc thang đứng phải nối các sàn hợp lý. Lan can có nhịp cột rõ, một đoạn hỏng nhỏ ở phía khuất đường.
- Trên sàn chỉ có chòi che nhỏ, một anten/chảo và một đèn tìm kiếm tắt. Không tạo đèn realtime soi liên tục nếu không cần.
- Dưới chân có hộp dụng cụ, gỗ thừa, bụi khô và một lối mòn đến H01. Không làm tháp lớn đến mức áp đảo toàn bộ dãy nhà.

### H12 — Trạm bơm bỏ hoang ở phía đông

- Tâm `(+50, 0, −8)`, bounds khoảng 8 × 12 × 7 m; nằm ngoài đoạn chiến đấu trung tâm.
- Nhà nhỏ bằng gỗ/tôn, một mái che ngắn, tủ điều khiển và ống dẫn thấp nối tới bồn công nghiệp.
- Mặt cửa quay chéo về đường. Bên ngoài có hai thùng phuy, một cuộn cáp và nền đất nén.
- Giữ khoảng cách với cuối đường để camera nhìn được tuyến thoát giữa các vách đá.

## 6. Các thực thể công nghiệp có vị trí và công năng

- I01 — Cụm ba bồn đứng tại `(+43, 0, −42)`: ba bồn cao 6–8 m, lệch nhau nhẹ về cao độ; ống nối dưới chân, khung đỡ tiếp đất, gỉ tập trung ở mối hàn và mép ống.
- I02 — Bồn nước lớn tại `(+49, 0, +41)`: đường kính 5–6 m, cao 10–12 m; thang ngoài, ống dẫn và bệ đỡ. Chỉ dùng chi tiết sẵn có hoặc dựng có kết cấu hợp lý.
- I03 — Máy phát tại `(+29, 0, +38)`: đặt cạnh H04, dưới mái che một phần; dây đi về tủ điện trên tường, có vết đất nén quanh bệ.
- I04 — Bãi vật tư tại `(+32, 0, −40)`: 7–10 thùng/kiện lớn và nhỏ, ba thùng phuy, một pallet nghiêng; cao tối đa 3 m để không biến thành tường kín.
- I05 — Mái xưởng phụ tại `(−42, 0, −40)`: footprint 12 × 7 m, 4–6 cột, tôn thiếu vài tấm, bên dưới 2 máy hoặc cụm phế liệu.
- I06 — Mạng điện: 6–8 cột dọc hai dãy nhà, cách mép đường an toàn; dây chùng nối có đầu bám. Cao hơn đỉnh di chuyển dự kiến của robot ở đoạn đi qua đường, hoặc chỉ chạy phía sau công trình.
- I07 — Hàng rào: các đoạn dài 3–7 m quanh bồn/kho, hở ở cửa ra vào, có cột đỡ. Không quây kín toàn map.
- Tổng thùng phuy gợi ý: 24–32, chia 8–10 cụm; thùng hàng 25–40 chia quanh công trình. Đây là mục tiêu bố cục, điều chỉnh theo mật độ và hiệu năng thực tế.

## 7. Cây có danh tính — không rải ngẫu nhiên toàn bản đồ

Chọn loài chịu hạn có asset phù hợp. Dáng cây chính là thân vặn nhẹ, tán thưa không đều, lá xanh olive/xám, nhiều khoảng hở nhìn xuyên. Không dùng hàng cây rừng thông hoặc cây nhiệt đới tán đặc để đại diện sa mạc.

### Các cây thân gỗ T01–T08

- T01 tại `(−18, 0, +21)`: cao 6–7 m, tán rộng 5 m, xanh olive; một nhánh nghiêng nhẹ ra đường nhưng không che tâm chiến đấu. Gốc có đất sẫm, 3 bụi khô và sỏi.
- T02 tại `(+16, 0, +40)`: cao 8–9 m, tán thưa rộng 6 m, thân hơi cong về đông; làm silhouette phía sau H03/H04. Lá có phần úa, không xanh bão hòa.
- T03 tại `(+36, 0, +18)`: cao 5 m, tán lệch một phía, nằm ngoài mép làn phụ. Không để tán xuyên mái H05.
- T04 tại `(−29, 0, −41)`: cao 7–8 m, là cây xanh thứ hai rõ nhất, tán hẹp; tạo đối trọng H11 ở phía đối diện.
- T05 tại `(+5, 0, −40)`: cây chết cao 5–6 m, 3–5 nhánh lớn gãy, không có lá; gỗ xám nâu, gốc ăn vào đất, kèm một nhánh khô nằm dưới.
- T06 tại `(+48, 0, +8)`: cao 5 m, bụi/cây nhiều thân, màu xanh xám ít lá; đóng khung cuối đường, không chắn lối.
- T07 tại `(−50, 0, −10)`: cây chết nhỏ cao 3–4 m, nghiêng nhẹ về đá; tạo tiền cảnh ở góc nhìn ngược.
- T08 tại `(+52, 0, −38)`: cây chịu hạn cao 6 m, thưa lá, chỉ nhìn rõ từ góc đông; tránh copy nguyên scale và rotation T01.

Tổng tỷ lệ thị giác: thực vật vàng/nâu/olive khô chiếm khoảng 80–90%; xanh lá còn sức sống chỉ 10–20%. Nếu asset không có cây thân gỗ phù hợp, báo rõ thiếu và dùng bụi chịu hạn lớn; không gắn nhãn một loài không giống hình.

### Xương rồng C01–C16 — điểm đặt chính

| ID | Offset X,Z | Cao | Tạo hình và chi tiết đi kèm |
|---|---|---|---|
| C01 | −44,+19 | 5.5 m | Cột cao hai nhánh lệch cao độ, nền sỏi, không nằm sát chân tháp |
| C02 | −39,+37 | 3.2 m | Cột đơn, hơi cong, đặt cạnh tường khuất gió |
| C03 | −24,+40 | 4.5 m | Một nhánh, đứng sau H01 tạo silhouette |
| C04 | −9,+21 | 2.4 m | Hai cột nhỏ lệch nhau, cạnh đá thấp |
| C05 | +9,+40 | 5.0 m | Cột cao thưa nhánh, không xuyên mái nhà sập |
| C06 | +20,+20 | 3.6 m | Ba thân thành cụm không đều, chừa lối vào H04 |
| C07 | −19,+37 | 0.8–1.3 m | Cụm xương rồng thấp nhiều bẹ nếu có asset |
| C08 | +46,+35 | 6.0 m | Cây cao làm mốc gần bồn, cách bồn ít nhất 2 m |
| C09 | −45,−23 | 4.0 m | Hai nhánh, xoay tránh trùng dáng C01 |
| C10 | −26,−39 | 2.2 m | Cột nhỏ cạnh H06, có cây con |
| C11 | +3,−40 | 4.8 m | Cột cao cạnh cây chết T05, không chạm cành |
| C12 | +13,−20 | 2.8 m | Cụm thấp cạnh đá, ngoài làn phụ |
| C13 | +30,−40 | 3.8 m | Cột đơn gần kho, tách khỏi đống thùng |
| C14 | +45,−20 | 5.3 m | Hai nhánh hướng chéo, đóng khung trạm bơm |
| C15 | −52,+4 | 3.0 m | Cây góc tây, cạnh bụi gai và đá vụn |
| C16 | +51,+18 | 2.0 m | Cụm thấp ở lối cuối đường, không chắn tuyến |

Mỗi cây có vùng tiếp đất hợp lý và biến thể màu nhẹ. Không kéo dãn một xương rồng nhỏ theo trục Y để giả cây cao. Nếu không có asset xương rồng, dựng mesh riêng có thân sườn dọc, đầu bo và nhánh nối tự nhiên; tránh các capsule trơn lộ rõ ở cự ly gần.

### Cỏ vàng, bụi khô và cây con

- Chia thành các patch có tên G01–G12 quanh từng công trình, thay vì scatter đồng đều.
- Mỗi patch khoảng 3–7 m, gồm 8–20 cụm cỏ nhỏ, 2–4 bụi khô và 1–3 đá nhỏ. Tổng số phụ thuộc mesh và overdraw; bắt đầu khoảng 180–300 cụm cỏ nhìn thấy trong toàn scene.
- Cỏ thường cao 0.2–0.6 m, vài bông cao đến 0.9 m; màu vàng rơm, nâu xám, olive úa, biến thiên nhẹ.
- Chân cỏ ăn vào đất vài cm; alpha clip đúng, không có nền chữ nhật, không đổ bóng đen quá đặc.
- Mật độ dày hơn ở chân tường, chân đá, sau hàng rào và các chỗ lõm. Thưa ở lối mòn. Rất thưa giữa khu chiến đấu; không tạo biển cỏ che chân robot.
- Dùng seed cố định, ví dụ 48271. Scatter theo mask loại trừ đường đi, cửa, cầu thang, collider máy và footprint nhà. Khi cập nhật không đổi toàn bộ vị trí vô cớ.
- Với cụm cây quan trọng, lưu transform cụ thể; không phụ thuộc random phát lại mỗi khi vào Play Mode.

## 8. Đồi, núi và vách đá — từng khối chủ đạo

Tất cả đá gần camera phải có hình dạng xói mòn, bề mặt phân tầng và chuyển tiếp chân đá. Không để người chơi thấy một dãy cube xoay ngẫu nhiên. Tận dụng mesh Mountains Canyons Cliffs nếu đã cài; nếu thiếu, tạo mesh địa hình có silhouette và topology phù hợp.

### M01 — Vách sa thạch chính phía bắc

- Tâm khoảng `(−20, 0, +84)`, dài 100 m, sâu 25–35 m, cao biến thiên 35–58 m.
- Khối lớn có 3–5 tầng bậc, các tầng không cùng chiều cao. Đỉnh lượn gãy nhẹ và vài hốc sâu; không phải tường đứng phẳng kéo dài.
- Mặt đá nâu đỏ cam, dải ngang sáng/tối mềm, các khe nứt đứng hiếm hơn dải ngang. Phần nhô có highlight, hốc sâu nâu trầm.
- Chân vách có talus đá vụn và sườn cát thoải, chuyển tiếp vào nền; nhà không bị mesh vách đá xuyên qua.

### M02 — Vòm đá tự nhiên, điểm nhấn của hẻm núi

- Tâm khoảng `(+16, 0, +103)`, rộng toàn khối 42–48 m, cao 38–45 m; lỗ vòm rộng 20–26 m, cao 18–24 m.
- Hai trụ không đối xứng, một phía dày hơn và có bậc. Cầu đá phía trên dày 7–10 m tùy hình.
- Lỗ mở phải là hình học thật nhìn xuyên được từ nhiều góc, thấy núi xa hoặc trời. Không dùng hình vòm đen dán lên mặt phẳng.
- Đường lớp trầm tích tiếp nối qua thân vòm. Mép có khuyết xói mòn; tránh cung tròn hoàn hảo như cổng xây.
- Từ camera gameplay dọc đường phải nhìn thấy ít nhất một phần vòm, không bị H01 hoặc tháp che hoàn toàn.

### M03 — Mesa phía nam, thấp hơn để cân bằng bầu trời

- Tâm `(+3, 0, −82)`, dài 90 m, cao 24–38 m.
- Đỉnh rộng, bậc sườn bị chia thành các khối; một khe sâu lệch về đông. Màu hơi xám và ít bão hòa hơn M01.
- Chừa khoảng mở để ánh nắng vào khu dân cư. Không tạo hẻm núi kín tối cả ngày.

### M04 — Cụm trụ đá phía tây

- Tâm `(−79, 0, +10)`, 3 trụ lớn cao khoảng 18, 26 và 33 m; khoảng cách không đều.
- Trụ có chân rộng, cổ khối thu nhẹ, mặt gãy tự nhiên và đá tảng ở chân. Không dùng nón nhọn trơn.
- Giữ khe mở nhìn về núi xa, tạo chiều sâu ở cuối đường phía tây.

### M05 — Khe núi phía đông

- Tâm `(+80, 0, −4)`, gồm hai vách lệch nhau, cao 28–42 m; khoảng mở giữa rộng 18–25 m.
- Đường cát hướng về khe nhưng ranh gameplay được xử lý bằng đá/dốc hợp lý ở xa. Không để camera nhìn xuyên xuống đáy scene.
- Thêm lớp bụi xa nhẹ trong khe để tách silhouette; không dựng một tấm sương trắng rõ hình chữ nhật.

### M06 — Hai đồi cát thấp

- M06A tâm `(−40, 0, +53)`, footprint khoảng 26 × 18 m, cao 3–5 m.
- M06B tâm `(+34, 0, −54)`, footprint khoảng 31 × 20 m, cao 4–6 m.
- Dáng bo, bất đối xứng, một mặt thoải và một mặt dốc hơn. Texture gợn cát nhỏ đúng tỷ lệ, không phóng thành sóng khổng lồ.
- Chân đồi hòa với mặt đất; che một phần móng công trình ở xa nhưng không tràn vào làn chính.

### M07 — Núi xa bao quanh đường chân trời

- 5–8 khối núi cách O khoảng 150–280 m, cao 60–110 m, chia 2–3 lớp khoảng cách.
- Hình dáng khác nhau: mesa thấp, đỉnh nhọn gãy và sườn núi dài. Không copy cùng mesh cùng scale thành một vòng đều.
- Giảm tương phản và bão hòa theo khoảng cách, tông nâu xám xanh nhẹ do khí quyển; giữ rõ sự khác biệt giữa núi gần và xa.
- Bảo đảm camera xoay 360 độ, ngẩng trời và zoom xa vẫn thấy bối cảnh liên tục ở các hướng.

### R01–R08 — Cụm đá gần

- R01 `(−40, +17)`: đá lớn cạnh H01, cao 2.2 m, chân có sỏi và cỏ.
- R02 `(−21, −18)`: cụm ba đá thấp 0.5–1.2 m, không chắn hẻm.
- R03 `(+13, +19)`: đá dẹt cạnh nhà sập, mặt nghiêng có lớp trầm tích.
- R04 `(+35, −19)`: hai đá làm ranh sân kho, cách đường đủ rộng.
- R05 `(−51, −34)`: cụm đá gãy 2–3 m, nối nhà với dốc xa.
- R06 `(+48, +12)`: đá cao 2.5 m gần T06, không che cửa H12.
- R07 `(−8, +41)`: đá nửa vùi cát, chỉ lộ 0.8 m.
- R08 `(+23, −43)`: talus nhỏ nối bãi vật tư và sườn núi.
- Kích thước/position bảng R dùng X,Z; Y lấy theo mặt đất. Các cụm có 1 đá chính, 2–4 đá phụ và sỏi; tránh rải đá đồng đều như họa tiết.

## 9. Mặt đất và chất liệu ở cự ly gần

- Nền dùng blend cát sáng, đất nén tối, sỏi/đá vụn và lớp bụi. Tạo biến thiên rộng trước rồi thêm chi tiết nhỏ; không để một texture tile lặp lộ rõ toàn map.
- Khu trung tâm tương đối phẳng, vẫn có biến đổi màu, vài rãnh cát nông và đá nhỏ; không dùng bumps mạnh gây cảm giác robot đứng trên mặt gồ ghề dù collider phẳng.
- Hai vệt bánh xe cũ chạy đứt quãng dọc X, cách nhau theo xe tưởng tượng; mờ dần ở đoạn gió phủ cát. Không dùng đường kẻ thẳng tối suốt 100 m.
- Mặt cát hạt nhỏ; specular vừa phải, roughness cao. Tắt hiệu ứng bóng ướt không phù hợp.
- Gỗ có thớ theo chiều thanh, bạc ở mặt phơi nắng, sẫm ở khe và mặt khuất; kim loại gỉ tại mép, rivet và vệt nước cũ.
- Dùng decal hoặc mesh patch có chủ đích cho đất bẩn, cát bồi, nứt và vệt dầu. Tránh z-fighting và mép decal rõ.
- Không bake bóng đen cứng vào albedo khiến hướng nắng mâu thuẫn. Normal map phải đúng import type và mức độ chi tiết.
- Kiểm tra texel density nhất quán khi đặt hai package cạnh nhau; sửa material bản sao của Desert nếu cần.

## 10. Ánh sáng, trời, bụi và chuyển động

- Chọn một thiết lập chính: buổi chiều khô, ánh nắng ấm, trời xanh nhạt với mây mỏng; góc nắng khoảng 25–40 độ trên đường chân trời. Không trộn đồng thời sky cam rực và ánh sáng xanh giữa trưa.
- Directional light có bóng mềm vừa phải; chỉnh exposure theo Gundam và vật liệu nhà. Mặt trong tối nhưng còn đọc được chi tiết.
- Ambient từ trời sáng trung tính hơi xanh, bounce từ cát ấm. Không nhuộm mọi thứ một màu cam phẳng.
- Fog màu bụi cát nhẹ, tập trung tác động tầm xa; nhân vật ở khoảng cách đánh không bị phủ mờ. Chọn thông số từ ảnh render thực tế, không áp một con số tùy ý rồi coi là xong.
- 4–6 emitter bụi sát đất ở viền khu dân cư và chân núi; hạt lớn mềm, alpha thấp, trôi cùng một hướng gió, có fade đầu/cuối đời và soft intersection khi pipeline hỗ trợ.
- Bụi trong khu giao tranh thưa hơn; không che animation, mục tiêu hoặc hiệu ứng kỹ năng. Không tạo khói núi lửa trong sa mạc.
- Gió nhẹ cho cỏ/bạt nếu có shader hoặc rig phù hợp; gốc cỏ đứng yên, đầu lá dao động nhỏ. Không làm cả thân cây và nhà rung đồng bộ.
- Heat haze chỉ dùng nhẹ ở xa và gần mặt đất nếu hỗ trợ, có thể tắt ở cấu hình thấp. Không làm méo HUD hoặc Gundam.
- Âm thanh có thể thêm gió sa mạc nhỏ và tiếng kim loại xa, dùng nguồn có sẵn hoặc có quyền sử dụng và ghi provenance. Không tự trích âm thanh từ ảnh/video tham khảo.

## 11. Collider, camera và tích hợp game

1. Giữ scene path `Assets/Environments/Desert/Scenes/Desert_Battle.unity`, catalog ID `desert-outpost`. Cập nhật entry hiện có; không tạo ID trùng hoặc selector riêng.
2. Dùng hệ thống `EnvironmentNavigator`/catalog hiện tại. Chuyển scene phải chờ load hoàn tất rồi mới đánh giá map đã sẵn sàng.
3. Giữ HUD, robot, gameplay và camera của scene chiến đấu. Không tạo thêm robot trang trí trùng player hoặc duplicate TurnBattleManager.
4. Tắt cả Renderer lẫn Collider của sàn tập và môi trường cũ trong scene này. Không để một sàn vô hình cũ đè lên mặt cát.
5. Xác nhận chân robot, hiệu ứng mặt đất và bóng chạm đúng nền. Chọn mặt đường tại cao độ anchor hiện có, không tùy tiện nâng sàn lên xuyên bàn chân.
6. Collider công trình có thể đơn giản hóa thành khối hợp lý, nhưng không bịt cửa/lối đi nhìn thấy thông thoáng nếu gameplay có thể đi vào. Đồ nhỏ và cây cỏ thường không cần collider.
7. Camera giữ xoay 360 độ, ngẩng lên trời và zoom xa theo yêu cầu đã có. Tinh chỉnh collision layer theo code thực tế. Không mặc định mọi object vào một layer nếu chưa đọc mask camera/gameplay.
8. Kiểm tra khung nhìn dọc đường, nhìn ngược, hai bên và nhìn lên. Không còn terrain edge, backface lộ hoặc núi bị far clip cắt mất.
9. Chỉ một camera gameplay và một AudioListener hoạt động. Camera review lưu disabled, không có AudioListener. Tránh EventSystem trùng khi về Hub.
10. Bảo toàn hoạt động 1v1/1v3, lock target, kỹ năng, né tránh và các phím điều khiển đã có. Không đổi cơ chế chiến đấu để phục vụ cảnh.

## 12. Cách tổ chức và công cụ chỉnh sửa

Thư mục quản lý: `Assets/Environments/Desert/` với `Scenes`, `Prefabs`, `Materials`, `Meshes`, `Textures`, `Lighting`, `Settings`, `Scripts`, `Documentation/Review`.

Hierarchy gợi ý:

```text
Desert_Battlefield
  Desert_LayoutOrigin
    01_TerrainAndRoad
    02_Buildings_H01_H12
    03_Industry_I01_I07
    04_Trees_T01_T08
    05_Cacti_C01_C16
    06_DryFlora_G01_G12
    07_Mountains_M01_M07
    08_RockClusters_R01_R08
    09_DecalsAndSmallProps
    10_DustAndWind
    11_Lighting
    12_ReviewCameras
```

- Gắn ID vào tên root từng cụm để có thể nói “dịch H04”, “tăng cỏ G07” hoặc “giảm chiều cao M02” mà không phải dựng lại cả map.
- Lưu layout/seed và tham số mật độ vào cấu hình hoặc dữ liệu editor. Tách đặt asset, chỉnh material và cập nhật FX thành thao tác riêng.
- Nếu viết builder, builder chỉ quản lý root/asset của Desert; không ghi đè map khác hay sửa source package. Có backup trước thao tác tái tạo phá thay đổi thủ công.
- Lưu prefab công trình/cụm độc lập khi cần để tái sử dụng. Mesh procedural phải persist thành asset, không chỉ tồn tại trong bộ nhớ Editor.
- Không sinh random layout mỗi lần chạy game. Không làm scene tự build lại trong Awake.

## 13. Tối ưu dựa trên scene thực tế

- Dùng LOD của package nếu có; đơn giản hóa cây và núi xa, hạn chế shadow của props nhỏ ở xa.
- Bật instancing cho material thích hợp, giữ dùng chung mesh/material cho các cụm lặp. Không tạo hàng trăm material chỉ để đổi sắc độ nhỏ.
- Đánh dấu static đúng đối tượng; cây/FX có chuyển động không đánh dấu bằng những tùy chọn gây sai kết quả render.
- Kiểm soát alpha overdraw của cỏ và bụi; tránh các lớp particle lớn chồng nhau phủ màn hình.
- Giữ layer, culling và far clip đủ cho núi xa nhưng không tăng vô hạn. Profile trước khi khẳng định đạt một mức FPS.
- Tạo cấu hình giảm cỏ, bụi và bóng cho máy yếu; không hạ silhouette nhà và núi chủ đạo xuống mức blockout.

## 14. Trình tự thực hiện và tiêu chí hoàn thành

Thực hiện lần lượt, không dừng ở bảng kế hoạch:

1. Khảo sát asset, backup scene và ghi manifest.
2. Chốt đường chính, vùng giao tranh, mặt đất và layout H01–H12 theo tỷ lệ Gundam.
3. Dựng M01–M07 để có bố cục hẻm núi và vòm đá thật; thay các khối hộp cảnh nền cũ bằng mesh phù hợp.
4. Hoàn thiện từng căn nhà theo mô tả, sau đó mới thêm I01–I07 và props nhỏ.
5. Đặt cây T01–T08, xương rồng C01–C16, patch cỏ G01–G12 và đá R01–R08. Điều chỉnh tiếp đất và loại bỏ giao nhau.
6. Làm material, ánh sáng, sky và bụi. Render nhiều hướng để chỉnh sự thống nhất giữa các package.
7. Kiểm tra game trong Play Mode: robot đúng mặt đất, camera hoạt động, kỹ năng vẫn dùng được, không có duplicate runtime object.
8. Kiểm tra Hub → Desert → Hub → một map khác → Desert; chờ mỗi lần load xong. Xác nhận catalog và build list đúng.
9. Kiểm tra Console và shader, collider, texture mất, mesh lỗi và lỗi culling. Sửa lỗi trực tiếp do thay đổi gây ra.
10. Lưu scene, prefab và asset; tạo thumbnail từ chính map mới và gắn vào entry `desert-outpost`.

Các ảnh cần bàn giao:
- `Desert_Gameplay.png`: từ camera chiến đấu, thấy rõ robot, đường cát, nhà và một phần vòm đá.
- `Desert_StreetReverse.png`: góc nhìn ngược trên cùng tuyến đường.
- `Desert_Overview.png`: toàn cảnh đọc được khu dân cư, công nghiệp và các lớp địa hình.
- `Desert_H01_H02.png`: chi tiết ban công, vữa đất và đồ vật tiếp đất.
- `Desert_H04_H10.png`: mái tôn, máy móc, kho và bồn chứa.
- `Desert_Flora.png`: xương rồng, cây chịu hạn và cỏ khô ở cự ly chơi.
- `Desert_Arch.png`: vòm đá có lỗ mở thật và các lớp núi xa.

Viết báo cáo ngắn về asset đã dùng, asset thiếu, thay thế đã thực hiện và kiểm tra thực sự đã chạy. Không ghi “100% giống ảnh”, “đã hoàn thiện” hoặc “60 FPS” nếu chưa có bằng chứng phù hợp. Mọi phần còn thiếu như xương rồng, tháp hoặc vòm đá phải được nêu rõ.

Điều kiện hoàn thành hình ảnh: nhìn từ vị trí chiến đấu phải nhận ra đây là khu định cư bỏ hoang trong hẻm núi sa thạch, có nhà khác nhau về silhouette và vật liệu, cây khô/xương rồng bố trí có chủ đích, đường cát chơi được và hậu cảnh có chiều sâu. Số lượng object hoặc Console không lỗi không thay thế cho việc xem và chỉnh ảnh render cuối.
