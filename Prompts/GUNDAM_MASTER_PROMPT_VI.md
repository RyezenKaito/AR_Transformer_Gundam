> ĐẶC TẢ MỚI NHẤT — 29/10/2026
> Đọc [master mới](2026-10-29-Camera-Flight-Weapons/00_MASTER_PROMPT_VI.md) và các prompt 01–05 cùng thư mục. Camera mặc định sau lưng nhưng xoay 360°; Attack, Skill và Ultimate đều bay tới phía địch rồi trở về; khớp chuyển động mượt và vũ khí chỉ rút khi dùng.
> Phần bên dưới là tài liệu ngày trước được giữ để tham chiếu. Các ràng buộc camera fixed, root tuyệt đối đứng yên khi đánh, skill/ultimate đứng tại chỗ hoặc súng cầm sẵn đã được thay thế.

---

# MASTER PROMPT — GUNDAM ĐÁNH THEO LƯỢT, HAI PHÍA CỐ ĐỊNH, NÉ ĐÚNG NHỊP

Phiên bản thiết kế ngày 28/10/2026. Đây là đặc tả hiện hành, thay thế hướng combat thời gian thực trong các prompt trước. Earth Prison là ultimate của nhân vật. Người dùng đã chọn cơ chế bấm đúng thời điểm để né tại chỗ.

Hãy xây dựng trong project Unity của tôi một bản demo chiến đấu theo lượt: nhân vật ở bên trái màn hình, quái ở bên phải; hai phía đứng tại vị trí cố định, camera giữ một khung hình xuyên suốt trận. Tôi chọn đòn và mục tiêu trong lượt của mình; đòn thực hiện xong thì tới lượt quái. Trong lượt quái, tôi có thể bấm đúng thời điểm để né tại chỗ. Ultimate của nhân vật là Earth Prison: tụ điện, phóng sét, dựng sáu trụ đá thành lồng điện quanh phía quái, rồi tan rã.

Tôi cần hệ thống chạy được trong Unity, có animation/VFX thật và tài liệu chỉnh sửa. Hãy đọc project trước, tái sử dụng rig/model phù hợp, làm từng mốc có thể kiểm chứng và tiếp tục tới bản demo hoàn chỉnh. Các con số cân bằng dưới đây là đề xuất ban đầu, phải chỉnh được bằng dữ liệu.

## 1. Các yêu cầu đã chốt với người dùng

- Combat theo lượt, người chơi và quái thay phiên thực hiện hành động.
- Nhân vật/quái đứng ở hai phía cố định, không chạy qua lại để tiếp cận.
- Camera cố định, cả hai phía cùng ở trong khung hình kể cả lúc ultimate.
- Earth Prison là ultimate chính của nhân vật.
- Né chủ động bằng một lần bấm đúng thời điểm khi bị quái tấn công; animation né diễn ra tại chỗ.

Phạm vi demo đề xuất: 1 nhân vật người chơi, cấu hình thử 1 quái và cấu hình mở rộng tối đa 3 quái. Mặc định 1v1 để nghiệm thu luật xen kẽ trước. Khi 1v3, mỗi quái sống được một hành động trong pha quái, xử lý tuần tự. Chưa làm đội nhiều nhân vật người chơi, PvP hoặc multiplayer.

## 2. Hiện trạng project và tài liệu liên quan

