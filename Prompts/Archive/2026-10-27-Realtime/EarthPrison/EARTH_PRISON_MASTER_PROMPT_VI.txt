# MASTER PROMPT — EARTH PRISON: SÁU TRỤ ĐÁ DẪN SÉT VÀNG

Hãy trực tiếp xây dựng trong Unity một skill Earth Prison dựa trên video tham khảo của tôi: người thi triển tụ điện vàng, phóng tia sát mặt đất về nhóm mục tiêu, tạo vòm năng lượng xoáy, dựng sáu trụ đá quanh vùng và nối các đỉnh bằng sét thành lồng, sau đó tan rã có lớp lang.

Tôi cần scene chạy được, prefab, mesh, material, texture, animation và script đã lưu, có thể chỉnh trong Inspector. Đừng chỉ đưa kế hoạch, vài Particle System mặc định hoặc đoạn mã rời không kết nối. Thực hiện lần lượt các mốc bên dưới; báo đúng phần đã làm, đã chạy thử và phần còn thiếu.

## 1. Vai trò và kết quả ưu tiên

Bạn đảm nhiệm Unity VFX Artist, Technical Artist, Gameplay Engineer và Technical Animator. Ưu tiên theo thứ tự:

1. Đúng silhouette, nhịp và các pha của video.
2. Đọc rõ sáu trụ đá, sáu điểm sáng và đường sét nối nhau.
3. Chuyển tiếp mượt, có chuẩn bị, bùng lực và đuôi hiệu ứng.
4. Đá có khối, sét có cấu trúc, bụi có độ mềm; màu vàng không cháy trắng toàn bộ.
5. Prefab chạy lại ổn định, quản lý vòng đời tốt.
6. Tích hợp vào Gundam và đo hiệu năng.

Mặc định làm chế độ **VisualReference** để tái dựng hình ảnh. Tạo sẵn lựa chọn **GameplayDemo** theo mục 15, nhưng để tắt damage/control khi so sánh với video. Không diễn giải các thông số gameplay đề xuất thành thông số gốc của video.

## 2. Tài liệu đầu vào và dữ kiện project

Video gốc trên máy:
`D:/Downloads/YTSave.to_YouTube_Media_6vd41UGQT74_Earth-Prison-Ultimate-Skill-Unity-6-5-Particle-System_002_720p.mp4`

Tài liệu cùng bộ:
- `E:/My project/Prompts/EarthPrison/PHAN_TICH_VIDEO_EARTH_PRISON_VI.md`
- `E:/My project/Prompts/EarthPrison/References/KEYFRAMES.jpg`
- `E:/My project/Prompts/EarthPrison/References/detail_sheet_01.jpg`
- `E:/My project/Prompts/EarthPrison/References/detail_sheet_02.jpg`
- `E:/My project/Prompts/EarthPrison/References/overview_024.000s.jpg`
- `E:/My project/Prompts/EarthPrison/References/overview_033.000s.jpg`

Video dài khoảng 35,875 s, 1280×720, có nhiều lượt trình diễn từ các góc khác nhau. Một chuỗi đầu kéo dài khoảng 7,5–8 s tính từ đầu file; đầu file đã có hiệu ứng nên không biết chính xác lúc bấm phím. Timestamp phân tích lấy mẫu 0,25 s; không giả vờ đã đo chính xác từng frame.

Snapshot cấu hình project khi soạn prompt ngày 27/10/2026:
- Project: `E:/My project`.
- Unity: `6000.5.8f1`; URP `17.5.0`.
- Input System `1.20.0`; Timeline `1.8.12`.
- Manifest chưa liệt kê VFX Graph hoặc Cinemachine. Không mặc định hai package này tồn tại.
- Demo Gundam có tại `Assets/MechaCombatDemo/` với `MechaPlayer`, `MechaEffects`, `MechaHealth`, `MechaTuning`.
- Rig Gundam demo được mô tả cao 2,6 m; đọc lại prefab để xác nhận trước khi scale.
- Q/E/R đã được dùng; R hiện là Celestial Breaker.

Hãy kiểm tra lại project hiện tại trước khi sửa. Nếu đọc được dữ liệu thì tự đọc, không hỏi lại tôi những thông tin đã có. Nếu không xem được video, đọc các ảnh đã trích và nói rõ giới hạn.

## 3. Các đặc điểm bắt buộc phải giữ

- Sét vàng chanh với lõi vàng trắng; đá xám than; bụi chân trụ vàng cam/nâu.
- Pha điện tỏa rộng quanh người thi triển thu lại thành một cầu điện nhỏ trước khi phóng.
- Tia chính nối từ nguồn phát tới vùng mục tiêu, chạy thấp gần mặt đất, có nhánh và độ gấp khúc.
- Impact bật từ mặt đất lên; vòng xoáy mở thành vòm thấp gồm nhiều dải rách, tâm còn trong.
- Chính xác **sáu** trụ bố trí quanh vùng theo lục giác, thân không đều, có bệ đá vỡ và bụi riêng.
- Các trụ mọc lên trước khi lồng sét hoàn chỉnh xuất hiện.
- Sáu đỉnh phát sáng, có sét dọc trụ, sét nối chu vi và vài nhánh chéo bên trong.
- Đá, điện, bụi và mảnh vụn biến mất theo thời gian khác nhau.
- Camera so sánh giữ cố định trong mỗi lượt; các góc trong video là góc quan sát khác nhau, không phải một chuỗi cinematic bắt buộc.

