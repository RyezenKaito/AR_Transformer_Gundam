# PROMPT THEO GIAI ĐOẠN — BIOGEAR LAB

Đọc `01_MASTER_PROMPT_VI.md` làm brief cố định. Gửi các prompt dưới đây theo thứ tự sau khi đã đính kèm reference. Mỗi prompt kế thừa toàn bộ ràng buộc của brief. Những điểm chưa có bằng chứng phải được lưu trong sổ giả định. Sau giai đoạn dựng map, làm giai đoạn 9 để đưa map vào luồng chọn nhiều môi trường.

Các checkpoint giúp phát hiện sai tỷ lệ sớm, không bắt buộc người dùng phê duyệt từng bước. Nếu đã được giao dựng toàn bộ map, AI tiếp tục sau khi tự kiểm tra checkpoint, trừ khi có quyết định thực sự cần người dùng hoặc quyền riêng của công cụ.

## Giai đoạn 0 — Đọc project và lập bản tái dựng

```text
Thực hiện giai đoạn 0 của brief Biogear Lab.

1. Đọc Unity version, render pipeline, packages, scene hiện tại, thư mục asset và hướng dẫn project. Xác nhận có thể gọi Unity Editor. Chỉ đọc trạng thái project ở bước này.
2. Xem 10 ảnh gốc và những vùng gameplay/render trong video. Đặc biệt xem tổng thể 00:09, concept 01:09, nhà vòm 01:45 và nội thất 02:09–02:15. Không coi concept/blockout là bản cuối.
3. Tạo bảng reference: tên ảnh, góc nhìn, landmark thấy rõ, chi tiết bị che, độ tin cậy. Lập danh sách các phần chưa đủ bằng chứng.
4. Vẽ sơ đồ top-down tái dựng trên XZ, đặt origin ở tâm hub, +Z về tháp, +X về nhà vòm. Đánh dấu các đường đi đã nhìn thấy bằng nét liền và đường nối suy luận bằng nét đứt. Tách chú thích camera khỏi đường đi.
5. Đặt thông số seed R=16 m trong layout config. Dùng tỷ lệ với R; đánh dấu mọi kích thước là provisional. Tạo bảng cao độ và sơ đồ quan hệ đường ngoài → hub → lòng chảo → tầng dưới → vòm → chân tháp.
6. Lập asset manifest: tên, nhóm, số lượng nhìn thấy hoặc chưa xác định, reference, cần mesh mới hay có thể dùng asset đang có, trạng thái hoàn thiện.
7. Lập kế hoạch camera matching với 8 camera theo brief. Chọn góc toàn cảnh và góc central front làm anchor đầu tiên.

Đầu ra: Documentation/ReferenceIndex.md, Assumptions.md, AssetManifest, LayoutPlan, config seed và ghi chú kiến trúc nêu scene hub/map/gameplay hiện có cùng điểm tích hợp. Báo các mâu thuẫn reference có ảnh hưởng tới hình học. Tiếp tục giai đoạn 1 khi đã có đủ để dựng blockout.
```

## Giai đoạn 1 — Blockout đúng không gian