- Project `E:/My project`; Unity 6000.5.8f1; URP 17.5.0; Input System 1.20.0; Timeline 1.8.12 theo cấu hình đọc ngày 28/10/2026.
- Demo trước ở `Assets/MechaCombatDemo/`, có rig, clip, mesh, vật liệu và prefab Gundam để kiểm tra/tái sử dụng.
- Code hiện có: `MechaPlayer` đọc input và chạy combat thời gian thực; `MechaEnemy` tự tiếp cận/đánh; `MechaCamera` orbit/follow/cinematic; các behavior này không làm bộ điều khiển cho trận theo lượt mới.
- Có `MechaHealth`, `MechaRigData`, `MechaEffects`, `MechaTuning`; kiểm tra phụ thuộc thật trước khi dùng lại.
- Không coi snapshot cũ nói model chưa rig là hiện trạng. Đọc rig/prefab/clip hiện có, tránh dựng skeleton thứ hai nếu rig đã đúng.
- Prompt ultimate: `E:/My project/Prompts/EarthPrison/EARTH_PRISON_MASTER_PROMPT_VI.md`.
- Ảnh video ultimate: `E:/My project/Prompts/EarthPrison/References/KEYFRAMES.jpg` và các bảng ảnh cùng thư mục.

Đặc tả combat theo lượt này quyết định luật trận. Prompt Earth Prison quyết định hình ảnh và timeline riêng của ultimate. Không ghép lại các yêu cầu WASD/boost/dash/hover hoặc R Celestial Breaker từ bản cũ. Trong scene mới, nút Ultimate và shortcut R dùng Earth Prison.

## 3. Bố trí sân và camera cố định

Tạo `PlayerAnchor` ở phía trái và `EnemyAnchor_0..2` ở phía phải. Trục bố trí ngang đề xuất là world X; nhân vật hướng vào phía đối diện. Vị trí là dữ liệu scene, không suy ra lại mỗi frame từ camera.

Khởi điểm với Gundam cao khoảng 2,6 m: người chơi x=-6 m, tâm đội quái x=+6 m, các quái phụ lệch chiều sâu z khoảng ±2,2 m và chỉnh nhẹ x nếu cần tránh che nhau. Một quái dùng anchor giữa. Căn lại dựa trên bounds thật và không gian lồng đá.

`BattleRoot` của mỗi actor giữ world position/rotation đã thiết lập cho slot trong suốt trận. Bone, tay, thân, vũ khí cử động bình thường. Animator root motion tắt. Né/trúng đòn tạo pose nghiêng, hạ trọng tâm hoặc giật thân; không thay đổi BattleRoot, không đổi slot hoặc lao sang bên kia.

Camera dùng orthographic hoặc perspective ít méo, một góc chéo nhẹ nhìn rõ mặt trước hai phía. Fit một lần khi setup dựa trên bounds lớn nhất của sân, actor và Earth Prison; sau đó khóa transform, FOV/orthographic size trong trận. Khi đổi độ phân giải/aspect ratio thì xử lý layout có kiểm soát, không chạy camera theo từng đòn.

Giữ lề cho đỉnh trụ, glow, VFX ở hai đầu và vùng UI. Kiểm tra 16:9, 21:9 và 4:3. Camera không orbit, follow, zoom, cắt cảnh, cinematic hoặc rung khi đánh/né/ultimate. Tạo lực bằng animation, flash cục bộ, SFX và nhịp VFX.

Giữa hai đội có khoảng trống cho projectile/tia năng lượng. Đòn cơ bản có thể là súng hoặc nhát kiếm phát ra sóng chém; người đánh đứng tại anchor. Không trình diễn kiếm chạm trực tiếp quái ở xa nếu chưa có sóng chém/hiệu ứng nối khoảng cách để giải thích hit.

## 4. Luật lượt và khái niệm round

`PlayerTurn` là một cơ hội chọn và thực hiện đúng một hành động. `EnemyPhase` là hàng đợi hành động của các quái sống. Một round gồm PlayerTurn rồi EnemyPhase.

Luồng mặc định:
`BattleStart → PlayerCommand → PlayerAction → ResolvePlayerAction → EnemyPhase → ResolveEnemyPhase → PlayerCommand`.

Với 1v1: tôi đánh → quái đánh → tôi đánh. Với 1v3: tôi hành động một lần → quái 0, quái 1, quái 2 còn sống lần lượt hành động → tôi hành động tiếp. Đây là lựa chọn phạm vi đề xuất, expose cấu hình đội hình; không cho tất cả quái chạy animation/damage đồng thời.