Không đưa taskbar, giao diện Unity, nền trời tím hoặc ô đỏ dưới mục tiêu vào cấu trúc skill. Ô đỏ đã tồn tại trước khi chiêu tác động. Không tự thêm rune, cổng dịch chuyển, thiên thạch hay vụ nổ cầu khổng lồ cuối chiêu.

## 4. Timeline chuẩn để dựng bản đầu

Đặt thời gian skill `t = 0` là lúc bắt đầu trình diễn. Các mốc dưới đây là **timeline tái dựng đề xuất dựa trên quan sát**, có thể tinh chỉnh khi so ảnh; không phải thông số trích từ mã nguồn tác giả.

| Pha | Thời gian | Hành vi bắt buộc |
| --- | --- | --- |
| ChargeScatter | 0,00–1,10 s | Nhánh điện và hạt phụ quanh tay/thân; silhouette còn rõ |
| ChargeOrb | 1,10–2,65 s | Gom thành cầu điện nhỏ, vòng điện quấn quanh, giữ một nhịp |
| GroundTransfer | 2,65–3,35 s | Tia sét phát từ socket tới điểm đất gần tâm địch; có bụi dọc đường |
| GroundImpact | 3,35–3,65 s | Lõi chớp và tia vàng hướng lên từ đất; không phủ kín cả màn hình |
| SwirlDome | 3,45–4,30 s | Vòm xoáy tăng bán kính nhanh, các dải vàng cuộn, sau đó dissolve |
| PillarRise | 4,10–4,75 s | Sáu bệ và trụ hiện lên có chuyển vị từ dưới mặt đất; bụi/mảnh bắn |
| EnergizeTips | 4,65–5,10 s | Lần lượt bật đỉnh sáng và sét dọc thân; độ lệch rất nhỏ giữa trụ |
| CageSustain | 5,05–6,45 s | Vành sét lục giác và nhánh chéo hoạt động; ổn định hình lớn, dao động chi tiết nhỏ |
| Dissolve | 6,45–7,30 s | Sét chu vi yếu đi, thân đá tan, mép điện đứt đoạn, bệ còn lưu |
| Residual | 7,00–8,00 s | Bụi/mảnh/hạt cuối tắt dần; trở lại trạng thái sạch |

Các pha chồng nhau có chủ đích. Không biến timeline thành chuỗi hiệu ứng rời với một frame trống giữa các pha.

Cho phép `durationScale` để phát 0,5×/1×/2× trong preview. Khi đổi tốc độ, clock, animation, emission và thời gian fade phải đi cùng nhau. Khi phát lại mặc định, giữ nhịp khoảng 8 s để so video trước khi rút ngắn cho gameplay.

## 5. Quy mô và hình học đề xuất

Đơn vị Unity xấp xỉ mét. Gọi `H` là chiều cao nhân vật mục tiêu chuẩn; bản Gundam demo khởi điểm `H = 2,6 m`. Mọi kích thước dưới là lựa chọn dựng lại, không đo được mét thật từ video.

| Tham số | Giá trị ban đầu |
| --- | --- |
| Khoảng từ người thi triển tới tâm vùng | 10–14 m; tầm chọn tối đa đề xuất 18 m |
| Bán kính đặt tâm trụ | 4,8 m, khoảng 1,85H |
| Vòm xoáy lớn nhất | Bán kính 5,3 m, cao 2,0–2,4 m |
| Chiều cao trụ đã mọc đủ | 4,2 m, khoảng 1,62H; biến thiên ±8% |
| Bề rộng thân trụ | 0,55–0,85 m |
| Bán kính bệ đá từng trụ | 0,8–1,0 m |
| Bán kính cầu điện lúc giữ | 0,30–0,45 m |
| Tia truyền gần đất | Cao khoảng 0,10–0,25 m dọc phần giữa quỹ đạo |
| Bán kính vùng gameplay tùy chọn | 4,0 m, nhỏ hơn vòng trụ |

Sáu vị trí theo công thức:
`theta_i = rotationOffset + i * 2*pi/6`, `P_i = center + radius*(cos(theta_i)*U + sin(theta_i)*V)`.

`U,V` là hai trục trực chuẩn trên mặt phẳng bố trí; hướng lên được xác định rõ. Dùng cast/probe xuống đất ở tâm và sáu vị trí. Bản đầu ưu tiên nền phẳng; từ chối rõ vùng quá dốc, trụ nằm trong tường, thiếu mặt đất hoặc chênh cao vượt cấu hình. Không âm thầm bỏ một trụ làm mất lục giác.