```text
Triển khai blockout Biogear Lab trong scene mới, dùng layout config đã lập.

Tạo đủ các khối: vòng hub có lỗ giữa, lòng chảo thấp, bệ mục tiêu, đĩa vành khuyên trên cao, cầu đôi về chân tháp, tháp ba nhánh đơn giản, nhà vòm hở đỉnh, 5 bệ nước, đường ngoài, cầu hướng tâm, lối dưới, đá bao ngoài và vòm đá lớn.

Dùng vật liệu phẳng phân loại theo nhóm. Chỉ dùng primitive cho những phần chưa cần lỗ/cong; tạo mesh blockout đơn giản cho vành khuyên, vòm hở, ramp và vòm đá. Không dùng collider/mesh hộp kín che phần rỗng.

Tạo 8 review camera, nhưng tập trung solve Overview và CentralFront trước. Lưu FOV, aspect, transform và đường dẫn reference vào config camera. Tạo thêm camera top-down kỹ thuật để xem cấu trúc.

Kiểm tra cao độ và khoảng trống bằng capsule đại diện mech. Mỗi cầu/ramp phải nối được hai mặt sàn. Các tuyến chưa xác nhận đặt trong InferredGeometry và hiển thị khác màu ở chế độ debug.

Lưu scene và mesh asset. Xuất ảnh overview, front, top-down và một ảnh nhìn xuyên tầng dưới. Ghi 5 sai lệch hình khối lớn nhất so với ảnh rồi sửa ngay. Kết thúc blockout khi các landmark lớn đọc được và đường đi không đứt; chưa đắp texture hoặc chi tiết nhỏ.
```

## Giai đoạn 2 — Hub và đĩa treo

```text
Hoàn thiện hình học CentralArena và UpperDiskAndTwinBridge theo ảnh tropical03.webp, vadym-krukovets-04.webp và hai model sheet.

CentralArena cần: vành sàn có độ dày, mép/nẹp, lòng chảo chia mảng quạt, bệ dưới đáy, cửa và lối xuyên thành, các cầu hướng tâm và dốc xuống. Phân biệt bề mặt đi được với vỏ máy và lớp trang trí. Tạo seam theo nhịp panel, không cắt mesh ngẫu nhiên quá dày.

UpperDisk cần: thân dẹt vát mép, vành trắng, mặt tối, lỗ giữa và thành lỗ, các cánh đứng nghiêng, lan can cam theo đoạn, rãnh mặt dưới và vòng đèn cyan. Cầu đôi phải nối thực tới phía chân tháp và giữ khoảng âm giữa hai nhánh; không thêm cột giữa lòng chảo nếu reference không có.

Tách mesh UpperDisk_Body, UpperDisk_DarkDeck, UpperDisk_Fins, UpperDisk_Rails, UpperDisk_Underside, TwinBridge_L/R và Supports. Chọn số segment vòng đủ mượt ở 1920px nhưng giữ silhouette hơi stylized; thử 48–64 segment làm seed rồi điều chỉnh theo camera, không coi đó là topology gốc.

Dùng material phẳng có màu đại diện. Chụp Front, UnderDisk và Overview. Khớp kích thước lỗ, khoảng hở từ đĩa tới hub, độ dày vành và vị trí cầu trước khi làm chữ EVERLIFE. Bàn giao prefab và mesh có thể mở lại, kèm các tham số tỷ lệ đã điều chỉnh.
```

## Giai đoạn 3 — Tháp ba nhánh và khu chân tháp

```text
Tạo TowerComplex bám model sheet locationplariummecharena-003.webp và các góc tropical03.webp, locationplariummecharena-005.webp, tropical.webp.

Tạo riêng 3 nhánh thuôn, lõi kỹ thuật tối, đế và reactor tròn nghiêng. Giữ đầu nhánh hẹp và cao, chân nhánh cong/loe. Reactor có vành cam, gông tối, vòng cyan, tâm cam, các vạch sáng theo chu vi. Có liên kết ngang nhỏ giữa các nhánh theo model sheet.

Lắp công trình vào đá. Dựng cửa GATE01, trụ đỡ phía trước và đầu cầu đôi. Quan sát từ góc side và overview để không kéo dài cầu quá mức hoặc để tháp lọt sau đĩa.

Không đánh đổi độ cao tháp để vừa khung hình: điều chỉnh camera trước nếu khung hình sai. Không làm reactor như một bánh răng fantasy, không thêm vũ khí/pháo/ăng ten không có reference.

Xuất ảnh silhouette trắng đen và ảnh màu phẳng từ TowerSide, CentralFront, Overview. Kiểm tra đỉnh tháp, tâm reactor và hai đầu cầu là những landmark đo được. Lưu prefab modular và báo phần mặt sau suy luận.
```