Quy tắc:
- Chỉ một action có quyền thực thi tại một thời điểm.
- Chọn mục tiêu và hủy chọn trước xác nhận không tốn lượt/tài nguyên.
- Xác nhận một action hợp lệ tiêu thụ lượt; double-click/hotkey lặp không tạo action thứ hai.
- Ultimate cũng tiêu thụ đúng một lượt và chỉ kích hoạt trong PlayerCommand khi đủ thanh ultimate.
- EnemyPhase chỉ bắt đầu sau khi action người chơi hoàn tất các marker, phản ứng và phần VFX cần thiết. Đặc biệt Earth Prison khóa tới hết khoảng 8 s và cleanup.
- Quái chết trước khi tới lượt bị bỏ qua; quái chết không được gây damage bằng callback trễ.
- Hết quái → thắng; người chơi hết HP → thua. Kết thúc trận là trạng thái cuối, không chuyển lượt thêm.
- Sau khi hoàn tất action, tái kiểm tra điều kiện thắng/thua trước khi mở pha tiếp theo.

## 5. Bộ lệnh và input

UI trong lượt người chơi có: `Tấn công`, `Kỹ năng`, `Ultimate — Earth Prison`, `Phòng thủ`. Người chơi chọn mục tiêu bằng UI/click vào quái; chọn tự động nếu chỉ có một quái.

Mapping đề xuất: 1 Tấn công, 2 Kỹ năng, R Ultimate, 3 Phòng thủ, Enter xác nhận; Escape hủy chọn khi đang chọn mục tiêu, còn nút Pause hoặc P dùng tạm dừng để không nhập nhằng. Space dùng né trong EnemyAction. Các binding chỉnh được qua Input System.

Trong EnemyAction, command panel bị khóa nhưng nút né được điều khiển riêng theo cue. Không cho Space vừa xác nhận menu vừa né. Không cho giữ phím/auto-repeat tính như lần nhấn mới. Khi focus UI, input battle vẫn theo state đã định; không để UI nuốt ngẫu nhiên lần né.

Không có WASD, nhảy, dash, chase, aim camera hoặc click để đi trên sân trong chế độ trận này.

## 6. Cơ chế né đúng thời điểm — quy tắc bắt buộc

Né là phản ứng trong lượt quái, không lấy mất lượt tấn công tiếp theo của người chơi. Thành công do timing, không do RNG hoặc collider thật né ra khỏi đòn.

Mỗi hit né được có `hitTime`, `cueStart`, `windowOpen`, `windowClose` và `reactionAttemptId`. Gợi ý cho đòn một hit:
- Đòn quái bắt đầu tại t=0, animation báo trước rõ.
- `cueStart=0,30 s`: UI vòng thu hẹp hoặc thanh chỉ báo xuất hiện.
- `windowOpen=0,95 s`; `windowClose=1,15 s`; `hitTime=1,20 s`.
- Bấm trong đoạn đóng [0,95;1,15] → `Dodged`, damage hit này bằng 0.
- Bấm ngoài cửa sổ nhưng trong khoảng cue tới hit → `Failed`, mất cơ hội cho hit đó.
- Không bấm → `Failed` tại hitTime.

Khoảng 50 ms từ cửa sổ kết thúc tới impact giúp bắt đầu pose né; có thể chỉnh đường animation/timing cho đọc tốt. Ở frame đi qua nhiều mốc, xử lý theo timestamp sự kiện, không chỉ thứ tự hàm Update.

