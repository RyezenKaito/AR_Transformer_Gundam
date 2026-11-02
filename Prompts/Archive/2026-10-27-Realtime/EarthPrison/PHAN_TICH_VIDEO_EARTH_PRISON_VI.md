# Phân tích video — Earth Prison / Lồng đá dẫn sét

Ngày phân tích: 27/10/2026. Đây là phân tích hình ảnh từ tệp video người dùng cung cấp và đề xuất tái dựng; chưa triển khai skill mới trong Unity.

## 1. Nguồn và cách đọc kết quả

- Video: `D:/Downloads/YTSave.to_YouTube_Media_6vd41UGQT74_Earth-Prison-Ultimate-Skill-Unity-6-5-Particle-System_002_720p.mp4`.
- Thời lượng đọc được từ bộ giải mã: **35,87483 giây**; kích thước **1280 × 720**.
- Đã trích 36 ảnh tổng quan cách nhau 1 giây và 37 ảnh chi tiết cách nhau 0,25 giây trong đoạn 0–9 giây. Đã xem các bảng ảnh và phóng lớn những pha chính.
- Video trình diễn cùng một cấu trúc chiêu từ nhiều góc: góc cao chéo, phía sau người thi triển, phía nhóm mục tiêu và nhìn thẳng từ trên xuống. Không coi cả 35,9 giây là thời lượng một lần dùng chiêu.
- Các mốc bên dưới là **timestamp của lượt trình diễn đầu tiên**, có độ bất định khoảng 0,25 giây theo bước lấy mẫu. Tại 0 giây đã có hạt điện, nên không thể suy ra chính xác thời điểm bấm phím.
- **Quan sát** nghĩa là thấy trên ảnh. **Đề xuất** nghĩa là cách dựng lại trong Unity. Không có Inspector, shader hoặc mã nguồn của tác giả để xác nhận kỹ thuật gốc.
- Phần âm thanh chưa được nghe/đối chiếu; mọi thiết kế SFX trong prompt là đề xuất mới.

## 2. Bản chất của skill

Đây là một chiêu tạo vùng giam bằng **sáu cột đá dẫn điện**, kết hợp nguyên tố đất với sét vàng. Điểm nhận diện nằm ở trình tự:

**Điện tích quanh người thi triển → cầu điện cô đặc → tia truyền sát mặt đất → xung va chạm tại nhóm mục tiêu → vòm xoáy vàng mở rộng → sáu trụ đá trồi lên → đỉnh trụ phát sáng và nối sét → đá, điện và bụi tan dần.**

Lồng hoàn chỉnh không phải một quả cầu kín. Sáu đỉnh sáng tạo một vành đa giác; các tia sét phụ chạy dọc trụ và chớp ngang bên trong. Khoảng trống giữa các tia vẫn cho thấy đối tượng bị bao vây.

## 3. Timeline quan sát

