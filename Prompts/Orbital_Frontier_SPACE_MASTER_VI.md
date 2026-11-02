# ORBITAL FRONTIER — Prompt tổng và đặc tả dựng cảnh Unity

**Cập nhật V2:** Dùng `Orbital_Frontier_V2_Wrecks_Flybys_VI.md` làm yêu cầu ưu tiên cho trạm/vệ tinh, xác tàu, thiên thạch lướt, số hành tinh và tốc độ xoay. ST01 hình trụ và D01 vệ tinh đơn giản ở mô tả V1 bên dưới đã bị bỏ; scene hiện dùng xác tàu từ asset thật.

Bạn là Unity Environment Artist, Technical Artist và Gameplay Integrator. Hãy dựng một chiến trường Gundam ngoài không gian có thể chơi và chuyển đến từ Environment Hub. Làm trực tiếp bằng asset có trong project. Đọc project, xác nhận prefab và render pipeline trước khi tạo scene. Hoàn tất scene, vật liệu, ánh sáng, camera, animation bối cảnh, đăng ký catalog và kiểm tra trận đấu; không kết thúc ở bản mô tả.

## 1. Ý tưởng, quyết định gameplay

Ưu tiên chiến đấu giữa khoảng không. Các Gundam lơ lửng trong vùng giao chiến tự do, phía sau là Trái Đất, bên hông là tàu mẹ và một trạm nghiên cứu quỹ đạo. Thiên thạch, mảnh vỡ và các tốp chiến đấu cơ tạo chiều sâu ở phía ngoài. Trạm là bối cảnh và điểm định hướng, không phải sàn đấu.

Giữ hệ thống đánh theo lượt, sát thương, chọn mục tiêu và đường về đội hình hiện có. Chế độ không trọng lực áp dụng riêng scene này: thay dáng chạy trên đất bằng bay tiếp cận, gập nhẹ chân khi chờ, thruster yếu khi giữ vị trí. Đây là trận đấu ngoài không gian trên hệ thống gameplay hiện có, chưa phải mô phỏng điều khiển phi thuyền sáu bậc tự do. Không thay Physics.gravity toàn project.

Phong cách: hard-surface sci-fi tương đối chân thực, kim loại sơn bạc/xám, điểm nhấn cyan nhỏ, ánh nắng có hướng, mặt tối vẫn đọc được robot. Khoảng không gần đen, sao nhỏ rõ, tinh vân mờ. Không phủ sương xanh như dưới biển. Không có mặt phẳng giả, bụi bay kiểu sa mạc, cây, nước hoặc lửa bốc như trong khí quyển.

## 2. Asset thực sự có và cách dùng

- SpaceSkies Free: chọn một skybox có sao; clone material, hạ exposure, không sửa material gốc. Kiểm tra cả sáu hướng để tránh đường nối.
- FreeSciFi/PolygonSpaceship: StarSparrow_1_LP_Red là tàu hoàn chỉnh. Dùng tốp 3 chiếc nhỏ ở trung cảnh; các bộ phận modular dùng cho phế tích vệ tinh.
- SF_Fighter/SF_Free-Fighter.prefab: hai chiến đấu cơ hộ tống, đội hình so le, không chắn Gundam.
- R35/Dropship_R35.prefab: tàu vận tải ở gần trạm, đủ lớn để đọc silhouette nhưng không áp sát camera.
- Omega_fighterG: kiểm tra scene mẫu để lắp đầy đủ thân/frame/cockpit nếu sử dụng; không dùng một mảnh hull như tàu hoàn chỉnh.
- Barking_Dog/3D Free Modular Kit: module vách, khung cửa, quạt và mái để hoàn thiện trạm, tránh dựng trạm chỉ bằng một khối hộp.
- Cosmic_Retro_Reactor_Demo: canister và power module làm thiết bị trạm, giảm mật độ để thống nhất phong cách.
- Modular SciFi / Sci-Fi Construction Kit: dùng chi tiết panel, truss, đèn, máy móc phù hợp nếu đã có prefab và material tương thích.
- FreeSciFi/Tinniuq Planet: một hành tinh giả tưởng rất xa, nhỏ hơn Trái Đất nhiều trong khung hình.
- cool_alien_spaceship_by_jungle_jim.glb: tàu lớn đặc trưng; giữ đủ 114 primitive, 49.230 tam giác; import qua mesh buffer, normalize theo bounds, bảo toàn texture và material.
- asteroids_pack_metallic_version.glb: tách thành 10 asteroid độc lập từ 10 group; không bê cả bảng asset vào scene như một khối. Giữ tổng 23.076 tam giác nguồn và dùng lại mesh.
- Procedural Planet Generation Lite xuất hiện với dấu cộng trong ảnh: chưa giả định được cài. Tự tạo cầu UV cho Earth, cloud, atmosphere và Moon.
- Trái Đất dùng NASA Blue Marble Next Generation global equirectangular, không dùng texture đất tên Earth02 như bản đồ hành tinh.

