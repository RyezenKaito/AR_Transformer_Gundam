> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# PROMPT 05 — EARTH PRISON: BAY TỚI VÙNG ĐỊCH, THI TRIỂN, THU CHIÊU VÀ TRỞ VỀ

Hãy hoàn thiện Earth Prison thành ultimate trong game Gundam theo lượt. Yêu cầu mới nhất là **Gundam phải bay tới vùng đối phương rồi thi triển**, thay thế bản đứng nguyên tại đội hình và chỉ phóng VFX từ xa. Giữ hình ảnh nhận diện của Earth Prison: tụ điện vàng, cầu điện, tia truyền, vòm xoáy có khoảng hở, sáu trụ đá, lồng sét rồi tan rã.

## 1. Di chuyển và vị trí thi triển

Chọn ultimate khi đủ thanh, chọn vùng đối phương và xác nhận. Chỉ khi xác nhận mới khóa action và trừ 100 thanh một lần. Snapshot danh sách mục tiêu còn sống để các hit của action xử lý nhất quán.

Gundam chuẩn bị, kích hoạt booster và bay tới `CastAnchor` gần vòng mục tiêu. Điểm này nằm ngoài bán kính trụ cộng khoảng trống thân Gundam; không cho caster đứng trong trụ/lồng. Ví dụ với vòng bán kính R, tâm caster phải cách tâm vùng tối thiểu R + bán kính thân + margin an toàn. Chọn phía tiếp cận theo bàn đấu và mục tiêu, không theo góc camera.

Hãm tốc tại CastAnchor, ổn định chân/tư thế bay thấp, hướng phần trên cơ thể vào vùng thi triển. Mới bắt đầu tụ điện sau khi vào vị trí. Tụ điện theo `CastSocket` ở bàn tay/focus hiện hành, không bắt buộc dùng khẩu súng luôn cầm của prototype cũ.

## 2. Hai trục thời gian phải quy đổi rõ

Dùng **ActionTime** cho toàn hành động và **VfxTime** cho trình tự hiệu ứng đã tham khảo. `VfxTime = ActionTime - VfxStart`, chỉ bắt đầu khi caster đã tới điểm thi triển. Nếu thời gian bay được tính theo khoảng cách, VfxStart phải dịch cùng với marker; không giữ damage tại các mốc global cũ.

Timeline khởi đầu đề xuất, chưa phải thời lượng bắt buộc:

| ActionTime | Hành động |
| --- | --- |
| 0–0,35 s | Commit, chuẩn bị, lấy focus nếu cần |
| 0,35–1,25 s | Cất cánh/bay tới phía địch |
| 1,25–1,55 s | Hãm tốc và vào cast pose |
| 1,55–9,55 s | VFX Earth Prison 8 giây, giữ caster ngoài lồng |
| 9,55–10,60 s | Thu chiêu, bay trở về |
| 10,60–10,90 s | Hạ xuống, cất focus nếu có, về idle |
| Sau 10,90 s | Cleanup hoàn tất rồi mới mở pha đối phương |

Không rút ngắn tùy tiện các lớp VFX để ép tổng action giữ 8 giây như trước. Tổng action giờ bao gồm bay đi, VFX và bay về. Có thể tinh chỉnh nhịp để chơi tốt hơn, nhưng phải giữ nguyên quan hệ pha và cập nhật marker cùng nhau.

## 3. Timeline VFX tính từ lúc caster bắt đầu tụ lực

| VfxTime | Hiệu ứng |
| --- | --- |
| 0–1,10 s | Điện rải tụ vào bàn tay/focus; phản chiếu ánh vàng vừa phải lên caster |
| 1,10–2,65 s | Cầu điện rõ lõi/halo, các hồ quang quấn quanh; pose tụ lực có chuyển động nhỏ |
| 2,65–3,35 s | Tia truyền từ CastSocket tới đất ở tâm đội hình địch |
| 3,35 s | Chạm đất, flash ngắn, sparks và bụi; impact damage |
| 3,45–4,30 s | Vòm xoáy vàng thấp, các dải rách/khoảng hở; vẫn thấy mục tiêu ở trong |
| 4,10–4,75 s | Sáu trụ đá trồi lên theo vòng, có đá vụn và bụi quanh chân |
| 4,65–5,10 s | Đầu trụ tích sáng, năng lượng lan lên trụ |
| 5,05–6,45 s | Các đầu trụ nối thành lồng sét lục giác, hồ quang trong lồng có nhịp |
| 6,45–7,45 s | Sét giảm, trụ dissolve, đá vụn và bụi mờ dần |
| 7,45–8,00 s | Tàn dư kết thúc, sẵn sàng thu hồi instance |