Chống spam:
- Mỗi hit chỉ nhận **lần nhấn Space đầu tiên** từ cueStart tới hitTime; các lần sau không cứu lại kết quả.
- Nhấn trước cueStart không buffer sang cửa sổ; phím đang giữ từ trước phải nhả rồi nhấn lại.
- Vào từng cửa sổ mới cần cạnh nhấn mới. Không cho một lần bấm miễn mọi hit của combo.
- Không có perfect-dodge/RNG/giảm damage ngầm ở bản đầu; chỉ hai kết quả Dodge hoặc Hit. Mức dễ có thể mở cửa sổ rộng hơn qua config, không đổi luật bí mật.

Timestamp input được ánh xạ vào action clock có pause; không đếm frame. Đánh giá cửa sổ trên cùng clock với hit marker. Tạm dừng không tạo thời gian né miễn phí; nhấn lúc pause không được giữ lại để né khi resume. Debug hiển thị hitTime, inputTime, window và kết quả.

## 7. Animation và phản hồi né tại chỗ

Với Dodge thành công, actor nghiêng thân/đầu, xoay vai hoặc hạ người, chân vẫn ở slot; có thể thêm dư ảnh ngắn tại chỗ và tiếng gió. Bắt đầu pose khi chấp nhận input, giữ tư thế tránh đòn qua hitTime rồi blend về idle trong khoảng 0,25–0,40 s sau hit. Không phát một clip quá ngắn khiến actor đứng thẳng trước impact.

Quỹ đạo projectile/đòn phải biểu đạt kết quả: ở hitTime tia đi sượt qua hoặc flash miss phía sau, không nổ trúng ngay ngực trong khi UI ghi Né. Với beam/đòn vùng, diễn hoạt hạ thân/lách qua đường chính và hiệu ứng tránh phù hợp; không đẩy root khỏi vùng.

`Hit`: trừ HP đúng một lần, phát recoil và impact gắn vào actor. `Dodge`: 0 HP, không hit reaction, không blood/impact trúng người; hiển thị “NÉ!” ngắn gần nhân vật và HUD dễ đọc. Kết quả reaction điều khiển damage, animation và VFX cùng một nguồn.

Né thành công không tạo counterattack tự động, không ngắt lượt quái hoặc mở ngay menu. Đợi attack recovery xong rồi mới tiếp tục hàng đợi. Camera luôn đứng yên.

## 8. Quái, đa hit và độ công bằng

AI theo lượt chỉ chọn lệnh khi BattleManager cấp quyền; không phát hiện khoảng cách/chạy tới người chơi. Bản đầu có đòn một hit để dạy né. Đòn hai hit thử sau, mỗi hit có cue/window/attempt riêng và khoảng cách tối thiểu đề xuất 0,65 s để animation né/hit có thời gian phục hồi.

Nếu đòn là beam kéo dài, bản đầu resolve một hit logic; các tia phụ không tự biến thành nhiều damage tick. Nếu thiết kế multi-hit thật phải khai báo tất cả hit marker và các cue tương ứng.

Mọi đòn địch trong demo đều né được theo cue. Không tự thêm đòn chắc chắn trúng hoặc unblockable. Độ khó tăng bằng timing/nhịp/hình thức báo trước có cấu hình, tránh thay đổi hitTime sau khi cue đã bắt đầu.

Enemy không tự động RNG né ultimate của người chơi trong bản đầu. Có thể mở rộng một hệ AI defense về sau, nhưng phải là luật được thiết kế riêng chứ không dùng chung input người chơi cho quái.

## 9. Tài nguyên và thời gian hồi

Đề xuất đơn giản: HP, thanh Ultimate 0–100, cooldown Kỹ năng theo lượt; không dùng energy hồi mỗi giây, stamina chạy hoặc dash cooldown.