## 3. Hệ tọa độ và khoảng trống

Một Unity unit được quy ước là một mét cho bố cục. Giữ tỷ lệ robot hiện có; chuẩn hóa asset theo bounds thay vì scale ngẫu nhiên.

Tâm bố cục C=(4.85,0,0), trùng vùng giữa hai phe. Trục Y hướng lên trong đội hình, có thể nhìn xuyên xuống dưới. Vùng bán kính 40 m quanh C phải trống vật cản và không có asteroid cắt qua. Tàu tuần tra không bay qua trung tâm. Các tọa độ dưới đây là tọa độ thiết kế tương đối C, không phải đo đạc quỹ đạo thiên văn.

Ba lớp chiều sâu:
1. Gần: robot và hiệu ứng chiến đấu, 0–35 m; chỉ vài mảnh nhỏ ở rìa, không bám vào camera.
2. Trung: asteroid 55–220 m, tàu 70–200 m, trạm 120–230 m.
3. Xa: Earth và Moon, tinh vân và sao. Tỷ lệ biểu kiến được chọn cho bố cục game; không giả vờ đây là mô hình hệ Mặt Trời đúng khoảng cách.

Không phân bố đá thành một vòng tròn hoàn hảo. Tạo các cụm lệch tâm có khoảng hở; cạnh Earth cần thoáng để đường cong hành tinh rõ.

## 4. E01 — Trái Đất, vật thể chính

Tâm thiết kế khoảng (+2200,-400,+2100), bán kính 650 m. Điều chỉnh sau khi chụp góc gameplay để chiếm 25–40% chiều ngang ảnh, không phủ toàn bộ bầu trời và không bị cắt bởi far clip. Góc nhìn mặc định thấy robot phía trước và một phần đầy đủ đĩa Earth phía sau.

Tạo sphere UV đủ phân đoạn để đường viền mịn, tối thiểu 128 kinh tuyến × 64 vĩ tuyến. UV equirectangular không gấp ngược lục địa; kiểm tra Á–Âu, châu Phi, Nam Mỹ nhận diện được. Seam đặt ở phía khuất camera chính. Texture global NASA 5400×2700, importer tối đa 4096, mipmap bật; không thêm noise màu lấn át bản đồ.

Bề mặt đại dương xanh hải quân, đất liền giữ màu ảnh nguồn, không bão hòa quá mức. Chiếu sáng bằng cùng hướng mặt trời của cảnh; có đường phân chia ngày đêm mềm hẹp, mặt tối gần đen, không phát sáng toàn cầu.

E01-C: lớp mây riêng lớn hơn bán kính bề mặt khoảng 0,3–0,5%, mây trắng xanh nhạt trên vùng sáng; lớp mây xoay chậm khác tốc độ bề mặt. E01-A: lớp khí quyển cao khoảng 1–1,8%, xanh lam ở rìa, alpha thấp ở tâm, cường độ giảm ở nửa tối. Không dùng quả cầu trong suốt xanh đồng đều che bản đồ.

Bề mặt xoay quanh local Y khoảng 0,25°/giây, mây 0,32°/giây, trục nghiêng nghệ thuật khoảng 18–23°. Đây là chuyển động tăng tốc để người chơi cảm nhận được; không ghi là tốc độ thực tế của Trái Đất. Không dịch chuyển cả hành tinh theo camera. Không collider.

## 5. E02 — Mặt Trăng

Một vệ tinh nhỏ màu xám bạc ở một phía Earth nhưng không chồng lên mép khí quyển. Kích thước biểu kiến khoảng 2–5°; bề mặt có vùng sáng tối, crater nhiều kích cỡ, viền crater không phát sáng. Hướng sáng khớp mặt trời. Có thể dùng noise/crater procedural nếu không có ảnh lunar map, ghi rõ là bề mặt mô phỏng. Xoay cực chậm, không có khí quyển xanh và không có vành đai.

