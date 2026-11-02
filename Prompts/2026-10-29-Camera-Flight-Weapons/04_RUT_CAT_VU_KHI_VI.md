> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# PROMPT 04 — KHÔNG CẦM SẴN VŨ KHÍ, RÚT VÀ CẤT THEO HÀNH ĐỘNG

Hãy sửa hệ thống vũ khí Gundam theo video `...23-18-08.mp4` và các yêu cầu dưới đây. Điểm cần đạt là nhìn thấy nhân vật lấy vũ khí để dùng rồi thu về, không chỉ bật một prefab trong tay ngay khi bấm nút.

## 1. Trạng thái chờ

Gundam ở `IdleUnarmed`: tay không giữ súng hoặc kiếm dài chĩa sẵn. Nếu thiết kế có chuôi kiếm/vũ khí cất trên lưng hoặc hông, phần đó có thể hiện ở đúng vị trí cất; lưỡi kiếm năng lượng phải tắt. Súng không có vị trí cất trên model thì phải có cơ chế deploy/retract được animate rõ, không tự xuất hiện ở bàn tay đang mở.

Kiểm tra prefab hiện tại: vũ khí năng lượng đang gắn sẵn dưới gun socket phải được đưa vào trạng thái cất đúng. Không chỉ thêm kiếm mới mà để nguyên súng luôn cầm trong tay còn lại. Khiên/giáp thuộc model được xử lý riêng theo thiết kế; không xóa chi tiết model chỉ vì video minh họa dùng Gundam khác.

## 2. Socket và dữ liệu vũ khí

Mỗi loại vũ khí cần:

- `HolsterSocket` hoặc vị trí deploy xác định, `MainHandGrip`, `OffHandGrip` nếu hai tay, `Muzzle` nếu bắn, `BladeBase/BladeTip` nếu kiếm.
- Offset theo từng model/kiểu cầm; kiểm tra trục local, không đoán grip bằng tên bone.
- Kiểu vũ khí, prefab instance/pool, marker attach/detach, bật/tắt năng lượng, trail và hit window.
- Một owner rõ ràng. Không tạo vũ khí mới mỗi hit và bỏ lại bản cũ trên scene.

State tối thiểu: `Holstered → Reaching → Drawing → Equipped → Active → Recovering → Sheathing → Holstered`. Có xử lý `Interrupted/Disabled` để cleanup. Vũ khí không tự điều khiển lượt hoặc tự áp damage ngoài ActionController.

## 3. Chuỗi rút kiếm đầy đủ

1. Nhìn/hướng thân về đối phương, tay bắt đầu với tới chuôi đang cất. Vai/ngực/eo xoay bù để với hợp lý.
2. Đến marker `GripContact`, bàn tay khép/nắm tại chuôi; gắn vũ khí sang hand socket mà không nhảy vị trí world. Blend grip offset nhỏ nếu cần.
3. Tay kéo vũ khí khỏi vị trí cất theo một quỹ đạo nhìn thấy được; không bật đầy đủ kiếm ngay frame đầu của Reach.
4. Sau khi vũ khí thoát vị trí cất, bật lưỡi kiếm từ gốc tới ngọn trong khoảng 0,08–0,18 s tham khảo; âm thanh bật kiếm cùng marker.
5. Vào stance/wind-up và chỉ bật trail trong pha vung có ý nghĩa. Khi đứng giữ kiếm, không để trail kéo dài như dây.
6. Tại contact, gửi yêu cầu hit đã định danh tới hệ combat; chỉ hiển thị hit spark và âm thanh trúng nếu hit hợp lệ, không bị né.

Chuỗi này có thể chồng một phần với cất cánh/bay nếu tay vẫn đi đúng quỹ đạo. Không rút kiếm xuyên đầu, vai hoặc backpack. Khi xem 360° phải giữ đúng hình học từ cả phía sau.

## 4. Thu và cất