Trụ chính dùng mesh có khối, không thay bằng sáu billboard hoặc cylinder trơn trong bản nghiệm thu hình ảnh. Có thể dùng mesh procedural do công cụ Editor tạo, nhưng phải có tiết diện gãy, thay đổi bán kính theo chiều cao, đỉnh lệch và mặt bên có sắc độ. Dùng ít nhất 2–3 biến thể rồi xoay/scale có kiểm soát.

## 6. Cấu trúc asset và prefab

Tạo trong `Assets/MechaCombatDemo/Abilities/EarthPrison/`:

```text
EarthPrison/
  Prefabs/EarthPrison_VFX.prefab
  Prefabs/EarthPrison_Pillar.prefab
  Scenes/EarthPrison_VFXLab.unity
  Materials/
  Meshes/
  Textures/
  Shaders/
  Animation/
  Config/EarthPrison_Default.asset
  Scripts/Runtime/
  Scripts/Editor/
  Reports/
  README_VI.md
```

Hierarchy gợi ý:

```text
EarthPrisonInstance
  CastAnchor
    ChargeCore
    ChargeOrbRibbons
    ChargeLightning
    ChargeSparks
  Transfer
    MainBoltCore
    MainBoltGlow
    SideBranches
    GroundDust
  AreaAnchor
    GroundFlash
    GroundRing
    SwirlDome_RibbonA
    SwirlDome_RibbonB
    SwirlDome_RibbonC
    Pillars
      Pillar_00 ... Pillar_05
        RockBody
        RubbleBase
        BaseDust
        Debris
        TipSocket
        TipCore
        TipGlow
        VerticalArcs
    CageEdges_00_01 ... CageEdges_05_00
    InteriorArcs
    ResidualDust
```

Tên là đề xuất; có thể gộp component nhỏ nhưng phải đọc được trách nhiệm. Root scale mặc định 1. Không parent toàn bộ vùng skill dưới người chơi vì lồng phải đứng tại vị trí đã chọn khi người chơi di chuyển.

## 7. Cầu điện và animation thi triển

Tạo socket thật sau khi xem rig, lưu reference trong Inspector; không tìm bone bằng chuỗi đoán tên mỗi frame.

Pha 0–1,1 s: điện tỏa thành 4–8 nhánh quanh vùng tay/ngực, không phủ kín toàn thân. Pha 1,1–2,65 s: giảm nhánh xa, tăng độ tập trung vào lõi; cho 2–3 vòng lệch mặt phẳng cuộn quanh cầu. Vòng cần có khoảng rỗng, không là một quả bóng đục vàng.

Gợi ý animation cho Gundam, có thể đổi theo giới hạn khớp thực:
- 0–0,6 s: mở thế đứng, gối hạ nhẹ, ngực hướng mục tiêu.
- 0,6–1,3 s: đưa tay có socket lên ngang ngực/đầu; điện gom theo tay.
- 1,3–2,6 s: giữ pose có lực, chỉ thêm chuyển động phụ nhỏ.
- 2,65–3,15 s: đưa tay xuống/trước để phát tia; hông/ngực dẫn trước cánh tay, giữ chân chống.
- 3,15–3,65 s: hồi về tư thế, người thi triển vẫn ở ngoài vùng.

Các góc khớp phải kiểm tra trên rig thật; không cho cánh tay xuyên sừng, giáp vai hoặc backpack. Clip thật được lưu dưới Animation. Tách thời lượng animation của người thi triển khỏi vòng đời vùng hiệu ứng; sau khi phát có thể trả locomotion theo thiết kế, trong khi lồng tiếp tục chạy độc lập.

## 8. Tia sét truyền và thuật toán đường sét

Dùng đường điều khiển cho các tia cần nối đúng đầu mút. Triển khai bằng mesh ribbon hoặc LineRenderer kèm vật liệu URP phù hợp; hạt dùng cho tia nhỏ/bụi. Đây là phương án dựng lại, không phải khẳng định công cụ gốc của video.

Mỗi bolt có `startAnchor`, `endAnchor`, số điểm, seed, amplitude, branch count, width curve và color gradient. Có 2 lớp: core mảnh vàng trắng và glow rộng, alpha thấp hơn.

Gợi ý tạo đường:
- Với `u` từ 0 đến 1, lấy đường cơ sở `lerp(A,B,u)` hoặc polyline bám đất.
- Thêm offset theo hai trục vuông góc với tiếp tuyến; biên độ nhân `sin(pi*u)` để đầu mút luôn bám socket.
- Kết hợp noise tần số thấp tạo ngoặt lớn và noise nhỏ tạo mép rung. Không chỉ dùng một đường sin trơn.
- Dùng seed riêng theo instance và edge. Đổi hình noise khoảng 8–14 lần/giây rồi nội suy giữa hai mẫu; không random toàn bộ từng render frame.
- Mỗi cạnh chu vi dùng khoảng 12–24 điểm; tia truyền dài dùng 24–40 điểm làm điểm xuất phát, tăng nếu cần dựa trên silhouette.
- Bề rộng core gợi ý 0,025–0,05 m; glow 0,10–0,20 m; nhiễu cạnh 0,04–0,15 m, tùy độ dài.
- Nhánh phụ dài 10–30% đường chính, số lượng ít; không để nhánh ngẫu nhiên trở thành tia dài bắn ra ngoài phạm vi.
- Hai lớp phải cùng skeleton đường đi, không rung lệch nhau như hai tia không liên quan.