## 6. E03 — Hành tinh giả tưởng phụ

Chỉ một hành tinh phụ Tinniuq, nhỏ và xa, đặt ở hướng khác Earth để tạo phần thưởng khi xoay camera. Giữ texture/normal nguồn, không biến thành ngôi sao phát sáng. Tránh đặt hai hành tinh cùng kích thước cạnh nhau. Moon là vệ tinh, không tính như một hành tinh thứ hai của hệ thực tế. Không thêm Saturn, Mars và Jupiter chỉ để lấp chỗ trống.

## 7. S01 — Tàu lớn ngoài hành tinh

Dùng GLB nguồn đầy đủ ở trung cảnh, chiều dài lớn nhất khoảng 65–85 m; vị trí khoảng (+65,+18,-110). Yaw nghiêng 15–35°, roll nhẹ tối đa 12°, silhouette chéo tạo động lực. Khoảng cách đến C đủ lớn để không va vào camera ở zoom thông thường.

Giữ phân mảnh cấu trúc gốc, ống/khối động cơ, panel và các khoảng lõm. Vật liệu kim loại có roughness, màu nguồn, normal map nếu file cung cấp; kính và emissive dùng đúng alpha/giá trị nguồn. Không đổi toàn bộ thành chrome bóng hoặc emissive xanh. Thêm tối đa vài đèn cyan/amber nhỏ nếu tàu nguồn không có; không dùng hàng chục point light.

Tàu giữ vị trí với drift biên độ 1–3 m trong chu kỳ dài, xoay rất chậm dưới 0,03°/giây. Không chạy xuyên asteroid và trạm. Cảm giác tàu rất nặng. Có thể dùng một vài vệt động cơ hình côn ngắn bằng mesh emissive; không có cột khói.

## 8. S02–S04 — Tốp StarSparrow

Ba chiếc dùng cùng prefab, kích thước 7–10 m, khoảng cách 10–18 m, xếp chữ V lệch độ cao. Vị trí nền bên trái Earth, một chiếc trước và hai chiếc sau. Clone material sang môi trường nếu phải chuyển shader; giữ mảng sơn đỏ để phân biệt tốp này với SF fighter.

Động cơ xanh nhỏ, drift chung nhẹ cùng hướng. Không chuyển động random mỗi frame, không xoay tại chỗ như đồ chơi, không loop bay xuyên đội hình. Chỉ có chuyển động bối cảnh, không tạo HP bar như kẻ địch thật.

## 9. S05–S06 — Hộ tống SF fighter

Hai chiếc khác hướng tốp đỏ, chiều dài 9–13 m, pitch nghiêng nhẹ, khoảng cách 12 m. Đặt phía trên bên phải trong góc nhìn về trạm. Ánh sáng sắc trên vỏ, vùng bụng tối mềm. Không scale non-uniform làm méo cánh. Vị trí và animation lưu cố định trong scene.

## 10. S07 — Dropship R35

Một tàu vận tải chiều dài 22–30 m gần cổng docking của trạm nhưng không lồng vào tường. Tâm tàu cách module gần nhất ít nhất một nửa chiều dài tàu cộng 4 m. Giữ đủ chi tiết của chín renderer. Không autoplay script điều khiển tàu từ demo gốc. Không gắn thêm camera hoặc AudioListener theo prefab nguồn.

## 11. ST01 — Trạm quỹ đạo

Trạm lệch phía sau bên trái C, cỡ 70–110 m, không chiếm trung tâm. Silhouette gồm một trục chính dài, ba module áp suất, nút docking, truss, 2–4 cánh pin mặt trời và đèn định vị nhỏ.

ST01-A: spine trung tâm bằng các đoạn hard-surface, đường kính khoảng 6–8 m, dài 40–60 m. Có cổ nối và vòng flange, không chỉ là một hộp kéo dài.

ST01-B/C/D: ba module khoang làm việc màu trắng bạc hơi xám, kích thước khác nhau, nối bằng cổ hẹp có gờ tối; có hatch, cửa sổ nhỏ tối, panel seams. Các module có logic kết nối thật, không trôi rời rạc.

