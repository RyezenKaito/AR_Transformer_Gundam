# PROMPT — CHẠY SẢI DÀI, NHỊP CHẬM, TAY ĐÚNG NHỊP; EARTH PRISON BAY CAO RỒI THI TRIỂN

Trạng thái: đã được người dùng yêu cầu thực hiện và đã triển khai; thông số thực tế xem HighCast_20261029_VI.md. Các bảng trong prompt là đề xuất ban đầu. Khi được yêu cầu thực hiện, ưu tiên nội dung này hơn các thông số chạy/bay trong bộ prompt cũ.

Bạn là technical animator và Unity gameplay engineer. Hãy chỉnh hệ chiến đấu theo lượt trong `E:/My project`, scene `Assets/TurnBasedBattle/Scenes/TurnBattleDemo.unity`. Dùng rig Gundam hiện có và bảo toàn các cơ chế chiến đấu. Mục tiêu là chuyển động tự nhiên khi xem cả chuỗi, không chỉ nội suy root mượt hoặc đạt test logic.

## 1. Kết quả người chơi phải nhìn thấy

- Đòn kiếm: Gundam lấy đà nhẹ, chạy tới địch bằng những bước dài, nhịp từ tốn và có trọng lượng. Chân đạp xuống đất để đẩy thân đi; tay vung có chủ đích và phối hợp với chân. Hãm tốc, đứng vững rồi chém; thu động tác, bước quay người, chạy về và cất kiếm.
- Súng/vũ khí tầm xa: tiếp tục đứng tại đội hình để rút, ngắm, bắn và cất. Không chuyển sang chạy hoặc bay áp sát.
- R — Earth Prison: Gundam bật booster, bay tới vị trí thi triển ở trên cao phía ngoài vùng địch, giảm tốc và ổn định tư thế bay. Chỉ sau khi đã lên đủ cao và ổn định mới tụ lực, phóng chiêu xuống vùng địch. Giữ trên không suốt pha thi triển; sau đó thu chiêu, bay về và hạ cánh.
- Idle và né vẫn ở vị trí đội hình. Camera mặc định phía sau nhưng xoay 360°, zoom và thanh máu trên đầu địch phải tiếp tục hoạt động.

## 2. Chẩn đoán trước khi chỉnh

Đọc `BattleActor`, `BattleLocomotionPose`, `BattleClipBuilder`, `BattleWeaponRig`, `BattleActionDefinition`, `BattleOrbitCamera` và các config hiện hành. Xem lại clip chạy hiện có ở tốc độ 1× và 0.5× từ góc bên.

Ghi rõ nguyên nhân bước chạy thô: tần số bước quá cao, stride không khớp tốc độ, chân duỗi tới giới hạn IK, pelvis thiếu dịch trọng lượng, pose chân bị bật khi blend hoặc tay bị nhiều hệ cùng ghi đè. Đo theo kích thước chân của rig, không chỉ tăng `runStride` cho đến khi trông như bước dài.

Không đặt mục tiêu bằng cách giảm PlaybackSpeed toàn trận: nhịp chạy chậm phải nằm trong dữ liệu di chuyển/animation của từng action. Hit reaction, projectile, né và camera giữ tốc độ phù hợp riêng.

## 3. Chạy bước dài và từ tốn

### Nhịp và tốc độ

- Tách rõ `stepLength` (một lần chuyển chân), `strideLength` (hai bước, cùng chân chạm lại), cadence và tốc độ tiến. Dùng cùng đơn vị bàn đấu, có xét scale.
- Quan hệ cần giữ: vận tốc tiến xấp xỉ chiều dài bước × số bước mỗi giây. Animation chân phải đi theo quãng đường đã chạy; không chạy chu kỳ nhanh cố định trong khi root đi chậm.
- Dải thử ban đầu: mỗi bước khoảng 0.7–0.9 chiều dài toàn chân; chu kỳ hai bước khoảng 0.9–1.2 giây. Đây là thông số tinh chỉnh theo rig, không phải tiêu chí cứng. Nếu muốn bước dài hơn, phải kiểm tra độ với của chân và dịch chuyển hông trước.
- Ưu tiên cadence chậm và bước rõ ràng. Tốc độ tiến giảm so với bản đang chạy; cho phép action dài hơn. Không ép nhân vật vượt khoảng cách lớn trong thời lượng cũ bằng cách tăng tốc đột ngột.
- Khởi động khoảng 0.25–0.4 giây, tăng tốc dần; giữa quãng đường giữ nhịp; bước cuối ngắn lại để hãm khoảng 0.3–0.5 giây. Chuyển tốc độ có vận tốc và gia tốc liên tục.