## Giai đoạn 4 — Nhà vòm và hành lang

```text
Hoàn thiện DomeLab và Corridors theo ảnh tropical02/04/06, video khoảng 01:45, 02:09 và 02:15.

Tạo vòm thấp có lỗ đỉnh tròn, sườn kinh tuyến tối, giằng chéo, panel trắng và lime. Vòm phải có vỏ trong đủ nhìn từ dưới, vành chân, cửa chính và các cửa phụ thực sự xuyên qua tường. Không làm sphere kính kín.

Nội thất cần bệ tròn giữa sàn, các vòng panel, cửa có nhãn GATE03/GATE04 theo reference, dải vàng thấp, giằng tam giác quanh chân mái, bể kính cong có rong và console dưới bể. Bố trí lỗ đỉnh cùng cột sáng qua tâm.

Tạo kit corridor gồm portal nhiều cạnh, straight, ramp, ceiling, wall panel, floor panel, light fixture và transition. Bắt buộc có rãnh sàn giữa, vệt vàng chân tường, đèn trần cyan đôi, cửa sổ/đèn nghiêng hai bên và sàn dốc. Giữ tiết diện đủ để capsule đại diện mech đi qua.

Đối chiếu DomeApproach để sửa mặt ngoài, DomeInterior để sửa độ cong/trần/bể, Corridor để sửa tiết diện/ramp/nhịp khung. Xem từ cả hai phía cửa để phát hiện normals sai, z-fighting, lỗ hở và collider bịt đường.

Lưu prefab kết nối được theo socket hoặc thông số đầu nối. Không đặt một hành lang trang trí không nối vào mạng đường đi chỉ để khớp ảnh đơn lẻ.
```

## Giai đoạn 5 — Đá, đường ngoài, nước và bệ

```text
Hoàn thiện phần đảo và liên hệ giữa các công trình.

Đặt các cụm vách đá lớn theo reference tổng thể; chừa hướng nhìn biển, tạo các cổng/vòm đá mà đường đi chui qua. Dùng hình khối gãy mảng lớn, cỏ nằm trên mặt trên, vách đứng lộ đá. Tạo vùng núi xa riêng với giảm tương phản theo khoảng cách.

Hoàn thiện vòng đường ngoài và các nhánh tới hub, vòm, khu tháp. Làm panel sàn, thành sàn và các chỗ bẻ cong theo đá. Lắp nẹp vàng/cam và giằng tối có nhịp nhất quán, không tạo hàng rào kín khắp map nếu reference chừa khoảng mở.

Tạo prefab bệ nước vuông với thân cam đồng, đai đen, mặt xám vát, viền trắng và dấu lõm thập bốn cánh. Đặt 5 bệ theo cung nông, bệ giữa nối về cửa vòm; solve lại từ video 01:45 và ảnh tropical02.

Làm shader nước URP với shallow/deep color, sóng normal nhỏ, foam/highlight có pattern stylized, vùng tiếp xúc đá/bệ. Nếu dùng depth/opaque texture, kiểm tra cài đặt thật sự của renderer và camera; shader phải có giải pháp phù hợp với pipeline hiện tại, không chỉ copy từ HDRP.

Tạo sky profile ban ngày với mây tích khối lớn, vùng mặt trời sáng và horizon khớp nền núi/biển. Giữ được các khoảng âm giữa vòm đá và landmark. Chụp Overview và DomeApproach trước/sau, sửa bố cục thiên nhiên trước khi thêm cây nhỏ.
```

## Giai đoạn 6 — Props, UV và vật liệu