ST01-E: docking arm ra ngoài 12–18 m, cuối là khung cửa từ modular kit. Có không gian để R35 đứng cạnh. Đèn cyan chỉ ở viền khóa và chỉ dẫn tiếp cận.

ST01-F/G: hai cụm solar wings đối xứng theo truss, mỗi cụm 16–25 m, mặt xanh navy gần đen chia cell nhỏ, khung nhôm mỏng. Bề mặt phản sáng nhẹ, không glow trắng. Thêm giằng chéo thật ở truss. Hai cánh nghiêng cùng logic về hướng nắng.

ST01-H: thiết bị phụ từ Reactor Demo, bố trí 4–8 canister/power module ở mặt service, có dây/ống nối đơn giản và bệ đỡ. Chỉ dùng ở khoảng cách phù hợp để phong cách low-poly không phá chất lượng cận cảnh.

ST01-I: antenna gồm cần gắn và đĩa nhỏ hoặc mast, cảm biến, 2 đèn đỏ định vị. Không thêm tháp canh sa mạc hay crane cảng chưa chuyển đổi hình dáng.

Trạm đứng yên; chỉ một vòng anten hoặc module riêng xoay cực chậm nếu có cấu trúc giải thích được. Không xoay toàn bộ trạm nhanh. Không đặt sàn dưới người chơi để giả lập trạm.

## 12. A01–A10 — Họ thiên thạch

Import đủ 10 group thành 10 prefab riêng, recenter về geometric bounds center, normalize chiều lớn nhất về 1. Mỗi instance có transform và material riêng nếu cần tint. Normal/tangent đúng sau đổi hệ tọa độ glTF sang Unity; triangle winding không lộn.

Tạo khoảng 70–100 thiên thạch, phân cấp:
- 8–12 khối lớn 8–17 m, xa 100–220 m: silhouette góc cạnh tự nhiên, vài mặt lõm, ánh sáng rõ hướng.
- 30–45 khối vừa 2–7 m, xa 70–180 m: theo 3–4 cụm xiên, có lỗ hổng để thấy sao.
- 30–45 khối nhỏ 0,5–2 m ở rìa: không phủ như bụi dày, không xuất hiện ngay trước mặt robot.

Xoay mỗi khối theo trục riêng, khoảng 0,1–1,2°/giây; khối lớn chậm hơn. Không rung bằng sin tần số cao. Không meteor trail bốc lửa trong chân không. Tránh đá bóng gương dù tên file có metallic: hạ metallic nếu chất liệu trông như đồ trang sức; giữ texture đá và roughness.

## 13. D01–D40 — Mảnh vỡ và vệ tinh hỏng

Tạo một vệ tinh gãy ở phía đối diện trạm: core sáu cạnh/box có bevel, một pin mặt trời còn nguyên, một cánh bị gãy; gắn anten ngắn và thanh kết nối. Ba mảng panel tách ra quanh nó theo hướng nhất quán, không nổ liên tục.

Khoảng 30–50 mảnh gồm panel vỏ tàu, dầm chữ nhật mảnh, module động cơ nhỏ, vòng flange. Kích thước 0,2–2,5 m; màu xám bạc/đen/cam sơn cũ. Chỉ vài mảnh nhìn rõ, phần còn lại rải thưa ngoài vùng an toàn. Mảnh nhẹ xoay 1–3°/giây, drift biên độ nhỏ, không có collider hay Rigidbody rơi xuống.

## 14. Ánh sáng, sky và màu

Một Directional Light như mặt trời, màu gần trắng hơi ấm, hướng xiên để Earth có terminator và tàu có silhouette rõ. Ambient lạnh rất yếu nhưng đủ đọc giáp robot. Không dùng nhiều directional light đối nghịch làm hành tinh sáng đều.

Skybox tối, sao hầu hết trắng ngà/xanh nhạt, kích thước điểm nhỏ. Tinh vân tím/xanh chỉ tạo độ sâu mờ. Bloom vừa đủ cho thruster, không quầng quanh tất cả cạnh. Không fog, không under-water full-screen renderer feature. Main camera dùng renderer mặc định project; chỉ underwater map dùng renderer nước.

Cho tương phản giữa robot tiền cảnh và Earth nền: khung gameplay đọc được silhouette đầu, tay và vũ khí, HUD không chìm vào mây trắng. Không áp vignette nặng hay chromatic aberration để che lỗi vật liệu.

## 15. Camera và thao tác xoay