GroundTransfer: raycast kiểm vật cản từ nguồn phát tới vùng trước khi commit. Trên nền gồ ghề, lấy mẫu chiều cao dọc đường, nối lên từ tay tới đường sát đất; không chiếu từng điểm noise xuống đất vì sẽ làm mất độ dày nhánh. Tia có thể reveal theo tiến độ từ A tới B trong khoảng 0,15–0,25 s, sáng giữ ngắn rồi fade trước 3,5 s. Các số này là tuning, cần đối chiếu ảnh 2,75/3,00/3,25 s.

Không dùng collision của hàng trăm hạt để quyết định nơi đặt vùng; gameplay xác định điểm đến một lần, VFX biểu diễn nó.

## 9. Impact và vòm xoáy

GroundImpact có 3 lớp: core flash ngắn, tia/phiến hướng lên, bụi mềm. Flash khoảng 0,10–0,18 s rồi giảm nhanh; bụi lưu 0,6–1,0 s. Tâm phát nằm trên mặt đất thực, không chìm xuống sàn.

Vòm xoáy là phần quan trọng nhất sau lồng sét:
- Tạo 2–3 ribbon dạng vành/vòm thấp có độ cao và độ nghiêng khác nhau.
- Mở bán kính từ khoảng 1,5 m lên 5,3 m trong 0,35–0,45 s với ease-out.
- Chiều cao từ 0,2 m lên khoảng 2,2 m; các dải có khe và mép xé.
- Cuộn UV theo chu vi, giữ noise thế giới/UV ổn định đủ để nhìn thấy hướng xoáy.
- Dùng mask dài theo hướng tiếp tuyến; không trải noise đều như đám mây tròn.
- Hai hoặc ba lớp lệch tốc độ và pha; chọn một hướng quay chính, tránh mọi lớp quay ngược triệt tiêu cảm giác chuyển động.
- Tâm trong, viền rõ; giảm alpha ở mặt nhìn thẳng vào camera nếu cần để thấy mục tiêu.
- Khoảng 4,1–4,3 s: dissolve dải vàng khi bệ đá/trụ bắt đầu đọc rõ, không giữ vòm kín suốt pha lồng.

Vòng sát đất có offset vài centimet để tránh z-fighting, gồm một vành mỏng rách và mảng bụi. Không dùng một đĩa vàng đục phủ toàn vùng.

## 10. Trụ đá, bệ và chuyển động mọc lên

Với mỗi trụ, dùng riêng body, rubble base, tip socket, dust và debris.

Chuyển động đề xuất:
- Body bắt đầu dưới mặt đất khoảng 85–100% chiều cao; đi lên theo ease-out trong 0,45–0,60 s.
- Chân trụ cuối cùng nằm đúng mặt đất; không scale quanh tâm làm trụ mọc cả lên lẫn xuống.
- Có thể nảy rất nhẹ ở cuối, tối đa 1–2% chiều cao, sau đó đứng yên.
- Sáu trụ lệch nhau 0–0,06 s; tổng thể đọc như một đợt mọc gần đồng thời.
- Bệ mở ra trước/đồng thời với thân, bụi và mảnh nhỏ phát ở chân chứ không từ đỉnh.
- TipSocket đi cùng thân để điện kích hoạt đúng đầu trụ đang chuyển động.

Body dùng URP Lit hoặc shader Lit custom cho đá. Mặt đá xám than, roughness cao, ít metallic, có sắc độ/ngấn bề mặt để ánh sáng đọc ra khối. Normal/detail noise nếu có phải hỗ trợ hình khối lớn, không bù cho mesh cylinder trơn.

Tạo bệ bằng 8–14 viên đá không đều quanh chân và một mảng nền nứt tối mềm. Mảnh bay Particle System mesh dùng vài mesh nhỏ, quay khác nhau, gravity hợp lý; không bật Rigidbody riêng cho mọi mảnh nếu không cần.

## 11. Lồng sét và nạp điện

TipCore là điểm nhỏ rất sáng, TipGlow lớn hơn nhưng alpha thấp; có thể thêm một starburst ngắn khi bật. Không dùng sprite trắng vuông hoặc sáu quả cầu khổng lồ che mất trụ.

Kết nối chính: `0→1→2→3→4→5→0`. Cạnh luôn bám TipSocket, kể cả khi đổi scale/địa hình. Đừng random lại bố cục sáu trụ trong lúc sustain.

Pha kích hoạt 4,65–5,10 s:
1. Điện leo dọc thân và lõi đỉnh tăng sáng.
2. Cạnh chu vi reveal tới đỉnh kế tiếp, độ lệch tổng nhỏ để lồng khép nhanh.
3. Sau khi chu vi đọc rõ mới thêm nhánh chéo.