```text
Tạo bộ props và vật liệu cuối cho Biogear Lab.

Các prefab bắt buộc: cover trắng lưng ghế, cover cam đôi, crate xám có giằng, power pod đứng/nghiêng, tube rack, gate marker, panel sàn rời, console và hazard trim. Dựng một prefab chuẩn mỗi loại rồi lặp với biến thể có căn cứ; đối chiếu góc gameplay trước khi chốt vị trí.

Đặt cover theo reference, giữ cửa và sightline. Những pod trên lòng chảo phải có góc nghiêng theo mặt dốc và chân tiếp xúc chắc, không nổi. Gate marker dùng số đúng vị trí, không sinh ngẫu nhiên.

Làm UV và atlas/trim sheet theo module; gân lớn dùng mesh, rãnh nhỏ dùng normal. Chuẩn hóa texel density ở vùng chơi, dùng texture lớn hơn cho hero nếu cần. Không dùng ảnh chụp có ánh sáng/bóng mạnh làm albedo không xử lý.

Vật liệu: sơn trắng xanh không phải chrome, khung than tối, kim loại trần chỉ ở mask phù hợp, cam vàng rõ nhưng không neon toàn bộ. Cyan emission có mask nhỏ. Đá/cỏ stylized với variation lớn và noise nhẹ. Dùng bảng màu seed trong brief rồi cân lại dưới lighting đã khóa.

Kiểm tra importer sRGB/linear, normal map và channel metallic/smoothness/AO theo shader thực tế. Giữ smoothness khác nhau giữa sơn, đá, kính và nước. Thêm EVERLIFE, GATE và số cổng bằng decal/atlas, không thêm watermark/footer ảnh vào cảnh.

Xuất material swatches và ảnh cận 6 loại props. Tất cả material phải chạy trong URP hiện tại, không missing texture hoặc material hồng.
```

## Giai đoạn 7 — Ánh sáng, VFX và khớp camera

```text
Thực hiện pass lighting và camera matching, dùng bộ review camera đã khóa.

Khớp ngoại thất nắng sáng, ambient xanh và đá có bóng lạnh; nội thất cyan rõ nhưng còn vùng tối đọc được gân khung. Dùng lightmap/mixed/realtime theo điều kiện project và kiểm tra thực tế. Emission và Bloom tạo cảm giác sáng, nhưng cần ánh sáng/bake riêng nếu phải hắt sáng lên sàn/tường.

Tạo cột sáng với lõi mảnh, lớp glow trong suốt và ring chân. Kiểm tra bị che đúng bởi sàn/đĩa/đá, không xuyên tất cả vật thể như overlay. Kiểm tra sorting giữa nước, bể kính và beam ở cả xa/gần.

Thử tone mapping Neutral và ACES rồi chọn bằng ảnh đối chiếu; cố định profile cho review. Bloom vừa đủ đọc đèn và mặt trời, không xóa viền panel. Tắt DOF/motion blur khi kiểm tra.

Với mỗi camera: khớp crop/aspect và horizon trước, tiếp đến FOV/pose, rồi mới chỉnh transform landmark. Không scale sai từng asset để bù một camera đang sai. Ghi lại mọi thay đổi camera để tái lập ảnh.

Xuất ít nhất 8 ảnh 1920x1080 hoặc resolution tương ứng vùng crop. Tạo cặp reference/render và overlay 50% nếu có công cụ. Chỉ dùng pixel error khi đã loại HUD/footer/người thuyết trình và khớp viewport; không lấy SSIM/PSNR làm kết luận duy nhất về độ giống.

Lập 10 sai lệch lớn nhất, ưu tiên theo thứ tự layout → silhouette → cao độ → vật liệu → lighting → chi tiết. Sửa và chụp lại cho đến khi hết lỗi lớn hoặc gặp thiếu dữ liệu thực sự. Báo rõ phần chưa thể xác định.
```

## Giai đoạn 8 — Hoàn thiện scene có thể sử dụng