| Mốc trong video | Điều nhìn thấy | Vai trò thị giác |
| --- | --- | --- |
| 0,00–1,00 s | Các tia vàng có nhánh và hạt nhỏ lóe quanh người thi triển; một số tia kéo dài ra ngoài | Báo trước, thu hút mắt về người dùng chiêu |
| 1,25–2,50 s | Một quả cầu điện nhỏ, sáng, có các đường vòng quấn quanh, ở gần tay/phía trên thân | Thu năng lượng về một điểm; tạo khoảng giữ trước khi phóng |
| 2,75–3,25 s | Tia sét vàng kéo từ người thi triển về cụm ba mục tiêu, chạy thấp gần mặt sàn; kèm bụi/vệt sẫm | Nối rõ nguồn phát với vị trí tác động |
| Khoảng 3,50 s | Ánh sáng vàng bùng lên từ mặt đất tại tâm mục tiêu, có tia hướng lên và bụi xám | Xác nhận điểm đến của chiêu |
| 3,50–4,00 s | Các dải vàng rách, xoáy thành vòng/vòm thấp, mở rộng bao quanh nhóm mục tiêu | Công bố phạm vi; chuyển từ đòn định hướng sang hiệu ứng diện rộng |
| 4,25–4,50 s | Vòng vàng yếu dần; sáu cụm đất đá và trụ xám xuất hiện quanh chu vi; bụi vàng cam ở chân | Chuyển từ năng lượng mềm sang cấu trúc vật chất cứng |
| Khoảng 4,75 s | Sáu đỉnh trụ bật sáng; sét chạy dọc thân trụ | Trụ được nạp điện, chuẩn bị liên kết |
| 5,00–6,25 s | Sét nối các đỉnh thành vành lục giác; các nhánh bên trong đổi hình liên tục | Pha lồng điện duy trì, sáng nhất ở đỉnh và các cạnh |
| 6,50–6,75 s | Điện và bề mặt trụ vỡ vụn/tan mất dần; còn vệt vàng trên thân, bụi xám và bệ đá | Kết thúc có thứ tự, không tắt toàn bộ đồng thời |
| 7,00–7,50 s | Chủ yếu còn mảnh, hạt điện và dư ảnh quanh mục tiêu; các mục tiêu cũng dần khó thấy/biến mất | Đuôi hiệu ứng; chưa đủ dữ kiện để khẳng định cơ chế chết hoặc reset |
| Khoảng 8,00 s | Vùng hiệu ứng gần như trống | Hoàn tất một lượt trình diễn |
| 8,75–9,00 s | Hạt xuất hiện lại tại các vị trí mục tiêu | Có thể là reset vòng trình diễn; không đưa thành một pha bắt buộc của skill |

Ảnh đối chiếu: `References/detail_sheet_01.jpg`, `References/detail_sheet_02.jpg`. Góc nhìn trên tại **33,00 s** xác nhận rõ **sáu nút sáng** quanh chu vi.

## 4. Tách lớp hiệu ứng

### 4.1. Tụ lực ở người thi triển

Có ít nhất ba thành phần nhìn thấy: lõi sáng, đường điện dài có nhánh, hạt nhỏ xung quanh. Sau đoạn điện tỏa rộng, ánh sáng thu thành cầu nhỏ. Nhịp “rộng → gom lại → phóng đi” quan trọng hơn việc chỉ tăng emission.

Đề xuất: 1 lõi billboard, 2–3 dải vòng không đồng phẳng, 4–8 nhánh điện có đường đi kiểm soát và 1 Particle System hạt phụ. Đặt dưới socket tay riêng. Không suy đoán tay trái/phải hoặc tên bone chỉ từ video.

### 4.2. Tia truyền tới mục tiêu

Đây là tia sét vàng trắng, đường đi gấp khúc, bề dày thay đổi, có nhánh và glow; không phải laser thẳng màu vàng. Chạy thấp gần đất, có bụi ở dọc đường và điểm phát.

Đề xuất: một đường sét chính có đầu mút xác định, một lớp glow rộng hơn, các nhánh ngắn, cộng Particle System bụi. Có thể dựng bằng ribbon/LineRenderer và texture; chưa có bằng chứng tác giả dùng đúng kỹ thuật này. Dù tiêu đề có “Particle System”, không có lý do ép toàn bộ hình học sét vào các hạt ngẫu nhiên.

### 4.3. Vòm xoáy báo phạm vi

Ở 3,75 s thấy rõ các dải vàng dài, đứt/rỗng, cuốn theo bề mặt một vòm thấp. Tâm khá trong để còn nhìn thấy ba mục tiêu; đường viền đậm hơn phần giữa. Không thấy chữ rune hoặc pháp trận nhiều ký tự cần sao chép.