Thanh Ultimate gợi ý:
- Khởi đầu 0 trong trận thường; debug có nút Fill Ultimate riêng.
- Tấn công thường hoàn tất có hit hợp lệ: +25, chỉ một lần/action dù nhiều mục tiêu/đoạn animation.
- Kỹ năng thường hoàn tất có hit: +20, một lần/action.
- Trong mỗi EnemyAction, nếu người chơi né thành công ít nhất một hit: +15 một lần/action; nếu không né được hit nào nhưng thực sự nhận damage: +10 một lần/action. Các trường hợp khác +0. Không cộng chồng +15 và +10 trong cùng action.
- Phòng thủ tự nó không nạp; lượng nạp do kết quả bị đánh/né vẫn theo quy tắc trên.
- Earth Prison tiêu thụ 100 tại commit và không tự nạp lại từ các hit của chính ultimate.
- Clamp trong [0,100]; sự kiện action/hit có ID để callback lặp không nạp lại.

Ultimate đầy không cho phép dùng ngoài lượt. Không có cooldown theo giây cho Earth Prison; cần tích lại đủ thanh.

Kỹ năng thường có cooldown đề xuất 2 cơ hội PlayerCommand kế tiếp. Ghi bằng `readyAtPlayerTurnIndex`: dùng ở turn k → sẵn sàng ở k+3; k+1 và k+2 bị khóa. UI hiển thị đúng số cơ hội còn phải chờ. Cooldown không giảm trong lúc xem animation hay pause.

## 10. Các action khởi điểm và Phòng thủ

Thông số demo đề xuất, chỉnh bằng dữ liệu:
- Player HP=1000; quái thường HP=600.
- Tấn công: 100 damage một mục tiêu; animation khoảng 1,5 s; hitTime 0,8 s; +25 ultimate khi có hit.
- Kỹ năng: 180 damage một mục tiêu; khoảng 2,0 s; hitTime 1,1 s; cooldown theo mục 9.
- Phòng thủ: dùng một lượt, có pose phòng thủ; giảm 50% damage nhận trong EnemyPhase ngay kế tiếp. Người chơi vẫn có thể né; né thành công thì 0 damage, né thất bại mới áp giảm 50%.
- Đòn quái thường: 80 damage, khoảng 2,0 s; cue và hitTime theo mục 6.
- Earth Prison: 360 damage tối đa cho mỗi quái hợp lệ nếu còn sống qua đủ các mốc, theo mục 11.

Guard có owner/state và hết khi EnemyPhase kế tiếp kết thúc, kể cả khi một số quái bị bỏ qua; không tồn tại vô hạn khi queue trống. Reset/end battle dọn Guard. Không cho chọn nhiều lệnh để cộng dồn giảm damage trong một lượt.

## 11. Earth Prison — ultimate chính trong lượt người chơi

Đọc prompt Earth Prison chuyên biệt để dựng hình ảnh đầy đủ. Luật trận:
- Chọn Ultimate khi đang PlayerCommand, sống, đủ 100 và có ít nhất một quái sống.
- UI highlight toàn bộ quái sống bên phải vì đây là AoE; không cần click một điểm đất hoặc bắn bằng aim camera.
- Confirm → commit một lần: khóa command, chụp danh sách target ID, trừ 100, bắt đầu action khoảng 8 s.
- Player đứng tại PlayerAnchor và làm động tác tụ/phóng tại chỗ.
- Vùng sáu trụ đặt quanh EnemyFormationAnchor; scale theo bounds đội hình đã bố trí, không dịch quái vào lồng bằng teleport.
- Damage theo timeline: 120 tại 3,35 s; 60 tại 5,10 / 5,50 / 5,90 / 6,30 s. Chỉ target đã chốt, còn sống ở mỗi marker được nhận hit. Target chết không được nhận thêm hit/nạp tài nguyên.
- VisualReference trong lab không damage và không tham gia lượt; BattleUltimate dùng đúng BattleManager/DamageResolver.
- Không tự stun, bỏ lượt quái, kéo quái, nhấc quái bay, root di chuyển hoặc dựng collider chặn đường. “Prison” ở bản đầu là hình ảnh lồng và nhịp damage; các actor đã đứng cố định theo luật trận.
- Sau khi đủ hit, tan rã và actor hồi về pose chờ, gửi completion đúng một lần; BattleManager kiểm tra thắng/thua rồi mở EnemyPhase nếu còn quái.