```text
Hoàn thiện bản tái dựng để có thể mở, kiểm tra và chỉnh sửa trong Unity.

1. Lưu scene/prefab/mesh/material/config và mọi texture đang dùng. Kiểm tra đóng/mở scene vẫn giữ đúng asset, không phụ thuộc object procedural chỉ nằm trong RAM.
2. Thêm collider phù hợp, kiểm tra capsule đi từ đường ngoài vào hub, xuống lòng chảo, qua tầng dưới và vào vòm. Ghi rõ đoạn nào là suy luận. Không coi khả năng đi lại này là bằng chứng cơ chế game gốc.
3. Nếu gameplay đã yêu cầu navigation: bake NavMesh theo capsule và slope đã chọn, kiểm tra tầng trên/dưới không nối nhầm, không đưa mặt nước vào vùng đi được.
4. Giảm material slot, instance props lặp, LOD cho phần xa và hạn chế transparent overdraw. Chỉ chốt budget/FPS khi có thiết bị đích và kết quả profile.
5. Kiểm tra Console và asset missing; sửa lỗi thuộc scene mới. Báo riêng lỗi có sẵn ngoài phạm vi.
6. Tạo README hướng dẫn mở scene, rebuild, đổi layout config, chọn camera, bật/tắt VFX, phần nào đang inferred và cách thay asset.
7. Bàn giao bộ ảnh so sánh cùng bảng tiêu chí. Mỗi tiêu chí ghi đạt/chưa đạt và ảnh chứng minh. Không báo '100%' nếu còn thiếu hình học/mặt khuất/texture gốc.

Nếu đang chỉ được giao dựng môi trường thì dừng ở scene môi trường hoàn chỉnh; hệ chiến đấu và luật chiếm điểm cần brief riêng.
```

## Giai đoạn 9 — Đổi qua lại giữa các môi trường

```text
Triển khai hệ thống nhiều môi trường trong Assets/EnvironmentSystem/ theo mục 10 của 01_MASTER_PROMPT_VI.md. Trước khi sửa, đọc game flow và danh sách scene hiện có để giữ nguyên điểm vào battle và scene gameplay.

1. Tạo EnvironmentHub scene và UI chọn môi trường dạng card/list dùng chung. Đăng ký BiogearLab vào catalog. Tạo thêm TestEnvironment scene tối giản để xác nhận đổi được giữa ít nhất hai môi trường; đánh dấu scene thử rõ ràng, và không hiện nó trong UI cuối nếu không yêu cầu.
2. Tạo EnvironmentDefinition và EnvironmentCatalog: ID ổn định, tên, mô tả, scene, thumbnail/fallback, thứ tự, trạng thái available. Tham chiếu scene an toàn khi rename. Validation/editor tool kiểm tra scene được đưa vào Build Settings/Build Profiles; selector không được hardcode từng map.
3. Chọn một chiến lược load: LoadSceneAsync Single có đường quay lại hub, hoặc additive với lifecycle rõ ràng. Không để hai map chơi được cùng lúc. Khi load, khóa tương tác, hiện loading state, chặn nhấn lặp và báo lỗi nếu scene không tồn tại.
4. Giữ service nền thực sự cần qua scene change trong bootstrap/persistent root chỉ một lần. Ngăn duplicate EventSystem, AudioListener, camera, input router và subscriptions; dọn object tạm khi rời scene.
5. Thêm nút về EnvironmentHub trong BiogearLab và test map. Nếu game hiện tại đã có launcher/main menu, nối lựa chọn ở điểm thích hợp thay vì thay menu hoặc đổi startup scene âm thầm.
6. Lưu ID môi trường gần nhất được chọn, thêm fallback khi catalog thay đổi; không lưu thông tin gameplay không liên quan.
7. Tạo Tools/Environment System/Validate Catalog để báo ID trùng, scene thiếu/chưa build, definition null, thumbnail thiếu và build profile thiếu hub.
8. Tạo Assets/EnvironmentSystem/Documentation/AddingAnEnvironment.md: tạo folder và scene mới, đặt camera/spawn point, chụp thumbnail, tạo definition, đăng ký catalog, thêm scene vào build profile, thêm nút quay hub và chạy validation. Map mới phải thêm được mà không sửa EnvironmentSelector.
9. Kiểm tra Editor: Hub → BiogearLab → Hub → TestEnvironment → Hub; thử nhấn liên tục, thiếu thumbnail và ID không còn trong catalog. Kiểm tra không nhân đôi hoặc mất camera, input, audio hay EventSystem. Đọc Console và báo kết quả thật.

Bàn giao hub scene, catalog, definition, selector, validation tool và hướng dẫn thêm map. Nêu rõ điểm đi vào selector từ flow hiện tại. Không sửa hệ chiến đấu.
```