Đề xuất: 2–3 mesh ribbon tròn hoặc vòm hở với UV cuộn, noise mask và dissolve; một vòng nền mỏng nằm sát đất. Tạo hình khối trước rồi mới tăng bloom. Hạt billboard độc lập khó giữ được các dải xoáy dài có cấu trúc như ảnh.

### 4.4. Sáu trụ đá

Góc nhìn 24,00 s và 33,00 s cho thấy sáu vị trí chia đều quanh tâm. Thân trụ xám sẫm, tiết diện không đều, đầu nhọn/khấp khểnh, chân có một bệ đá vỡ riêng. Bụi vàng cam tụ ở chân, các mảnh nhỏ màu đen bay lên. Bệ đá không phải một chiếc đĩa phẳng màu đen.

Đề xuất: mesh đá thực cho từng trụ, 2–3 biến thể hình học để lặp có kiểm soát; chuyển vị từ dưới đất lên bằng đường cong. Bụi và mảnh phụ dùng Particle System. Trụ chính có timeline xác định, không thả sáu rigidbody ngẫu nhiên rồi kỳ vọng chúng đứng đúng vị trí.

### 4.5. Sáu nút điện và lồng sét

Đỉnh mỗi trụ có điểm vàng trắng rất sáng và một vầng glow. Có sét dọc trụ. Khi hoàn thiện, đường sét ở phía trên nối các đỉnh, trong khi các nhánh phụ chớp giữa những phần bên trong. Góc trên cho thấy vành lục giác cùng các liên kết chéo thay đổi; không đủ dữ kiện để gán một đồ thị nhánh phụ cố định là đồ thị gốc.

Đề xuất: 6 liên kết chu vi luôn đọc được, 2–3 liên kết chéo luân phiên và sét dọc 6 trụ. Bố cục lớn ổn định; nhiễu nhỏ thay đổi theo thời gian. Giữ vị trí đầu mút, tránh toàn bộ đường sét đổi ngẫu nhiên mỗi frame gây cảm giác dây rung hoặc nhiễu trắng.

### 4.6. Tan rã

Ở 6,75 s còn thấy bệ đá, bụi và vệt điện sau khi phần lớn thân trụ đã biến mất. Đây là đuôi có phân lớp. Video không cho biết chính xác tác giả dùng dissolve, thay mesh, scale hay phương án khác.

Đề xuất tái dựng: hạ emission các liên kết → dissolve thân đá với mép vàng mảnh → để bệ/mảnh/bụi lưu thêm → thu hồi từng thành phần. Không thêm một vụ nổ cầu khổng lồ cuối chiêu vì không thấy rõ yếu tố đó trong video.

## 5. Màu sắc, độ sáng và bố cục

Màu tham khảo định tính: lõi điện vàng trắng; sét chính vàng chanh; glow vàng ấm; bụi chân trụ vàng cam/nâu; thân đá xám than; bụi tan xám. Mã màu và HDR trong prompt là giá trị đề xuất, không phải giá trị đã đo từ tài nguyên gốc.

Ba bậc độ sáng cần phân biệt: **lõi/nút điện sáng nhất → các nhánh điện và vòng xoáy → đá, đất và bụi tối hơn**. Nếu mọi lớp cùng trắng cháy thì không còn đọc được sáu trụ.

Nền ô caro, bầu trời tím, thanh công cụ Unity và taskbar thuộc bối cảnh quay. Những ô đỏ dưới mục tiêu đã có từ trước khi chiêu tới, nên không coi chúng là vòng cảnh báo của chiêu.

Chuyển góc trong video là các lần trình diễn/đổi góc quan sát. Chưa có bằng chứng cho một camera cinematic tự động đi qua cả bốn góc trong mỗi lần cast. Khi tái dựng nên dùng camera cố định trước để đối chiếu.

## 6. Animation người thi triển

Có chuyển từ tư thế tụ lực sang giữ cầu và động tác phát chiêu; sau khi năng lượng tới vùng địch, người thi triển đứng ngoài lồng. Không thấy người thi triển lao vào hoặc chém kiếm trong chuỗi này.