Earth Prison thay vai trò ultimate Celestial Breaker trong **scene theo lượt mới**. Không gọi action R cũ, camera cinematic cũ hoặc các nhánh dash/root-lunge của MechaPlayer. Prefab và scene cũ có thể giữ để tham khảo; người dùng đang yêu cầu đổi thiết kế, không cần xóa tài nguyên gốc.

## 12. Damage và nguồn quyết định kết quả

`BattleManager/ActionRunner` quản lý action hợp lệ. `DamageResolver` quyết định damage. Animation và particle chỉ biểu diễn; OnParticleCollision/OnTriggerEnter không tự trừ HP.

Mỗi hit có khóa duy nhất `(battleId, actionId, hitIndex, targetId)`. Resolve tối đa một lần, kể cả Animation Event và timeline callback cùng được gọi. Source/target chết hoặc action token đã invalid thì callback không tác động.

Đòn quái: kiểm reaction result → nếu Dodged trả 0 → nếu không áp Guard → trừ HP → phát event/VFX tương ứng. Kiểm tra HP và chết ngay sau từng hit, dừng các hit sau vào người đã chết. Health chỉ có một nguồn lưu trữ, không tạo hai thanh HP khác nhau cho cùng actor.

Nếu tái dùng `MechaHealth.TakeDamage`, kiểm tra subscriptions từ behavior realtime và loại bỏ chúng khỏi prefab battle; tránh callback cũ tự cancel/cleanup animation mới. Không giả lập death bằng SetActive(false) khi HP còn.

## 13. Kiến trúc runtime gợi ý

- `TurnBattleManager`: state, turn index, enemy queue, victory/defeat.
- `BattleActor`: ID, phe, slot/anchor, HP, animation/socket, sống/chết.
- `BattleActionDefinition`: targeting, duration, hit markers, cue/window, damage, cooldown/ultimate cost và presentation.
- `BattleActionRunner`: action clock, marker dispatch, cancellation token, completion.
- `TimedDodgeController`: nhận input, ánh xạ timestamp, một attempt/hit và kết quả.
- `BattleDamageResolver`: kết quả damage/guard/dodge, chống gọi lặp.
- `BattlePresentation`: animation, projectile, recoil, dodge, death.
- `EarthPrisonController`: timeline VFX chuyên biệt, bridge marker tới action runner.
- `BattleHUD`: command, target, turn banner, HP, ultimate, dodge cue, pause/result.
- `FixedBattleCamera`: fit lúc setup và khóa khung hình.

Có thể gộp lớp nhỏ khi hợp lý. Dữ liệu action dùng ScriptableObject hoặc cấu trúc tương đương, không nhét toàn bộ vào Update lớn. Runtime không phụ thuộc UnityEditor; builder/validator ở thư mục Editor.

Chỉ một action clock có thẩm quyền. Khi tốc độ debug 0,5×/1×/2× thay đổi, timeline/animation/particle/projectile/cue phải cùng tiến độ. Không dùng deltaTime đã clamp để tính hit nhưng input dùng thời gian thực khác vì sẽ sai cửa sổ khi tụt FPS.

## 14. State, commit và phục hồi

State tối thiểu: BattleSetup, PlayerCommand, PlayerTargeting, PlayerAction, ResolveAction, EnemyAction, RoundTransition, Victory, Defeat; Paused là lớp đóng băng với state trước đó được ghi lại.

Phản ứng né là substate của EnemyAction, không mở một lượt mới. Không chuyển toàn battle sang PlayerCommand khi né xong.

