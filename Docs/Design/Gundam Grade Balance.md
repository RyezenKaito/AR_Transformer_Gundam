# Cơ chế Grade và cân bằng Gundam

Ghi chú thiết kế cho bước cân chỉ số của Strike Rouge và Unicorn, dựa trên [GDD v0.1, mục 3.2–3.6](GDD_AR_Transformer_Gundam_v0.1_Revised.md#32-lịch-hành-động-theo-speed).

## Vai trò theo Grade

| Grade | Vai trò | Điểm mạnh | Điểm yếu |
| --- | --- | --- | --- |
| HG | Balanced / Tactical | Chỉ số đồng đều, dễ xoay vòng chiêu 1–2 và tích Burst ổn định | Ít lợi thế khi so trực tiếp với Gundam chuyên về Speed, Defense hoặc Break Dmg |
| RG | Assassin / Breaker | Speed mục tiêu 150–160, phá Armor nhanh, thường có lợi thế Dodge | Max HP, Armor và Defense thấp. Dễ mất nhiều HP khi né hụt hoặc gặp đối thủ có Speed tương đương |
| MG | Defender / Bruiser | Defense và Armor cao, dễ đạt điều kiện Parry toàn phần, đòn đơn mục tiêu mạnh | Speed mục tiêu 120–130, ít lượt hơn và dễ bị bào giáp qua nhiều lượt |
| PG | Burst Carry | Dồn sát thương cao khi đủ năm ô Burst | Cần lượt để tích Burst và chịu áp lực khi đối thủ phá Armor sớm. Phân bổ chỉ số cần chừa sở trường Speed và phá giáp cho RG, chống chịu cho MG |

Strike Rouge đại diện RG. Unicorn đại diện PG. Phạm vi Full Burst do từng bộ chiêu quyết định. Đề xuất hiện tại cho Unicorn là giữ đòn kết liễu một mục tiêu. Phương án Full Burst diện rộng của PG vẫn là lựa chọn thiết kế cần chốt cho từng bộ chiêu.

## Luật chiến đấu dùng khi cân chỉ số

- Speed trong khoảng 120–160. Thời gian chờ lượt trong GDD là `10.000 / Speed`.
- Dodge đúng nhịp né 100% khi Speed của người né lớn hơn Speed người đánh. Trường hợp bằng hoặc thấp hơn có 50% xác suất né.
- Parry đúng nhịp chặn toàn bộ sát thương HP và Armor khi `Defense × 1,25 > Attack` của người đánh. Trường hợp bằng hoặc thấp hơn nhận 50% sát thương.
- Khi còn Armor ở đầu hit, sát thương HP sau Defense giảm 50%. Sát thương giáp cơ sở là `0,5 × Attack + 0,5 × Break Dmg`, sau đó nhân hệ số phá giáp của chiêu và hệ số phản ứng phòng thủ.
- Burst bắt đầu ở 2/5. Chi phí bốn chiêu là 0/1/2/5. Chiêu 1 hồi thêm 1 ô, cuối lượt của chính Gundam hồi 1 ô.

## Bốn ô tóm tắt trên Lobby

Mỗi ô dùng thang S/A/B/C/D và cùng một bộ đối thủ chuẩn cho các Gundam. Radar sáu trục tiếp tục hiển thị chỉ số gốc.

| Ô hiển thị | Cách đo đề xuất |
| --- | --- |
| Burst DMG | Sát thương Full Burst lên mục tiêu chính trong cùng điều kiện thử |
| Armor Break | Lượng Armor phá được qua hai lượt với cùng mức Burst khởi đầu |
| Evasion (Dodge) | Tỷ lệ đối thủ chuẩn có Speed thấp hơn Gundam, với giả định canh Dodge đúng nhịp |
| Guard (Parry) | Tỷ lệ đòn đánh chuẩn bị chặn hoàn toàn khi canh Parry đúng nhịp |

Bộ đối thủ chuẩn, ngưỡng S–D và xếp hạng thực tế sẽ được chốt sau khi cân hai profile. Các ví dụ S/A/B/C/D trong thảo luận là mục tiêu vai trò.

## Mốc hiện tại trước khi cân

| Gundam | Grade | Max HP | Attack | Defense | Speed | Break Dmg | Armor |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Strike Rouge | RG | 400 | 100 | 80 | 144 | 60 | 300 |
| Unicorn | PG | 680 | 100 | 86 | 153 | 60 | 185 |

Hướng đề xuất cho Strike Rouge là thử Speed 156–158, tăng hiệu quả phá Armor của chiêu chủ lực và giảm Armor so với mốc hiện tại. Giá trị cuối của hai Gundam sẽ được chốt trong buổi cân chỉ số.
