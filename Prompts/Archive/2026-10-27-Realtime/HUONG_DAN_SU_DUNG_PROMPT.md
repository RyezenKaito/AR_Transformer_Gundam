# Cách dùng bộ prompt Gundam

Đây là bộ đặc tả để giao cho chat/agent triển khai trong Unity. Lượt soạn prompt đã kiểm tra project ở chế độ đọc; chưa dựng rig hoặc tạo animation trong project.

## 1. Gửi lần đầu

Đính kèm `GUNDAM_MASTER_PROMPT_VI.txt` hoặc `.md` vào chat có kết nối Unity, rồi gửi nguyên câu sau:

> Hãy thực hiện đặc tả trong file đính kèm trên model Unicorn Gundam hiện có. Bắt đầu ngay bằng kiểm tra project và dựng rig cơ khí ở giai đoạn 0–1; kiểm chứng một cánh tay và một chân trước khi áp dụng toàn thân. Giữ asset gốc, lưu bản làm việc riêng. Rig đạt thì tiếp tục tuần tự locomotion, combat và VFX theo đặc tả. Tự kiểm tra kết quả trong Unity, báo rõ phần đã chạy được và phần còn thiếu; không chỉ trả kế hoạch.

Bản `.md` và `.txt` có cùng nội dung. Không cần gửi cả hai. Nếu chat không đọc được file, dán toàn bộ nội dung `.txt`; các mục 1–24 đều thuộc prompt.

## 2. Vì sao prompt này khác yêu cầu chung chung

Model thực tế có 375 mesh rời, chưa có Animator/clip, các mesh đang chung local position. Vì vậy prompt bắt đầu bằng phân nhóm part, xác định pivot/axes và nghiệm thu rig. Chỉ tạo các node có tên bone hoặc xoay toàn root chưa đáp ứng yêu cầu.

Prompt đặt ra clip sheet, key pose, foot contact, damage window, input/cancel, energy/cooldown, VFX/camera và cleanup. Các con số là starting point để agent tinh chỉnh trên model, không phải lời hứa chất lượng game thương mại chỉ sau một lần gửi.

## 3. Prompt tiếp tục theo giai đoạn

Dùng các đoạn dưới khi cần tiếp tục một lượt dài hoặc khi chat đã dừng sau một mốc. Không cần gửi tất cả cùng lúc. Nếu mở chat mới, đính kèm lại master và báo cáo tiến độ/đường dẫn asset mới nhất.

### A. Kiểm chứng rig trước khi đi tiếp

> Tiếp tục theo GUNDAM_MASTER_PROMPT_VI. Đọc lại rig và mapping hiện có, không dựng skeleton thứ hai. Thực hiện toàn bộ rig validation, đặc biệt vai/khuỷu/gối/cổ chân, giáp vai/hông, giữ cán kiếm và đưa kiếm lên cao không xuyên sừng/backpack. Kiểm kê mọi part hiện hình. Lưu pass/fail và ảnh các pose. Sửa các lỗi cấu trúc trước khi tạo combat clips.

### B. Locomotion, camera và trên không

> Rig đã qua nghiệm thu theo báo cáo hiện có. Thực hiện giai đoạn 2–3 của master: motor/collision và camera, clip thật cho idle/walk/run/strafe/start/stop/turn/boost/dash/jump/hover/fall/land. Đo stride để khớp tốc độ; không dùng lắc toàn root thay bước chân. Thử dốc, tường, trần thấp, cạn energy và 30/60/120 FPS. Lưu scene/prefab/clip/controller, báo kết quả thật.

### C. Combo và hit detection

> Thực hiện giai đoạn 4 của master trên rig và controller hiện có. Tạo ba clip theo key pose/timeline, continuity giữa đòn và chuyển trọng tâm đúng. Gắn base/mid/tip kiếm, quét theo chuyển động và chặn vật cản. Xác minh queue/cancel và target nhiều collider bằng dummy. Tắt VFX để kiểm pose trước khi thêm hit feel. Không thay tên parameter hoặc tạo controller mới không cần thiết.