Trong 5,05–6,45 s: giữ 6 cạnh chu vi; chọn 2–3 liên kết chéo luân phiên, ví dụ `(0,3)`, `(1,4)`, `(2,5)`, nhưng không bật cả ba liên tục thành dấu sao cứng. Thêm vài nhánh rơi xuống phía trong hoặc chạy quanh đối tượng; không để hình lồng trở thành khối trắng.

6 vertical arcs có một số điểm bám mặt trụ; dùng offset nhỏ để tránh z-fighting và cho đường điện ôm đá. Có thể làm bằng ribbon hoặc lớp emissive nứt; phải đồng bộ với phần thân đang dissolve.

Độ sáng nhấp nháy nhẹ khoảng ±10–20%, có kiểm soát. Các đỉnh không tắt ngẫu nhiên toàn bộ cùng một lúc. Cấu trúc phải đọc được cả từ camera thấp và nhìn trên xuống.

## 12. Bảng Particle System khởi điểm

Đây là **giá trị đề xuất** cho quy mô ở mục 5. Dùng Particle System cho hạt/bụi/mảnh; dùng hình học điều khiển cho trụ và các liên kết sét chính. `Looping = false`, `Play On Awake = false` mặc định; phát bằng controller.

| Hệ | Emission đề xuất | Lifetime | Size / Speed khởi điểm | Space và renderer |
| --- | --- | --- | --- | --- |
| ChargeSparks | 35–55 hạt/s, chỉ trong charge | 0,2–0,55 s | 0,02–0,07 m; 0,5–2,5 m/s | World; billboard hoặc stretched |
| ChargeOrbDust | 15–25 hạt/s | 0,25–0,6 s | 0,02–0,05 m; chuyển động hướng vào tâm có kiểm soát | Local dưới CastAnchor; billboard |
| TransferSparks | 30–60 hạt rải dọc đường | 0,12–0,35 s | 0,02–0,06 m; 1–4 m/s | World; stretched |
| TransferDust | 12–20 hạt dọc đường | 0,4–0,8 s | 0,2–0,6 m; 0,2–1 m/s | World; alpha billboard |
| ImpactStreaks | Burst 20–35 | 0,15–0,4 s | Rộng 0,03–0,08 m; 3–6 m/s hướng lên/ra | World; stretched |
| ImpactDust | Burst 12–20 | 0,6–1,0 s | 0,5–1,2 m; 1–2 m/s | World; alpha billboard |
| PillarBaseDust | Burst 8–12 mỗi trụ | 0,5–1,0 s | 0,35–0,8 m; 0,5–1,5 m/s | World; alpha billboard, vàng cam/nâu |
| PillarDebris | Burst 8–12 mỗi trụ | 0,8–1,4 s | Mesh 0,04–0,18 m; 1,5–4 m/s | World; mesh, gravity 0,6–1,2 |
| TipSparks | 8–15 hạt/s mỗi đỉnh khi active | 0,12–0,3 s | 0,02–0,06 m; 0,5–2 m/s | World; additive |
| DissolveSparks | Burst 8–15 mỗi trụ | 0,25–0,6 s | 0,02–0,06 m; 0,3–1,5 m/s | World; additive |
| ResidualDust | Burst 12–20 tổng vùng | 0,8–1,3 s | 0,4–0,9 m; 0,1–0,6 m/s | World; alpha billboard |

Color over Lifetime: tránh alpha bật/tắt cứng; có ramp vào rất ngắn, giữ giữa, fade cuối. Size over Lifetime: bụi nở dần rồi tan; tia nhỏ thu nhanh; mảnh đá không nở thành bóng.

Noise nhẹ cho bụi; nhánh điện chính dùng thuật toán riêng để giữ đầu mút. Texture Sheet Animation chỉ bật nếu thực sự có flipbook; không bịa atlas khi chưa tạo. Nếu dùng sprite tĩnh, ghi đúng là sprite tĩnh.

Đặt Max Particles riêng theo emission × lifetime và khoảng chồng burst, không để 10.000 cho từng hệ. Mục tiêu khởi điểm tổng dưới khoảng 1.500 hạt sống/instance, nhưng phải đo cả overdraw và renderer; ít hạt không tự động đồng nghĩa nhanh.

## 13. Texture, vật liệu và hậu kỳ

Chuẩn bị các asset thật:
- `T_SoftGlow`: gradient tròn mềm, alpha mượt.
- `T_LightningStrip`: lõi sáng mảnh, mép feather, UV dọc đường; không phải ảnh tia sét nguyên tấm gắn như billboard.
- `T_SwirlStreakMask`: dải dài đứt đoạn, có hướng, dùng cho ribbon vòm.
- `T_DissolveNoise`: noise có cấu trúc phù hợp khoảng nhỏ/lớn.
- `T_DustSoft` hoặc flipbook bụi đã tạo thật.
- Texture/mask đá và nền nứt nếu cần.

