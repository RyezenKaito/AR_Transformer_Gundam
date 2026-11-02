> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# PROMPT 01 — CAMERA THIRD-PERSON PHÍA SAU, XOAY TỰ DO 360°

Hãy sửa camera trận đấu Unity theo yêu cầu sau. Game đánh theo lượt, nhân vật đứng ở đội hình khi chờ và có thể bay tới đối phương trong action. Camera phải hoạt động độc lập với bộ điều khiển hành động.

## 1. Góc nhìn mặc định đúng tham chiếu

Dùng ảnh `References/DefaultCamera.png` làm chuẩn bố cục: camera ở sau lưng Gundam, cao hơn thân trên một chút, hướng nhìn chếch xuống phía trước. Thấy rõ lưng, vai, tay và chân Gundam ở vùng giữa-dưới khung hình; phía trên là khoảng không để nhìn mục tiêu và đường bay.

Đây là phối cảnh third-person tương đối gần, không phải camera ngang sân, orthographic, bản đồ từ trên cao hoặc góc isometric nhìn toàn sân. Không hiểu “nhìn từ trên xuống phía sau” thành camera gần thẳng đứng trên đầu. Nhân vật không bị thu nhỏ chỉ để ép mọi vật luôn cùng nằm trong khung.

Thông số khởi đầu, chuẩn hóa theo chiều cao Gundam H:

- Perspective, FOV dọc khoảng 45–55°.
- Pivot gần ngực/vai, cao khoảng 0,65–0,80 H trên chân.
- Pitch mặc định khoảng 15–25° nhìn xuống; chỉnh trực quan cho gần ảnh.
- Khoảng lùi khởi đầu khoảng 2,4–3,4 H; Gundam chiếm khoảng 35–50% chiều cao màn hình khi chờ, có khoảng trống trên đầu và phía trước.
- Có thể lệch vai nhẹ khoảng 0,10–0,25 H để nhìn mục tiêu rõ hơn, không che trung tâm bằng thân Gundam.

Các giá trị phải chỉnh được trong Inspector. Nghiệm thu bằng ảnh Game View so với bố cục tham chiếu, không chỉ kiểm tra các con số.

## 2. Xoay 360° thật sự

- Yaw không khóa trái/phải; kéo liên tục qua 0°/360° nhiều vòng không giật, không nhảy chiều.
- Chuột phải kéo để orbit, bánh xe zoom; vuốt ở vùng trống để orbit, pinch hai ngón để zoom. Cho phép gán lại input.
- Pitch giới hạn hợp lý, khởi đầu 5–75°, không lộn ngược. Zoom giới hạn theo H và tránh xuyên nhân vật.
- Có nút/phím “Góc nhìn sau” chủ động, ví dụ Home. Reset chuyển mượt về góc sau Gundam.
- Thả chuột không tự xoay về sau; chuyển lượt, đổi target và tung ultimate cũng không tự reset yaw của người dùng.
- Xoay camera không thay đổi hướng đứng, hướng tấn công, vị trí đội hình, mục tiêu hay kết quả né của Gundam.
- Hai phe đối diện nhau trong thế giới game; không ép phe ta luôn ở trái màn hình sau khi camera xoay ra phía đối diện.

## 3. Theo nhân vật khi bay, vẫn giữ quyền điều khiển

Trong chế độ desktop/mobile không AR, pivot theo Gundam bằng độ trễ nhẹ. Khi bay áp sát, camera đi theo mượt, giữ yaw/pitch/zoom mà người chơi đã chọn. Không lấy hướng xoay của thân trong animation làm góc orbit mới mỗi frame.

Có thể dịch điểm ngắm một phần về trung điểm Gundam–mục tiêu trong action để thấy đòn chạm. Blend trọng số focus khoảng 0,15–0,35, có giới hạn tốc độ; không tự cắt góc hoặc ép quay. Điểm focus và vị trí camera phải liên tục lúc bắt đầu bay, thắng đòn, quay về và đổi lượt.

