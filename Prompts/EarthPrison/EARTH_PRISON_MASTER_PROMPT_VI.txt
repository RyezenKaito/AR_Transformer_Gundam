> ĐẶC TẢ MỚI NHẤT — 29/10/2026
> Đọc [master mới](../2026-10-29-Camera-Flight-Weapons/00_MASTER_PROMPT_VI.md) và các prompt 01–05 cùng thư mục. Camera mặc định sau lưng nhưng xoay 360°; Attack, Skill và Ultimate đều bay tới phía địch rồi trở về; khớp chuyển động mượt và vũ khí chỉ rút khi dùng.
> Phần bên dưới là tài liệu ngày trước được giữ để tham chiếu. Các ràng buộc camera fixed, root tuyệt đối đứng yên khi đánh, skill/ultimate đứng tại chỗ hoặc súng cầm sẵn đã được thay thế.

---

# MASTER PROMPT — EARTH PRISON: ULTIMATE TRONG TRẬN ĐÁNH THEO LƯỢT

Phiên bản 28/10/2026. Thay thế bản Earth Prison dùng trong combat thời gian thực trước đây. Quyết định mới của người dùng: hai phía đứng cố định, camera cố định, đánh luân phiên; né bằng bấm đúng thời điểm tại chỗ. Earth Prison là ultimate của nhân vật.

Hãy tạo ultimate Earth Prison trong Unity và tích hợp vào hệ thống đánh theo lượt của tôi. Người chơi đứng bên trái, quái bên phải. Nhân vật tụ điện vàng, phóng sét về phía quái, dựng sáu trụ đá bao quanh đội hình quái, nối sét thành lồng, gây damage tại các mốc đã định rồi tan rã. Toàn bộ chuỗi được nhìn từ một camera cố định, hai phía luôn cùng trong khung hình. Các actor không chạy, dash, teleport hoặc đổi vị trí khi thi triển/nhận đòn.

Hãy đọc `E:/My project/Prompts/GUNDAM_TURN_BASED_MASTER_PROMPT_VI.md` để lấy luật lượt và né. Prompt này quy định chi tiết hình ảnh/animation/timeline của Earth Prison; nó không tự tạo một vòng combat riêng. Nếu chưa có BattleManager theo lượt, làm mốc nền của master trước và dùng VFXLab để phát triển hiệu ứng song song về mặt công việc.

Tôi cần scene, prefab, mesh, texture, material, shader và code thật đã lưu, có thể chỉnh bằng Inspector. Thực hiện từng mốc, chạy thử và báo đúng kết quả. Các thông số mỹ thuật/gameplay là điểm khởi đầu để tinh chỉnh.

## 1. Mục tiêu và hai chế độ rõ ràng

Đảm nhiệm VFX Artist, Technical Artist, Technical Animator và Gameplay Engineer. Thứ tự ưu tiên: đúng luật lượt/anchor/camera → đúng nhịp video → đúng sáu trụ và lồng sét → animation/hiệu ứng mượt → lifecycle → hiệu năng.

- `BattleUltimate`: chế độ sản phẩm chính, kích hoạt từ command Ultimate trong PlayerCommand, dùng 100 thanh Ultimate và tiêu thụ một lượt, kết thúc mới chuyển pha.
- `VisualReference`: chế độ lab để so video, có seed/seek, không HP, tài nguyên hoặc chuyển lượt.

Scene battle dùng Earth Prison làm Ultimate thay vai trò R Celestial Breaker. Trong trận, R là shortcut chọn Ultimate, xác nhận theo command UI; không phải hotkey tự cast ngoài lượt. Mỗi lúc chỉ có một action; không cho quái đánh trong lúc ultimate đang chạy.

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

Snapshot cấu hình project khi soạn prompt ngày 28/10/2026:
- Project: `E:/My project`.
- Unity: `6000.5.8f1`; URP `17.5.0`.
- Input System `1.20.0`; Timeline `1.8.12`.
- Manifest chưa liệt kê VFX Graph hoặc Cinemachine. Không mặc định hai package này tồn tại.
- Demo Gundam có tại `Assets/MechaCombatDemo/` với `MechaPlayer`, `MechaEffects`, `MechaHealth`, `MechaTuning`.
- Rig Gundam demo được mô tả cao 2,6 m; đọc lại prefab để xác nhận trước khi scale.
- Demo realtime cũ có Q/E/R và R Celestial Breaker. Scene theo lượt mới dùng Earth Prison làm Ultimate/R, không kích hoạt MechaPlayer/MechaEnemy/MechaCamera realtime.

