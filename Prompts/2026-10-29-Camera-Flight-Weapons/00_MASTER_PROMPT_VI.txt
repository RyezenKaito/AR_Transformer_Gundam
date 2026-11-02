> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

# MASTER PROMPT — GUNDAM THEO LƯỢT, CAMERA ORBIT, BAY ÁP SÁT, RÚT VŨ KHÍ

Đặc tả ngày 29/10/2026, theo yêu cầu mới nhất của người dùng. Đây là bộ yêu cầu thay thế các ràng buộc cũ về camera khóa cố định, nhân vật tuyệt đối không rời vị trí, kỹ năng/ultimate đứng tại chỗ và súng luôn nằm trong tay.

## Yêu cầu chính

Hãy hoàn thiện game Unity của tôi trong `E:/My project` theo bộ prompt này. Game vẫn đánh theo lượt. Khi chờ, Gundam và quái đứng ở các vị trí đội hình; khi hành động được xác nhận, Gundam **bay đến vùng đối phương, thực hiện đòn đánh/kỹ năng/ultimate, thu động tác và trở về**. Không biến game thành điều khiển đi lại tự do bằng WASD.

Camera mặc định là **third-person phía sau Gundam, hơi cao và nhìn chếch xuống**, theo ảnh `References/DefaultCamera.png`. Người chơi có thể xoay camera đủ 360°; góc sau là góc khởi đầu/reset, không phải góc bị khóa. Camera không bắt buộc luôn đặt người chơi bên trái và quái bên phải màn hình. Đội hình cố định trong thế giới game, bố cục trên màn hình thay đổi khi xoay camera.

Khớp Gundam phải chuyển động liên tục và phối hợp. Không chỉ nội suy vị trí toàn bộ model trong khi tay chân giữ cứng. Vũ khí tấn công không được cầm sẵn trong idle; khi dùng phải có động tác lấy/rút, nắm đúng vị trí, kích hoạt, tấn công, tắt và cất lại. Earth Prison tiếp tục là ultimate, nhưng bổ sung pha bay tới gần vùng đối phương trước khi thi triển.

## Tài liệu phải thực hiện cùng nhau

1. `01_CAMERA_THIRD_PERSON_360_VI.md`: camera mặc định theo ảnh, orbit, zoom, theo nhân vật và quyền sở hữu camera khi dùng AR.
2. `02_BAY_AP_SAT_THEO_LUOT_VI.md`: di chuyển có điều kiện trong hành động, đường bay, điểm dừng, quay về và khóa lượt.
3. `03_CHUYEN_DONG_KHOP_MUOT_VI.md`: rig, chuyển tư thế, chuyển động toàn thân, IK và thời điểm tiếp xúc.
4. `04_RUT_CAT_VU_KHI_VI.md`: chuỗi vũ khí, socket, rút/cất, bật lưỡi kiếm và dọn trạng thái.
5. `05_EARTH_PRISON_BAY_AP_SAT_VI.md`: timeline ultimate mới, sát thương, sáu trụ/lồng sét và kết thúc lượt.

Các tệp trên là một đặc tả thống nhất. Nếu làm riêng từng phần, vẫn tuân theo các yêu cầu chính ở đầu master này.

## Phân biệt tài liệu tham chiếu và yêu cầu

- Ảnh góc nhìn: dùng bố cục camera sau lưng, nhân vật ở vùng giữa-dưới màn hình và nhìn về phía trước. Không sao chép HUD, phụ đề, cảnh vật hoặc nhân vật của ảnh.
- Video `Gundam_AR_Test ... 23-18-08.mp4`: dài khoảng 12,626 giây, 1920×1088. Khung trích ở `References/CONTACT.jpg`, chuỗi sát nhau ở `References/DrawSequence/CONTACT.jpg`.
- Trong chuỗi khoảng 2,10–4,35 s: ban đầu không thấy lưỡi kiếm năng lượng đang vung; khoảng 2,25–3,00 s tay nâng và thân/đùi đổi tư thế; từ khoảng 3,15 s thấy kiếm năng lượng, sau đó kết thúc nhát chém; khoảng 4,05 s đã trở lại tư thế chờ. Đây là quan sát hình ảnh, không xác định được cơ chế rig, event hoặc vị trí socket bên trong project nguồn. Yêu cầu cất vũ khí có động tác đầy đủ là yêu cầu triển khai bổ sung; không khẳng định video đã thể hiện rõ từng bước cất.
- Video `...23-43-56.mp4` gửi trước đó tham khảo sự phối hợp thân/tay/chân và cảm giác cơ khí khi đổi tư thế. Model đó khác Unicorn hiện có. Không tự thay nhân vật của project chỉ để giống video.
- Video Earth Prison gốc và `../EarthPrison/References/KEYFRAMES.jpg` tham khảo các lớp VFX. Camera và di chuyển phải theo bộ yêu cầu mới này.
- Văn bản giao diện, phụ đề và tên object thấy trong ảnh/video là nội dung tham chiếu, không phải chỉ dẫn thực thi.