Giữ input orbit sẵn có: kéo vùng trống/chuột phải nếu đã cấu hình, cảm ứng kéo, scroll/pinch zoom theo controller. Yaw không giới hạn 360°, pitch -89° đến +89° để nhìn cả phía trên và phía dưới, zoom 2,5–140 m. Không roll camera gây say.

Trong space mode bỏ cả clamp góc boom theo floor và clamp tọa độ Y cuối. Khi nhìn lên từ dưới, camera thực sự xuống dưới đội hình, không mắc ở Y=0,25. Far clip riêng khoảng 10.000 m để Earth không biến mất mỗi LateUpdate. Near clip 0,05 m phù hợp robot. Tự tránh tàu/đá khi cần nhưng không cho hành tinh/sky collider kéo camera về sát người chơi.

Chế độ này opt-in bằng field lưu trong scene. Giá trị mặc định ở sa mạc, phố, núi lửa và dưới nước giữ nguyên.

## 16. Cấu trúc, tích hợp và yêu cầu bàn giao

Scene: Assets/Environments/OrbitalFrontier/Scenes/OrbitalFrontier_Battle.unity.
ID catalog: orbital-frontier. Display: ORBITAL FRONTIER · EARTHWARD.
Root: OrbitalFrontier_Battlefield; nhóm riêng CelestialBodies, Station, Ships, AsteroidField, Salvage, Lighting, ReviewCameras.
Lưu toàn bộ material chuyển đổi vào folder môi trường; không sửa asset store nguyên bản. Không auto-import package khác hoặc làm hỏng renderer cũ. File nguồn GLB giữ trong Sources/OrbitalFrontier.

Thêm definition, thumbnail và scene vào build list dùng quy trình EnvironmentTools hiện có. Chọn map từ Hub; trở lại Hub và đổi sang scene khác không nhân đôi camera, listener hay EventSystem.

Tạo các camera review tắt mặc định: Gameplay/Earthward, nhìn trạm, nhìn tàu lớn, thiên thạch, góc toàn cảnh, hướng nhìn lên/xuống. Chụp 1600×900, kiểm tra đường viền Earth, UV bản đồ, độ tối tàu, shader hồng, silhouette và mật độ đá. Sửa sau khi xem ảnh, không chỉ kiểm tra object count.

## 17. Tiêu chí nghiệm thu

1. Có Earth với địa hình nhận dạng được, lớp mây/khí quyển, một Moon và chỉ một hành tinh giả tưởng phụ.
2. Có tàu lớn GLB, dropship và hai tốp fighter; trạm hoàn chỉnh có module nối, docking, solar wings.
3. Có đủ 10 biến thể asteroid, bố cục cụm bất đối xứng, không có sàn hoặc collider địa hình dưới Gundam.
4. Vùng giao chiến rõ ràng, tất cả asset bối cảnh nằm ngoài bán kính an toàn; animation không xâm phạm vùng đó.
5. Robot chờ và tiếp cận với tư thế không trọng lực, không chạy chân trên khoảng không; sát thương, timing, lượt địch, ultimate và anchor return vẫn đúng.
6. Camera xoay một vòng và nhìn từ dưới lên/phía trên xuống; zoom xa; Earth không bị far clip cắt khi Update chạy.
7. Các hành tinh, mây và đá thực sự đổi rotation khi Play; không tích lũy scale/position sau reload.
8. Shader không lỗi; scene không thêm underwater fog/optics vào map khác.
9. Hub → OrbitalFrontier → SunkenHarbor → OrbitalFrontier hoạt động; không tăng camera/listener/EventSystem.
10. Báo rõ kiểm tra nào đã chạy trong Editor; chưa benchmark thì không cam kết FPS/Android hoặc giống 100%.

Nguồn Earth: NASA Earth Observatory, Blue Marble Next Generation, Base Map with Topography, January global image. https://science.nasa.gov/earth/earth-observatory/blue-marble-next-generation/base-topography/ ; texture https://assets.science.nasa.gov/content/dam/science/esd/eo/images/bmng/bmng-topography/january/world.topo.200401.3x5400x2700.jpg . Giữ nguồn trong tài liệu project. Tinniuq từ FreeSciFi, mây được tạo procedural; Moon procedural là mô phỏng mỹ thuật. Các tàu/đá giữ nguồn asset địa phương; không suy đoán tác giả/giấy phép từ tên file.



