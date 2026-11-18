# Skill + Animation task

**Project:** AR_Transformer_Gundam · **Nhân vật mẫu:** RX-0 Unicorn Gundam · **Ngày:** 14/11/2026  
**Trạng thái:** Task 2 đã tích hợp và kiểm thử trên sân ảo Unity. Chờ chủ project duyệt hình ảnh và nhịp chiêu trong **Unicorn Demo** trước khi mở Task 3.

## Bản Psycho-Frame hiện tại

- Khiên vẫn có trong dữ liệu Blender và FBX. Prefab Unity tắt khiên. Beam Magnum vác chéo trên backpack và chỉ chuyển sang tay phải trong chiêu 3.
- Chiêu 2 **Psycho-Frame Awakening** là buff bản thân. Frame 48 kích hoạt buff, vòng xung kích và trạng thái trên HUD. Asset **Unicorn Psycho-Frame Buff** cấu hình ATK, DEF, giảm sát thương vào giáp và thời lượng.
- Chiêu 3 **Beam Magnum** rút súng, nhắm, bắn ở frame 48 rồi cất về backpack. Chiêu 1–3 dùng góc chiến trường và HUD thường.
- Chiêu 4 **Psycho-Frame Full Burst** có hai cú đấm và cú đá ở frame 30, 44 và 61, rồi từ trên không tụ năng lượng vào hai tay và bắn Ultra Beam ở frame 139. Chỉ chiêu 4 có camera điện ảnh. Chi phí các chiêu vẫn là 0/1/2/5 Burst.
- [Nguồn Blender v0.4, bake 30 FPS](../../SourceArt/Blender/Unicorn/Unicorn_Combat_v04_PsychoFrame.blend). Những mục ở dưới ghi lại các bản thiết kế trước.

## Bản chỉnh nhát bổ và Full Burst trước đó

- Chiêu 2 rút kiếm phải, đưa lưỡi qua đầu rồi bổ dọc một nhát. Unity dùng Animation Event ở frame 48 để hiện vệt chém và tính một lần sát thương lên mục tiêu đã chọn.
- Full Burst rút kiếm phải, ném khiên sang trái, rút kiếm còn lại rồi lao vào bổ cả hai kiếm. Vệt chém, mảnh sáng, sóng xung kích và sát thương cùng bắt đầu ở Animation Event frame 108. Khiên trở về vị trí gắn trong đoạn hồi tư thế.
- [Nguồn animation và mesh hiệu ứng Blender](../../SourceArt/Blender/Unicorn/Unicorn_Combat_v03_Overhead.blend). Hai clip giữ nguyên chi phí Burst hiện tại.

## Bản kiếm trước đó

Phần này ghi lại bản v0.2 trước khi đổi sang nhát bổ dọc.

- Chiêu 2 dùng đúng cán Beam Saber bên phải của backpack. Unicorn chuyển súng ra sau lưng, rút cán, bật lưỡi đỏ, chém hai nhát ở frame 41 và 54 rồi cất kiếm. Chi phí vẫn là 1 Burst.
- Full Burst rút cả hai kiếm. Bốn nhịp gây sát thương ở frame 60, 71, 99 và 127 với tỷ lệ 16%, 19%, 25% và 40%. Bỏ qua cinematic xử lý những nhịp còn lại theo thứ tự. Chi phí vẫn là 5 Burst.
- Lưỡi, vệt chém, mảnh sáng và sóng xung kích là mesh được dựng và keyframe trong [Blender](../../SourceArt/Blender/Unicorn/Unicorn_Combat_v02_Sabers.blend). Unity xử lý shader, camera, âm thanh và sát thương.
- [Video chiêu 2 và Full Burst](../Reports/UnicornCombat/Unicorn_Sabers.webm) · [Kiểm thử Play Mode bản kiếm](../Reports/UnicornCombat/SabersValidation.txt). Mở trong Unity bằng **Unicorn Combat → Open Demo**.
- Các storyboard chiêu 2 và Full Burst ở dưới ghi lại thiết kế v0.1. Bản này thay thế hai storyboard đó.

## 1. Thứ tự làm việc