### Một bước phải có đầy đủ pha

1. Tiếp đất: bàn chân tới trước thân trong phạm vi giải phẫu của rig; gối không khóa thẳng, không giật hoặc đổi chiều IK.
2. Nhận trọng lượng: gối và hông hạ nhẹ, thân chuyển lên chân trụ. Không cho cả hai chân cùng trượt như đứng trên băng.
3. Đẩy đi: chân trụ duỗi vừa đủ, thân tiến qua phía trên bàn chân; chân sau rời đất có chủ đích.
4. Vung chân: gối gập tạo khoảng hở cho bàn chân, đưa chân qua rồi duỗi dần để đón bước tiếp theo. Không kéo một bàn chân nằm ngang xuyên sàn.

Chân trụ giữ vị trí ổn định tương đối với bàn đấu trong pha tiếp xúc. Chân đang vung có quỹ đạo riêng. IK dùng độ dài xương thật, giới hạn gối và pole vector ổn định. Khi mục tiêu vượt khả năng với, điều chỉnh vị trí hông/chiều dài bước; không kéo dài xương hoặc giật gối.

Root chạy giữ trên mặt bàn. Dao động nhỏ của pelvis để nhận trọng lượng là hợp lệ; không biến cả nhân vật thành chuỗi bật nhảy. Nếu model không có khớp ngón chân, mô phỏng lăn bàn chân nhẹ ở ankle, tránh xoay gót quá mức làm xuyên giáp.

### Thân và tay vung chuẩn

- Chân trái tiến thì tay phải tiến; chân phải tiến thì tay trái tiến. Hai tay lệch pha 180°, không vung cùng hướng và không đánh tay nhanh gấp nhịp chân.
- Vai dẫn động, khuỷu gập tự nhiên, cổ tay tương đối ổn định. Dải thử: vai trước/sau khoảng 15–25°, khuỷu khoảng 65–90°, tinh chỉnh để không xuyên giáp hoặc ngực. Không áp cứng cùng biên độ cho mọi khớp.
- Ngực xoay nhẹ ngược pelvis để cân bằng, thân nghiêng tới trước vừa đủ; đầu ổn định nhìn hướng chạy. Tránh lắc đầu, lắc ngực quá lớn hoặc giữ thân cứng trong khi chỉ đạp chân.
- Khi đã rút chuôi kiếm: tay cầm kiếm dùng dáng chạy riêng, vung nhỏ hơn tay trống và giữ chuôi hướng an toàn; không vung kiếm xuyên đùi/giáp. Lưỡi sáng chỉ bật khi chuẩn bị đánh, không chạy qua người bằng lưỡi đang chĩa ngang.
- Một hệ chịu trách nhiệm cuối cùng cho mỗi khớp trong mỗi pha. Quy định thứ tự action pose → locomotion blend → IK → attachment; không để arm swing, animation clip và weapon IK giành nhau gây rung.

## 4. Nối chạy, đánh và trở về

- Chạy → hãm → đặt chân trụ → windup → chém → follow-through. Không bật tức thì từ chân đang bước sang pose chém hoặc trượt thêm trong lúc kiếm đã chạm.
- Căn thời điểm dừng với phase chân phù hợp. Nếu đoạn đường không đủ một stride, điều chỉnh bước cuối; không reset cycle để ép khớp.
- Quay về bằng bước xoay/đổi chân, trọng lượng chuyển giữa hai chân. Không quay root 180° trong khi bàn chân vẫn khóa tại chỗ.
- Bắt đầu chạy về sau khi đã hướng đủ về đường về; không chạy lùi trong tư thế chạy tới. Đến đội hình thì hãm, đặt hai chân, cất vũ khí và blend về idle.
- Chỉ hoàn tất action và mở lượt địch sau khi return, sheathe và settle hoàn tất. Marker damage, cue né, cost và cooldown phải theo action timeline mới; không giữ timestamp cũ khi đã kéo dài phần chạy.
- Cùng một lần hit chỉ gây damage một lần. Pause/resume, đổi tốc độ thử nghiệm, reset và death không để lại bước chạy, weapon hoặc callback cũ.

