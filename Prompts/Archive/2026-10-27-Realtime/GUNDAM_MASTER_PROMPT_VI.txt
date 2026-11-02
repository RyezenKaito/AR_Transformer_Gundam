Hãy trực tiếp hoàn thiện một bản demo chiến đấu Gundam trong Unity, bắt đầu từ dựng khung khớp đúng cho model chưa có skeleton, sau đó tạo animation di chuyển, chiến đấu, chiêu thức, VFX và camera theo đặc tả dưới đây.

# 1. Vai trò, mục tiêu và cách thực hiện

Bạn đảm nhiệm công việc của Unity Gameplay Engineer, Mechanical Rigging Artist, Technical Animator, Combat Designer và VFX Artist. Hãy kết hợp các chuyên môn để tạo một hệ thống chạy được trong project thật, có asset và cấu hình được lưu lại.

Tôi chưa biết rig và animation. Hãy đọc project trước khi hỏi, giải thích quyết định quan trọng bằng tiếng Việt dễ hiểu và thực hiện phần đã đủ dữ liệu. Khi có quyền điều khiển Unity, hãy sửa trực tiếp trên bản làm việc riêng, chạy thử, xem kết quả và sửa lỗi. Nếu chỉ trao đổi qua chat, cung cấp từng giai đoạn chạy được, code đầy đủ và hướng dẫn chính xác trong Editor.

Ưu tiên: rig đúng → pose đẹp → điều khiển phản hồi tốt → nhịp đòn đánh → chuyển tiếp → VFX/camera/âm thanh → tối ưu. Chất lượng phải được chứng minh bằng chuyển động thực tế, không bằng từ ngữ như AAA hay siêu mượt hoặc số lượng script.

Phạm vi: một Gundam người chơi, sân thử, dummy, AI đơn giản, camera góc nhìn thứ ba, UI máu/năng lượng/hồi chiêu, locomotion mặt đất/trên không, combo kiếm ba đòn, súng năng lượng, Q, E và một R hoàn chỉnh. Chưa làm multiplayer, nhiệm vụ, inventory hay thế giới mở.

Thực hiện tuần tự theo các mốc nghiệm thu. Không dừng ở kế hoạch nếu có thể triển khai. Tự quyết các chi tiết kỹ thuật nhỏ có thể kiểm chứng và hoàn tác. Chỉ hỏi khi thiếu dữ liệu quyết định, cần thay đổi phạm vi, cần thao tác ngoài khả năng công cụ hoặc công cụ/dịch vụ yêu cầu chấp thuận riêng.

Không tuyên bố đã tạo rig/clip/VFX hay chạy Play Mode nếu chưa thực hiện. Phân biệt: đã tạo, đã chạy thử, mới kiểm tra tĩnh, placeholder, chưa thể kiểm chứng. Không dùng dấu ba chấm hoặc TODO để bỏ qua code bắt buộc trong chức năng được tuyên bố hoàn thành.

# 2. Bối cảnh project đã được kiểm tra

Đây là snapshot đọc từ Unity Editor ngày 26/10/2026. Kiểm tra lại trước khi thay đổi vì project có thể đã cập nhật. Các dữ kiện giúp bắt đầu, không thay thế kiểm tra hiện trạng.

| Hạng mục | Kết quả quan sát |
| --- | --- |
| Unity | 6000.5.8f1 |
| Project | E:/My project |
| Scene | Assets/Scenes/SampleScene.unity |
| Pipeline đang dùng | Universal Render Pipeline |
| URP / Render Pipeline Core | 17.5.0 |
| Input System package | com.unity.inputsystem 1.20.0 |
| Root model trong scene | Unicorn Gundam |
| Prefab | Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab |
| Asset nguồn | Assets/Models/Unicorn Gundam/UNICORN_GUNDAM_OBJ.obj |
| Cấu trúc | 375 con trực tiếp dưới root, tổng 376 Transform |
| Geometry | 375 MeshRenderer; tổng vertex cộng qua MeshFilter: 218.438 |
| SkinnedMeshRenderer | 0 |
| Animator / Animation component | 0 / 0 trên model được kiểm tra |
| AnimationClip tìm trong Assets | 0 |
| Local position của mesh con | Cả 375 đều bằng zero tại thời điểm kiểm tra |
| Scale root trong scene | Xấp xỉ (0,0038; 0,0038; 0,0038), cần đọc độ chính xác đầy đủ |
| World bounds toàn model | Rộng khoảng 0,601 × cao 1,071 × sâu 0,377 đơn vị |
| Vị trí root | Xấp xỉ (-84,979; 0; 22,286) |
| Model Importer | animationType = Generic; importAnimation = true |
| Máy Editor báo cáo | i5-12500H; RTX 3060 Laptop GPU khoảng 6 GB VRAM; RAM khoảng 16 GB |

Tên mesh chứa thông tin cụm như `head_assy`, `shoulder_joint`, `shoulder_rotor`, `back_pack_assy`, `saber_hub`. Một số tên có `unicorn_mode` và `destroy_mode`; chưa xác minh là các biến thể cần bật/tắt hay chi tiết cùng tồn tại. Không tự ẩn/xóa dựa vào tên.

Ảnh nhiều góc cho thấy robot dạng người, giáp cứng, hai tay hạ gần thân, giáp vai/hông/chân lớn và backpack. Hướng mặt trước chính xác trong tọa độ import, tâm khớp, cấu trúc từng chi và vũ khí sử dụng vẫn phải xác minh.

Nhận định cần kiểm chứng: nhiều part rời có thể phù hợp rigid-part rig. Nhưng các Transform mesh đang chung local position; không được xem origin sẵn có là tâm khớp vai/khuỷu/gối. Bounding-box center cũng không tự động là tâm khớp.

Generic trên Importer không chứng minh đã có skeleton, Avatar hữu dụng hay clip. Đừng báo đã có rig chỉ dựa vào trường này.

Chưa xác nhận Active Input Handling, action asset, material/texture, forward axis, topology tại khớp, hệ thống camera, VFX Graph, Cinemachine, Animation Rigging và clip nguồn được cấp phép. Chỉ dùng những gì kiểm tra thấy thật; cài package tương thích khi có lý do, không nâng Unity hay đổi pipeline tùy tiện.

# 3. Định hướng hình ảnh và tham khảo

Thiết kế chuyển động mới phù hợp model. Game tham khảo là định hướng quan sát, không yêu cầu sao chép animation, shader hoặc trích xuất asset.

| Tham khảo | Yếu tố tôi muốn học | Áp dụng cho demo |
| --- | --- | --- |
| Mecha BREAK | Cơ động mặt đất/trên không, hướng lực đẩy dễ đọc | Boost/dash/hover, nozzle và vận tốc ăn khớp; phân biệt chạy với lướt phản lực |
| Honkai: Star Rail | Tư thế chủ đạo rõ, tích tụ rồi bùng nổ, bố cục điện ảnh | R có storyboard, key pose, shot list, khoảng yên trước impact và đoạn trở về gameplay |
| ARMORED CORE VI | Boost để tiếp cận/thoát giao tranh, nối cận chiến và hỏa lực | Q có giới hạn quãng lao, tăng/giảm lực; bắn và né kết nối locomotion |

Đây là yếu tố thiết kế tôi chọn học, không phải thông số hay phân tích frame chính thức. Nếu xem video, ghi URL và timestamp thực sự đã xem; không bịa mốc hoặc nói đã phân tích video khi chỉ đọc trang mô tả.