Luật combat hiện hành: `E:/My project/Prompts/GUNDAM_TURN_BASED_MASTER_PROMPT_VI.md`. Hãy kiểm tra lại project hiện tại trước khi sửa. Nếu đọc được dữ liệu thì tự đọc, không hỏi lại tôi những thông tin đã có. Nếu không xem được video, đọc các ảnh đã trích và nói rõ giới hạn.

## 3. Các đặc điểm bắt buộc phải giữ

- Sét vàng chanh với lõi vàng trắng; đá xám than; bụi chân trụ vàng cam/nâu.
- Pha điện tỏa rộng quanh người thi triển thu lại thành một cầu điện nhỏ trước khi phóng.
- Tia chính nối từ nguồn phát tới vùng mục tiêu, chạy thấp gần mặt đất, có nhánh và độ gấp khúc.
- Impact bật từ mặt đất lên; vòng xoáy mở thành vòm thấp gồm nhiều dải rách, tâm còn trong.
- Chính xác **sáu** trụ bố trí quanh vùng theo lục giác, thân không đều, có bệ đá vỡ và bụi riêng.
- Các trụ mọc lên trước khi lồng sét hoàn chỉnh xuất hiện.
- Sáu đỉnh phát sáng, có sét dọc trụ, sét nối chu vi và vài nhánh chéo bên trong.
- Đá, điện, bụi và mảnh vụn biến mất theo thời gian khác nhau.
- Camera battle cố định cả trận, giữ hai phía trong hình, không zoom/cắt cảnh/rung; các góc khác nhau trong video chỉ dùng tham khảo dựng hình. Actor giữ nguyên BattleRoot khi cast, hit và dodge.

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

Cho phép `playbackSpeed` 0,5×/1×/2× trong preview; action clock, animation và VFX cùng tốc độ. Khi đổi tốc độ, clock, animation, emission và thời gian fade phải đi cùng nhau. Khi phát lại mặc định, giữ nhịp khoảng 8 s để so video trước khi rút ngắn cho gameplay.

Trong BattleUltimate, Confirm là commit tại t=0: validate target/resource, trừ 100 và khóa command một lần. Mốc 2,65 s chỉ là release hình ảnh, không phải thời điểm trừ tài nguyên. Damage tại 3,35 s và 5,10 / 5,50 / 5,90 / 6,30 s. Action chỉ completion sau khoảng 8,00 s và cleanup cần thiết; quái không xen lượt ở giữa. Các mốc này dùng cùng action clock, không khởi chạy clock riêng cho VFX.

## 5. Bố trí cố định, quy mô và sáu vị trí trụ

Đơn vị Unity xấp xỉ mét. Chiều cao Gundam demo khởi điểm khoảng 2,6 m; xác minh rig thật. Nhân vật ở PlayerAnchor phía trái, quái tại EnemyAnchor phía phải. EnemyFormationAnchor là tâm vùng ultimate đã bố trí trong scene, không phải vị trí click đất hoặc lock-on đuổi theo quái.

| Tham số đề xuất | Giá trị khởi điểm |
| --- | --- |
| PlayerAnchor / EnemyFormationAnchor | x=-6 m / x=+6 m, điều chỉnh theo sân và camera |
| Quái phụ trong cấu hình 1v3 | Lệch chiều sâu z khoảng ±2,2 m; một quái dùng slot giữa |
| Bán kính đặt trụ | 4,8 m; fit toàn bộ bounds các slot quái và khoảng hở |
| Vòm lớn nhất | Bán kính 5,3 m, cao 2,0–2,4 m |
| Cao / rộng trụ | 4,2 m ±8% / 0,55–0,85 m |
| Bán kính bệ đá | 0,8–1,0 m |
| Bán kính cầu điện | 0,30–0,45 m |
| Tia truyền phần giữa quỹ đạo | Cách đất khoảng 0,10–0,25 m |

Đây là kích thước dựng lại, không phải mét đo được từ video. Camera được fit trước trận theo toàn bộ bounds actor và VFX lớn nhất, sau đó giữ transform và lens cố định. Nếu lồng bị cắt hình, sửa bố trí/quy mô khi setup; không zoom camera trong lúc ultimate.