Sau follow-through: thu tay về pose an toàn → tắt trail → tắt lưỡi kiếm theo chiều ngọn về gốc → đưa chuôi về chỗ cất → attach tại marker `HolsterContact` → bàn tay nhả → tay trở về idle.

Có thể cất trên đường về hoặc sau hạ xuống tùy clip, nhưng phải hoàn tất trước khi mở lượt kế tiếp. Không làm vũ khí biến mất ở vị trí đang chém rồi reset tay trong cùng một frame.

Vũ khí hai tay cần nhả tay phụ và IK đúng thứ tự. Một frame không được tồn tại hai bản nhìn thấy: một bản vẫn ở lưng và một bản vừa xuất hiện trong tay. Nếu dùng hai representation cho holster/equipped, chuyển hiển thị tại thời điểm tiếp xúc liên tục và kiểm tra không có khoảng hở/pop.

## 5. Loại chiêu và vũ khí tương ứng

- **Attack:** đề xuất kiếm năng lượng; rút → bay áp sát → chém → quay về/cất.
- **Skill:** nếu dùng pháo/súng, deploy/rút súng → bay đến vị trí thi triển gần phía địch → aim/fire/recoil → về/cất. Không mặc định đổi skill thành đứng bắn từ home.
- **Earth Prison:** được cấu hình focus/gauntlet hoặc tay không điều khiển năng lượng; không bắt buộc cầm kiếm/súng nếu chiêu không dùng chúng. Nếu có focus rút ra thì phải áp dụng cùng chuỗi rút/cất, không spawn tùy ý trên tay.
- **Guard/Dodge:** không tự rút vũ khí tấn công trừ khi animation và action được thiết kế rõ. Nếu guard dùng khiên riêng, điều phối trạng thái khiên độc lập và không làm mất trạng thái vũ khí.

Đây là cấu hình ban đầu cho project, không phải kết luận rằng video có đầy đủ tất cả các loại vũ khí trên.

## 6. Marker, hiệu năng và gián đoạn

Marker có thể dùng Animation Events, Timeline Signals hoặc action timeline đã có; chọn một nguồn chuẩn. Phải xử lý được khi deltaTime vượt qua marker: sự kiện xảy ra một lần, không mất do đòi thời gian bằng tuyệt đối. Callback action cũ không được bật kiếm lại sau reset/death.

Tách `Equipped`, `Visible`, `BladeActive`, `TrailActive`, `CanHit`; không coi active GameObject là quyền áp damage. Dùng pooling nếu phù hợp; kiểm tra trail cũ, particle cũ, audio loop và collider khi tái dùng instance.

Hủy chọn trước xác nhận không làm rút. Hủy action hợp lệ, actor chết, scene đổi, restart, pause/resume đều có trạng thái xác định. Reset được phép khôi phục ngay trạng thái đầu vì là reset trận; kết thúc đòn bình thường phải dùng động tác cất, không dùng nhánh reset để che animation thiếu.

## 7. Nghiệm thu

- Vào scene và chờ 10 giây: tay không cầm sẵn vũ khí tấn công, không có blade/trail chạy.
- Xem chậm chuỗi rút: tay chạm chuôi trước khi chuyển attachment; không teleport hoặc xuyên giáp rõ rệt.
- Chém và cất nhìn ổn từ phía trước/sau/hai bên. Chạy thực tế phải thấy nhịp rút/đánh/cất, không chỉ đúng trong Inspector.
- Dùng liên tục 10 lần: số instance ổn định, không duplicate vũ khí, không sót audio/trail/collider.
- Pause giữa rút, giữa strike, giữa cất; restart/death/đổi lượt không để vũ khí treo sai chỗ.
- Attack, Skill, Ultimate dùng đúng loadout; không giữ súng cố định trong idle của bản cũ.

Bàn giao prefab/socket, dữ liệu marker, cách thay vũ khí và một clip quay đầy đủ từ tay không trở lại tay không.