Commit chỉ khi action, resource và target hợp lệ. Nếu target đã biến mất trước commit, trở lại chọn và không tiêu tốn. Sau commit, không cho user hủy để lấy lại tài nguyên. Nếu lỗi kỹ thuật giữa action, log và cleanup vào trạng thái recoverable xác định; không tự phát damage giả để ép tiến trình.

Death giữa đa hit: hủy các marker vào actor đó, phát death tại anchor, tiếp tục phần an toàn để resolve. Tất cả target chết trong ultimate vẫn cleanup VFX/đóng action rồi báo thắng; không cho quái phản công trong lúc ultimate còn đang xử lý.

Watchdog completion dựa trên duration và tail cấu hình để lỗi Animation Event không treo trận. Watchdog báo lỗi; không che việc thiếu asset bằng cách luôn timeout. Restart cấp battleId mới, invalid mọi callback từ trận trước.

## 15. HUD và hướng dẫn người chơi

Khung trái: HP/Ultimate nhân vật; phía phải: HP và highlight quái. Banner rõ “Lượt của bạn”, “Quái đang tấn công”, “Né!”, “Thắng/Thua”. Menu chỉ sáng khi chọn được; Ultimate chưa đầy có lý do rõ, skill hồi hiển thị số lượt.

Dodge cue nằm gần nhân vật hoặc vùng HUD cố định, không bị particle/bloom che. Vòng thu hẹp có vùng thành công đánh dấu cả hình dạng lẫn màu, kèm icon Space; không chỉ dùng màu xanh/đỏ. Hiển thị quá sớm/quá muộn khi thất bại để người chơi học timing.

Damage numbers chỉ hiện khi damage > 0. Né thành công hiện “NÉ”, không hiển thị số damage đỏ hoặc rung actor như trúng đòn. Cho tắt flash mạnh và chỉnh cửa sổ né ở chế độ dễ.

## 16. Chuyển project hiện có sang demo theo lượt

Tạo scene/prefab battle mới dưới `Assets/TurnBasedBattle/`, scene đề xuất `Scenes/TurnBattleDemo.unity`; Earth Prison có thể ở `Assets/TurnBasedBattle/Abilities/EarthPrison/`.

Tái sử dụng mesh/material/rig/clip phù hợp bằng reference hoặc prefab variant. Prefab battle không kích hoạt `MechaPlayer`, `MechaEnemy`, `MechaCamera`, `MechaPlaytest` realtime. Đừng chỉ đặt speed=0 rồi giữ logic attack/update/camera cũ hoạt động nền.

Kiểm tra `MechaEffects` có phần nào tự sửa Time.timeScale, camera shake hoặc cleanup toàn bộ; nếu tái dùng thì tách đúng phần hiệu ứng thuần. ActionRunner mới là nguồn quyết định thời gian và hit. CharacterController/NavMeshAgent/motor không điều khiển root trong scene battle.

Scene gốc vẫn để tham khảo; luồng chạy và README mới dẫn vào TurnBattleDemo. Không tự build lại toàn bộ demo realtime và ghi đè các clip đã chỉnh để làm trận theo lượt.

## 17. Asset/animation tối thiểu

IdleBattle, BasicAttack, SkillCast, EarthPrisonCharge/Release hoặc một clip cast đủ pha, DodgeInPlace, Guard, HitReaction, Death. Clip có thể tái biên tập từ clip sẵn có nếu phù hợp, nhưng phải kiểm tra trong camera battle cố định.

Attack/cast giữ chân chắc, có anticipation và follow-through. Dodge dùng khớp thân/chân hợp lý, không kéo giãn robot hoặc xuyên giáp. Death có thể gục tại slot; không bị motor cũ kéo xuống/xuyên sàn. Crossfade và local pose reset tránh trôi sau 20 lượt.

Vũ khí/sóng chém/beam phải nối rõ người đánh với phía bị đánh. VFX không che cue né, thanh HP hoặc cả hai actor cùng lúc. Ultimate ưu tiên giữ đúng sáu trụ và vòm xoáy từ video trong khung cố định.