Sáu vị trí: `theta_i = offset + i*2*pi/6`, `P_i = center + radius*(cos(theta_i)*U + sin(theta_i)*V)`. U,V là trục mặt sàn. Dùng nền sân phẳng và kiểm sáu vị trí trước khi chạy trận; đủ sáu trụ, bệ không xuyên quái hoặc nhau. Quái không được di chuyển/teleport để nhường chỗ cho trụ.

Trụ dùng mesh gồ ghề có thể tích, ít nhất 2–3 biến thể, đầu nhọn không đều và bệ đá vỡ. Billboard/cylinder trơn chỉ được dùng trong blockout, không phải nghiệm thu hình ảnh. Sáu trụ là cảnh trí hiệu ứng, không bật collider đẩy actor khỏi slot.

## 6. Cấu trúc asset và prefab

Tạo trong `Assets/TurnBasedBattle/Abilities/EarthPrison/`:

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

Tên là đề xuất; có thể gộp component nhỏ nhưng phải đọc được trách nhiệm. Root scale mặc định 1. AreaAnchor thuộc world/EnemyFormationAnchor, CastAnchor bám socket người chơi. Các actor giữ BattleRoot cố định; animation chỉ thay pose. Dùng đủ root riêng để scaling hoặc animation tay không kéo cả lồng theo.

## 7. Cầu điện và animation thi triển

Tạo socket thật sau khi xem rig, lưu reference trong Inspector; không tìm bone bằng chuỗi đoán tên mỗi frame.

Pha 0–1,1 s: điện tỏa thành 4–8 nhánh quanh vùng tay/ngực, không phủ kín toàn thân. Pha 1,1–2,65 s: giảm nhánh xa, tăng độ tập trung vào lõi; cho 2–3 vòng lệch mặt phẳng cuộn quanh cầu. Vòng cần có khoảng rỗng, không là một quả bóng đục vàng.

Gợi ý animation cho Gundam, có thể đổi theo giới hạn khớp thực:
- 0–0,6 s: mở thế đứng, gối hạ nhẹ, ngực hướng mục tiêu.
- 0,6–1,3 s: đưa tay có socket lên ngang ngực/đầu; điện gom theo tay.
- 1,3–2,6 s: giữ pose có lực, chỉ thêm chuyển động phụ nhỏ.
- 2,65–3,15 s: đưa tay xuống/trước để phát tia; hông/ngực dẫn trước cánh tay, giữ chân chống.
- 3,15–3,65 s: hồi về tư thế, người thi triển vẫn ở ngoài vùng.

Các góc khớp phải kiểm tra trên rig thật; không cho cánh tay xuyên sừng, giáp vai hoặc backpack. Clip thật được lưu dưới Animation. Sau khi phát, nhân vật hồi về IdleBattle tại chỗ, nhưng action vẫn khóa cho tới khi toàn bộ lồng/tail và marker kết thúc. Không trả locomotion, menu chọn lệnh hoặc quyền hành động cho quái giữa ultimate. Animator root motion tắt; BattleRoot không đổi vị trí.

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

GroundTransfer nối source socket đến vùng EnemyFormationAnchor đã chốt. Kiểm đường truyền và mặt sàn khi bố trí sân; dùng đường cong nối từ tay xuống phần sát đất. Đây là hình ảnh trong trận theo lượt, không raycast đổi danh sách damage theo số collider. Không chiếu từng điểm noise xuống đất vì sẽ làm mất độ dày nhánh. Tia có thể reveal theo tiến độ từ A tới B trong khoảng 0,15–0,25 s, sáng giữ ngắn rồi fade trước 3,5 s. Các số này là tuning, cần đối chiếu ảnh 2,75/3,00/3,25 s.

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

## 14. Clock, tan rã và vòng đời action

BattleActionRunner giữ clock có thẩm quyền; EarthPrisonController nhận tiến độ và biểu diễn các lớp. Các marker logic do runner phát một lần theo `(actionId, hitIndex, targetId)`. Animation Event có thể phục vụ presentation, không trở thành nguồn damage thứ hai.

Giao diện gợi ý: `Play(ActionContext)`, `EvaluateVisual(time, seed)`, `Cancel(reason)`, `ResetForPool()`, completion có action token. Context có caster/target ID, source socket, formation anchor, seed, scale và mode.