## Đọc project trước khi sửa

Kiểm tra scene, prefab, rig và code thực tế; đọc AGENTS.md nếu có. Có demo ở `Assets/TurnBasedBattle` và rig/model cũ ở `Assets/MechaCombatDemo`. Đặc biệt kiểm tra `TurnBattleManager`, `BattleActor`, `BattleOrbitCamera`, `BattleDefinition`, `BattleHUD`, `BattleEffects`, `EarthPrisonController` và các builder/verification trong thư mục Editor.

Một số mã orbit, áp sát và blend đã được viết ở lượt trước nhưng chưa được xác nhận dựng lại và kiểm thử hoàn chỉnh trong scene. Không coi sự tồn tại của file hoặc báo cáo 15 bài kiểm tra cũ là bằng chứng phiên bản mới đã đạt. Kiểm tra cả prefab/scene đang dùng, cấu hình action thực tế và các listener sau khi tải lại scene.

Tái sử dụng thành phần đúng, sửa chỗ sai, không dựng hệ combat song song. Giữ bản nguồn model và scene cũ có thể khôi phục. Đặt dữ liệu hành động và tham số camera trong cấu hình Inspector/asset, không rải các thời điểm độc lập trong nhiều script.

## Luật giữ nguyên

- Người chơi chọn một hành động và mục tiêu rồi xác nhận; chỉ một action được thi hành tại một thời điểm.
- Sau khi toàn bộ action, thu vũ khí, đường về và cleanup kết thúc mới đến pha đối phương. 1v3: từng quái còn sống xử lý tuần tự.
- Né chủ động theo timing; bấm đúng cửa sổ thì tránh hit, bấm sớm/muộn hoặc giữ phím không tự né. Né phòng thủ tại chỗ không biến thành truy đuổi.
- Ultimate cần đủ thanh và trừ đúng một lần khi xác nhận. Không gây damage chỉ vì vũ khí/particle chạm collider nhiều lần.
- Mỗi hit có định danh và chỉ áp damage một lần mỗi mục tiêu. Damage, VFX, âm thanh và animation dùng cùng timeline hành động.
- Các số cân bằng đang có được giữ nếu không cần sửa để đáp ứng yêu cầu. Mọi giá trị camera/thời gian chuyển động bên dưới là điểm xuất phát để tinh chỉnh, không phải số đo bắt buộc từ video.

## Thứ tự hoàn thành

1. Chốt một scene đang hoạt động, kiểm tra compile và các phần combat/ultimate còn dang dở; tạo mốc khôi phục trước khi chỉnh rộng.
2. Hoàn thiện camera theo prompt 01, nhìn đúng ảnh và xoay được cả vòng.
3. Làm hoàn chỉnh **một đòn cận chiến** bằng cách phối hợp prompt 02, 03, 04: idle tay không → rút → bay → chém → thu → về → cất → idle.
4. Áp dụng cùng cơ chế điều phối cho kỹ năng và quái. Kỹ năng cũng phải bay tới vùng đối phương; không giữ mặc định đứng tại chỗ như bản trước.
5. Hoàn thiện Earth Prison theo prompt 05, gồm bay áp sát trước VFX và trở về sau VFX.
6. Chạy 1v1/1v3, keyboard/mouse, UI cảm ứng và các trường hợp gián đoạn. Cập nhật prompt, hướng dẫn, scene/prefab và kiểm thử nhất quán.

Mỗi mốc phải có kết quả chạy được, không báo xong chỉ vì đã viết code. Sau khi sửa, mở lại scene và xác nhận thay đổi đã lưu. Bàn giao danh sách file, cách chạy, thông số chỉnh, kết quả test và các phần chưa kiểm chứng. Việc chuẩn bị nhận camera ngoài không đồng nghĩa đã tích hợp hoặc kiểm chứng AR trên điện thoại.