## Prompt sửa sai sau một lần dựng chưa đạt

```text
Đánh giá lại bản dựng Biogear Lab theo reference. Chưa thêm chi tiết mới.

Chụp Overview, CentralFront, UnderDisk, DomeApproach, DomeInterior và Corridor. Với mỗi ảnh, chỉ ra các sai lệch cụ thể bằng vật thể và hướng sửa: ví dụ đĩa quá dày, tháp quá thấp, vòm quá cao, cầu đôi bị nhập một, lòng chảo thiếu đường xuống, đường ngoài sai cao độ, đá che mất silhouette, nước thiếu pattern, sky thiếu mây lớn.

Phân biệt lỗi camera với lỗi mesh: giữ mesh, thử solve camera bằng 6–12 landmark. Nếu không thể khớp nhiều góc bằng cùng hình học thì mới sửa layout/tỷ lệ. Lưu phiên bản config trước và sau sửa.

Sửa theo mức ảnh hưởng lớn nhất. Không tăng Bloom, fog, saturation hoặc rải props để che lỗi bố cục. Không thay thế reference bằng bản thiết kế tự sáng tác. Xuất cặp ảnh trước/sau và giải thích những sai lệch còn lại bằng bằng chứng.
```

## Brief chuyên biệt cho DCC / công cụ tạo mô hình

Các brief dưới đây mô tả asset để dựng thủ công, procedural hoặc dùng công cụ tạo mesh. Chúng không tự thực thi việc sinh asset. Một lượt text-to-3D thường cần sửa lại topology, tỷ lệ, UV và các lỗ xuyên trước khi dùng cho scene.

### A. Đĩa và cầu đôi

```text
Create a game-ready stylized sci-fi annular platform matching the attached Biogear Lab central disk model sheet. Thin beveled ivory outer hull, broad dark graphite top deck, a true open circular hole through the center, segmented orange two-bar railings, inclined trapezoidal fins with ivory outer faces and dark inset mechanical inner faces, thin orange hull trim, cyan underside ring and small edge lamps. Two parallel raised bridge arms extend from the rear toward the tower, leaving a visible gap between them. Preserve the exact silhouette, negative spaces and relative proportions of the reference. Separate body, deck, rails, fins, underside and bridge arms. No ground base, no environment, no characters, no solid cap across the center opening. Supply clean UVs and editable meshes; branding goes in a separate texture/decal layer. Scale, pivot and export format follow the Unity scene specification.
```

### B. Tháp

```text
Reconstruct the attached Biogear Lab central building as modular game-ready geometry. Three tall slender tapered ivory blades surround a dark mechanical core; blades flare and curve near their heavy lower mounts, with long orange-gold insets near the upper portion. A large tilted circular reactor sits at the front base, with an orange outer housing, dark radial clamps, a cyan luminous ring and an orange center. Include the small crosspiece between the upper blades and the stepped ivory base, front gate and bridge connection. Use the reference model sheet to preserve blade spacing and silhouette. Separate blades, core, reactor, base and structural supports. Stylized broad shapes and controlled bevels, restrained panel detail, no extra antennas or weapons. Provide a clean low-to-medium polygon editable mesh with UVs and material regions; do not generate the surrounding island as part of the asset.
```