Mốc vượt qua kiểm bằng khoảng thời gian, không bằng `time == marker`. Khi frame dài vượt nhiều marker, xử lý đúng thứ tự, kiểm target sống trước mỗi hit. Khi source/target chết hoặc trận reset, token cũ vô hiệu. Seek trong VisualReference không dispatch gameplay.

Tan rã: hạ glow/cạnh, dừng sinh hạt; dissolve thân đá có mép vàng mảnh; bệ/bụi/mảnh lưu thêm rồi tan. Hạt/beam/audio đều có thời điểm kết thúc, mục tiêu tail không vượt khoảng 8 s; nếu lifetime làm tail dài hơn phải sửa emitter timing/lifetime hoặc cập nhật ActionDuration có chủ đích. Không để quái bắt đầu tấn công dưới một vòm đang che cue né.

Pool reset Transform/scale/emission/color/dissolve/trail/ribbon buffers/seed/event flags/audio/property blocks. Dùng StopEmittingAndClear cho reset cứng; kết thúc tự nhiên cho phép tail. Không tạo material mới mỗi frame hoặc để particle loop giữ instance mãi.

Pause đóng băng clock, actor animation, particle, SFX cần thiết và UI cue theo cùng policy. Không có nhiều coroutine ghi Time.timeScale. Action completion phát đúng một lần sau khi marker, recovery và tail đã hoàn tất; BattleManager mới quyết định EnemyPhase hoặc Victory.

## 15. Ultimate, target và damage trong combat theo lượt

Điều kiện: PlayerCommand, nhân vật sống, thanh Ultimate đủ 100, có ít nhất một quái sống. UI highlight toàn bộ quái sống phía phải; xác nhận bằng command. Validate và commit tại t=0, trừ 100 một lần, khóa các lệnh còn lại và tiêu thụ một lượt. Không có chi phí energy 60, cooldown 18 giây hoặc nhấn T ngoài lượt như bản trước.

AoE chọn target bằng roster/phe, chụp danh sách ID sống tại commit; không chọn theo va chạm particle hoặc vòng overlap bất kỳ. Ground/VFX radius chỉ phục vụ bố cục; bounds quái phải nằm trong lồng đã bố trí.

| Mốc action clock | Damage mỗi target còn sống |
| --- | --- |
| 3,35 s — GroundImpact | 120 |
| 5,10 s — điện trong lồng | 60 |
| 5,50 s | 60 |
| 5,90 s | 60 |
| 6,30 s | 60 |

Tổng tối đa 360/target cho đủ năm mốc; số liệu là tuning. DamageResolver chống trùng hit dù target có nhiều collider. Quái đã chết không nhận tick tiếp, không bật lại renderer/health và không hồi ultimate từ các tick. Earth Prison không tự nạp thanh Ultimate của caster.

Quái không RNG né chiêu này ở bản đầu. Cơ chế người dùng bấm né thuộc các đòn quái tấn công nhân vật trong EnemyAction, không chen input né vào ultimate của chính mình.

Không thêm stun/bỏ lượt, kéo quái, knockback, root di chuyển hoặc barrier vật lý: các actor vốn đứng tại slot. Lồng đá mang vai trò hình ảnh và damage theo mốc. Nếu về sau muốn debuff mất lượt, phải định nghĩa riêng theo turn, không suy ra tự động từ tên Prison.

Mục tiêu còn HP không tự biến mất để bắt chước video. Death animation diễn ra tại anchor. Nếu toàn bộ quái chết trước 8 s, hoàn tất cleanup an toàn rồi chuyển Victory; không mở EnemyPhase. Nếu còn quái, mở EnemyPhase sau completion và kiểm roster sống lại.

## 16. Tích hợp vào battle scene, thay vai trò ultimate cũ

Scene chính là `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity`; VFXLab riêng dùng thử hình ảnh. Tái sử dụng rig/mesh/material phù hợp từ MechaCombatDemo, nhưng battle prefab không chạy MechaPlayer, MechaEnemy, MechaCamera hoặc MechaPlaytest realtime.

Nút Ultimate và R trong **scene mới** là Earth Prison; không gọi TryAction("R") của MechaPlayer vì nhánh đó chứa Celestial Breaker, di chuyển và camera cũ. Không chỉ thêm config vào MechaTuning rồi coi đã chuyển sang theo lượt.