Nguồn: [Mecha BREAK trên trang nhà phát triển tại Steam](https://store.steampowered.com/app/2452280/Mecha_BREAK/), [trang trailer chính thức Honkai: Star Rail](https://hsr.hoyoverse.com/en-us/news/163127), [gameplay walkthrough ARMORED CORE VI từ Bandai Namco](https://en.bandainamcoent.eu/armored-core/news/armored-core-vi-fires-of-rubicon-extended-gameplay-preview-trailer-highlights).

Ngôn ngữ chuyển động: có khối lượng nhưng phản hồi nhanh; hông phát lực rồi ngực/vai/tay truyền lực; đầu tương đối ổn định; giáp cứng có khoảng hở cơ khí. Tránh nhún như cao su, tay vung tách khỏi thân và trượt toàn thân thay bước chân.

Palette khởi điểm: cyan #55DFFF, core gần trắng #E8FCFF, jet viền xanh #258BFF, impact kim loại trắng/vàng cam #FFB04D. R có thể thêm tím #A68BFF tiết chế. Nếu material Unicorn có màu phát sáng khác, điều chỉnh hài hòa và ghi quyết định.

# 4. Giai đoạn và điều kiện đi tiếp

| Giai đoạn | Đầu ra bắt buộc |
| --- | --- |
| 0 — Audit | Báo cáo, bản làm việc riêng, mapping ứng viên, quy ước scale/axes |
| 1 — Rig | Mesh theo đúng khớp, pivot/axes, socket, rest pose, bài thử và prefab đã lưu |
| 2 — Locomotion | Motor/collision, camera, idle/walk/run/boost/dash/start/stop/turn |
| 3 — Trên không | Jump/ascend/hover/fall/land, energy và chuyển tiếp |
| 4 — Combo | Ba clip thật, hit sweep, buffer/cancel, dummy xác nhận damage |
| 5 — Súng/Q/E | Từng chiêu chạy được, nối state machine, chặn đúng vật cản |
| 6 — Hit feel | VFX/SFX, pooling, hit stop, quality settings |
| 7 — R | Storyboard thực thi, hit thật, fallback sát tường, trả điều khiển |
| 8 — Hoàn thiện | AI, reset, test tình huống, profiling, hướng dẫn chỉnh sửa |

Rig chưa đạt thì sửa rig trước. Vẫn được chuẩn bị sân thử/dữ liệu/công cụ độc lập. Không làm VFX để che việc model chưa cử động đúng.

# 5. Audit và bảo toàn model gốc

Trước khi sửa:

- Ghi asset nguồn, định danh ổn định, hierarchy, active state, renderer enabled, material/submesh, vertex, scale và bounds.
- Lưu transform/parent/rest pose của từng part để so sánh/phục hồi.
- Kiểm tra shared mesh L/R, negative/non-uniform scale và mirror làm đảo dấu khớp.
- Tên chỉ hỗ trợ: `foot_pedal` trong cockpit không phải bàn chân; `shoulder_gun` không mặc nhiên thuộc upper arm.
- Phân loại part theo chức năng cơ khí/chế độ hiển thị; không xóa part chưa nhận diện.
- Kiểm tra material/ánh sáng nhiều góc; đừng dùng exposure/bloom làm mất giáp trắng.
- Ghi lỗi Console có từ trước riêng khỏi lỗi mới.

Tạo bản làm việc trong `Assets/MechaCombatDemo/`, prefab và scene mới. Giữ model nguồn cùng scene gốc. Không lưu skeleton bằng cách sửa trực tiếp OBJ import vì có thể mất khi reimport.

Tên đề xuất: `Unicorn_CombatRig.prefab`, `MechaRigValidation.unity`, `MechaCombatTest.unity`. Nếu đã tồn tại, kiểm tra và tiếp tục bản đúng; không sinh hàng loạt Final2/Final3.

Quy ước: 1 Unity unit ≈ 1 m, robot demo cao khoảng 2,6 m. Đo geometry và lưu hệ số scale. Nguồn đang cao khoảng 1,071 đơn vị, không mặc định cao 18–20 m. Đưa bản thử về sân ở gốc tọa độ, chân chạm đất. Root motor scale (1,1,1); tầng bù scale hình ảnh được quy định rõ và không làm sai chiều dài IK.

Nếu giữ tỷ lệ khổng lồ, đổi đồng bộ tốc độ/gia tốc, trọng lực, collider, tầm đánh, camera, vật cản và VFX. Đừng chỉ phóng to model.

# 6. Dựng khung xương/khớp thật

## 6.1. Phương án theo hình học

Ưu tiên kiểm chứng rigid-part rig cho 375 mesh:

1. Gom part thật sự cùng một cụm cứng.
2. Dựng joint Transform tại tâm cơ khí.
3. Gắn cụm vào đúng joint, lưu offset bù.
4. Xoay joint: cả cụm đi đúng, part không liên quan không bị kéo theo.
5. Biên tập clip trên skeleton ổn định; không key ngẫu nhiên 375 mesh.

Nếu một mesh chứa nhiều cụm cần độc lập: kiểm tra đảo hình học rời có thể tách; material/submesh không mặc nhiên là tay/chân. Khi tách, giữ UV, normals, tangents, material và hình dạng. Nếu geometry liền cần biến dạng, dùng skeleton + skin weights thật, có thể cần Blender/DCC sửa topology. Không gán vertex vào bone gần nhất rồi gọi hoàn thiện; chỉ dùng làm nháp có kiểm chứng.

Một bộ GameObject rỗng chưa gắn mesh đúng chưa phải rig đạt. Xoay/di chuyển toàn root chưa phải animation cơ thể.

## 6.2. Mapping chỉnh được

Tạo `MechaRigDefinition` hoặc tương đương trong Inspector:

| Trường | Nội dung |
| --- | --- |
| SourcePath / SourceId | Định danh part ổn định, không nhầm tên trùng |
| MechanicalGroup | Head, Chest, UpperArm_L, LowerArm_L, Hand_L, Pelvis, UpperLeg_L và các nhóm tương ứng |
| DriverJoint | Joint thực sự điều khiển |
| RestTransform / Parent | Dữ liệu phục hồi trước rig |
| JointCenter | Tâm khớp và hệ tọa độ |
| FlexionAxis / Sign | Trục gập/dấu quay đã thử |
| TwistAxis | Nếu cơ cấu hỗ trợ |
| AngularLimits | Giới hạn đã kiểm chứng |
| Mode / Visibility | Chế độ hiển thị nếu có |
| Confidence / Status | Verified, cần xem lại, chưa phân loại; lý do |

Tự động nhận tên/vị trí được dùng để tìm ứng viên, sau đó preview tô màu nhóm và xem nhiều góc. Không suy L/R bằng world X trước khi biết hướng model. Mapping phải sửa được mà không đổi code.

Kiểm kê đủ 375 mesh: mỗi part hiện hình thuộc một driver đúng hoặc nhóm chưa giải quyết có báo cáo. Không nhét mọi part chưa biết vào pelvis để báo 100%.

## 6.3. Hierarchy đề xuất

Đây là tên cho rig mới, không phải khẳng định model đã có bone. Bỏ node không có chức năng thật; thêm cơ cấu phụ sau khi kiểm tra.

```text
PlayerRoot                         motor/collision/hướng gameplay
├── CameraTarget                   ổn định, không nhún theo pelvis
├── GameplayReferences             targeting/UI
└── VisualRoot                     Animator/RigBuilder, binding root và scale thống nhất
    ├── SkeletonRoot               nhánh skeleton cơ khí
    │   └── Pelvis
    │       ├── Spine
    │       │   └── Chest
    │       │       ├── Neck
    │       │       │   └── Head
    │       │       ├── Shoulder_L
    │       │       │   └── UpperArm_L
    │       │       │       └── LowerArm_L
    │       │       │           └── Hand_L
    │       │       ├── Shoulder_R
    │       │       │   └── UpperArm_R
    │       │       │       └── LowerArm_R
    │       │       │           └── Hand_R
    │       │       └── Backpack
    │       ├── UpperLeg_L
    │       │   └── LowerLeg_L
    │       │       └── Foot_L
    │       │           └── Toe_L  nếu độc lập thật
    │       └── UpperLeg_R
    │           └── LowerLeg_R
    │               └── Foot_R
    │                   └── Toe_R  nếu độc lập thật
    └── RigControls                IK target/hint không tạo vòng lặp
```

Khi dùng Animation Rigging, đặt Animator và RigBuilder cùng trên VisualRoot; SkeletonRoot và RigControls đều nằm bên dưới VisualRoot. Clip binding và mask paths tính tương đối từ VisualRoot, ví dụ `SkeletonRoot/Pelvis/Spine/Chest`. Giữ vị trí Animator/binding root ổn định sau khi tạo clip; đổi hierarchy phải cập nhật binding và kiểm tra lại.

Rigid geometry đặt dưới driver qua offset cần thiết; không để thêm duplicate trong Geometry. Skinning có thể giữ geometry riêng và tham chiếu bones. Chọn cách rõ ràng.

Giáp vai/hông trước-hai bên-sau, nắp gối, nozzle/cánh/backpack có node riêng nếu thực sự cần. Không tạo 375 bone vì có 375 mesh; không tạo finger/toe/twist giả khi geometry không dùng.

## 6.4. Pivot và local axes cho OBJ hiện tại

- Mesh đang có origin chung: dựng joint mới, không dùng origin mesh làm khớp.
- Xác minh vai/khuỷu/hông/gối/cổ chân bằng cơ cấu hình học; không đặt mọi joint tại tâm bounds hoặc trung điểm mesh.
- Giữ world pose khi reparent rồi lưu offset; so sánh ma trận hoặc vertex mẫu để phát hiện model bật vị trí.
- Nếu prefab instance hạn chế reparent, xử lý trên working clone/prefab, không phá nguồn.
- Root mới: +Z tiến, +Y lên, +X phải. Bone local axes có quy ước và mapping riêng; không giả định X luôn là flexion.
- Gizmo hiển thị tâm/trục/hướng dương, thử cả L/R để bắt mirror.
- Lưu rest local position/rotation/scale; offset procedural tính từ pose được quy định, không cộng dồn mỗi frame.
- Quaternion/curve không chọn vòng xoay dài qua biên ±180°.

Phân biệt rest pose, bind pose nếu skin, calibration pose nếu Humanoid. Không reset rotation về zero sau skin, bake scale thiếu bù hoặc đổi parent làm sai bind pose.

## 6.5. Generic, Humanoid và skinning

Ưu tiên hierarchy cơ khí + transform clips riêng, Animator và binding path ổn định. Generic thường phù hợp. Không ép Humanoid chỉ vì robot có hai tay/hai chân.

Nếu retarget clip người, xác minh skeleton/mapping/tỷ lệ/calibration/Avatar rồi sửa contact và khoảng hở giáp. Chọn Humanoid không tự sinh skeleton/weights; Avatar ánh xạ xương đã có. Xem [Unity Humanoid import](https://docs.unity3d.com/6000.0/Documentation/Manual/ConfiguringtheAvatar.html) và [Unity Generic animation](https://docs.unity3d.com/6000.0/Documentation/Manual/GenericAnimations.html).

Rigid-transform prefab không bắt buộc có SkinnedMeshRenderer hoặc Humanoid Avatar; tiêu chí là mesh theo joint đúng, clip chạy và kiến trúc phù hợp. Nếu giải pháp chọn cần Generic Avatar, cấu hình/kiểm chứng đúng; không báo missing Avatar là lỗi khi kiến trúc không cần nó.

Nếu skin: giáp cứng ưu tiên 100% weight vào driver đúng; ống mềm/vùng cần uốn mới blend. Kiểm tra weight, bone index, bind poses, bounds/culling ở pose vươn xa. Không làm tấm giáp cao su hóa.

## 6.6. Giới hạn cơ khí

Số sau chỉ là dải thử, phải thu hẹp theo model:

| Joint | Dải thử ban đầu |
| --- | --- |
| Khuỷu | Gập khoảng 110–125° từ neutral, không gập ngược |
| Gối | Khoảng 90–110° nếu giáp cho phép |
| Cổ chân | Pitch 15–25°, roll 8–12° |
| Chest so pelvis | Yaw ±20–30°, chia xoay toàn thân |
| Head/neck | Yaw ±35–50°, pitch ±15–20° |
| Vai/hông | Khảo sát đa trục, không áp một số trước khi xem geometry |

Ưu tiên sửa key pose tránh xuyên giáp. Piston có hai điểm gắn và cơ cấu riêng; không scale toàn chi để giả piston.

## 6.7. Socket và evaluation order

Socket thật có gizmo, orientation và reference: kiếm tay chính, grip phụ nếu cần, súng/emitter, muzzle, blade base/mid/tip, charge origin, từng nozzle, foot contact L/R, camera target và target center.

Cán kiếm nằm trong tay, muzzle forward trùng hướng bắn, nozzle phun ngược hướng lực đẩy. Không bắn từ origin model hoặc giữa ngực khi thiết kế yêu cầu nòng súng.

Thứ tự: clip/blend → layer/additive → aim/IK được cấp quyền → cơ cấu phụ theo pose cuối → đọc socket cuối cho hiển thị/hit. Xác định callback/player-loop cụ thể; không viết chung LateUpdate rồi để nhiều hệ thống tranh bone.

Animation Rigging/IK hỗ trợ chỉnh pose khi có cấu trúc khớp; không thay thế dựng joint/gắn geometry. Kiểm tra package tương thích trước khi dùng. [Unity Animation Rigging](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.4/manual/index.html).

Tay phụ bám grip theo một chuỗi duy nhất; không tạo vòng vũ khí theo tay trong khi tay theo vũ khí. Foot IK có contact weight, reach limit, pelvis adjustment; giảm/tắt khi swing chân, bay hoặc action không bám đất.

## 6.8. Bài thử rig trước animation chiến đấu

Tạo preview/clip kiểm từng joint và Reset Pose. Thử neutral → pose → neutral ít nhất 20 lần để bắt drift.

1. Đầu nhìn L/R/lên/xuống.
2. Vai nâng trước/dang/kéo sau, quan sát giáp-ngực.
3. Khuỷu gập, wrist xoay, tay giữ cán kiếm.
4. Hông nâng, gối gập, cổ chân và bàn chân tiếp sàn.
5. Squat nhẹ, bước dài vừa phải, recoil brace, hover.
6. Kiếm sau vai, chém chéo/ngang, giơ cao không vướng sừng.
7. Pelvis/chest xoay độc lập, không kéo sai nhóm.
8. Root xoay 360°/đổi vị trí, socket không trôi.
9. Lưu prefab/mở lại scene; kiểm clip bindings và round-trip nếu có DCC.
10. Tô màu nhóm để kiểm kê part hiện hình.

Đạt khi không part bay/nổ, gập ngược, giáp mềm, geometry nhân đôi, drift, socket sai hoặc binding mất sau reload. Lưu pass/fail và ảnh front/side/three-quarter. Chỉ chuyển sang combat khi lỗi cấu trúc quan trọng đã sửa.

# 7. Quyền Transform và kiến trúc runtime

Mặc định CharacterController điều khiển root, clip tại chỗ và displacement theo dữ liệu được motor kiểm tra collision. Không có Rigidbody động cùng kéo root.

| Hệ thống | Quyền sở hữu |
| --- | --- |
| Motor | PlayerRoot position, gravity/ground/collision, quãng dash/lunge |
| State/action | Permission, rotation policy, input/cancel, action clock |
| Animator | Skeleton pose và blend |
| Procedural/IK | Offset được cấp quyền, không tích lũy, weight/order rõ |
| CameraDirector | Camera/FOV/shot; không di chuyển player |
| VFX/SFX | Theo event, không tự quyết damage |

Nếu dùng root motion, một adapter gửi displacement qua motor; tắt displacement trùng từ dữ liệu. Ghi lựa chọn từng clip. Không bật root motion tùy tiện để che trượt.

Trách nhiệm cần có, gộp thành phần nhỏ khi hợp lý: InputReader/buffer; MechaMotor; StateMachine/ActionRunner; AnimationDriver/RigDefinition; CombatController/WeaponHitDetector; AbilityController/energy/cooldown; TargetingSystem; Health/DamageReceiver; VFXController/AudioController; CameraDirector; TimeEffectCoordinator nếu cần; EnemyController; UI/reset.

Đòn/chiêu dùng dữ liệu chỉnh được: clip/state, duration/phase markers, damage windows, hit shape/mask, damage/stagger, motion curve, cost/cooldown, queue/cancel, turn limit, VFX/SFX/camera, cleanup. Dùng giây/mét/độ; không hardcode toàn bộ trong một script.

Tách runtime/Editor; build không tham chiếu UnityEditor. Dùng API Unity 6000.5.8f1, biên dịch thật; API obsolete dạng error phải được thay đúng, không bỏ qua hoặc ép cast quay lại API đã bị loại.

Editor tool lưu `.anim`, `.controller`, `.asset`, `.prefab`, scene/material thật. Chạy lại không nhân đôi; có preview/báo cáo và chỉ cập nhật asset được quản lý. Không ghi đè clip người dùng đã chỉnh mà không xử lý xung đột rõ.

# 8. Điều khiển và thông số ban đầu

Giữ input hiện có nếu hoạt động tốt. Input System package đã có nhưng phải kiểm action asset/Active Input Handling. Không đọc cả input cũ/mới khiến một lần nhấn kích hoạt hai action.

| Input | Hành động |
| --- | --- |
| WASD | Mặc định run 6 m/s theo camera; lock-on chuyển strafe |
| Mouse | Xoay camera |
| Alt giữ | Walk 3 m/s; thả Alt về run. Boost hợp lệ ưu tiên hơn walk |
| Shift giữ | Boost |
| Ctrl nhấn | Dash theo input; input zero dùng hướng được quy định |
| Space nhấn | Jump khi grounded |
| Space giữ trên không | Hover/hỗ trợ lực nâng, tiêu energy |
| C giữ trên không | Hạ độ cao có kiểm soát |
| LMB nhấn | Combo kiếm |
| RMB giữ | Bắn theo fire interval, không theo mỗi rendered frame |
| MMB nhấn | Lock/unlock |
| Q / E / R nhấn | Vector Slash / Charged Beam / Celestial Breaker |
| Esc | Pause/resume và cursor đúng trạng thái |

Bản đầu: kiếm tay phải, súng/emitter bên trái nếu geometry cho phép. Kiểm model rồi chọn cách gắn. Nếu chưa có súng, tạo weapon proxy đơn giản có nhãn. Nếu dùng saber trên backpack, kiểm cán thật có thể tách/gắn; chưa có animation rút kiếm thì bắt đầu đã trang bị và ghi rõ. Không tuyên bố nguồn có vũ khí chưa kiểm thấy.

| Thông số cho robot thử cao 2,6 m | Giá trị khởi điểm |
| --- | --- |
| Walk / run / boost | 3 / 6 / 12 m/s |
| Ground acceleration / braking | 20 / 25 m/s² |
| Gravity / jump height | Khoảng -22 m/s² / 2 m; tính vận tốc rời đất tương ứng |
| Air acceleration | 8–12 m/s² |
| Ground dash | Tối đa 4,6 m trong 0,22 s khi không bị chặn |
| Air dash | 1 lần/lần rời đất; reset sau grounded xác nhận |
| Energy max / regen | 100 / 18 mỗi giây, sau trễ 0,8 s |
| Boost drain / hover drain | 14 / 18 mỗi giây; nếu cùng hoạt động, khai báo chi phí kết hợp |
| Dash cost / cooldown | 12 / 0,65 s |
| Player HP / dummy HP | 1.000 / 5.000 |
| Input buffer chưa tiếp nhận | 0,15 s |
| Slope / step | Khởi điểm 45° / 0,25 m, chỉnh theo collider |

Tinh chỉnh theo clip thật. Energy không âm, không hồi bù sai sau pause. Khóa regen trong toàn bộ dash/Q/E/R và khi boost/hover đang tiêu năng lượng; khi hành động nguồn tiêu hao cuối cùng kết thúc hoặc bị ngắt mới bắt đầu đếm trễ 0,8 s, sau đó hồi 18/s. Basic combo và bắn thường không tự khóa regen trong thiết kế này. Không chỉ kiểm thời điểm trừ cost một lần rồi hồi ngay giữa cinematic. Chống hover vô hạn bằng spam input. Cạn energy thì ngừng boost/hover và rơi có kiểm soát. Dash chưa có i-frame trừ khi thiết kế rõ. Jump có chuẩn bị ngắn theo clip; jump buffer/coyote khoảng 0,10 s nếu hữu ích, không nhảy lặp vì giữ Space.

# 9. Clip thật và Animator

Mỗi clip có clip sheet: tên/duration/loop; entry/exit pose; key times; foot contacts; đường hông/ngực/vũ khí; nguồn root motion; VFX markers; damage markers liên quan; trạng thái vào/ra; blend và IK weight.

Workflow: blocking key pose → sửa silhouette/contact → timing/spacing → spline → kiểm overshoot/clipping → secondary motion. Không xuất curve sin/cos rồi gọi combat hoàn thiện. Clip tạo bằng Editor code vẫn phải có key pose có chủ đích và xem lại trong Editor.

| Nhóm | Clip/thời lượng khởi điểm |
| --- | --- |
| Idle | CombatIdle 2,4 s loop, biến thiên nhỏ |
| Ground | Walk F/B/L/R 0,8–1,0 s; Run F và hướng lock-on cần thiết 0,58–0,72 s |
| Start/stop/turn | Start 0,18 s; Stop 0,25 s; Turn180 L/R khoảng 0,35 s |
| Boost | Start 0,16 s; Loop khoảng 1 s; Stop 0,25 s |
| Dash | F/B/L/R 0,22 s |
| Air | JumpStart 0,18 s; Ascend; Hover 1,2 s loop; Fall; LandSoft 0,28 s; LandHard 0,55 s |
| Melee | DiagonalEntry 0,55 s; HorizontalReturn 0,65 s; OverheadBreak 0,90 s |
| Gun | AimIdle; Fire/Recoil khoảng 0,20 s trên upper body |
| Q / E / R | VectorSlash 0,95 s; ChargedBeam 1,25 s; CelestialBreaker 4,00 s hoặc chuỗi clip tương đương |
| Reaction | HitFront/Back/L/R, Stagger, Death, reset pose |

Generic rig dùng mask transform paths đúng hierarchy, không mặc định Humanoid IK/body mask hoạt động. Base layer có ground/air/full-body actions; upper-body Aim/Fire hoạt động khi được cấp quyền. Khi chém/Q/E/R, fade upper layer để tránh hai pose tranh tay.

Tham số dự kiến: MoveX, MoveY, Speed, VerticalSpeed, Grounded, Boosting, LockedOn, ActionId, AttackIndex, AimWeight. Có thể giản lược nhưng thống nhất code/controller, không tạo parameter vô dụng.

Crossfade khởi điểm locomotion 0,10–0,18 s, action 0,05–0,10 s. Không Play/CrossFade lại cùng state mỗi frame; không Any State làm đòn tự restart; không dùng exit time thay mọi action logic. Loop khớp pose/vận tốc đầu-cuối. Nối combo phải xem cả đường kiếm trong blend, không chỉ pose riêng.

# 10. Locomotion chi tiết

## 10.1. Walk và run

Ví dụ walk loop 0,90 s, motor quyết root, clip tại chỗ:

| Mốc | Body mechanics |
| --- | --- |
| 0,00 | Chân trái contact trước, phải sau; bắt đầu chuyển tải |
| 0,10–0,23 | Trái stance, phải rời đất; pelvis đi qua chân trái |
| Khoảng 0,23 | Passing pose: phải qua cạnh trái, đủ khoảng hở sàn/giáp |
| 0,45 | Phải contact trước, đổi chân chịu lực |
| 0,55–0,68 | Phải stance, trái swing |
| 0,90 | Khớp pose/vận tốc đầu loop, không pop hoặc đổi chân |

Pelvis lên/xuống khoảng 2–4 cm ở tỷ lệ thử, giảm nếu giáp vướng. Chest counter-rotation nhỏ, đầu ổn định. Chân nâng đủ không quệt sàn; tay giữ vũ khí không xuyên hông. Đây là starting pose guide, không áp cứng khi geometry chưa đo.

Run 6 m/s phải đo stride và cycle. Motor speed, stride và clip speed khớp nhau; giới hạn multiplier đã kiểm chứng, không kéo walk lên nhiều lần. Strafe/backward có clip/pose riêng; lock-on không chỉ xoay toàn robot ngược 180°.

Stance foot ít trượt trên nền phẳng. Điều chỉnh clip speed và foot IK theo contact curve; IK không sửa được stride sai nghiêm trọng. Chân L/R phải đồng pha giữa các hướng trong blend tree để không đổi chân chạm đất đột ngột.

## 10.2. Start, stop và turn

Start 0,18 s: 0–0,06 nghiêng/hạ nhẹ; 0,06–0,14 chân chống đẩy; 0,14–0,18 vào đúng locomotion phase. Input phản hồi ngay, không bắt đứng chờ dài.

Stop 0,25 s: chân chống phía trước trọng tâm, gối/hông nén, ngực trễ rồi ổn định. Theo vận tốc thật; đã bị tường chặn thì không tiếp tục bước lớn.

Turn180 phân biệt đứng/chạy: giảm tốc, chuyển chân chịu lực, pelvis/chest xoay có thứ tự. Không quay capsule tức thì trong khi cả hai chân khóa sàn. Steering/turn clip phải cho input mới cập nhật hợp lý.

## 10.3. Boost và dash

Boost 12 m/s mặc định lướt phản lực thấp, không dùng walk cycle. Thân nghiêng trước khoảng 8–15° nếu rig cho phép; chân thu/kéo sau vừa phải, đầu ổn định. Bụi theo sàn thật, bay thì không bụi.

Dash 0,22 s: 0–0,04 dồn lực; 0,04–0,17 motion chính; 0,17–0,22 giảm lực và exit pose. Motion curve có tổng quãng mục tiêu 4,6 m. Collision rút quãng thật; VFX theo quãng/vận tốc thật. Không xuyên vật cản mỏng ở FPS thấp.

Dash L/R có lean/nozzle đúng; dash lùi không quay lưng khi lock-on. Vũ khí/ngắm không xuyên thân. Khi bị chặn, motor dừng và clip/VFX không kéo root vượt tường.

# 11. Nhảy, hover, rơi và hạ cánh

State chain: JumpStart → Ascend → Hover hoặc Fall → Land → Ground. Ground probe có normal/distance/slope policy/hysteresis; chạm một collider không đồng nghĩa đứng trên đất.

- JumpStart 0,18 s: 0–0,10 nén gối/hông; 0,10–0,18 đạp và bật jet. Chốt marker rời đất khoảng 0,12–0,16 s sau khi xem pose, motor/clip đồng bộ.
- Ascend: chân thu vừa phải, vũ khí cân bằng, vai đúng khớp; motor quyết vertical velocity.
- Hover loop 1,2 s: chân co, thân ổn định, dao động rất nhỏ, jet biểu thị lực nâng; không bob như bong bóng.
- Thả hover/hết energy: blend Fall khoảng 0,10–0,15 s, không pop đầu gối. C hạ độ cao theo motor.
- LandSoft 0,28 s: contact → gối nén → pelvis hạ → hồi, không khóa điều khiển lâu.
- LandHard 0,55 s: nén sâu hơn trong giới hạn, ngực trễ/hồi chậm, không xuyên giáp hông/đùi.

Chọn land theo vận tốc trước contact. VFX/SFX đúng contact/normal sàn, cường độ theo lực đáp. Reset air dash/foot IK/energy drain khi grounded xác nhận. Không kích Land mỗi frame. Thử trần thấp, mép vực, dốc, hover cạn, giữ Space qua landing và bị hit lúc đáp.

# 12. Action clock, input buffer và hit detection

Có một nguồn gameplay timing. Mặc định ActionRunner đọc dữ liệu phase/window và đồng bộ clip. VFX/event không tự tạo damage thứ hai. Thay clip speed phải quy đổi timing hoặc dùng cùng action clock; không để hit window và pose lệch.

Buffer 0,15 s dành cho lệnh chưa được nhận. Input được nhận trong queue window trở thành lệnh nối đã xếp hàng, giữ tới link marker; không hết hạn giữa recovery. Mỗi đòn tối đa một lệnh nối. Lệnh trong pause/death/action không hợp lệ không tự kích khi mở khóa.

Xử lý đoạn thời gian đã đi qua, không chỉ điểm cuối frame. Một frame băng qua trọn damage window vẫn phải xử lý hit hợp lệ theo pose/motion tương ứng. Callback idempotent khi event lặp.

Mỗi activation có ActionInstanceId; mỗi damage window có WindowId. Dedupe theo DamageReceiver/nhân vật, không collider. Một người nhiều collider nhận một hit/window, trừ multi-hit được khai báo.

Kiếm có base/mid/tip hoặc mẫu dọc lưỡi; sweep giữa pose trước-hiện tại. Quay nhanh cần chia mẫu theo góc/khoảng cách, không chỉ nối endpoint bằng đường thẳng rồi bỏ sót cung. Nếu sample clip giữa frame, chọn cách không làm hỏng Animator/IK hiện hành và chứng minh ở 30 FPS. Nếu dùng authored hit volume thay mô phỏng lưỡi đầy đủ, nêu shape và khớp hình ảnh.

Có layer/owner/ally mask; phân biệt hurtbox và obstacle. Không đánh xuyên tường do chỉ overlap cuối cung. Hit feedback mạnh chỉ khi damage xác nhận. Miss có whoosh/trail, không có impact/hit stop/số damage giả. Cancel/hit/death đóng window cũ, không damage tới trễ từ coroutine.

# 13. Combo kiếm năng lượng ba đòn

Kiếm phải, chân trái trước; đổi bên qua mapping nếu cần, không đảo Euler tùy tiện. Mỗi đòn đẹp khi tắt VFX và nhìn từ gameplay camera.

| Đòn | Duration | Damage window | Damage | Tiến max | Queue window | Link marker | Dash cancel từ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 Diagonal Entry | 0,55 s | 0,12–0,23 | 100 | 0,45 m | 0,18–0,40 | 0,32 | 0,25 |
| 2 Horizontal Return | 0,65 s | 0,16–0,30 | 110 | 0,35 m | 0,24–0,48 | 0,39 | 0,32 |
| 3 Overhead Break | 0,90 s | 0,28–0,43 | 180 | 0,65 m | Không nối thêm | Không có | 0,58 |

Input tới sau link marker nhưng còn queue window thì chuyển ở thời điểm hợp lệ kế tiếp với blend đã kiểm chứng; không quay ngược clock. Giữ link pose đòn 1 trong 0,32–0,40 và đòn 2 trong 0,39–0,48; chỉ bắt đầu hồi ready sau khi queue window đóng nếu chưa nối. Bắt đầu đòn mới rồi không quay lại recovery cũ.

## 13.1. Đòn 1: chém chéo mở đầu

| Thời điểm | Key pose/chuyển động |
| --- | --- |
| 0,00 | Ready, trái trước, kiếm phải, tải hơi ở chân sau |
| 0,06 | Hông lấy lực, chest trễ, kiếm sau vai phải, không vướng backpack |
| 0,12 | Hông dẫn, ngực/tay theo; cung trên phải → dưới trái |
| 0,18 | Kiếm qua vùng trước thân, chân trái nhận tải, đầu giữ target |
| 0,23 | Damage đóng, kiếm tiếp tục follow-through |
| 0,32–0,40 | Giữ link pose kiếm bên trái, tương thích entry đòn 2 cho cả input tới muộn |
| 0,55 | Hồi ready nếu không nối, không snap kiếm qua ngực |

Trail khoảng 0,10–0,27 s; hit stop 0,035 s nếu trúng. Whoosh theo tốc độ kiếm, impact tại contact. Motor tiến tối đa 0,45 m với bước/chuyển contact chân tương ứng.

## 13.2. Đòn 2: chém ngang trả lực

| Thời điểm | Key pose/chuyển động |
| --- | --- |
| 0,00 | Nhận kiếm thấp bên trái từ đòn 1, liên tục cổ tay/hông |
| 0,08 | Đổi tải, hông xoay ngược, đầu không quay theo kiếm |
| 0,16 | Chém ngang trái → phải ở tầm thân |
| 0,23 | Chest truyền lực, chân chống, quỹ đạo rõ trước robot |
| 0,30 | Damage đóng, follow-through bên phải |
| 0,39–0,48 | Giữ link pose đòn 3, kiếm bên phải có thể thu lên cao |
| 0,65 | Hồi ready nếu không nối |

Trail 0,14–0,34 s; hit stop 0,045 s. Không dùng lại clip 1 bằng cách đổi dấu tốc độ. Nhịp và silhouette khác đòn đầu.

## 13.3. Đòn 3: bổ kết thúc

| Thời điểm | Key pose/chuyển động |
| --- | --- |
| 0,00–0,14 | Thu kiếm bên phải lên, gối/hông chuẩn bị brace |
| 0,14–0,28 | Giữ kiếm cao, khoảng hở đầu/sừng/vai, anticipation rõ |
| 0,28–0,35 | Hông/ngực kéo nhát bổ, tay theo lực |
| 0,35–0,43 | Trọng tâm hạ, kiếm qua vùng trước chân; không xuyên sàn vô cớ |
| 0,43–0,60 | Follow-through, tay/giáp dừng có trễ nhỏ |
| 0,60–0,90 | Hồi combat stance |

Trail 0,25–0,48 s; hit stop 0,065 s nếu trúng. Shockwave tại contact thật hoặc ground hit xác định. Shockwave trang trí không tự thêm damage; nếu muốn AoE phải khai báo shape/window riêng và dedupe.

Toàn combo: hỗ trợ xoay target chỉ windup, khoảng tối đa 40°/đòn và turn speed giới hạn; active chốt hướng, không bẻ 180°. Dash cancel theo bảng; hit/death ngắt theo priority và cleanup.

# 14. Bắn súng năng lượng

Demo: projectile damage 45, speed khoảng 80 m/s, fire interval 0,18 s, range max 60 m; chỉnh được theo arena. Pooling bắt buộc cho fire lặp. Nếu dùng hitscan thay projectile, ghi quyết định và giữ muzzle obstruction.

1. Camera ray tìm aim point, không hit thì lấy điểm xa trong giới hạn.
2. Muzzle thật → aim point kiểm vật cản đầu tiên và spawn clearance.
3. Nòng sát/trong tường thì chặn hoặc impact ngay theo policy; camera nhìn thấy target không cho phép bắn xuyên.
4. Projectile dùng sweep old→new hoặc collision đủ tin cậy, không chỉ trigger rời rạc.
5. Hit đúng layer/owner một lần; pool lại khi hit/hết range/lifetime.

Fire upper-body: vai/ngực ngắm trong giới hạn, recoil 0–0,04 s, hồi 0,04–0,16 s, biên độ vài độ rồi chỉnh rig. Tay không rời grip. Lower body được chạy khi hợp lệ; action toàn thân ưu tiên rõ.

Muzzle flash 0,03–0,07 s đúng forward, impact theo normal; không sửa shared material làm mọi nhân vật cùng sáng.

# 15. Q — Vector Slash: lao chém

Giá 20 energy, cooldown 6 s, damage 240, tổng 0,95 s. Bản đầu chỉ grounded; air-Q nếu thêm phải có clip/policy riêng.

| Thời gian | Animation/motion | VFX/SFX | Gameplay |
| --- | --- | --- | --- |
| 0,00–0,08 | Hạ hông, chân sau chống, chest hướng target | Jet khởi động | Kiểm trạng thái/energy, trừ cost một lần |
| 0,08–0,18 | Kéo kiếm sau thân, đầu giữ hướng | Jet tăng sáng, tụ lực | Hỗ trợ hướng giới hạn, chưa damage |
| 0,18–0,38 | Lao max 8 m, thân nghiêng/chân thu | Jet dài, streak theo quãng thật | Chốt hướng/điểm dừng; motor collision |
| 0,38–0,52 | Mở người chém ngang chéo, chuyển tải chuẩn bị phanh | Arc/trail, impact nếu hit | Active 0,38–0,52, một hit/target |
| 0,52–0,70 | Follow-through, chân đón lực, giảm jet | Vệt tan | Damage đóng, braking |
| 0,70–0,95 | Hồi ready/steering | Tắt loop jet/audio lao | Dash cancel từ 0,70 |

Điểm dừng theo collider và tầm kiếm, không teleport vào tâm target. Target gần thì giảm quãng; mất target thì giữ hướng đã chốt/fallback hợp lý. Chạm tường sớm thì dừng hợp lệ và chém tại chỗ cùng timing hoặc kết thúc an toàn, không hit target sau tường.

Hit stop 0,07 s, impact shake khoảng 0,12–0,18 s khi trúng. Miss chỉ âm lao/whoosh. Không tự thêm i-frame. Hit/death đóng damage/trail/motion request ngay.

# 16. E — Charged Beam: tụ lực bắn

Nhấn một lần, charge cố định; chưa làm giữ/thả với thời lượng khác. Grounded, chân brace và hạn chế dịch chuyển. Giá 30 energy, cooldown 9 s tính từ activation hợp lệ, damage 360 một hit, range 35 m, tổng 1,25 s.

| Thời gian | Pose | VFX/gameplay |
| --- | --- | --- |
| 0,00–0,20 | Hai chân mở brace, gối nén, nâng emitter | Charge audio/hạt hút về muzzle, trừ cost một lần |
| 0,20–0,45 | Trọng tâm giữa hai chân, chest ổn định | Tăng năng lượng, camera nhích nhẹ; turn max khoảng 60°/s |
| 0,45–0,65 | Pose commit, thêm nén trước recoil | Ring rõ, khóa cancel chủ động, chốt hướng tại fire marker |
| 0,65 | Vai/ngực recoil có trễ, tay giữ súng | Resolve hit một lần từ muzzle, flash/ring/impact tại collider đầu tiên |
| 0,65–0,85 | Giữ đường lực beam, pelvis brace | Beam nhìn thấy 0,20 s, không damage mỗi frame |
| 0,85–1,25 | Vai/súng hồi, chân về ready | Beam/audio tan; dash cancel từ 0,95 |

Trước 0,45 s được dash cancel: hoàn 24/30 energy, thay cooldown E bằng 1 s tính từ lúc cancel; dash vẫn trả cost riêng. Cưỡng bức ngắt trước commit áp cùng hoàn phí và cooldown 1 s từ lúc ngắt khi còn sống; death không hoàn và giữ cooldown thường. Sau commit không hoàn và giữ cooldown 9 s tính từ activation. Hit/death vẫn cleanup dù phase khóa cancel chủ động.

Beam chặn ở vật cản đầu tiên. Độ rộng core khớp hit shape: sphere/capsule cast bán kính phù hợp hoặc giải thích glow ngoài chỉ trang trí. Không ray mảnh gây damage khác xa beam dày. Kiểm overlap ngay muzzle, owner mask, start/end đúng contact.

VFX: hạt hút, core/glow, ring khai hỏa, beam core rõ, impact bề mặt. Không trắng toàn màn hình; vẫn thấy robot/target. Charge audio dừng trên cancel/death/unload và pause theo clock policy.

# 17. R — Celestial Breaker: tuyệt chiêu 4 giây

Thiết kế mới: một nhát chém năng lượng quyết định sau khi lao; một damage window chính, không multi-hit ngầm từ particle. Giá 80 energy, cooldown 25 s, damage 800, approach max khoảng 10 m, bản đầu grounded.

CanStart: còn sống, không action xung đột, đủ energy, cooldown xong, target hợp lệ trong tầm và có đường/phương án tại chỗ. Không đủ điều kiện thì UI báo ngắn, không trừ cost. Target chết hoặc mất sau khi bắt đầu có nhánh an toàn nêu ở mục 17.2; không tự bịa target hoặc bù damage để giữ cinematic.

## 17.1. Storyboard và key pose

| Thời gian | Animation/motion | Camera | VFX/SFX/damage |
| --- | --- | --- | --- |
| 0,00–0,15 | Hạ trọng tâm, mở chân, kiếm thấp bên phải | Blend tới ba phần tư trước, rõ robot | Kích hoạt, mắt/khe năng lượng tăng sáng |
| 0,15–0,45 | Chest lấy lực, đầu giữ target, activation pose rõ | Không che mặt/sừng/vũ khí | Một xung ngắn; khóa input theo policy |
| 0,45–0,80 | Kiếm thu sau vai trong khoảng hở, pelvis brace | Tiến nhẹ, toàn thân trong safe frame | Hạt/ring chung một nguồn năng lượng |
| 0,80–1,15 | Charge pose mạnh nhất, hold ngắn để đọc silhouette | FOV 50–54° nếu vẫn thấy hình thể | Energy blade dài hơn, shape tương ứng, chưa damage |
| 1,15–1,65 | Jet bùng, lao đường đã kiểm tra, dừng đúng tầm | Wide/ba phần tư bên, rõ đường lao/target | Streak theo motion thật; không xuyên collider |
| 1,65–1,90 | Chân đón lực, kiếm lên cao, hông→ngực lấy lực bổ | Góc thấp vừa, vẫn thấy contact/target | Giảm hạt để rõ pose, căng âm trước impact |
| 1,90–2,12 | Nhát bổ lớn, trọng tâm hạ, vai/tay theo lực | Giữ khung contact, không cắt đúng lúc cần đọc hit | Active 1,96–2,10, một hit/target; hit stop 0,08 s nếu hit |
| 2,12–2,35 | Follow-through, chân chịu tải, không snap | Shake ngắn theo hit thật | Flash khoảng max 0,04 s mức thường, arc/ring/impact; không hit lặp |
| 2,35–2,85 | Giữ finish pose rồi hồi, giáp phụ trễ nhỏ | Lùi nhẹ thấy dư chấn | Arc tan, bụi/debris tại contact/ground thật, số lượng hạn chế |
| 2,85–3,20 | Thu kiếm về stance | Hướng dần về gameplay | Giảm emission; target reaction theo HP thật |
| 3,20–4,00 | Ready, chân vững, ra locomotion hợp lệ | Blend về gameplay/trả orbit | Thu hồi effect/audio, giải phóng lock đúng thứ tự |

Energy blade là asset/shape riêng, không tầm vô hạn. Đề xuất active reach khoảng 4–5 m cho bản thử rồi đo silhouette. Không damage toàn scene vì đang cinematic. Tư thế quyết định phải đẹp khi tắt VFX.

## 17.2. Collision/fallback

Trước kích hoạt, tính approach, stopping distance, clearance và shot candidates. Sát tường dùng R tại chỗ: bỏ lunge, giữ body pose hợp lệ, đổi bên/lùi camera, damage chỉ trong shape thật. Không teleport xuyên collider để khớp storyboard.

Nếu target chết/mất trước mốc approach 1,15 s, ngắt R an toàn và giữ cost/cooldown đã trả. Nếu mất từ mốc 1,15 s trở đi, tiếp tục chuỗi tại hướng/vị trí đã chốt, không homing hoặc truy cập reference đã mất; damage chỉ theo shape thật, không hoàn phí. Chỉ cleanup khi nhánh tiếp tục kết thúc hoặc có interrupt ưu tiên cao khác. Muốn giữ target cinematic phải có cơ chế gameplay riêng, điều kiện/phạm vi/miễn nhiễm rõ; mặc định không tự thêm bắt giữ.

Không mặc định bất tử. Có thể dùng chống stagger khoảng commit 0,45–2,35 s nếu ghi rõ, nhưng damage/death vẫn xử lý. Death luôn thắng cinematic. Target mất không làm player kẹt bất động.

## 17.3. Thời gian và cleanup

Mặc định gameplay chạy bình thường, AI/target không bị khóa chết để diễn. Có thể giảm nhịp AI không phải target bằng policy/token phục hồi rõ. Không pause toàn scene rồi mong Animator/hit tự tiếp tục.

Nếu slow motion/hit stop: lập bảng clock cho action, Animator, motor, AI, camera, particle, UI/cooldown. Baseline ưu tiên thời gian thường + hit stop ngắn có phạm vi. Một TimeEffectCoordinator quản lý nhiều yêu cầu và pause; không cho mỗi coroutine tự đặt Time.timeScale = 1 khi xong.

Baseline clock cần triển khai rõ:

| Hệ thống | Pause | Hit stop cục bộ |
| --- | --- | --- |
| Action clock, Animator, motor của attacker/receiver chịu hit stop | Dừng | Cùng dừng theo token, không lệch pose/hit timing |
| AI/projectile của đối tượng khác | Dừng | Tiếp tục nếu không thuộc phạm vi hit stop được khai báo |
| Cooldown/regen | Dừng, không hồi bù khi resume | Theo gameplay clock; regen vẫn bị chặn bởi action như mục 8 |
| Gameplay VFX | Dừng | Có thể tiếp tục để đọc impact, theo policy effect |
| Timeline shot R | Dừng | Theo cùng R action clock |
| Camera smoothing/shake | Dừng camera gameplay/cinematic khi pause | Có thể dùng unscaled delta để diễn impact; không tự tiến shot phase |
| UI/menu | Hoạt động bằng unscaled time | Hoạt động; giá trị resource theo dữ liệu thật |

Bản đầu ưu tiên hit stop cục bộ và token pause; nếu dùng global timeScale/slow motion phải quy đổi đồng bộ theo bảng, không thêm clock trái nhau. Không tích lũy delta trong pause rồi bù toàn bộ ở frame resume. E có protected interval 0,45–0,95 đối với dash chủ động; death/hard interrupt vẫn áp priority chung.

Cleanup chung chạy khi complete/interrupted/player death/disable/reset/scene unload/lỗi reference; target mất đi qua bộ chọn nhánh ở mục 17.2, rồi cleanup ngay nếu ngắt hoặc ở exit nếu tiếp tục. Idempotent: đóng damage, hủy motion, tắt trail/beam, dừng loop audio, trả pool, phục hồi camera/FOV, trả input/AI/time token, cursor đúng, xóa lệnh cũ. Không chỉ dựa một Animation Event cuối clip.

# 18. VFX đủ cụ thể để thực hiện

Ưu tiên công cụ tương thích URP đang có: Particle System, TrailRenderer, mesh/LineRenderer và shader phù hợp. Không mặc định VFX Graph/plugin ngoài đã cài. Native geometry/texture thủ tục được dùng cho bản đầu; đánh dấu placeholder còn cần làm đẹp. Không viện dịch vụ generate chưa chạy để tuyên bố asset đã có.

Mỗi effect có anticipation, release, decay; ghi parent/local hay world space. Số dưới là starting point mỹ thuật cho robot 2,6 m, không phải cam kết hiệu năng.

| Effect | Cấu trúc/timing khởi điểm | Kiểm tra |
| --- | --- | --- |
| Jet | Core trắng, outer xanh; lifetime 0,06–0,18 s, scale theo throttle/velocity | Đúng nozzle/chiều, không xuyên thân |
| Ground dust | Ground probe, hạt đục nhẹ, lifetime 0,25–0,60 s | Không phát trên không, không che hết chân |
| Sword trail | Dải hẹp theo blade, history 0,06–0,12 s | Chỉ swing, clear khi reset/teleport/pool |
| Sword arc | Cung mỏng 0,12–0,25 s theo hướng nhát | Không rời đường kiếm, không thay clip tay |
| Muzzle flash | Core/vài spark, 0,03–0,07 s | Trục đúng, không trắng toàn screen |
| Projectile | Core rõ/glow nhỏ, lifetime theo range/speed | Thu hồi khi hit, không trail nối vị trí cũ |
| Metal impact | Khoảng 12–30 spark khởi điểm, 0,08–0,30 s | Normal thật, chỉ khi hit |
| E charge | Hạt hút/ring/core tăng trong 0–0,65 s | Cancel được, không audio/hạt treo |
| E beam | Core + outer sheath + impact, khoảng 0,20 s | Length/width đúng hit shape/collider |
| R charge | Cường độ theo phase, giữ silhouette | Không spam ring giống nhau |
| R impact | Một arc lớn, ring, flash ngắn, debris giới hạn | Contact thật, không che target lâu |

Bloom hỗ trợ, không là nguồn tạo hình duy nhất. Giảm bloom vẫn phải thấy hình beam/trail. Dùng material URP đúng, tránh pink shader, cân emission/exposure với giáp trắng. Không thay lighting scene gốc toàn diện chỉ để VFX nổi.

World impact độc lập sau khi vũ khí đi tiếp; jet/trail bám socket. Reset ParticleSystem/TrailRenderer khi lấy pool, clear history trước emission. Pool projectile/impact/arc có capacity và fallback khi đầy, không sinh vô hạn.

Không sửa shared material khiến mọi instance sáng cùng lúc; dùng per-renderer properties hoặc material instance được quản lý theo pipeline/profiling. Hạn chế realtime light, particle shadow, overdraw. Số renderer model cao phải đo, không hứa 60 FPS trước khi profile.

Low/Medium/High thay particle count, trail detail, light/debris. Có mức giảm flash/shake/bloom/motion blur; motion blur mặc định tắt hoặc nhẹ. Không bắt buộc full-screen flash để nhận impact.

# 19. Camera, âm thanh và hit feel

Gameplay camera: follow damping, orbit chuột, pitch clamp khởi điểm khoảng -25° tới +65° theo cách định nghĩa; collision volume bằng sphere cast hoặc tương đương. Khoảng cách 5–7 m, target quanh ngực cho robot 2,6 m, sau đó chỉnh theo silhouette. Không cắt vào sừng/backpack/đầu ở pitch xấu.

FOV khoảng 60°, boost 66–68°, blend 0,15–0,25 s. Một nguồn quản lý FOV, không nhiều script ghi đè. CameraTarget ổn định, không kế thừa bob pelvis quá mức.

Lock-on chọn mục tiêu sống theo khoảng cách/góc nhìn/line of sight với hysteresis. Target gần/qua sau lưng không làm camera flip 180° tức thì. Target chết/mất thì unlock. Giữ player/target trong frame nếu có thể; vật cản dùng fallback thay vì camera xuyên tường.

R tối đa khoảng bốn shot có mục đích: activation, charge, approach/contact, return. Kiểm cả camera volume và đường nhìn, không chỉ điểm đặt. Có shot wide/đổi bên khi bị che.

Hit feel gồm anticipation, tăng tốc kiếm, contact thật, reaction, hit stop, SFX/VFX/shake. Hit stop combo 0,035/0,045/0,065 s; Q 0,07; R 0,08, xem lại khi test. Miss không hit stop. Shake theo lực, biên độ nhỏ theo scale, nhanh tắt, không rung lúc idle.

SFX có servo/footstep, jet start-loop-stop, sword whoosh, metal hit, gun flash, charge-fire-decay và UI cues. Footstep theo contact/loại mặt sàn, không phát theo timer bất chấp chân. Charge/hover loop có handle dừng khi action ngắt. Chưa có audio thì dùng placeholder hợp pháp có nhãn hoặc báo thiếu; tên file không tồn tại không phải asset hoàn chỉnh.

# 20. State, cancel, tài nguyên và recovery

Priority đề xuất: Death > hard interrupt/reset > Hit/Stagger khi không armor hợp lệ > protected commit chưa mở cancel > dash/cancel hợp lệ > queued combo > locomotion/aim. Protected commit chỉ tồn tại trong phase chưa cho cancel, không phủ quyết những cửa sổ dash cancel đã khai báo. Khi dash và nối combo cùng hợp lệ, dash thắng và xóa queue của action cũ. Không tập bool rời có thể vừa Dead vừa Attacking vừa Hovering không kiểm soát.

Tách movement state và action/upper-body permission khi giúp đơn giản, nhưng full-body action có quyền khóa pose/rotation/input rõ. Không cần state tổ hợp khổng lồ cho mọi cặp.

Mỗi action khai báo CanStart, resource check, rotation từng phase, motion policy, input được phép, damage/dedupe, queue/cancel/interrupt, cost/cooldown/refund, exit và cleanup.

Dash/Q/R cooldown bắt đầu khi activation hợp lệ; E bắt đầu cùng lúc và được override thành 1 s từ cancel nếu hủy trước commit như mục 16. UI đọc dữ liệu thật. Pause không reset cooldown/cost. Energy dùng delta phù hợp, hover/boost ngừng drain đúng lúc. Khóa input vẫn xử lý death/pause/reset. Cursor mở khi pause/death, khóa khi resume hợp lệ, không ép khóa ở menu.

Reset arena phục hồi HP/energy/cooldown/target/camera/root position/rig pose/pool/input và time tokens đúng cấu hình, không chỉ teleport nhân vật về đầu sân.

# 21. Sân thử, dummy và AI

Scene mới, mặt phẳng khoảng 40×40 m, vạch đo quãng, dốc hợp lệ/quá giới hạn, bậc thấp, tường mỏng, góc hẹp, trần thấp và khu kiểm camera. Collider/layer thật, không chỉ mesh nhìn thấy.

Dummy: Health, nhiều hurtbox chung DamageReceiver, HP UI, log số hit theo action/window, reset dễ dùng. Có debug active window/sweep/muzzle/beam endpoint. Tùy chọn không mất HP nhưng vẫn xác minh timing.

AI: detect → approach có collision/path hợp lệ → dừng đúng tầm → windup dễ đọc → active → recovery; nhận hit/stagger/death/reset. Không damage từ windup. Nếu cần NavMesh/package, kiểm package/bake/references thật; không MoveTowards xuyên tường rồi báo hoàn thiện.

AI được dùng proxy hoặc bản sao phù hợp có nhãn. Không đổi Gundam người chơi sang model khác để dễ rig rồi báo xong yêu cầu.

# 22. Kiểm thử và nghiệm thu có bằng chứng

## 22.1. Kỹ thuật/lưu asset

- Không compile error mới, Missing Script/reference bắt buộc null.
- Prefab/scene/controller/clip/mapping/material đã lưu, mở lại hoạt động.
- Clip binding không mất khi chạy tool lần hai; không duplicate skeleton/renderer/socket.
- Reset về rest pose, không drift.
- Runtime/build không tham chiếu UnityEditor.
- Báo riêng lỗi cũ, giới hạn công cụ và bài chưa chạy.

## 22.2. Rig/animation

- Tay/chân/ngực/đầu cử động đúng joint, không chỉ root trượt.
- Không nổ mesh, gập ngược, kéo part đối diện, sai culling ở pose rộng.
- Key pose đẹp/dễ đọc khi tắt VFX; không xuyên giáp rõ ở pose/blend.
- Stance foot trên nền phẳng hướng tới sai lệch khoảng 3–5 cm hoặc ít hơn ở tỷ lệ thử; đo/xem video và báo nếu chưa đạt.
- Loop không pop, stop/turn không snap, jump/land không bật pose, IK không tranh Animator.
- Combo có silhouette/nhịp riêng, entry/exit liên tục, vũ khí không đi xuyên thân khi blend.

## 22.3. Tình huống bắt buộc

1. Idle/walk/run/strafe/boost, đổi hướng liên tục, dừng gấp.
2. Lock/unlock khi chạy, target gần/ra sau lưng/mất.
3. Dash vào tường/góc hẹp/cạnh vật cản/dốc/tường mỏng.
4. Jump trần thấp, rơi mép, land dốc, hover cạn, spam Space.
5. Combo miss/hit, nhiều collider, spam LMB, input sát mép queue/cancel.
6. Ngắt trước/trong/sau active, xác minh damage cũ đóng.
7. Muzzle sát/trong tường, camera khác phía vật cản, projectile nhanh/FPS thấp.
8. Q gần/xa, target sát tường/chết giữa lao, lao bị chặn sớm.
9. E cancel trước/sau commit, target rời aim, muzzle chắn, audio cleanup.
10. R góc hẹp, target mất, player chết ở từng phase, shot che, fallback tại chỗ.
11. Pause trong charge/hit stop/cinematic, resume đúng clock/cursor/energy.
12. Disable/reset/đổi scene, chơi lại, không trail/audio/camera/input/time token treo.

Lặp ở 30/60/120 FPS hoặc cap tương ứng. Mục tiêu ban đầu: quãng dash khi không bị chặn và duration theo logical action clock sai lệch không quá khoảng 5%; hit count không đổi theo FPS; window giữa hai frame không bị bỏ. Khi đo wall-clock duration, loại trừ pause, slow motion và hit stop có chủ đích hoặc báo riêng phần kéo dài đó; không coi hit stop đúng thiết kế là lỗi duration. Ghi điều kiện/số đo, không khẳng định độc lập FPS chỉ từ đọc code.

## 22.4. Hình ảnh/hiệu năng

Xem bình thường, 0,25× và frame-step: anticipation/contact/follow-through/foot slide/socket/camera. Ảnh rig front/side/three-quarter, video locomotion/combo/Q/E/R nếu công cụ hỗ trợ; thiếu video thì chuỗi key pose/log và ghi rõ hạn chế.

Mục tiêu 1080p khoảng 60 FPS trên máy đã ghi nhận, chỉ xác nhận khi đo. Profiler CPU/GPU frame time, batches/draw calls/renderers, animation/IK, particle/overdraw, memory/GC. 375 renderer là điểm cần đo; chỉ combine theo cùng driver/material hoặc giải pháp phù hợp sau khi rig đúng. Không combine xuyên joint làm mất animation. Giữ bản chỉnh sửa được trước tối ưu.

So sánh trước/sau trong cùng scene/action. Sau warm-up không GC tăng liên tục từ Instantiate/Destroy, LINQ/string log/material mỗi frame. Pool có giới hạn, không rò sau 50 lần dùng chiêu. Phân biệt Editor overhead và build nếu đo build. Console sạch chưa chứng minh tối ưu.

# 23. Đầu ra từng giai đoạn và bàn giao

Mỗi giai đoạn báo ngắn nhưng đủ để mở/xem/chạy:

1. Kết quả và điều đã xác minh.
2. File/asset/scene/prefab tạo/đổi, đường dẫn chính xác.
3. GameObject có component gì, reference đã gán gì.
4. Thông số thật, khác đặc tả ở đâu và vì sao.
5. Cách mở scene/chạy và input thử.
6. Bằng chứng pass/fail bằng ảnh/video/log thích hợp.
7. Placeholder/hạn chế, việc thực sự cần tôi bổ sung nếu có.

Nếu chỉ chat, đưa code hoàn chỉnh giai đoạn hiện tại, thao tác theo thứ tự, cách sửa lỗi thường gặp. Không dồn hàng chục script chưa kiểm chứng. Khi sửa giữ nhất quán class/parameter/input/folder/data schema và nêu file/phần đổi.

Giữ tài liệu sống: ProjectAudit, RigMapping, RigValidation, AnimationClipSheets, CombatTuning, TestResults, README. Có thể gộp nếu nhỏ nhưng không bỏ thông tin. README hướng dẫn thay clip, chỉnh hit windows, màu VFX, weapon/socket, reset/chạy tool.

Toàn demo hoàn thành khi player điều khiển Gundam đã rig, clip chạy đúng, hit/energy/cooldown thật, VFX/camera cleanup đúng, asset mở lại hoạt động và phần chưa kiểm chứng ghi rõ. Không đánh dấu xong chỉ vì code biên dịch.

# 24. Bắt đầu thực hiện ngay

Kiểm kết nối Unity và đọc lại snapshot mục 2. Báo ngắn cấu trúc 375 part, hướng tiến, scale, tư thế gốc và các joint có thể xác định. Trước tiên thử một cánh tay và một chân trên bản làm việc để chứng minh pivot/mapping rồi mới áp dụng toàn thân.

Hoàn thành giai đoạn 0–1 trong khả năng công cụ: working copy, mapping có preview, skeleton cơ khí thật, geometry đi đúng, rest-pose snapshot, socket, rig validation và prefab/scene đã lưu. Nhóm chưa đủ bằng chứng thì khoanh chính xác/giải thích/tiếp tục phần độc lập, không đoán để đủ số.

Rig đạt thì tiếp tục tuần tự các giai đoạn sau, tự kiểm tra và sửa. Đừng trả lại kế hoạch hoặc viết lại prompt này. Tôi cần kết quả mở và chơi thử trong Unity, cùng thông tin trung thực về chất lượng và phần còn thiếu.