Tách `follow smoothing`, `orbit smoothing`, `zoom smoothing`; ví dụ 0,10–0,20 s để bắt đầu tinh chỉnh. Input delta chuột không nhân deltaTime hai lần; tốc độ quay theo phím phải tính theo thời gian. Cập nhật camera sau chuyển động nhân vật để tránh rung một frame. Thử ở tốc độ mô phỏng và FPS khác nhau.

Nếu tường hoặc trụ đá che camera, spherecast từ pivot tới vị trí mong muốn, thu ngắn cánh tay camera mượt và trả lại khoảng cách khi hết cản. Bỏ qua collider của chủ thể cần theo, hiệu ứng không cản và trigger. Đặt khoảng cách tối thiểu; tránh camera dao động giữa nhiều collider. Không để trụ Earth Prison làm camera bật ra bật vào liên tục.

## 4. UI và đầu vào

- Bắt đầu drag/pinch trên nút chọn chiêu, target, thanh né hoặc panel thì không orbit.
- Drag hợp lệ bắt đầu ở vùng trống có thể tiếp tục khi con trỏ đi ngang UI; kết thúc đúng khi nhả/mất focus.
- Chuyển một ngón sang hai ngón và ngược lại phải đặt lại dữ liệu điểm chạm, không nhảy camera.
- Nhấn nút né cảm ứng không vừa né vừa xoay camera. Không dùng thao tác xoay camera làm lệnh điều khiển nhân vật.
- Health/nameplate/damage text phải chiếu theo camera đang dùng. Glow/billboard VFX cập nhật góc nhìn kể cả khi timeline combat đang pause và người dùng vẫn orbit.

## 5. Chuẩn bị AR, không chiếm camera tracking

Tách `BattleContent/BoardRoot` khỏi camera. Tất cả vị trí đội hình, đường bay, mục tiêu và hiệu ứng được quy về hệ tọa độ bàn đấu. Chuyển vị trí, hướng và scale của bàn không được làm sai đường bay hoặc hướng đánh.

Khi dùng camera AR/XR bên ngoài, tắt controller orbit/follow của camera mô phỏng. Không ghi đè transform, FOV, projection hoặc pose tracking của camera AR. Cập nhật HUD, raycast và billboard để dùng camera AR; không vô tình bật hai AudioListener/camera nền cùng lúc.

Trong AR, người dùng nhìn quanh bằng việc di chuyển thiết bị; đó là cách quan sát 360° của chế độ AR. Nếu thêm xoay bàn bằng touch, phải là tính năng riêng xoay `BoardRoot`, không giả lập orbit bằng cách sửa pose camera tracking. Không tự cài package AR hoặc báo AR hoàn tất chỉ vì có API nhận `Camera`.

## 6. Nghiệm thu

1. Ảnh idle từ góc mặc định gần ảnh tham chiếu; sau khi reset scene vẫn đúng.
2. Quay ít nhất hai vòng đầy đủ, qua đường nối 360°, ngẩng/cúi và zoom không giật.
3. Để camera ở góc bên hoặc đối diện rồi dùng attack, skill, Earth Prison: camera không tự khóa lại góc sau, vẫn theo action mượt.
4. Click UI không làm orbit; chuột/keyboard và touch được kiểm riêng.
5. Camera không xuyên sàn, thân Gundam hoặc bị rung vì trụ đá.
6. Thử 16:9, 16:10 và màn hình di động ngang; mặc định gần ảnh nhưng vẫn thấy rõ mục tiêu liên quan.
7. Camera ngoài được nhận đúng, không bị script ghi pose; chuyển lại desktop đúng. Báo riêng phần AR chưa kiểm thử trên thiết bị.

Bàn giao scene/prefab đã lưu, cách điều khiển, tham số camera và ảnh trước/sau. Nếu thấy code camera fixed cũ còn điều khiển scene thì gỡ xung đột, không để hai controller cùng ghi transform.