## 5. R bay cao rồi mới dùng chiêu

### Điểm thi triển trên không

- Lấy chiều cao Gundam H từ kích thước rig đã chuẩn hóa, không lấy bounds đang bị ảnh hưởng bởi kiếm/booster đang bật.
- Đề xuất ban đầu: khoảng hở từ lòng bàn chân tới mặt bàn tại điểm thi triển bằng khoảng 1.0–1.3 × H. Ví dụ H ≈ 2.6 đơn vị thì khoảng hở khoảng 2.6–3.4 đơn vị bàn đấu. Hiển thị rõ để có thể tinh chỉnh trong Inspector.
- Phân biệt độ cao root/pivot với khoảng hở bàn chân: tính mục tiêu root từ vị trí bàn chân khi hover, không gán thẳng khoảng hở vào root.y nếu pivot khác.
- Vị trí ngang của caster vẫn nằm ngoài vòng trụ: khoảng cách ngang từ caster tới tâm vòng ≥ bán kính vòng + bán kính thân + margin. Bay cao không cho phép đặt caster thẳng trong vùng trụ/lồng.
- Tọa độ theo `BattleContent`/arena, xét scale đồng đều. Đổi vị trí hoặc xoay bàn không làm đường bay và VFX lệch nhau.

### Timeline đề xuất để tinh chỉnh

| ActionTime | Pha |
|---|---|
| 0–0.45s | Chuẩn bị, hạ trọng tâm nhẹ, booster tăng lực |
| 0.45–2.20s | Bay lên cao và tiến tới điểm thi triển, tăng tốc rồi hãm êm |
| 2.20–2.55s | Ổn định trên không, chuyển sang pose tụ lực |
| 2.55–10.55s | Earth Prison đủ 8s; caster giữ độ cao |
| 10.55–12.50s | Thu chiêu, bay về, hạ dần về đội hình |
| 12.50–13.10s | Chạm đất, gối/hông hấp thụ lực, booster tắt dần, về idle |

Đây là mốc khởi đầu để review, không bắt buộc ép thời lượng nếu chuyển động chưa tự nhiên. Điều kiện bắt đầu charge là đã đến vị trí và ổn định; hết một timer trong khi vẫn đang bay chưa đủ để coi là đạt.

Không còn nhánh hạ về mặt đất tại `arriveTime` rồi mới thi triển. Trong toàn bộ 8 giây VFX, caster phải giữ vị trí trên không, có tư thế hover: chân hơi thu/gập, thân cân bằng, tay điều khiển năng lượng, booster mức thấp. Dao động hover rất nhẹ, khoảng 1–2% H, chu kỳ khoảng 2–3s; không nhấp nhô liên tục như nhảy.

### Phóng chiêu từ trên cao xuống

- Cầu điện bám CastSocket ở bàn tay trong pose trên không. Sau charge, tia đi từ bàn tay tới mặt đất ở tâm vùng địch; có góc hướng xuống rõ ràng.
- Chỉ orb/tia đầu vào theo caster. Vòm, trụ, bụi, rubble và lồng sét vẫn đặt trên mặt bàn quanh địch, không bị nâng lên theo caster.
- Giữ hình ảnh nhận diện và nhịp 8s của Earth Prison. Không thêm vụ nổ cuối hoặc thay cơ chế damage để che vấn đề chuyển động.
- Quy đổi `VfxTime = ActionTime - VfxStart`. Nếu dùng VfxStart = 2.55 thì damage tại **5.90 / 7.65 / 8.05 / 8.45 / 8.85s**, tương ứng **120 / 60 / 60 / 60 / 60**. Nếu thay VfxStart, tự dịch tất cả marker liên quan từ nguồn dữ liệu chung.
- Ultimate không tự cộng thanh; không tự thêm stun. Địch chỉ bắt đầu lượt sau khi caster đã trở về/hạ cánh/cleanup xong.

