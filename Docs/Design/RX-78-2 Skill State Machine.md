# RX-78-2 Part 2 — Kịch bản bản người chơi

Phần này đặc tả bộ chơi All-Rounder. Enemy trong Combat Test dùng ba chiêu theo archetype và hồi chiêu riêng. Các hệ số là mốc cân bằng đề xuất cho lần test đầu.

## State machine

```text
Idle → Anticipation → Active → Recovery → Idle
Shield Guard: RaiseShield → ParryWindow (0–0,5 s) → Hold (đến 2 s) → LowerShield
                         └─ đỡ cận chiến đúng cửa sổ → Counter → Recovery
Full Energy + Ultimate → Awakening (8 s) → Last Shooting nếu recast → Recovery
                                        └─ hết giờ → Idle
```

- **BladeOff.** Object_8 tắt, trả scale xương lưỡi và dọn trail trong Idle, hủy action, đổi mục tiêu ngoài action, bị hạ, reset hoặc unload.
- **Saber.** Event `BladeOn` bật renderer Object_8 ở đầu pha rút kiếm. Các event đánh chém lấy đoạn từ gốc tới đầu lưỡi BEAM đã skin ở pose hiện tại. Mỗi nhát chỉ gây sát thương một lần lên một mục tiêu. Event `BladeOff` tắt khi kết thúc recovery.
- **Javelin.** Sau `BladeOn`, nội suy xương lưỡi theo trục dài từ 1× tới 1,75× và bật hai nhánh plasma ở đầu để tạo ba mũi. `ThrustContact` chỉ tính trúng trong đoạn đâm quét từ vị trí cũ tới mới. Phiên bản trên không khóa hướng khi thả và dùng projectile quét liên tục. Khi hết Active, co lưỡi trước khi tắt.
- **Vulcan/Rifle.** Tap chốt khi thả trước ngưỡng 0,30 s. Giữ qua ngưỡng thì chuyển sang Rifle Aim. Cả hai dùng chung cooldown 8 s từ lúc phát đòn. Chỉ Vulcan mới kiểm giao cắt projectile hạng nhẹ. Mỗi mục tiêu chỉ bị stagger một lần mỗi loạt.
- **Guard.** Cooldown 7 s bắt đầu khi hạ khiên hoặc chuyển Counter. Trong cung trước ±60°, đạn bị chặn hoàn toàn. Counter chỉ mở từ một cú cận chiến trong 0,50 s đầu, khóa vào người vừa đánh và gây stun 1 s nếu đâm trúng.
- **Boost.** Lướt và cancel tiêu hao Boost riêng. Chỉ cho cancel sau event mở Recovery. Hit đã xác nhận vẫn được tính. Lệnh cancel vô hiệu hóa hit và VFX đang chờ của action cũ.

## Năng lượng và Learning Computer

1. Thử nghiệm vào trận ở 2/5. Sau mỗi sự kiện hợp lệ, đặt `Energy = clamp(Energy + gain, 0, 5)`. Cấp +1 nếu nhát 3 Saber trúng, Rifle tụ lực trúng hoặc Counter trúng. Miss, gọi lại event và đạn phụ không cộng.
2. Mỗi action đánh trúng cùng mục tiêu cấp một tầng phân tích, tối đa 5. Đổi mục tiêu thì làm lại chuỗi. Khi lần đầu đạt 5 tầng của chuỗi, bật Magnet Coating và cộng +1 Energy. Nếu cùng lúc có thưởng nhát cuối thì cả hai được phép cộng và vẫn bị giới hạn 5.
3. Magnet Coating tăng tốc phần animation cận chiến 15%. Animation Event dùng cùng tốc độ clip, nên hitbox chuyển theo. Hệ số chi phí lướt là 0,8. Khi mất chuỗi mục tiêu thì xóa hiệu lực.
4. Ultimate tiêu 5 điểm khi bắt đầu Awakening, hồi đầy Boost và giảm cooldown thường còn 50% trong 8 s. Chỉ cho một lần recast Last Shooting trước khi hết hạn. Recast dùng token của Awakening, không tiêu thêm. Kết thúc recast hoặc hết hạn thì xóa token. Cấm chồng hai Awakening.
5. Log mỗi action theo `actionId + markerIndex + targetId`. Mỗi khóa chỉ cập nhật HP, Armor, Energy hoặc phân tích đúng một lần. Tạm dừng phải đóng băng đồng thời timeline, cooldown và thời lượng Awakening.

## Sát thương chuẩn hóa đề xuất

Một đơn vị 1,00 tương ứng 100 sát thương gốc ở ATK 100. Cột HP đưa ra tổng trước Def, Armor và Parry. Với mỗi hit, dùng `ATK × hệ số HP × 100/(100 + DEF)`. Khi giáp còn ở đầu hit, áp mức bảo vệ của GDD. Giáp mất `(0,5 × ATK + 0,5 × BreakDMG) × hệ số Armor`. Dodge trả về 0. Hệ số xuyên giáp bên dưới chỉ áp dụng cho Javelin.

| Chiêu | Hệ số HP / phân bổ | Hệ số Armor | Cửa sổ / hiệu ứng |
|---|---|---:|---|
| Saber Slash | 0,44 + 0,48 + 0,68 = 1,60 | 0,95 chia theo hit | Nhát 3 có lướt và đẩy lùi. Trúng nhát 3 hồi 1 Energy. |
| 60mm Vulcan | 0,07 × 8 = 0,56 | 0,32 | 1,5 s, stagger ngắn một lần lên mục tiêu đang lao. CD 8 s. |
| Beam Rifle | 1,45 | 0,90 | Một event xả tia thẳng. Trúng hồi 1 Energy. CD chung 8 s. |
| Shield Guard & Counter | 0 khi giữ, 0,85 nếu đâm trúng | 0,55 khi phản kích | Cửa phản kích 0,5 s, giữ 2 s, stun 1 s. CD 7 s. |
| Beam Javelin Thrust | 1,65 | 1,35 | Xuyên 25% lớp bảo vệ của Armor, làm chậm di chuyển 20% trong 2 s. CD 10 s. |
| Last Shooting | 0,90 bazooka + 2,50 rifle = 3,40 với đích chính | 1,80 | Đích phụ chỉ nhận 0,60 từ AoE. Recast tiêu token Awakening. |
