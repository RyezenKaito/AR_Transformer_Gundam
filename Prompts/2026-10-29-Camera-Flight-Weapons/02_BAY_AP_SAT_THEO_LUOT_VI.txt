> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# PROMPT 02 — BAY TỚI ĐỐI PHƯƠNG KHI ĐÁNH, KỸ NĂNG VÀ ULTIMATE

Hãy hoàn thiện chuyển động trong action của game Gundam theo lượt. **Cả đòn đánh, kỹ năng và Earth Prison đều có pha bay tới vùng đối phương trước khi tác động**. Không giữ thiết kế cũ chỉ đòn cận chiến áp sát, còn skill/ultimate đứng nguyên tại đội hình.

## 1. Đứng chờ và hành động là hai trạng thái khác nhau

Khi chờ lượt/chọn chiêu, Gundam và quái giữ `HomeAnchor`, chỉ có idle pose nhẹ. Không có đi tự do, truy đuổi tự động hoặc luồng AI chạy tới địch trước khi được cấp lượt.

Sau xác nhận: chuẩn bị → rút vũ khí nếu chiêu cần → cất cánh → bay tới `AttackAnchor` → hãm tốc → thi triển/tác động → thu chiêu → bay về → hạ xuống → cất vũ khí → idle. Cho phép một số pha rút/cất chồng thời gian với bay nếu animation thể hiện được và không bỏ qua thao tác tay.

“Bay qua đối phương” được triển khai thành bay đến vùng thực hiện đòn gần phía địch, không mặc định cho root xuyên tâm collider địch. Đòn xuyên qua hoặc lướt vòng ra sau chỉ dùng nếu được thiết kế riêng, có điểm đến và đường về an toàn.

## 2. Điểm đến theo chiêu

- Cận chiến: dừng ngoài thân mục tiêu, cách đủ gần để tầm tay/kiếm thật sự chạm. Khoảng dừng phải tính cả kích thước hai model và reach của đòn, không dùng một hằng số cho mọi quái.
- Kỹ năng: cũng rời `HomeAnchor`, bay tới vị trí thi triển ở phía đối phương. Với pháo/đạn, dừng ở cự ly bắn được cấu hình gần mục tiêu; vị trí muzzle, đường đạn và đích phải khớp.
- Ultimate Earth Prison: bay tới `CastAnchor` gần vùng quái nhưng bên ngoài phạm vi dựng sáu trụ, rồi mới tụ điện. Gundam không đứng trong trụ hoặc chính lồng sét.
- Multi-target: xác định vùng đội hình đối phương và chọn vị trí thi triển hợp lệ; không di chuyển theo camera hoặc nhảy qua từng mục tiêu cho các hit AoE.
- Quái thực hiện lượt riêng theo cùng nguyên tắc nếu dùng đòn áp sát; mục tiêu phòng thủ không tự chạy đuổi theo kẻ tấn công.

Lưu anchor theo `BattleContent`/bàn đấu, không chỉ chụp tọa độ world một lần. Khi bàn đổi vị trí, xoay hoặc scale, hướng tới đối phương và đường về vẫn đúng. Không dùng `Vector3.forward/up` toàn cục nếu bàn có hệ trục riêng.

## 3. Cảm giác bay phải có lực

Có nhún chuẩn bị, nghiêng thân theo hướng tăng tốc, chân thu/bẻ nhẹ khi bay, tay giữ vũ khí an toàn, booster sáng theo lực đẩy. Khi sắp tới địch: giảm tốc có chủ đích, thân ngả để hãm, chân mở lại, vai/tay chuẩn bị đánh. Khi quay về có chuyển hướng/boost phù hợp, không kéo nguyên tư thế đánh lùi suốt đường.

Đường bay dùng curve/spline hoặc quỹ đạo tham số với tốc độ tăng–giảm mượt. Có thể cong hoặc nâng nhẹ khỏi mặt đất nhưng phải nhất quán với trọng lượng mecha. Không teleport tới target, không `Lerp(current,target,t)` lệ thuộc FPS, không đi tốc độ hằng rồi dừng đột ngột.