### C. Nhà vòm

```text
Reconstruct a squat open-top sci-fi laboratory dome from the attached exterior and interior references. Dark meridional ribs, horizontal rings and diagonal braces divide opaque or softly translucent ivory and yellow-lime shell panels. Large circular opening at the top, heavy dark base ring, broad angular arched front doorway with yellow-black hazard trim, genuinely open entrances. Include an interior shell with visible ribs and ring truss, a circular segmented floor, a separate central objective pedestal, curved turquoise aquarium window and low console. Keep exterior, interior, ribs, panels, doors and floor as separate editable parts. Match the dome height-to-width ratio and panel arrangement from references. No closed glass sphere, no generic geodesic greenhouse, no sealed doorway, no baked lighting in the color texture.
```

### D. Hành lang

```text
Build a modular stylized sci-fi laboratory corridor kit matching the attached cyan-lit ramp image. Angular arched cross-section, thick dark structural ribs, blue-gray large wall panels, a continuous low yellow stripe, two wide polygon-seamed floor panels separated by a central dark channel, paired cyan ceiling strips around a center vent, small floor lamps, slanted trapezoidal cyan side fixtures with yellow-lime accents. Deliver straight, ramp, doorway and transition modules with matching cross-section and connection sockets. Real openings, complete visible interior faces and useful UVs. Do not flatten the ramp or replace the floor pattern with uniform hexagon tiles. Modules must support a configurable mech clearance and remain editable in Unity.
```

### E. Bệ nước

```text
Create one square beveled sci-fi water-platform module from the Biogear Lab reference. Dark gray flat deck, recessed four-lobed cross emblem in the center, thin white inset perimeter marking, small yellow corner accents, thick orange-copper sidewall, dark wraparound bands and the edge connector/latch shapes shown in reference. Broad stylized forms and restrained wear. Separate geometry/material regions for deck, sidewall and bands; pivot at the center of the top deck. No water, no floating animation, no surrounding bridge or island baked into this asset. The scene will instance this module five times along a shallow arc.
```

### F. Đá/vòm đá

```text
Build modular stylized tropical cliff rocks matching the attached Mech Arena environment. Broad gray-tan faceted stone masses with soft intentional bevels, large cracks and ledges, green grass caps on upward-facing surfaces, subtle moss at transitions. Include a true irregular rock arch with a walkable opening and thick uneven supports. Preserve large readable silhouettes and avoid dense photogrammetry noise. Separate stone and grass-cap regions, useful pivots and simple collision proxies. No embedded sci-fi machinery or random trees; those are separate scene assets.
```

### G. Cover trắng

```text
Create the freestanding ivory sci-fi cover shown around the Biogear Lab arena. A broad upright shield/backrest shape with clipped and beveled upper corners, thick off-white border, recessed dark gray front panel, a tiny horizontal cyan light near the lower front and a low dark support base. Match the reference proportions and slight backward lean. Stylized painted surfaces with subtle edge wear, no weapons, no logos, no extra spikes. Clean UVs, modular material regions, bottom-centered pivot and a simple collider proxy.
```

### H. Bộ texture tileable

```text
Produce separate seamless stylized game texture sets matching the attached texture board: turquoise ocean surface with broad irregular foam/wave patterns; pale warm gray sand with subtle grains and sparse small pebbles; vivid but controlled green grass blades with a few small stones; square gray painted metal panels with beveled seams and restrained wear. Use neutral illumination in base color; provide compatible normal and roughness/smoothness data where relevant and label all channels. No text labels, presentation background, copyright footer, cast shadows or perspective distortion. Each material must be delivered separately as a tileable texture, not as a combined contact sheet. The Unity material author must verify the intended URP channel packing before import.
```