BattleManager, ActionRunner và DamageResolver mới điều phối thứ tự và kết quả. Nếu dùng lại MechaHealth.TakeDamage(float, Vector3, Vector3), xác nhận chữ ký hiện tại và gỡ subscriptions từ các controller realtime trong prefab mới. Chỉ một nguồn health và damage.

Source cast animation, target hit reaction và world VFX đều nhận action ID/clock. Sau release, caster về IdleBattle nhưng menu vẫn khóa đến completion. Không thay đổi BattleRoot, không motor hoặc NavMesh chase chạy nền. Camera battle cố định xuyên suốt chuỗi.

Giữ asset/scene realtime cũ như tài nguyên tham khảo, không dùng chúng làm luồng chạy mặc định mới. README phải dẫn vào TurnBattleDemo. Không nâng Unity hoặc đổi render pipeline để thực hiện thiết kế này.

## 17. Camera cố định và âm thanh

Camera trận giữ nguyên position/rotation/lens từ đầu đến cuối, nhìn được người chơi trái và quái phải cùng sáu trụ/vòm. Không zoom, orbit, cut, cinematic hoặc shake kể cả ultimate. Tạo lực qua pose, flash cục bộ, điện và âm thanh.

Các camera cao/chính diện/top-down chỉ là preset trong VFXLab để kiểm hình học, chọn trước khi phát lại; chúng không trở thành chuỗi chuyển camera trong battle. Ưu tiên nghiệm thu trên camera battle cố định vì đó là hình người chơi sẽ thấy.

SFX là đề xuất mới: điện tụ, crack release, thump impact, rumble đá, electric loop sustain, fizz/vụn đá tan. Có spatial falloff, giới hạn voice, đánh dấu placeholder nếu âm tổng hợp. Pause/cancel/reset phải dừng/fade loop đúng. Không để SFX/VFX ultimate còn lấn cue né của lượt quái kế tiếp.

## 18. Công cụ preview và tạo lại asset

Scene VFXLab có sàn trung tính, người phát hoặc proxy và 3 dummy trong vùng. Dùng ánh sáng đủ đọc đá, không dựa vào nền quá tối để tạo cảm giác đẹp.

Cung cấp:
- Play/Stop/Replay, chọn seed cố định, tốc độ 0,5×/1×/2×.
- Chuyển 4 camera preset chỉ trong lab giữa các lần phát; camera battle không đổi; bật/tắt bloom, bụi, glow, vòm, đá, sét để xem từng lớp.
- Hiển thị thời gian hiện tại và tên pha.
- Nếu có seek, phải resimulate particle từ đầu với seed cố định và evaluate mesh/curve về đúng t; không chỉ đổi text thời gian.
- Preview seek không chạy damage, cooldown hoặc phát audio lặp.

Editor builder tách khỏi runtime. Khi chạy lại, cập nhật asset do công cụ sở hữu; không tạo hàng loạt bản sao và không ghi đè asset người dùng đã sửa mà không có cơ chế giữ phiên bản. Lưu config tạo asset để tái lập được.

Battle preview dùng command flow thật. Các nút Fill Ultimate/Skip Turn/seek chỉ là debug được đánh dấu; bản chơi không cho debug làm thay đổi lượt hoặc tái phát damage.

## 19. Hiệu năng và ngân sách ban đầu

Đo trên máy thật trong Game View/build với điều kiện ghi rõ. Mục tiêu 60 FPS ở 1080p chỉ là mục tiêu, không tuyên bố đạt khi chưa đo.

Ưu tiên: giảm overdraw vòm/bụi/glow, gộp vật liệu hợp lý, hạn chế số renderer, preallocate buffer sét, pooling, không LINQ/Find/GetComponents liên tục trong frame nóng.

Benchmark gồm baseline không skill, 1 instance battle và stress test 3 instance chỉ trong lab (trận vẫn tuần tự một action); ghi CPU/GPU frame time, GC allocation, batches/draw calls và số particle/renderer. Đừng so FPS giữa hai scene khác nhau rồi kết luận chi phí skill.

Low preset: giảm hạt, bỏ bớt nhánh chéo/glow và đèn phụ; vẫn giữ đủ 6 trụ, 6 đỉnh, chu vi lồng và timeline. Không giảm chất lượng bằng cách xóa trụ ngẫu nhiên hoặc biến vòm thành đĩa đục.