Có thể tạo texture thủ tục bằng công cụ Editor và lưu PNG; render xem từng texture. Không để placeholder đơn sắc rồi báo đạt. Nếu có công cụ tạo ảnh khả dụng và phù hợp thì dùng cho texture, nhưng không bắt workflow phải phụ thuộc dịch vụ ngoài.

Màu sRGB gợi ý, không phải màu đo chính xác từ asset gốc:
- Core: `#FFF9C4`, HDR multiplier khoảng 3–5.
- Lightning: `#FFE52B`, HDR khoảng 1,5–3.
- Glow: `#FFD64A`, alpha thấp, HDR khoảng 1–2.
- Bụi chân: `#A76B20` đến `#D6972D`, không phát sáng như lửa mặc định.
- Đá: `#33312D` đến `#5A554A` theo mặt; bụi tan xám nâu.

Các giá trị HDR là điểm bắt đầu; tinh chỉnh theo exposure/bloom thực tế. Giữ lõi có khoảng trắng vàng, thân đá vẫn nhìn ra khối.

Vật liệu:
- Điện/core/glow: URP transparent additive hoặc premultiplied đúng công thức blend, `ZWrite Off`, vẫn depth-test với cảnh.
- Bụi: alpha/premultiplied, không additive toàn bộ; mép mềm, không hiện hình vuông.
- Đá: opaque/alpha-clipped dissolve phù hợp, có lighting và shadow nếu ngân sách cho phép.
- Vòm: shader URP Unlit có UV scroll, noise mask, dissolve và vertex color; cân nhắc hai mặt cho ribbon mỏng.

Nếu dùng Shader Graph, kiểm tra package và shader compile thật. Nếu dùng HLSL, dùng include/pass tương thích URP hiện cài; không dùng surface shader Built-in trong URP.

Soft particles cần scene depth phù hợp và cấu hình camera/URP thực tế; kiểm tra nếu bật. Bloom đặt mức vừa đủ, exposure ổn định. Dùng material parameter/HDR thay vì đẩy exposure cả scene để làm điện sáng. Chỉ thêm 1–2 đèn động có phạm vi nhỏ khi cần; không gắn point light vào mọi particle.

## 14. Tan rã, clock và vòng đời

Một controller điều khiển timeline chuẩn. Không dùng nhiều coroutine độc lập ghi timeScale hoặc cùng sửa một material property.

Giao diện gợi ý:
- `Play(CastContext context)` với source socket, center, normal, seed, scale, mode.
- `Cancel(reason)` xử lý mọi đường thoát.
- `ResetForPool()` đưa hệ về trạng thái sạch.
- `EvaluatePreview(time, seed)` cho preview không gameplay nếu cần.

Các mốc thời gian kích hoạt một lần khi `previousTime < marker && currentTime >= marker`, có xử lý riêng trạng thái ban đầu t=0. Không kiểm tra bằng `time == marker`. Damage tick dùng accumulator/mốc chỉ số để không phụ thuộc FPS và không phát lại khi seek preview.

Khi dissolve:
- Hạ glow/cạnh, tắt sinh hạt mới.
- Dissolve thân đá theo noise với mép vàng mảnh; bệ giữ thêm một nhịp.
- Để hạt còn sống kết thúc theo lifetime; giới hạn toàn bộ đuôi trong thời lượng đã khai báo hoặc cập nhật duration theo max tail thật.
- Không deactivate root khi Particle System con còn sống ngoài ý muốn; cũng không để hệ looping giữ instance mãi.

Pooling phải reset Transform, scale, emission, color, dissolve, trail/ribbon buffers, seed, collider, event flags, audio và property blocks. Dùng `StopEmittingAndClear` cho reset cứng, còn kết thúc bình thường cho phép tail. Không tạo material mới mỗi frame qua `.material`; quản lý instance material hoặc MaterialPropertyBlock phù hợp.

Pause đóng băng clock, animation và các hạt cùng nhau. Preview độc lập không thay Time.timeScale toàn game. Khi object bị disable/destroy, đổi scene hoặc người thi triển chết, thu hồi các resource thuộc instance và gỡ đúng trạng thái do instance áp vào mục tiêu.

## 15. Gameplay tùy chọn — không suy ra từ video

Chỉ bật trong `GameplayDemo`; `VisualReference` không gây damage hoặc khóa mục tiêu.

Thông số đề xuất ban đầu:
- Energy 60; cooldown 18 s; tầm 18 m; bán kính damage 4 m.
- Commit tại 2,65 s, sau khi kiểm tra target point, ground và vật cản; trừ energy và bắt đầu cooldown một lần.
- Hủy trước commit: không trừ energy, không có damage/cooldown chính; ngăn spam VFX bằng trạng thái cast.
- Sau commit không cho tự cancel để lấy lại energy; nếu chết/disable/reset thì dọn VFX và dừng damage tương lai, không hoàn lại phần đã dùng.
- Impact tại 3,35 s: 120 damage một lần cho mỗi mục tiêu hợp lệ.
- Tick tại 5,10 / 5,50 / 5,90 / 6,30 s: 60 damage/lần. Tổng lý thuyết tối đa 360 cho đối tượng trúng cả impact và bốn tick; không tự thêm hit kết liễu.
- Tùy chọn hạn chế di chuyển trong 5,10–6,45 s, mặc định tắt đến khi có tích hợp trạng thái đúng.