| Task | Kết quả cần có | Điều kiện chuyển bước |
|---|---|---|
| 1 · Nghiên cứu và thiết kế Unicorn | Nguồn tham khảo, bộ 4 chiêu, storyboard, số liệu thử, phân việc Blender/Unity và kịch bản kiểm thử | Cùng duyệt các lựa chọn ở mục 9. Chỉ triển khai khi chủ project nhắn **“Bắt đầu task 2”**. |
| 2 · Sản xuất và tích hợp | Animation, asset vũ khí, VFX, âm thanh, camera và combat chạy trong scene **Unicorn Demo** | Chạy các ca kiểm thử ở mục 7, bàn giao demo và chờ chủ project duyệt. |
| 3 · Áp dụng cho roster | Dùng bộ tiêu chí Unicorn để thiết kế và triển khai từng Gundam còn lại | Task 1 đã duyệt, Task 2 đã qua kiểm thử và chủ project chấp thuận demo Unicorn. |

**Hàng đợi đã chốt:** Sau Unicorn, làm nhóm có clip trước gồm Strike Rouge Ootori, Justice, Extreme, RX-78-2 Rifle và RX-78-2 Saber. Đánh giá lại bộ chiêu Strike Rouge đang có trước khi quyết định phần cần làm mới. Hai bản RX-78-2 được tính là hai asset trong roster hiện tại.

**Ghi nhớ cho phiên sau:** BlueFlame, HaoLing, SkyDefender và Kamiki Burning Gundam hiện chưa có clip animation nhân vật được Unity import. HaoLing có chuyển động phượng bằng script trong showcase. Chỉ bắt đầu nhóm bốn model này sau khi đã thiết kế bộ chiêu cho nhóm có clip. Với nhóm bốn, kiểm tra rig và khớp trước khi dựng animation chiến đấu.

## 2. Nghiên cứu RX-0 Unicorn

Phạm vi là RX-0 của Banagher Links trong Gundam UC. Nguồn ưu tiên gồm hồ sơ anime, hướng dẫn Bandai, tài liệu và trailer game chính thức. Quy tắc riêng của game tham khảo chỉ dùng để học cách trình bày và tìm ý tưởng, còn chỉ số của project được thiết kế theo GDD nội bộ.