## 20. Mốc triển khai và nghiệm thu

Mốc A: xác nhận master turn-based, rig/anchors, scene hai phía và camera fixed; có luồng một lượt người chơi rồi quái. Lab tạo blockout sáu trụ tại vùng quái.

Mốc B: cầu điện/tia/impact/vòm; so ảnh 0,75 / 2,25 / 3,00 / 3,75 / 4,50 s. Người thi triển không dịch root, quái không đổi slot.

Mốc C: sáu đỉnh, sét chu vi/nhánh trong, dissolve/tail; so 5,50 / 6,75 / 7,50 s. Xác nhận đọc được sáu trụ trong camera battle.

Mốc D: command Ultimate/R, gauge 100, commit một lần, năm damage marker, completion 8 s, EnemyPhase/Win; không nhánh realtime cũ chạy nền.

Mốc E: pause/reset/death, 1v1/1v3, né đòn quái sau ultimate, profiling và tài liệu.

Kiểm bắt buộc:
- Camera/actor BattleRoot không đổi qua ultimate và lượt né kế tiếp; rig cử động bình thường.
- Hai phía và toàn lồng không bị cắt ở aspect ratios mục tiêu, cue né/HP không bị particle che.
- Có đúng 6 trụ, tip socket đúng, vòm có khe; tắt bloom vẫn thấy cấu trúc.
- Confirm lặp chỉ trừ100 một lần; chưa đầy hoặc ngoài lượt không cast.
- Damage đúng 120+4×60 tối đa/target; target chết không nhận tick tiếp; không damage từ particle.
- Không có quái tấn công hoặc mở menu người chơi trước completion/resolve.
- Quái chết trước hàng đợi bị bỏ; tất cả chết chuyển thắng; không tự stun hoặc mất lượt khi còn sống.
- 20 lần replay/reset không tăng material/instance liên tục và không callback trận cũ tác động trận mới.
- Pause tại từng pha và resume; 30/60/120 FPS hoặc cấu hình thực kiểm được không đổi số marker.
- VFXLab không thay HP/gauge/turn; debug seek không phát damage.
- Sau cleanup, cue né đòn quái rõ, phản ứng Space đúng luật master, không sót focus hoặc khóa input.
- Compile, material/reference, mở lại scene và hiệu năng được kiểm thật, có báo cáo giới hạn.

## 21. Bàn giao

Giao scene và prefab thực, texture/mesh/material/shader đã lưu, config tuning, animation cast, code runtime/editor, hướng dẫn tiếng Việt và báo cáo.

README phải có: cách mở scene, cách phát chiêu, camera battle cố định và preset lab, nơi chỉnh radius/height/duration/color/particle counts, cách đổi socket Gundam, cách tích hợp command Ultimate và gauge, cách reset và giới hạn còn lại.

Báo cáo tách rõ: đã tạo, đã compile, đã chạy Play Mode, đã so hình ảnh, đã đo hiệu năng, placeholder và chưa kiểm chứng. Kèm ảnh/clip thật từ Unity; không dùng video tham khảo như bằng chứng kết quả mới.

Nếu một texture/mesh hoặc phần rig không thể hoàn thiện, làm tiếp phần độc lập và nêu chính xác chỗ thiếu. Đừng tuyên bố giống 100% video chỉ vì đủ tên component.

## 22. Cơ sở kỹ thuật để kiểm tra API

Các trang dưới là tài liệu công cụ, không phải nguồn thông số thiết kế của skill:
- [Unity 6.5 — Particle System modules](https://docs.unity.com/en-us/engine/6000.5/manual/visual-effects/particle-systems/particle-system-modules)
- [Unity 6.5 — Particle rendering and shading](https://docs.unity.com/en-us/engine/6000.5/manual/particle-rendering-shading)
- [Unity 6.5 — URP Particles Unlit](https://docs.unity3d.com/6000.5/Documentation/Manual/urp/particles-unlit-shader.html)

Hãy đọc master combat theo lượt, project và ảnh tham khảo, kiểm tra component có sẵn, rồi triển khai Mốc A với Earth Prison là Ultimate và camera fixed. Tiếp tục tuần tự tới scene có thể chạy, so hình, sửa lỗi và bàn giao theo các tiêu chí trên.