### D. Súng, Q và E

> Thực hiện giai đoạn 5 theo master: bắn từ muzzle có obstruction, Q lao chém có collision, E charge/commit/fire/cancel rõ ràng. Đồng bộ pose với gameplay clock, energy/cooldown và hit windows. Kiểm target mất/chết, nòng sát tường, lao bị chặn và audio/trail cleanup. Tạo asset/reference thật, chạy từng chiêu rồi thử kết hợp locomotion/combo.

### E. VFX, SFX và cảm giác va chạm

> Thực hiện giai đoạn 6 theo master trên các chiêu đã chạy. Làm hiệu ứng có anticipation/release/decay, màu thống nhất, jet đúng nozzle, trail đúng swing, beam đúng collider. Hit impact/shake/hit stop chỉ khi hit xác nhận. Giữ silhouette và giáp trắng đọc được, có giảm flash/shake/bloom. Pooling/reset đầy đủ; báo asset nào là placeholder và đo chi phí effect.

### F. R và camera cinematic

> Thực hiện giai đoạn 7 theo storyboard Celestial Breaker 4 giây trong master, phương án một nhát chém chính với active 1,96–2,10 giây. Tạo clip/shot/VFX thật, đường lao collision và fallback tại chỗ. Không giả damage khi hụt. Kiểm target mất/player death/pause ở từng pha, rồi bảo đảm camera/input/time/audio/VFX luôn phục hồi. Lưu video hoặc chuỗi ảnh key pose và báo nhánh đã thử.

### G. Hoàn thiện và bàn giao

> Thực hiện giai đoạn 8 và toàn bộ nghiệm thu master. Hoàn thiện AI/dummy/UI/reset, sửa lỗi tích hợp, thử 30/60/120 FPS, lặp chiêu 50 lần, profile CPU/GPU/GC/renderer/particle ở 1080p. Không gọi đã tối ưu nếu chưa đo. Mở lại prefab/scene để kiểm asset. Bàn giao README, clip sheets, tuning và pass/fail, phân biệt hoàn chỉnh/placeholder/chưa kiểm chứng.

## 4. Khi kết quả chưa đạt

Nếu animation còn cứng:

> Tạm tắt VFX và camera shake. Xem chậm clip đang lỗi, chỉ ra frame/key time mất chuyển trọng tâm hoặc sai đường kiếm. Sửa anticipation, hip/chest lead, foot contact, spacing và follow-through trên clip thật; không thêm lắc root để che lỗi. Kiểm lại cả đoạn blend vào/ra và gửi bằng chứng trước/sau.

Nếu mesh đi sai hoặc giáp xuyên:

> Tạm dừng thêm chiêu mới. Xác định part/driver/pivot/axis sai trong MechaRigDefinition. Thử riêng joint lỗi ở neutral và pose cực trị, sửa mapping/offset hoặc key pose rồi reset nhiều lần. Không đổi scale hoặc reset rotation toàn rig để chữa tạm. Cập nhật rig validation và kiểm lại clip binding.

Nếu chat nói đã hoàn thành nhưng chưa có bằng chứng:

> Đối chiếu từng tiêu chí master với asset và scene thật. Liệt kê phần nào đã tạo, phần nào đã chạy Play Mode, phần nào chỉ đọc code. Mở scene demo, kiểm các reference và chạy bài thử tương ứng. Chưa thử thì ghi chưa thử; không suy từ Console sạch rằng animation/collision/camera đã đúng.

Cấu trúc bối cảnh, đầu ra cụ thể và kiểm tra kết quả trong bộ prompt được tham khảo từ [OpenAI: Prompt engineering](https://developers.openai.com/api/docs/guides/prompt-engineering). Các đặc tả chuyển động/chiêu thức là thiết kế cho demo này.