| Chủ đề | Điều có thể xác nhận | Hướng áp dụng |
|---|---|---|
| Psycho-Frame | Khung của RX-0 tích hợp chức năng psycommu ở cấp hạt trong vật liệu. Tài liệu mô tả nó nhận tín hiệu tâm lý của phi công và cải thiện phản hồi điều khiển. [PG RX-0, Bandai](https://manual.bandai-hobby.net/pdf/1057.pdf) · [Hồ sơ Destroy Mode, G Generation ETERNAL](https://gget.ggame.jp/en/titles/detail.php?t=i3gcv8kx) | Hạt đỏ hoặc lục là cách **diễn họa** cộng hưởng trong game. Giữ ba khái niệm riêng: Psycho-Frame là khung, Psycho Field là trường, Burst 0–5 là tài nguyên của project. |
| NT-D và Destroy Mode | Khung mở rộng, giáp trượt để lộ Psycho-Frame, đầu đổi mặt và sừng tách thành V-fin. Tài liệu chính thức nhấn mạnh độ cơ động của Destroy Mode. [PG RX-0](https://manual.bandai-hobby.net/pdf/1057.pdf) · [Hồ sơ Destroy Mode](https://gget.ggame.jp/en/titles/detail.php?t=i3gcv8kx) | Bản đề xuất dùng chuyển động đóng/mở chốt và ánh sáng chạy theo khe giáp để tạo cao trào. Biến hình toàn thân cần hai hình thái đã kiểm tra hình học. |
| Đỏ và lục | NT-D gắn với ánh đỏ. Nguồn chính thức mô tả ánh lục ở giai đoạn thức tỉnh, điều khiển khiên từ xa và tạo trường đặc biệt. [Real Experience Model RX-0](https://en.gundam-official.com/feature/auto_trans/) · [Hồ sơ Unicorn Awakened, GBO2](https://bo2.ggame.jp/jp/ms_stage/ms_detail.php?ms=rz6NMaL2) | Đỏ là màu chính của khung trong ulti. Có thể chuyển sang lục ngắn ở đỉnh cộng hưởng nếu chọn phương án đó khi duyệt. Thời lượng và tác dụng trong trận là thiết kế riêng. |
| Beam Magnum | Hỏa lực một phát được mô tả khoảng bốn lần Beam Rifle thông thường, dùng một E-pack mỗi phát. [Hướng dẫn Unicorn Gundam, Bandai](https://manual.bandai-hobby.net/pdf/674.pdf) | Một phát rõ tư thế chống giật, có nhịp thải E-pack. Hệ số HP của project dùng số thử riêng. E-pack là đạo cụ hình ảnh, không mở thêm hệ đạn trong demo. |
| Head Vulcan | RX-0 có cặp Vulcan trên đầu, bên cạnh các vũ khí chính như Magnum, Bazooka và Saber. [Hướng dẫn Unicorn Gundam, Bandai](https://manual.bandai-hobby.net/pdf/674.pdf) | Chiêu cơ bản dùng hai nòng ở đầu. Hiệu ứng đạn vật lý ngắn, điểm xuất phát kiểm bằng góc trước và góc 3/4. |
| Beam Saber và Beam Tonfa | Có thể rút saber từ backpack. Tonfa triển khai từ cơ cấu ở cẳng tay để dùng cận chiến. [PG RX-0](https://manual.bandai-hobby.net/pdf/1057.pdf) | Dùng Tonfa để tạo khác biệt với chiêu saber của các máy khác. Tạo bản lề và chuôi nhỏ tại cẳng tay, lưỡi beam sinh ở Unity. |
| Shield và I-Field | Khiên có cơ cấu mở kiểu X và bộ tạo I-Field chống beam. Shield funnel thuộc tham chiếu Awakened. [Hồ sơ Destroy Mode](https://gget.ggame.jp/en/titles/detail.php?t=i3gcv8kx) · [Unicorn Awakened, GBO2](https://bo2.ggame.jp/en/info/?p=162302) | Nếu bổ sung khiên, dùng nó để diễn họa Parry. Tính thành công và sát thương vẫn do luật Parry hiện tại quyết định. Shield funnel, Bazooka và Gatling để lại cho biến thể hoặc đợt sau. |
| Lối đánh | GBO2 mô tả RX-0 đánh tầm trung đến gần, sử dụng Magnum, cận chiến và các hình thái tùy tình huống. Đây là cách diễn giải chính thức của game đó. [GBO2 Unit Adjustment](https://bo2.ggame.jp/en/info/?p=132850) · [GBO2 RX-0](https://bo2.ggame.jp/en/info/?p=94619) | Vai trò đề xuất cho project là **đánh đơn, phá giáp và dồn sát thương lên mục tiêu ưu tiên**. Cảm giác cơ động thể hiện bằng animation, còn vị trí chiến thuật vẫn là ô cố định. |

### Tư liệu xem khi duyệt chuyển động

- [Mobile Suit Gundam Unicorn, hồ sơ anime](https://en.gundam-official.com/series/unicorn) và [danh sách phát RE:0096 chính thức](https://en.gundam-official.com/news/mgo4blh4hp53v7z6jfcg8w1v). Khi xem, ghi riêng nhịp kích hoạt NT-D đầu loạt và chuyển màu cộng hưởng cuối loạt.
- [Real Experience Model RX-0](https://en.gundam-official.com/feature/auto_trans/) có mục **MOVIE**, cơ cấu biến hình, góc nhìn 360° và danh sách cảnh theo từng tập OVA. Dùng để đối chiếu thứ tự giáp mở và ánh sáng, rồi chọn các chi tiết phù hợp với rig hiện tại.
- [Trailer Unicorn Awakened của Bandai Namco](https://www.youtube.com/watch?v=K3s0mnsDkMY) là tham chiếu hình ảnh cho dạng thức tỉnh. Trong buổi duyệt, đánh dấu khoảng thời gian cụ thể cho những đoạn muốn lấy cảm hứng. Mốc frame ở mục 4 là storyboard mới cho project.

## 3. Asset hiện có và ràng buộc

- Blender đang mở `SourceArt/Blender/Unicorn/Unicorn_MotionPrototype_v01.blend`. Kiểm tra qua MCP xác nhận 375 mesh nguồn, rig cứng 17 xương và hai action `Unicorn_Idle`, `Unicorn_HitReact`.
- FBX thử ở `Assets/Sandbox/UnicornMotion/Art` đã gom model thành 16 cụm mesh. `UnicornMotionPrototype` là scene kiểm tra Idle và Hit React. `UnicornPhoneDemo` là bản thử cũ. Scene mới sẽ là **Unicorn Demo**, dự kiến lưu tại `Assets/Sandbox/UnicornCombat/Scenes/Unicorn Demo.unity`.
- Model hiện có V-fin mở và các khe đỏ. Tên mesh cho thấy có mặt nạ Unicorn, V-fin trái/phải, cụm súng đầu và chuôi saber backpack. Chúng là điểm xuất phát để rà hình học, chưa chứng minh có đủ một Unicorn Mode khép kín.
- Chưa xác nhận có Beam Magnum hoặc khiên tách rời dùng được trong bộ asset Unicorn. Trong Task 2, kiểm tra asset và tay cầm trước khi tạo đạo cụ. Phương án cơ sở là dựng một Magnum gọn, một E-pack và cơ cấu Tonfa đơn giản. Khiên vật lý là hạng mục bổ sung theo quyết định duyệt.
- Giữ quy tắc GDD: lượt theo Speed, chọn địch hợp lệ, Burst mỗi Gundam bắt đầu 2/5, các chiêu tốn 0/1/2/5. Chiêu 1 hồi 1 khi hoàn tất, cuối lượt của chính actor hồi thêm 1. Ulti dùng trong lượt và đòi đúng 5/5. Dodge và Parry dùng chung lựa chọn phản ứng theo GDD.

## 4. Đề xuất bộ bốn chiêu

| Ô | Tên làm việc | Burst | Mục tiêu | Hệ số HP / giáp thử | Vai trò |
|---|---|---:|---|---|---|
| 1 | **Twin Vulcan** | 0 | Một địch | 0,65 / 0,45 | Tích Burst, kết liễu mục tiêu yếu |
| 2 | **Beam Tonfa Breaker** | 1 | Một địch | 1,10 / 1,35 | Hai nhát liên tiếp, ưu tiên phá Armor |
| 3 | **Beam Magnum** | 2 | Một địch | 1,85 / 1,10 | Một phát nặng để khai thác địch đã mất giáp |
| 4 | **Psycho-Frame Resonance** | 5 | Một địch | 3,20 / 2,00 | Full Burst đánh mục tiêu ưu tiên, có cảnh riêng |

Hệ số HP nhân công thức HP trong GDD. Hệ số giáp nhân `0,5 × Attack + 0,5 × Break Dmg`. Với Attack 100 và Break Dmg 60, ngân sách giáp trước phản ứng lần lượt là 36, 108, 88 và 160. Chiêu 2 chia cả HP lẫn giáp theo tỷ lệ 45% / 55%, áp quy tắc giáp ở đầu **từng hit**. Các số là điểm xuất phát cân bằng, cần đo trong Demo. Kịch bản đề xuất là phá giáp bằng Tonfa rồi tận dụng Magnum ở lượt kế, hoặc dùng chiêu cơ bản để giữ tài nguyên cho Full Burst.

Các frame sau là **mốc tương đối tính từ 0 ở 30 fps**. Khi nhập vào Blender, mốc tương đối 0 có thể tương ứng frame 1 của Action. Thời gian impact được xuất và đối chiếu với Unity.

### Skill 1 · Twin Vulcan · 0,90 giây

| Frame | Chuyển động và tín hiệu |
|---|---|
| 0–6 | Đầu khóa mục tiêu, ngực xoay nhẹ, vai giữ trọng lượng. |
| 7–15 | Hai nòng đầu luân phiên lóe vàng trắng. Đạn ngắn, tia vạch và âm giòn. Mốc damage duy nhất ở **f12 / 0,40 giây**, dù hình ảnh có nhiều phát. |
| 16–27 | Nòng tắt, vỏ đạn hoặc khói ngắn nếu còn thời gian, đầu trả về Idle. |

**Blender:** xoay đầu, độ giật nhỏ ở cổ và ngực. **Unity:** hai socket `Vulcan_L/R`, flash xen kẽ, tracer và impact. HUD sáng lại khi action kết thúc. Camera giữ góc trận. Kiểm tra đạn phát từ cụm nòng ở đầu, góc bắn theo địch được chọn và đạn trang trí không sinh thêm damage.

### Skill 2 · Beam Tonfa Breaker · 1,70 giây

| Frame | Chuyển động và tín hiệu |
|---|---|
| 0–10 | Hạ trọng tâm, nghiêng thân, mở chuôi Tonfa ở cẳng tay và bật lưỡi beam. |
| 11–21 | Lướt ngắn về điểm đứng đánh. Nhát chéo thứ nhất ở **f20 / 0,67 giây**, 45% ngân sách. |
| 22–33 | Hãm đà bằng chân trước, xoay hông, nhát ngược thứ hai ở **f32 / 1,07 giây**, 55% ngân sách. |
| 34–51 | Thu lưỡi, lùi về ô, về pose Idle. |

**Blender:** khóa bàn chân ở pha hãm, diễn tư thế tay và hông, khớp mở Tonfa. **Unity:** dịch rig hiển thị từ ô tới điểm đánh và trả về, hit spark theo hướng lưỡi, trail hồng trắng bật riêng cho từng nhát. Actor logic giữ nguyên ô. Nếu nhát đầu đã hạ mục tiêu, nhát sau có thể kết thúc động tác nhưng bỏ damage và không đổi đích. Armor Break chỉ phát khi giáp chuyển từ dương về 0.

### Skill 3 · Beam Magnum · 1,60 giây

| Frame | Chuyển động và tín hiệu |
|---|---|
| 0–15 | Nâng Magnum, tay phụ đỡ thân súng nếu rig cho phép, dồn trọng tâm về chân sau. |
| 16–20 | Giữ ngắm ngắn, tích sáng tại nòng. Phát bắn ở f20. |
| 21–28 | **Impact f22 / 0,73 giây**. Giật lùi ở vai, khuỷu và thân, thoát một E-pack khoảng f25. |
| 29–48 | Flash và beam tan, kéo lại súng, trả tư thế ổn định. |

**Blender:** Magnum cầm thật, recoil giật nhanh và hãm chậm, tách E-pack có điểm thoát. **Unity:** beam lõi sáng, vỏ tia ấm, sóng khí hướng về địch, tiếng nổ chính và tiếng kim loại rơi. Beam nối đúng nòng với điểm nhận đòn. Địch đứng gần đường tia chỉ nhận damage nếu được bộ chọn mục tiêu trả về. Camera rung nhẹ trên sân ảo. Nếu tay phụ xuyên giáp, sửa pose hoặc dùng tư thế chống giật một tay trước khi khóa clip.

### Skill 4 · Psycho-Frame Resonance · 4,60 giây

**Đề xuất A cho đợt 2–3 ngày:** Unicorn ở dáng Destroy Mode sẵn có. Full Burst mở bằng nhịp cộng hưởng, sau đó thực hiện một phát Magnum cường hóa. **Phương án B:** dựng Unicorn Mode khép kín rồi làm clip biến hình đầy đủ. Phương án B cần kiểm tra thêm mặt nạ, khe giáp, sừng, trục khớp và thời gian, rồi mới cam kết trong lịch.

| Frame | Hình ảnh | Tín hiệu Unity |
|---|---|---|
| 0–23 | Máy giữ pose, máy quay vòng nhẹ sang góc 3/4 của Unicorn. Giáp tối vừa đủ để thấy silhouette. | Khóa input, ghi lại camera/HUD, trừ 5 Burst một lần khi action hợp lệ. |
| 24–57 | Cận thân và đầu, từng vùng khung đỏ sáng theo nhịp từ ngực ra tay/chân. Vài chốt giáp chuyển động nếu đã tách ổn. | Emission theo mask vùng, nhịp servo và bass ngắn. Với phương án màu lục, chuẩn bị đỉnh chuyển màu ở đoạn tiếp theo. |
| 58–91 | Lộ toàn thân, Unicorn dựng Magnum, lõi súng tích sáng. Có thể chuyển đỏ sang lục trong chớp ngắn ở f76–91. | Hạt đi vào vùng ngực/nòng rồi thưa dần để đọc pose. Camera ưu tiên Unicorn và hành lang bắn, tránh bị model địch che. |
| 92–109 | Bắn f96. Recoil rõ từ tay đến ngực. **Damage duy nhất f100 / 3,33 giây.** | Beam, âm trầm, impact và Armor Break nếu có. Vòng sóng là VFX, không tự tạo damage diện rộng. |
| 110–138 | Khung hạ sáng, hạ súng, giữ pose kết thúc vừa đủ rồi về góc trận. | Dọn effect, trả camera/HUD/input và kết thúc action. |

**Hướng chất lượng:** giai đoạn tích sáng, phát bắn và dư chấn phải nhìn ra ba nhịp khác nhau. Chỉ dùng một đỉnh sáng ngắn. Giữ thấy V-fin, mặt và hướng nòng ở góc toàn thân. Dùng SFX riêng cho chốt giáp, cộng hưởng, phát bắn và recoil. Bản AR giữ góc camera theo thiết bị và dùng framing/VFX theo anchor. Bản sân ảo dùng camera điện ảnh đầy đủ. Full Burst là một hành động trong lượt, phần cộng hưởng kết thúc cùng cảnh.

## 5. Kế hoạch sản xuất khi Task 2 được mở

### Blender

1. Tạo bản làm việc riêng từ prototype. Kiểm tra gốc, trục, tỷ lệ, trọng số cứng và khe khớp khi bẻ vai, khuỷu, đầu gối. Dùng bản 375 mesh để làm nguồn chỉnh sửa.
2. Dựng Magnum, E-pack và cụm Tonfa tối thiểu. Chốt ổ tay cầm, hướng nòng, điểm thoát E-pack, bản lề và điểm đầu/cuối beam trail. Kiểm tra tại Idle, pose ngắm, recoil và hai pose chém.
3. Khóa pose quan trọng trước, rồi chỉnh chuyển động đệm. Với recoil, đi nhanh vào pose giật và hồi chậm. Với Tonfa, khóa chân ở pha chịu lực. Chỉ tách thêm điều khiển giáp ở những vùng nhìn rõ trong Full Burst.
4. Giữ Idle và Hit React hiện có. Chuẩn bị thêm chuyển động nền cho Demo: xuất hiện/hạ cánh khoảng 1,2 giây, Dodge khoảng 0,6 giây, Parry khoảng 0,55 giây, bị hạ khoảng 1 giây. Ưu tiên bốn action chính trước khi đánh bóng các clip nền.
5. Xuất FBX Generic và bake 30 fps, giữ tên rig và đường dẫn xương ổn định. Clip nền Idle lặp. Clip chiêu và phản ứng phát một lần. Lưu danh sách clip, dải frame, mốc damage, socket và thay đổi material cùng nguồn Blender. Giữ nhóm mesh xuất theo xương cho phần giáp cố định, tách riêng các đạo cụ và chi tiết thực sự cần chuyển động.

### Unity

1. Import FBX và kiểm tra kích thước, hướng trước, prefab, clip, material URP và avatar Generic. Gắn socket `Vulcan_L/R`, `Muzzle`, `Eject`, `Tonfa_Base/Tip`, `Impact_Chest`, `CameraFocus_Head/Body` và `Ground`. Khóa root motion lên actor logic.
2. Tạo dữ liệu kỹ năng cho bốn chiêu gồm chi phí, hệ số, kiểu mục tiêu, action clip, mốc hit, audio, VFX và camera. Rà demo Rouge để tái sử dụng phần luật phù hợp. Kiểm tra API đang có trước khi chốt phần dùng chung.
3. Luồng thực thi: **chọn địch và chiêu → xem chi phí → Execute → xác thực → trừ Burst một lần → phát animation → giải quyết hit → hồi Burst một lần → trả điều khiển/chuyển lượt**. Combat sở hữu timeline hit. Animator, VFX và audio dùng mốc đồng bộ. Dùng mã action và chỉ số hit để chặn sự kiện lặp hoặc sự kiện từ action cũ.
4. Giữ Dodge/Parry theo GDD, một lựa chọn cho một đòn. Cửa sổ bám mốc impact của địch. Bật giáp, HP và Burst trong HUD. Khi full 5 ô, có cả màu và tín hiệu rõ. Cảnh ulti tạm ẩn phần HUD cần thiết, rồi khôi phục cả khi skip hoặc reset.
5. Làm VFX bằng emission, particle, trail, beam và shader URP. Tái sử dụng effect qua pool, giới hạn lớp trong suốt và ánh sáng động. Thử trước với hình và âm thanh đơn giản, sau đó tăng độ chi tiết ở các khoảnh khắc chính. Icon bốn chiêu dùng đúng nòng đầu, Tonfa, Magnum và Psycho-Frame.
6. Dựng **Unicorn Demo** với sân ảo, Unicorn và 3 địch thử. Có một địch giáp cao, một địch giáp thấp và một địch ra đòn có cue để kiểm Parry/Dodge. Phân biệt công cụ đặt nhanh Burst/HP phục vụ QA với luồng chơi chuẩn bắt đầu 2/5. Dùng replay/reset, xoay/zoom, đổi đích và chế độ xem chậm phục vụ duyệt.
7. Kiểm tra chế độ AR riêng khi có thiết bị phù hợp. Mất tracking hoặc app tạm dừng phải giữ action và mốc hit. Khi tiếp tục, dùng thời gian đã giữ. Reset dọn actor hiển thị, VFX, camera và timer trước khi dựng lại sân.

## 6. Lịch đề xuất trong 2–3 ngày

| Mốc | Khối việc | Kết quả để xem |
|---|---|---|
| Ngày 1 · Khoảng 4 giờ | Hoàn tất nghiên cứu và duyệt lựa chọn. Sau câu mở Task 2, kiểm rig và đạo cụ, khóa pose bốn chiêu, làm một đường thử từ Magnum trong Blender sang Unity. | Bảng skill đã duyệt, các pose chính và một phát Magnum trúng đúng đích. |
| Ngày 2 · Khoảng 4 giờ | Hoàn thành animation bốn chiêu, ráp scene, HUD, Burst, hit, giáp, camera và các hiệu ứng cơ sở. | Bốn chiêu chạy được từ đầu đến cuối trong Unicorn Demo. |
| Ngày 3 · Khoảng 4 giờ nếu cần | Chỉnh Tonfa, recoil, ánh sáng, âm thanh và cảnh ulti. Kiểm thử các nhánh lỗi, ghi clip so sánh và duyệt cùng chủ project. | Demo được kiểm tra và bộ quy trình để mở Task 3 khi có xác nhận. |

Lịch này dành cho việc hoàn thành và duyệt **Unicorn**, sau đó bắt đầu áp dụng cho roster nếu đủ thời gian và đã được duyệt. Nếu chỉ có hai ngày, ưu tiên bốn chiêu chạy trọn lượt và một cảnh Full Burst hoàn chỉnh ở mức demo. Những phần giảm trước là khiên mở dạng X, chuyển màu phụ, vỏ đạn trang trí và thêm góc camera. Chờ duyệt Task 1 có thể dịch lịch Task 2. Biến hình toàn thân cần ước lượng riêng sau kiểm tra mesh.

## 7. Kịch bản test và tiêu chí nghiệm thu Unicorn Demo

| Mã | Thao tác | Kết quả cần thấy |
|---|---|---|
| U01 | Mở scene, chạy trận chuẩn và reset 3 lần | Unicorn và địch lên đúng vị trí, khởi đầu 2/5 Burst, không nhân đôi camera/effect. |
| U02 | Chọn từng địch trong ba ô, đổi mục tiêu trước Execute | HUD, hướng nòng và damage theo đúng đích đã xác nhận. |
| U03 | Dùng Vulcan hai lượt của Unicorn từ 2/5 | Burst 2→4→5, chỉ hồi trong lượt hợp lệ. Full Burst khóa ở 4/5. |
| U04 | Thử đủ và thiếu Burst với skill 2, 3 và ulti. Bấm Execute liên tục | Từ 5/5, một ulti hợp lệ trừ 5 rồi về 1 sau cuối lượt nếu trận tiếp diễn. Lệnh trùng hoặc lệnh thiếu Burst không làm đổi tài nguyên. |
| U05 | Tonfa đánh mục tiêu giáp vừa đủ vỡ ở hit 1. Lặp với mục tiêu chết ở hit 1 | Hai ngân sách 45/55 đúng, Armor Break một lần. Hit 2 xét giáp đã vỡ hoặc bỏ damage nếu đích đã chết. |
| U06 | Magnum và ulti qua các góc trước, sau và 3/4 | Súng nằm đúng tay, beam ra đúng nòng. Mỗi chiêu chỉ có một mốc damage. Không có hiệu ứng giả từ khiên hoặc sai vũ khí. |
| U07 | Ép phản ứng Dodge và Parry ở hai nhánh so Speed/Def, đúng và sai timing | Một lựa chọn mỗi đòn. Parry dùng `Def × 1,25` so với Attack. Dodge nhánh 50% là xác suất né. Dùng seed hoặc ép kết quả khi kiểm thử. |
| U08 | Xem ulti với địch lớn đứng trước camera. Skip và reset ở giữa cảnh | Thấy rõ Unicorn và hướng bắn. Khi skip, phần gameplay còn lại được giải quyết một lần. Khi reset trận, action cũ bị hủy. Camera, HUD và input được trả đúng. |
| U09 | Pause trước hit và tiếp tục. Nếu có máy AR, thử mất tracking trước hit | Timer/animation tạm dừng cùng nhau. Tiếp tục chỉ phát đúng hit còn chờ, không trừ Burst hoặc tính damage lặp. |
| U10 | Quay lại clip ở tốc độ 1× và 0,5×, kiểm ở 16:9 và màn dài | Có đủ nhịp lấy đà, impact, hồi tư thế. Trong sân ảo, mốc hiển thị sai lệch không quá một frame nguồn ở tốc độ chuẩn khi đo bằng log và capture. |
| U11 | Chạy lặp bốn chiêu, đo Profiler trên cấu hình test đã ghi | Không có lỗi Console từ Demo, số effect hoạt động trở về nền sau mỗi trận. Mục tiêu thiết bị là 30 fps ổn định, chỉ ghi pass mobile khi đo trên máy thật. |

**Bàn giao Task 2**

- [Nguồn Blender](../../SourceArt/Blender/Unicorn/Unicorn_Combat_v01.blend), [FBX](../../Assets/Game/Features/Characters/Unicorn/Art/Unicorn_Base.fbx) và [prefab chuẩn](../../Assets/Game/Features/Characters/Unicorn/Prefabs/Unicorn_Base.prefab). Súng và hai nửa khiên lấy từ mẫu RX-0 Full Armor trong project, gắn vào rig của Unicorn đã duyệt. Tay cầm Tonfa, icon, hiệu ứng và âm thanh tổng hợp được tạo cho demo.
- [Scene Unicorn Demo](../../Assets/Sandbox/UnicornCombat/Scenes/Unicorn%20Demo.unity) chạy trận 1v3 với RX-78-2, Justice và Extreme. Chọn ô địch, chọn chiêu bằng nút hoặc phím 1–4, rồi bấm **Execute** hoặc Enter. Dùng Q để Parry, E để Dodge, kéo chuột phải để xoay và cuộn để zoom. **QA 5/5** cho phép xem ulti ngay từ lượt Unicorn, Esc bỏ qua cảnh ulti.
- [Video bốn chiêu](../Reports/UnicornCombat/Unicorn_Demo.webm), [ảnh sân ảo 16:9](../Reports/UnicornCombat/Arena_16x9.png), [ảnh Full Burst](../Reports/UnicornCombat/FullBurst.png), [kết quả Play Mode](../Reports/UnicornCombat/PlayModeValidation.txt) và [Unity Profiler](../Reports/UnicornCombat/DesktopProfile.txt). Video ghi hình Game View, âm thanh phát trong scene Unity.
- Đã đo mốc hit ở 1× và 0,5× trên Unity 6000.5.8f1. HUD được xem ở 1920 × 936 và 1920 × 1080. Tracking pause được kiểm qua mô phỏng. Còn chờ kiểm thử AR và hiệu năng trên điện thoại.

**Duyệt hình ảnh cùng chủ project:** kiểm tư thế ngắm/đỡ súng, đường chém của Tonfa, màu Psycho-Frame, sức nặng của cú giật, khả năng nhìn trọn Unicorn trong ulti, độ rõ của Armor Break và nhịp quay lại trận. Cảnh cinematic là phần được đánh bóng nhiều nhất. Hiệu quả chiến đấu vẫn phải cân theo vai trò và chi phí.

## 8. Task 3 áp dụng cho roster

- Dùng cùng cấu trúc hồ sơ cho từng Gundam: nguồn vũ khí, vai trò, 4 skill, clip tái sử dụng, clip cần làm mới, socket, thời điểm damage, VFX và ca test.
- Strike Rouge dùng làm mốc so HUD/camera. Justice có sẵn clip Saber/Rifle/Shield/Boomerang/Lifter để tuyển chọn. Extreme cần xem nội dung Action trước khi đặt chiêu. Hai RX-78-2 có clip riêng Rifle và Saber, kiểm rig và tỷ lệ trước khi dùng chung bất kỳ chuyển động nào.
- BlueFlame, HaoLing, SkyDefender và Kamiki nằm ở hàng đợi cuối. Khi đến lượt, thử chuyển động ngắn cho mỗi rig rồi mới gắn toàn bộ bộ chiêu. Chuyển động đặc trưng như cánh hay phượng phải ăn khớp với pose chính của Gundam.
- Dùng tiêu chí chất lượng của Unicorn cho từng nhân vật, đồng thời giữ vũ khí và silhouette đặc trưng. Mỗi nhân vật được duyệt riêng trước khi đưa vào roster chiến đấu.

## 9. Các quyết định cần chốt trước câu mở Task 2

| Quyết định | Phương án đề xuất để duyệt |
|---|---|
| Vai trò và bốn chiêu | Đánh đơn/phá giáp. Twin Vulcan → Beam Tonfa Breaker → Beam Magnum → Psycho-Frame Resonance. |
| Hình thái và màu ulti | Giữ Destroy Mode có sẵn, đỏ là màu chính. Chọn đỏ xuyên suốt hoặc thêm nhịp lục ở đỉnh. Nếu muốn biến hình thật, chọn phương án B và chốt lại thời lượng làm. |
| Phạm vi của ulti | Một địch, tập trung dồn sát thương. Nếu muốn lan, chốt lại hệ số và silhouette VFX trước khi làm. |
| Phạm vi asset | Magnum, E-pack và Tonfa là phần chính. Duyệt có cần thêm khiên vật lý ngay hay dùng pose Parry cơ khí ở bản đầu. |
| Mốc nghiệm thu 2–3 ngày | Chốt sân ảo làm đường test chính. Xác nhận riêng thiết bị, phiên bản và tiêu chí nếu cần kiểm thử AR thật trong cùng đợt. |

Sau khi duyệt, chủ project nhắn đúng **“Bắt đầu task 2”**. Chỉ mở Task 3 khi Unicorn Demo đã qua kiểm thử và chủ project xác nhận duyệt.