## 18. Các mốc triển khai

Mốc A: audit, lấy rig/prefab phù hợp, sân hai phía và camera cố định; kiểm root/khung hình.

Mốc B: 1v1, command, tấn công thường, damage, enemy action, turn switch và win/lose. Chạy không VFX phức tạp trước.

Mốc C: cue né, timestamp, anti-spam, dodge pose và hit/miss; Guard, ultimate gauge và cooldown theo lượt.

Mốc D: tích hợp Earth Prison làm Ultimate từ command; marker damage, khóa action khoảng 8 s, sáu trụ trong khung, cleanup và chuyển lượt.

Mốc E: 1v3 hàng đợi tuần tự, target chết giữa pha/chiêu, HUD, pause/restart và profiling.

Mỗi mốc phải có kết quả chạy được và bằng chứng phù hợp. Không để tất cả gameplay tới sau khi làm đẹp VFX.

## 19. Kiểm thử bắt buộc

- 1v1 chạy 20 vòng, thứ tự luôn player→enemy; 1v3 mỗi quái sống chỉ hành động một lần/pha.
- Double click/giữ phím không tạo nhiều action hoặc trừ Ultimate hai lần.
- Đo BattleRoot và camera transform/lens trước-sau attack/dodge/ultimate; giữ trong sai số số học, không drift.
- Né đúng giữa cửa sổ, đúng biên, trước cửa sổ, sau cửa sổ, không bấm, giữ từ trước, spam; debug ghi kết quả/time rõ.
- Frame dài băng qua window và hit vẫn dựa timestamp; kiểm 30/60/120 FPS hoặc điều kiện thực đo được.
- Đòn hai hit: né hit 1 không miễn hit 2; mỗi cue có lần nhấn mới.
- Né thành công HP không giảm, không impact trúng người; né sai Guard giảm 50% đúng một lần.
- Earth Prison đúng 5 marker damage: 120 + 4×60 tối đa/target; không damage thêm từ particle và không tự nạp ultimate.
- Skill dùng turn k khóa ở k+1,k+2 và mở ở k+3; chờ thật lâu trong menu không giảm cooldown.
- Quái chết trước slot không đánh; player chết trong action thì không mở lượt mới; restart không nhận callback cũ.
- Pause giữa cue, ngay trước hit, giữa ulti, rồi resume; input pause không thành né hợp lệ.
- Camera thấy hai phía và toàn lồng trên các aspect ratio; cue không bị HUD/VFX che.
- Mở lại scene/prefab vẫn đủ reference; không material hồng, compile error hoặc behavior realtime chạy nền.

Kiểm thử logic độc lập cho turn queue, marker deduplication và dodge window boundaries là cần thiết; Play Mode kiểm animation/VFX/pause thật. Không gọi đã đạt chỉ vì không có lỗi Console.

## 20. Bàn giao và cách báo kết quả

Giao scene, prefab, action/config assets, animation, VFX/shader/material, code runtime/editor và README tiếng Việt. Có debug panel để chọn 1v1/1v3, reset trận, fill ultimate, bật log hit/dodge/time và cố định seed khi cần.

README chỉ rõ cách mở trận, chọn lệnh, né bằng Space, dùng Earth Prison bằng UI/R, chỉnh window, đổi damage/gauge/cooldown và vị trí anchor/camera.

Báo rõ phần đã tạo, đã compile, đã chạy và còn chưa kiểm chứng. Đo hiệu năng trên điều kiện cụ thể trước khi nói đạt 60 FPS; không dùng video nguồn làm bằng chứng kết quả mới.

Hãy bắt đầu bằng audit project và Mốc A, rồi tiếp tục theo các mốc tới trận theo lượt chơi được với Earth Prison và né đúng nhịp. Giải thích quyết định quan trọng bằng tiếng Việt, không dừng ở bản kế hoạch khi có thể thực hiện.