Vòm xoáy phải là các dải năng lượng không đều có breakup, không giống lưới wireframe với nhiều vòng tròn mảnh đều nhau. Trụ có khối và mặt đá rõ; sáu đỉnh nối đúng, tia có điểm đầu/cuối gắn theo tip. Glow/bloom không che hết quái. Không thêm vụ nổ lớn ở cuối nếu không có trong thiết kế.

## 4. Sát thương và phản hồi đúng thời điểm

Giữ điểm xuất phát cân bằng của prototype: impact 120 tại VfxTime 3,35; bốn pulse 60 tại 5,10 / 5,50 / 5,90 / 6,30. Tối đa 360 mỗi mục tiêu còn sống đủ cả năm hit.

Với VfxStart = 1,55 trong bảng đề xuất, các mốc ActionTime tương ứng là **4,90 / 6,65 / 7,05 / 7,45 / 7,85**. Nếu VfxStart thay đổi, tất cả các mốc đó thay đổi theo. Không gây damage ở ActionTime 3,35 trong khi caster chưa tới hoặc hiệu ứng chưa impact.

Mỗi pulse hợp lệ: flash/spark trong lồng, hit reaction ngắn và HP/damage text cùng nhịp. Chỉ áp mỗi `(battleId, actionId, hitIndex, targetId)` một lần. Không lấy OnParticleCollision, số collider hoặc số tia làm số lần damage. Mục tiêu chết không nhận các hit kế tiếp; hiệu ứng vẫn hoàn tất hợp lý và caster trở về trước khi kết thúc action.

Ultimate không tự cộng thanh từ damage nó gây ra. Không tự thêm choáng/bỏ lượt chỉ vì hình ảnh là nhà tù. Pha quái vẫn diễn ra sau khi action hoàn tất nếu còn quái sống và nhân vật chưa thua.

## 5. Camera 360° và AR

Camera không cắt sang góc cinematic cố định khi dùng ultimate. Người chơi tiếp tục orbit; camera follow/focus blend giúp nhìn thấy caster và vùng thi triển mà không ghi đè yaw người dùng. Tự tránh clipping nhẹ, không biến camera thành xa nhìn toàn bản đồ.

Kiểm tra VFX từ trước/sau/trái/phải/trên: mesh dome, trụ, tip, line và thứ tự trong suốt phải hợp lý; không mất nửa hiệu ứng vì chỉ tối ưu cho góc ngang cũ. Billboard nhìn theo camera đang active, kể cả khi pause timeline và xoay góc quan sát.

Vùng thi triển đặt theo bàn đấu/target anchor, không bám theo camera. Chọn local/custom simulation space hoặc chuyển tọa độ nhất quán để board dịch, quay, scale không làm particle/line rơi lại ở world origin. Các khoảng cách, chiều rộng line và kích thước glow phải xét scale bàn; không mix trụ đã scale với vòng điện giữ bán kính world cố định.

Khi dùng camera AR, không điều khiển pose tracking để tạo cutscene. Việc dùng được camera ngoài chỉ là chuẩn bị kiến trúc; cần kiểm chứng thiết bị riêng trước khi báo hỗ trợ AR hoàn chỉnh.

## 6. Kết thúc và kiểm thử

Chỉ complete action sau khi: VFX hết pha chính/tàn dư cần thiết, caster thu pose, về đúng đội hình, focus/vũ khí được cất, particle/audio/trail được dọn và không còn callback cũ. Animation không đứng đông khi sét chạy; return không kéo caster ngược trong pose đang tụ lực.

Nghiệm thu:

1. Quay một action trọn vẹn từ home đến địch rồi về, có thể chỉ ra pha bay đi/thi triển/bay về.
2. Kiểm tra đủ sáu trụ và toàn bộ lớp VFX, xem ở ít nhất bốn yaw camera.
3. Dùng 1v1 và 1v3: đủ năm hit trên mục tiêu sống, tối đa 360/target; quái chết không nhận thêm hit.
4. Bấm ultimate hai lần, hủy chọn, pause khi bay/impact/lồng/dissolve, reset hoặc kết thúc trận; không nhân đôi cost/hit/effect.
5. Kiểm tra đối phương không bắt đầu lượt trong khi caster đang ở cạnh lồng hoặc chưa về/cất xong.
6. Đo hiệu năng Editor và thiết bị mục tiêu riêng; đưa số liệu có môi trường đo, không cam kết FPS chỉ bằng test bước mô phỏng.

Bàn giao scene/prefab, timeline có hai trục thời gian được ghi rõ, config sát thương, hình ảnh kiểm tra và kết quả test. Ưu tiên hoàn thiện ultimate hiện có, không thay bằng một hiệu ứng khác chỉ vì dễ dựng hơn.
