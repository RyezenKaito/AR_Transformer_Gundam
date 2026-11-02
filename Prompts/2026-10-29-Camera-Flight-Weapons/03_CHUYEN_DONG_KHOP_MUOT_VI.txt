> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# PROMPT 03 — CHUYỂN ĐỘNG KHỚP MƯỢT, CÓ TRỌNG LƯỢNG VÀ PHỐI HỢP TOÀN THÂN

Hãy chỉnh chuyển động Gundam hiện có để đạt cảm giác cơ khí linh hoạt như video tham chiếu. Giữ model Unicorn và luật theo lượt. Khi chờ giữ đội hình; khi đánh/kỹ năng/ultimate có bay áp sát theo prompt 02. Camera orbit tự do theo prompt 01, vì vậy animation phải hợp lý khi xem từ mọi phía.

## 1. Kiểm tra rig trước khi thêm animation

Kiểm tra skeleton, pivot vai/khuỷu/cổ tay, hông/gối/cổ chân, cổ, ngực, eo và các tấm giáp. Xác định model là skinned mesh hay các mảnh rigid mesh gắn vào joint; kiểm tra trục local và rest pose bằng thao tác từng khớp. Không tự ép Generic/rigid mecha thành Humanoid nếu mapping không phù hợp.

Tái sử dụng `MechaRigData` và clip có ích. Sau tối ưu mesh theo khớp, kiểm tra lại binding: tay/chân/giáp không mất liên kết, không có mesh đứng ở rest pose trong khi bone di chuyển. Đảm bảo một bộ điều khiển có quyền ghi pose; tránh Animator và `SampleAnimation`/script cùng ghi joint trong một frame.

Không kéo dài xương để giả vờ chạm mục tiêu, không scale tay/chân theo mỗi đòn. Nếu điểm nắm hoặc tầm chém chưa tới, sửa stance, quỹ đạo, root placement hoặc chiều dài vũ khí hợp lý.

## 2. Các trạng thái bắt buộc

Tạo hoặc sửa các trạng thái: `IdleUnarmed`, `Anticipate`, `ReachForWeapon`, `Draw`, `Takeoff`, `BoostTravel`, `Brake`, `Aim/WindUp`, `Strike/Cast`, `FollowThrough`, `Recover`, `ReturnFlight`, `Landing`, `Sheathe`, `Dodge`, `HitReaction`, `Death`.

Có thể dùng clip đầy đủ hoặc layer/Playable phù hợp; đây là các pha cần nhìn thấy, không bắt buộc số lượng file clip. Chuyển pha phải theo timeline của action và khả năng rig. Giữ input chọn hành động, chạy quỹ đạo và animation tách trách nhiệm.

## 3. Cách từng phần cơ thể tham gia

- **Idle:** tay ở tư thế tự nhiên, không giữ súng chĩa sẵn. Ngực/đầu/giáp thay đổi rất nhẹ theo chu kỳ; trọng tâm ổn định, chân không trượt. Không thêm bob toàn root khi đang đứng đất.
- **Chuẩn bị bay:** hạ trọng tâm qua hông/gối, thân nghiêng, tay thu vừa phải; sau đó nhả lực và booster sáng. Có anticipation trước tăng tốc.
- **Đang bay:** hông và thân tạo hướng lực, chân co hoặc kéo sau hợp lý, đầu hướng mục tiêu trong giới hạn. Tay/cổ tay giữ vũ khí chắc; các tấm giáp theo khớp có độ trễ nhỏ, không rung vô cớ.
- **Hãm:** thân chuyển nghiêng, chân mở và hạ dần, tay vào tư thế chuẩn bị đánh. Không đổi từ tư thế bay sang đứng thẳng trong một frame.
- **Rút vũ khí:** vai, cánh tay, khuỷu và cổ tay cùng đi tới nơi cất. Ngực/eo xoay bù; tay còn lại giữ thăng bằng hoặc đỡ vũ khí. Bàn tay thật sự tới chuôi trước khi chuyển socket.
- **Chém/đấm:** lực bắt đầu từ hông/eo, truyền qua ngực/vai rồi khuỷu/cổ tay. Có wind-up, tăng tốc trước contact, follow-through và recovery. Không chỉ vung cánh tay độc lập trong khi thân/chân giữ nguyên.
- **Bắn:** vai/khuỷu vào aim pose, hai tay nắm đúng nếu là vũ khí hai tay, recoil ngắn truyền vào vai/ngực và hồi lại có kiểm soát.
- **Tụ lực ultimate:** thân giữ thăng bằng, đầu theo điểm tụ, tay tạo pose điều khiển năng lượng. Trong thời gian giữ chiêu vẫn có chuyển động rất nhỏ, không đóng băng một frame pose suốt nhiều giây.
- **Hạ xuống/cất:** chân nhận lực, hông/gối hấp thụ nhẹ, thân hồi lại; tay cất vũ khí có đường đi thật rồi về idle tay không.