Nếu chuyển sang Gundam: giữ chân trụ chắc, hướng đầu/ngực về mục tiêu, đưa một tay lên để tụ cầu, rồi hạ/đẩy tay để phát tia. Đây là bản chuyển thể cho rig robot; không khẳng định video dùng đúng góc khớp hoặc nhịp pose của Gundam.

## 7. Điều chưa thể kết luận từ video

- Damage, cooldown, tiêu hao năng lượng, thời gian stun, va chạm của trụ và việc có chặn chạy thoát hay không.
- Các mục tiêu biến mất do chết, dissolve, object reset hay một đoạn dàn dựng khác.
- Số lượng particle thật, thông số noise, shader, texture, kích thước thế giới, post-processing, ánh sáng và âm thanh gốc.
- Hiệu năng trên máy người dùng. File quay 720p không chứng minh hiệu năng runtime ở 1080p.

Prompt vì thế có **chế độ tái dựng hình ảnh** làm mặc định và **lớp gameplay tùy chọn** với thông số được ghi rõ là đề xuất.

## 8. Áp dụng cho project hiện tại

Đã đọc cấu hình cục bộ: Unity **6000.5.8f1**, URP **17.5.0**, Input System **1.20.0**, Timeline **1.8.12**. Manifest hiện không liệt kê VFX Graph hoặc Cinemachine.

Project đã có `MechaPlayer`, `MechaEffects`, `MechaHealth`, `MechaTuning` trong `Assets/MechaCombatDemo/Scripts/Runtime/`. Q/E/R đã có chức năng; R đang là Celestial Breaker. Prompt yêu cầu dựng Earth Prison trong scene/prefab riêng trước, rồi dùng một action riêng nếu tích hợp vào Gundam.

Không chỉ thêm một cấu hình vào `MechaTuning`: `Get(string action)` hiện dùng switch cố định và `MechaPlayer` có nhiều nhánh riêng theo tên hành động. Tích hợp phải đọc các đoạn xử lý đó và nối đúng animation/input/energy/cleanup.

## 9. Tài liệu kỹ thuật đối chiếu

Các nguồn dưới xác nhận khả năng công cụ, không chứng minh cách tác giả làm hiệu ứng trong video:

- [Unity 6.5 — Particle System modules](https://docs.unity.com/en-us/engine/6000.5/manual/visual-effects/particle-systems/particle-system-modules): cấu hình emission, shape, noise, kích thước, màu và renderer.
- [Unity 6.5 — Particle rendering and shading](https://docs.unity.com/en-us/engine/6000.5/manual/particle-rendering-shading): billboard và mesh có mục đích hình ảnh khác nhau; đá có thể dùng mesh.
- [Unity 6.5 — URP Particles Unlit](https://docs.unity3d.com/6000.5/Documentation/Manual/urp/particles-unlit-shader.html): lựa chọn vật liệu hạt tương thích URP, blending và soft particles.

## 10. Tệp sử dụng

- `EARTH_PRISON_MASTER_PROMPT_VI.md`: prompt triển khai đầy đủ, độc lập, có thể giao cho chat làm Unity.
- `EARTH_PRISON_MASTER_PROMPT_VI.txt`: cùng nội dung ở định dạng văn bản để sao chép.
- `References/KEYFRAMES.jpg`: tám ảnh chính có timestamp và nhãn pha.
- `References/overview_sheet_01.jpg`, `overview_sheet_02.jpg`: toàn bộ video theo bước 1 giây.
- `References/detail_sheet_01.jpg`, `detail_sheet_02.jpg`: lượt đầu theo bước 0,25 giây.

Đọc ảnh cùng prompt sẽ giúp người triển khai giữ đúng kiểu vòm xoáy, số trụ và độ rỗng của lồng điện, thay vì tự tưởng tượng lại từ tên chiêu.
