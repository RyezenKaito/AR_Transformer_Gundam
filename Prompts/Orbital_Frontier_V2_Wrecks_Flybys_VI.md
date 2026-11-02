# Orbital Frontier V2 — xác tàu, thiên thạch lướt và các hành tinh

Phần này ưu tiên hơn mô tả trạm/vệ tinh và giới hạn hành tinh trong prompt V1.

## Xác tàu bằng asset thật

Bỏ hoàn toàn ST01 gồm ba hình trụ và D01 vệ tinh hộp ghép pin mặt trời. Không thêm lại khi rebuild. Giữ nhánh scene `02_Station` để tương thích bố cục, nhưng nội dung hiện là ba cụm xác tàu, không còn trạm đơn giản.

- W01: thân vỏ Omega OutBody, Frame_body lệch xuống/ra sau để lộ khoảng hở, Cockipt tách về phía trước, một động cơ và một cánh StarSparrow rời. Vị trí local (52,5,78), thân chính rộng nhất 22 m, góc (17,-34,21).
- W02: StarSparrow core, cánh trái bị bẻ nghiêng, cánh phải trôi rời, động cơ tách khỏi thân. Vị trí (-24,15,102), core 12 m, cánh 10–11 m.
- W03: Omega frame làm sống tàu, ba nacelle StarSparrow tách rời thành cụm drive hỏng. Vị trí (15,-24,-82), sống tàu 16 m.
- D00–D35: 36 mảnh dùng prefab cánh, engine, weapon, thruster và core từ FreeSciFi; kích thước khoảng 1,2–6 m. Sáu mảnh đầu nằm gần cụm W01; phần còn lại rải phía dưới/rìa. Xoay chậm 0,25–1,2°/giây, drift dưới 0,5 m.

Giữ texture, normal map, màu sơn và kim loại nguồn. Dừng particle/động cơ demo trong xác tàu. Không đưa camera, MonoBehaviour điều khiển tàu hay collider của demo vào bối cảnh. Đây là xác tàu trang trí được ghép từ bộ phận có sẵn, chưa có phá hủy động.

## Đưa tàu vào khung chính

S01 tàu lớn ở phía phải, lùi xa hơn tại (132,10,0), chiều dài chuẩn hóa 56 m; tâm tàu nằm trong viewport mặc định và cách player khoảng 137 m. R35 chuyển tới (48,-9,60). Ba StarSparrow hạ độ cao còn 6/9/12 m. Hai SF escort ở (65,-14,-5) và (78,-10,-15). Các vị trí tương đối LayoutOrigin=(4.85,0,0).

Giữ vùng an toàn bán kính 40 m và không cho tàu cắt qua robot. Vật thể gần nhất đã kiểm tra bằng renderer bounds cách tâm chiến đấu khoảng 56,2 m.

## Thiên thạch lướt liên tục

Thêm sáu `OrbitalFlyby` độc lập, dùng asteroid GLB đang có. Không thay 88 asteroid nền. Hướng trục bố cục forward=(0.906,0,0.423), right=(0.423,0,-0.906). Sáu làn sâu 95/113/131/149/167/185 m, độ cao luân phiên 10/22/34 m; đường bay chéo ngang ngoài vùng giao chiến.

Mỗi lượt dài 6,5–10,75 giây, phase ban đầu cách nhau 1/6 vòng để luôn có lượt bay đang diễn ra. Head rộng 0,65–1,09 m, tumble nhẹ; tail dài 14–24 m. Một lõi vàng sáng mảnh 0,16 m và outer wake cam đỏ rộng 0,65 m, thuôn dần về alpha/width bằng 0. Đây là vệt sáng mỹ thuật, không dùng khói/lửa khí quyển.

Tail tính theo hướng vận tốc và vị trí đầu hiện tại. Khi tới cuối đường: fade-out, tái sử dụng cùng object, reset ở đầu đường; tuyệt đối không nối một vệt sáng dài xuyên khung hình lúc teleport. Không spawn/destroy liên tục mỗi frame, không Rigidbody rơi xuống, không đụng camera hay actor. Kiểm tra nhiều vòng lặp và khoảng cách tối thiểu tới combat.

## Hành tinh và chuyển động

Giữ Earth, Moon và Tinniuq. Thêm Sao Mộc tại (2900,350,250), bán kính 200 m, xoay 1,1°/giây; shader procedural có dải mây kem/nâu với nhiễu xoáy và một vortex màu đỏ nâu. Đặt cách Moon trong khung nhìn, không để hai đĩa chồng lên nhau.

Thêm Sao Thổ ở hướng khác tại (-2400,650,1800), bán kính 180 m, thân xoay 0,7°/giây. Vành procedural bán kính 1,3–2,35 lần bán kính thân; nhiều dải bụi mảnh và một khoảng tối tách vành. Toàn hệ nghiêng (55,12,12). Khi xoay camera sang hướng đối diện có thể nhìn thấy.

Earth xoay 0,8°/giây, lớp mây xoay 1,03°/giây để chuyển động dễ quan sát. Chỉ quay hành tinh và lớp mây, không quay cả battlefield hoặc camera. Các tốc độ/khoảng cách là mỹ thuật cho gameplay, không phải mô phỏng thiên văn.

## Kiểm tra và lưu

Chạy `OrbitalVerification` trong Play Mode. Kiểm tra prefab nguồn và shader, đủ ba cụm wreck/36 mảnh, sáu flyby lặp lại với tail hữu hạn, tàu lớn vào góc mặc định, Earth/mây xoay, vòng camera/trên/dưới, sát thương và luồng đổi scene. Báo kết quả thực tế, không kế thừa con số kiểm thử cũ nếu chưa chạy lại. Lưu scene, prefab môi trường và thumbnail sau khi xem ảnh render.