Đây là số để test, phải expose trong config. Không gán damage theo Particle Collision hoặc số tia chạm nhân vật; nhiều collider thuộc cùng `MechaHealth` chỉ tính một lần/mốc. Dùng layer/faction, bỏ qua người thi triển và đối tượng chết, tránh buffer overlap bị đầy rồi âm thầm bỏ mục tiêu.

Không gọi biến mất mục tiêu là chết khi HP còn. Không gán `SetActive(false)` để bắt chước việc đối tượng biến mất trong video. VisualReference có thể giữ ba dummy tồn tại suốt lượt.

Sáu trụ rời chưa đủ làm tường kín. Nếu muốn giam thật, chọn một cơ chế rõ ràng: movement restriction có owner token và thời hạn, hoặc barrier collision được mô tả riêng. Bản đầu mặc định hình ảnh, không tạo tường vô hình. Không disable AI rồi enable vô điều kiện khi hết chiêu, vì mục tiêu có thể đang chết hoặc chịu một trạng thái khác.

## 16. Tích hợp với Gundam hiện có

Làm VFXLab độc lập trước, giữ nguyên hành vi Q/E/R hiện có. Sau khi đạt hình ảnh, thêm Earth Prison như một action riêng, gợi ý phím `T` nếu kiểm tra chưa trùng. Không tự thay R Celestial Breaker.

Đọc `MechaPlayer`, `MechaTuning`, `MechaEffects`, `MechaHealth` trước khi nối. `MechaTuning.Get` hiện có switch tên action; `MechaPlayer` có xử lý riêng cho cost, movement và hit theo action. Không chỉ thêm một ScriptableObject rồi coi đã tích hợp.

`MechaHealth` có `TakeDamage(float amount, Vector3 point, Vector3 direction)` ở snapshot đã kiểm tra; xác nhận lại chữ ký và phân phe trước khi gọi. Tận dụng kiểu health này, không tạo hệ HP song song.

Tạo adapter hoặc điểm mở rộng nhỏ, trách nhiệm rõ. Khóa input có chọn lọc trong tụ/phóng; sau phát trả điều khiển khi hợp lý, trong khi vùng tiếp tục độc lập. Chỉ một hệ chịu trách nhiệm trừ energy/cooldown. Nếu hai skill cùng active, không dùng biến static cho target center, seed hoặc renderer dùng chung.

Nếu bản demo mới cần layer, thêm theo cấu hình tên thật và tránh đụng layer đã dùng. Không tăng version Unity hoặc đổi render pipeline để làm hiệu ứng này.

## 17. Camera và âm thanh

Camera nghiệm thu có 4 preset: góc cao chéo, sau người thi triển, nhìn về lồng từ phía địch, top-down. Giữ nguyên preset trong một lượt; chuyển preset giữa các lượt. Lưu ảnh ở cùng timestamp để so video.

Camera gameplay tùy chọn: rung rất nhỏ lúc impact và lúc trụ mọc, một nhịp duy nhất mỗi mốc, không rung liên tục suốt sustain. Không tự chiếm camera cinematic 8 s khi người chơi đang điều khiển nếu chưa thiết kế việc trả camera.

Âm thanh là đề xuất vì chưa kiểm chứng track gốc: tiếng điện tụ tăng nhẹ, crack ngắn khi phóng, thump đất khi impact, rumble đá mọc, electric loop vừa phải trong sustain, vụn đá và fizz khi tan. Có spatial falloff và giới hạn số voice. Đánh dấu placeholder nếu tạo tổng hợp. Hủy/pause/thu hồi phải dừng hoặc fade đúng audio, không để loop sót.

## 18. Công cụ preview và tạo lại asset

Scene VFXLab có sàn trung tính, người phát hoặc proxy và 3 dummy trong vùng. Dùng ánh sáng đủ đọc đá, không dựa vào nền quá tối để tạo cảm giác đẹp.

Cung cấp:
- Play/Stop/Replay, chọn seed cố định, tốc độ 0,5×/1×/2×.
- Chuyển 4 camera preset; bật/tắt bloom, bụi, glow, vòm, đá, sét để xem từng lớp.
- Hiển thị thời gian hiện tại và tên pha.
- Nếu có seek, phải resimulate particle từ đầu với seed cố định và evaluate mesh/curve về đúng t; không chỉ đổi text thời gian.
- Preview seek không chạy damage, cooldown hoặc phát audio lặp.

Editor builder tách khỏi runtime. Khi chạy lại, cập nhật asset do công cụ sở hữu; không tạo hàng loạt bản sao và không ghi đè asset người dùng đã sửa mà không có cơ chế giữ phiên bản. Lưu config tạo asset để tái lập được.

## 19. Hiệu năng và ngân sách ban đầu