## 6. Camera khi R bay cao

- Giữ quyền xoay yaw 360° và zoom của người dùng. Không cắt camera sang cinematic cố định, không tự đưa yaw về sau lưng.
- Pivot/follow blend phải xét cả caster trên cao và vùng mục tiêu dưới đất. Cho phép nới khoảng cách và điều chỉnh focus/pitch nhẹ nếu cần để thấy bàn tay, đường tia và vùng lồng trong góc đang hướng về trận đấu.
- Không cộng độ cao caster vào pivot rồi làm toàn bộ địch biến mất dưới khung hình. Đồng thời không kéo camera ra quá xa thành nhìn toàn bản đồ.
- Nếu người dùng chủ động xoay ra khỏi vùng địch thì tôn trọng góc đó, không cưỡng ép tự xoay lại để thấy tất cả.
- Khi hạ xuống, camera trở về offset bình thường bằng blend, giữ yaw/zoom người dùng. AR camera do tracking quản lý vẫn không bị thay pose; chỉ dịch nội dung bàn đấu/các đối tượng theo quyền sở hữu đã thiết kế.

## 7. Nghiệm thu trước khi báo hoàn thành

### Quan sát chuyển động

- Quay góc bên và 3/4 ở tốc độ 1×: thấy rõ bước dài, nhịp chậm, chân nhận trọng lượng; không bước li ti, không nhảy, không trượt thân trên đôi chân đạp vô nghĩa.
- Xem 0.5× phần tiếp đất, chân trụ, đổi chân, hãm và quay: gối không bật, xương không giãn, chân không xuyên sàn, không snap vào pose đánh.
- Tay/chân đúng pha, khuỷu/cổ tay không rung, chuôi không xuyên giáp. Xem cả hai bên vì một góc có thể che lỗi.
- R phải cho thấy rõ đủ bốn trạng thái: bay lên cao → đứng trên không tụ/thi triển → bay về → hạ cánh. Chụp charge và cage để chứng minh caster vẫn ở trên cao trong lúc chiêu chạy.

### Logic và trường hợp biên

- 1v1/1v3, mục tiêu ở bên trái/phải và khoảng cách khác nhau: không tăng cadence đột ngột, không chạy xuyên đối tượng hoặc dừng trong mục tiêu.
- 30/60/120Hz mô phỏng và bước delta lớn: vị trí/chân/marker nhất quán; phân biệt bài logic với FPS thiết bị thật.
- Ultimate giữ đủ 5 hit, tối đa 360 mỗi mục tiêu sống đủ chuỗi; không damage sớm khi caster còn bay lên. Cost trừ một lần; callback cũ không tồn tại sau restart/death.
- Pause khi chân đang trụ, lúc cất cánh, lúc hover, impact, bay về, hạ cánh; resume không bật pose.
- Orbit/zoom trong mọi pha, đổi scale bàn, giao camera ngoài; thanh máu bám đầu địch. Không tuyên bố AR hoàn chỉnh khi chưa kiểm tra thiết bị.

## 8. Bàn giao

Lưu scene/prefab/config và cập nhật prompt. Nêu rõ các thông số mới: chiều dài bước, cadence, tốc độ chạy, thời gian hãm, biên độ vai/khuỷu, độ cao bàn chân khi R, VfxStart và thời lượng action. Giao clip so sánh trước/sau cùng góc camera, đủ toàn bộ chạy đi–đánh–chạy về và bay lên–thi triển–hạ xuống.

Không kết luận “siêu mượt” chỉ vì có SmoothDamp/Slerp hoặc test đã qua. Báo rõ phần đã kiểm chứng bằng logic, phần đã xem bằng clip và điểm còn cần tinh chỉnh mỹ thuật trên rig hiện có.