Vị trí và hướng phải liên tục giữa các pha. Giới hạn góc quay/tốc độ quay; không đổi quaternion 180° trong một frame. Blend nghiêng thân trong rig, không trộn tilt thân vào heading logic của toàn nhân vật/camera.

Sơ đồ thời gian khởi đầu cho đòn cơ bản khoảng 3,4–4,0 s, cho phép tinh chỉnh:

| Pha | Khoảng đề xuất |
| --- | --- |
| Chuẩn bị, với tay lấy vũ khí | 0–0,45 s |
| Rút/nhún cất cánh, tăng tốc | 0,35–0,80 s |
| Bay tới và hãm tại điểm đánh | 0,65–1,25 s |
| Wind-up, contact, follow-through | 1,25–1,85 s |
| Thu đòn, xoay và bay về | 1,85–2,95 s |
| Hạ xuống, tắt/cất vũ khí, idle | 2,95–3,70 s |

Không ép mọi chiêu dùng đúng bảng này. Damage phải nằm ở contact/release được khai báo trong timeline thực tế, không giữ thời điểm damage cũ sau khi đã kéo dài animation.

## 4. Một bộ điều phối action

Mỗi action có dữ liệu target, loại vũ khí, reach/cast range, đường bay, clip, marker rút/nắm/kích hoạt/contact/cất, damage events và VFX. Chọn một nguồn thời gian cho toàn action. Cùng một hệ thống sở hữu root motion; không để Animator, NavMesh, Rigidbody và script cùng kéo root.

Nếu dùng chuyển root theo script, clip không được tự cộng translation lần nữa. Nếu dùng root motion, phải có cơ chế căn quỹ đạo theo target/anchor và đảm bảo đúng range. Chọn giải pháp phù hợp rig thật; không bật root motion rồi chữa sai bằng teleport ở cuối.

Chỉ mở pha đối phương sau khi action thông báo đã hoàn tất **cả gameplay và presentation**: đủ hit hợp lệ, thu/cất vũ khí, về đội hình, dọn effect và bỏ khóa input. Không đổi lượt ngay khi damage cuối được áp.

## 5. Các trường hợp cần xử lý

- Chọn/hủy trước xác nhận: không bay, không trừ gauge, không rút vũ khí vô cớ.
- Bấm xác nhận hai lần: vẫn một action, một quỹ đạo, một lần tiêu hao.
- Target bị hạ trong đòn: không tiếp tục gây damage vào mục tiêu chết; vẫn kết thúc động tác và quay về an toàn.
- Action bị hủy, actor chết hoặc scene reset: dừng motion/event còn treo, dọn vũ khí/effect; không để kẻ kế tiếp dùng lại target hoặc callback của action cũ.
- Pause: đóng băng timeline và quỹ đạo, resume không nhảy vị trí. Thay tốc độ mô phỏng không làm sai mốc hit/né.
- Né: người phòng thủ né theo nhịp tại vị trí đang phòng thủ; không dùng né làm lệnh áp sát hoặc thoát đội hình. Vệt chém/đạn phải thể hiện đòn hụt phù hợp.
- Chướng ngại/đơn vị khác: chọn lane hoặc đường cong hợp lệ; không xuyên model đồng đội/quái khác trên đường bay.

## 6. Nghiệm thu

Thử Attack, Skill và Earth Prison riêng: cả ba đều rời đội hình, tới đúng cự ly trước khi tác động và quay về. Kiểm tra frame ngay trước hit, hit và sau hit để chứng minh vị trí/chuyển động/VFX/damage khớp.

Khi lượt kế tiếp mở, actor đã trở về anchor với sai số nhỏ được định nghĩa theo scale bàn, không có snap mắt thường thấy. Thử liên tục ít nhất 10 vòng, 1v1 và 1v3, chọn quái đầu/giữa/cuối, pause khi đang bay, target chết và restart. Dữ liệu test mô phỏng 30/60/120 Hz phải cho cùng kết quả gameplay; đánh giá độ mượt bằng chạy thật và clip quay màn hình riêng.