Đo trên máy thật trong Game View/build với điều kiện ghi rõ. Mục tiêu 60 FPS ở 1080p chỉ là mục tiêu, không tuyên bố đạt khi chưa đo.

Ưu tiên: giảm overdraw vòm/bụi/glow, gộp vật liệu hợp lý, hạn chế số renderer, preallocate buffer sét, pooling, không LINQ/Find/GetComponents liên tục trong frame nóng.

Benchmark gồm baseline không skill, 1 instance và 3 instance đồng thời; ghi CPU/GPU frame time, GC allocation, batches/draw calls và số particle/renderer. Đừng so FPS giữa hai scene khác nhau rồi kết luận chi phí skill.

Low preset: giảm hạt, bỏ bớt nhánh chéo/glow và đèn phụ; vẫn giữ đủ 6 trụ, 6 đỉnh, chu vi lồng và timeline. Không giảm chất lượng bằng cách xóa trụ ngẫu nhiên hoặc biến vòm thành đĩa đục.

## 20. Các mốc triển khai và nghiệm thu

**Mốc A — Hình khối và nhịp:** sân thử, 6 trụ/bệ, vị trí/socket, clock, camera. Chạy một lượt không VFX phụ để xác nhận silhouette và thứ tự.

**Mốc B — Tụ/phóng/vòm:** dựng cầu, tia sát đất, impact, ribbon vòm. Chụp ở 0,75 / 2,25 / 3,00 / 3,75 / 4,50 s và so với ảnh nguồn.

**Mốc C — Lồng và tan:** sáu điểm sáng, chu vi, nhánh phụ, dissolve và tail. Chụp 5,50 / 6,75 / 7,50 s; top-down phải đọc rõ sáu vị trí.

**Mốc D — Tích hợp:** animation Gundam, adapter, phím T hoặc mapping đã chọn, optional damage/control, pause/cancel/reset.

**Mốc E — Kiểm thử và tinh chỉnh:** các lỗi dưới, profiling, báo cáo và README.

Bắt buộc kiểm tra:
- Không có compile error hoặc material hồng; asset/reference còn sau khi mở lại scene.
- Đúng 6 trụ; sét bám đầu trụ, không lag một frame rõ rệt khi rise/scale.
- Khi tắt bloom vẫn phân biệt được cấu trúc và đường sét chính.
- Tâm vòm không che kín mục tiêu; không có square particle hoặc z-fighting mặt đất.
- Vật cản trước người thi triển, sát tường, mép vực, dốc và không tìm được ground đều có kết quả xác định.
- Replay 20 lần, kể cả cancel giữa từng pha, không tăng instance/material mãi và không còn audio/particle rác.
- 30/60/120 FPS hoặc giới hạn tương ứng không làm mất marker hay tăng số damage tick; ghi điều kiện chạy thật.
- Pause tại tụ lực, mọc trụ, sustain và dissolve; resume đúng thời gian.
- Người thi triển chết/disable, mục tiêu chết/mất, reset scene, hai lần cast chồng nhau đều cleanup đúng.
- GameplayDemo chống hit lặp qua nhiều collider; VisualReference không đổi HP.
- Các mốc tạo dữ liệu so ảnh có ghi cùng seed/camera/scale/lighting; không lấy một góc đẹp để thay nghiệm thu toàn bộ.

## 21. Bàn giao

Giao scene và prefab thực, texture/mesh/material/shader đã lưu, config tuning, animation cast, code runtime/editor, hướng dẫn tiếng Việt và báo cáo.

README phải có: cách mở scene, cách phát chiêu, cách chuyển camera, nơi chỉnh radius/height/duration/color/particle counts, cách đổi socket Gundam, cách bật gameplay, cách reset và giới hạn còn lại.

Báo cáo tách rõ: đã tạo, đã compile, đã chạy Play Mode, đã so hình ảnh, đã đo hiệu năng, placeholder và chưa kiểm chứng. Kèm ảnh/clip thật từ Unity; không dùng video tham khảo như bằng chứng kết quả mới.

Nếu một texture/mesh hoặc phần rig không thể hoàn thiện, làm tiếp phần độc lập và nêu chính xác chỗ thiếu. Đừng tuyên bố giống 100% video chỉ vì đủ tên component.

## 22. Cơ sở kỹ thuật để kiểm tra API

Các trang dưới là tài liệu công cụ, không phải nguồn thông số thiết kế của skill:
- [Unity 6.5 — Particle System modules](https://docs.unity.com/en-us/engine/6000.5/manual/visual-effects/particle-systems/particle-system-modules)
- [Unity 6.5 — Particle rendering and shading](https://docs.unity.com/en-us/engine/6000.5/manual/particle-rendering-shading)
- [Unity 6.5 — URP Particles Unlit](https://docs.unity3d.com/6000.5/Documentation/Manual/urp/particles-unlit-shader.html)

Hãy bắt đầu bằng đọc project và ảnh tham khảo, kiểm tra component có sẵn, rồi triển khai Mốc A. Tiếp tục tuần tự tới scene có thể chạy, so hình, sửa lỗi và bàn giao theo các tiêu chí trên.