Biên độ phải phù hợp giáp cứng. Không uốn thân mềm như cao su, không để giáp vai chui sâu vào đầu/ngực hoặc giáp váy cắt xuyên đùi rõ rệt.

## 4. Blend và điều khiển tiếp xúc

Chuyển clip dùng crossfade từ pose hiện tại, không reset rest pose trước mỗi action. Điểm bắt đầu tham khảo: blend idle/prepare/recover 0,12–0,25 s; phản ứng ngắn 0,05–0,12 s. Điều chỉnh theo thời gian thực, không theo số frame.

Blend quaternion theo đường ngắn và liên tục; xử lý đường nối vòng quay. Không nội suy Euler gây đảo góc. Pose khóa cuối của clip giữ chiêu phải nối được với clip tiếp theo. Loop idle không giật ở điểm cuối–đầu.

Nếu dùng layer: lower body chịu trách nhiệm bay/đứng/hạ; upper body chịu trách nhiệm rút/aim/chém; giới hạn mask rõ để layer vai/eo không phá tư thế cân bằng. Nếu dùng clip full-body: tạo các chuyển tiếp tương đương, không bắt buộc thêm layer chỉ để tăng độ phức tạp.

Foot IK chỉ hoạt động khi đang tiếp đất/đứng, weight giảm khi cất cánh và tăng lại khi hạ. Không ghim chân vào sàn trong lúc root bay. Khi đang đứng, thay đổi pelvis phải bù ở chân để giữ tiếp xúc, không làm chân trượt qua sàn.

Hand IK hoặc offset pose giúp nắm đúng chuôi/grip; blend IK vào sau khi tay tới vũ khí. Với hai tay, tay phụ theo `OffHandGrip` có weight mượt. Nhả IK trước khi cất/đổi kiểu cầm; không để constraint kéo tay về vũ khí đã ẩn.

HitReaction và Dodge phải blend từ tư thế đang có, rồi về đúng pose nền như Guard/Idle, không luôn giật về một frame Ready mặc định. Chết phải chấm dứt action và reaction còn treo theo luật trận.

## 5. Đồng bộ hình và gameplay

Contact marker phải đúng lúc vũ khí/đòn đánh tới mục tiêu. Recoil/reaction, hit spark, âm thanh và HP cùng xuất phát từ hit đã xác thực. Bị né thì không hiện blood/hit reaction/damage số như trúng; chỉ có vệt hụt và phản hồi né phù hợp.

Không kéo dài toàn clip chỉ để “mượt”: giữ nhịp ra đòn rõ, nhanh ở pha strike và có đủ thời gian chuẩn bị/phục hồi. Nếu dùng hit-stop rất ngắn, tạm dừng nhất quán timeline/animation/motion liên quan và không làm sai cửa sổ né. Mặc định không thêm rung/cắt camera nếu người dùng chưa yêu cầu.

## 6. Nghiệm thu trực quan và định lượng

1. Quay clip real-time idle → rút → bay → strike → về → cất; xem từ sau, bên, trước và góc cao.
2. Duyệt chậm tại tất cả ranh giới pha để tìm pop vai/tay, twist cổ tay, pose reset và root snap; chạy tốc độ thật để kiểm tra nhịp.
3. Kiểm tra bàn chân tiếp đất, vị trí nắm, tầm vũ khí tại contact, giới hạn giáp và hướng nhìn đầu.
4. Thử chuyển sang Guard/HitReaction/Dodge/Death ở các thời điểm hợp lệ; không T-pose, không đứng cứng lại giữa đòn.
5. Kiểm tra kết quả theo thời gian ở 30/60/120 bước mô phỏng; đo frame pacing thực tế riêng trên máy hiện có. Không gọi các bước mô phỏng là FPS đo được.
6. Nêu rõ chỗ nào là clip dựng lại và chỗ nào dùng asset gốc. Không tuyên bố sao chép chính xác animation nguồn từ một video.

Bàn giao clip/prefab đã lưu, cấu hình blend/IK, báo cáo lỗi đã sửa và clip hoặc ảnh kiểm tra. Nếu rig hiện có thiếu khớp cần thiết, chỉ rõ thiếu khớp nào và hoàn thiện trong giới hạn model, không che bằng hiệu ứng sáng.
