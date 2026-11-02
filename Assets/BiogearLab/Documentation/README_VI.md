# Biogear Lab — bản dựng môi trường

Scene: `Assets/BiogearLab/Scenes/BiogearLab_Reconstruction.unity`.

Vào EnvironmentHub → Play → BIOGEAR LAB để mở scene. Hoặc Tools → Biogear Lab → Open Environment.

## Điều khiển khi Play

- Giữ chuột phải và WASD để bay, Q/E để xuống/lên, Shift tăng tốc.
- 1: tổng thể; 2: đấu trường; 3: nhà vòm; 4: nội thất; 5: hành lang; 6: tháp; 7: dưới đĩa; 8: layout từ trên cao.
- C: bật/tắt camera xoay quanh khu trung tâm.
- CHỌN MÔI TRƯỜNG ở góc phải: về hub.

Đây là trình xem môi trường bằng camera tự do; chưa tích hợp robot, luật battle hoặc controller đi bộ vào map này.

## Nội dung

Map được dựng bằng mesh procedural và asset lưu trên đĩa: vòng đấu trường chia panel, lòng chảo có bệ sáng, đường vòng và ramp, đĩa vành khuyên có lỗ thật, cầu đôi và trụ, tháp ba nhánh với reactor cam/cyan, nhà vòm hở đỉnh với khung chéo/nội thất/bể nghiên cứu, 5 bệ nước, hành lang dốc, cover trắng/cam, crates, pods, tube racks, vách đá có grass caps, vòm đá và cây dừa. Shader biển động và sky stylized là asset riêng.

Prefab: `Assets/BiogearLab/Prefabs/BiogearLab_Environment.prefab`. Meshes, Materials, Textures và Lighting nằm trong các thư mục cùng tên.

## Chỉnh và dựng lại

Chỉnh `Data/Layout.asset` để đổi seed, radius hub, vị trí vòm/tháp và camera tổng thể. Chạy **Tools → Biogear Lab → Build or Update Environment** để tạo lại scene. Builder ghi đè phần scene do nó dựng, đồng thời sao lưu scene trước đó dưới `Backups/BiogearLab/`. Giữ các chỉnh sửa thủ công trong scene sao chép nếu cần bảo toàn khi rebuild. Nó không xóa map khác trong catalog.

Các mesh/material generated được cập nhật tại chỗ khi rebuild. Bản sao lưu scene không đóng băng các asset này; muốn giữ nguyên một phiên bản để phục hồi cả hình học, sao lưu cả thư mục BiogearLab hoặc dùng version control.

**Tools → Biogear Lab → Capture Review Views** xuất ảnh các góc vào `Documentation/Review/` và dùng ảnh tổng thể làm thumbnail selector.

## Reference và độ chính xác

Nguồn: 10 ảnh gốc trong D:/Downloads và video making-of đã được phân tích trong `Prompts/BiogearLab/03_REFERENCE_ANALYSIS_VI.md`. Khối kiến trúc chính bám reference; các kích thước, mặt khuất, nhánh đường, số lượng props và vật liệu procedural được dựng lại bằng ước lượng. Những phần này chưa được xác nhận từ dữ liệu map gốc. Bản dựng không phải asset game gốc và không được xác nhận giống 100%.

Các điểm cần tiếp tục đối chiếu mỹ thuật: layout/tỷ lệ nhiều góc, chi tiết chân tháp và portal, nhịp panel, đá/bầu trời, UV và pattern texture. Nước hiện dùng màu/highlight procedural; bể dùng mesh phát sáng với cây bên trong, chưa có khúc xạ. Không tự xem các đường suy luận là layout gameplay gốc. Chưa bake lightmap/NavMesh hoặc profile trên thiết bị đích.

## Hoàn tất lượt sửa ngày 31/10/2026

- Đã sửa material cache để không tạo material núi bị mất tham chiếu khi build theo batch.
- Cập nhật mesh bằng vertex/index/normal/UV và upload lại GPU buffer, tránh dùng dữ liệu hiển thị cũ sau rebuild.
- Vòng trung tâm có sàn lưu thông tầng dưới và collider liên tục tại đường nối panel; lỗ giữa đĩa vẫn là lỗ hình học thật.
- Hạ bệ nước xuống đúng mực nước; thêm ramp vào cửa vòm. Có 5 bệ.
- Loại đá lấn vào vòm và lối hành lang, mở cổng Gate01/Gate03, bổ sung vách trong và giằng vòm.
- Tách mesh panel sàn có bevel nhỏ để giảm khe lớn giữa các tấm; trần hành lang có shadow casting hai mặt.
- Chữ dùng shader có depth test và font atlas cập nhật; điều chỉnh tỷ lệ chữ và hướng GATE03. Ảnh review xuất đúng sRGB.
- Đã kiểm tra 8 góc render; ảnh nằm trong `Documentation/Review/` và thumbnail đã gán vào catalog.
- 94 kiểm tra Play Mode đạt, gồm chuyển toàn bộ 6 lựa chọn available và kiểm tra nội dung Biogear. Kết quả: `Assets/EnvironmentSystem/Documentation/Verification.md`.

Các kiểm tra xác nhận scene hoạt động và những điều kiện hình học cụ thể; chúng không chứng minh độ giống 100% với game gốc. Chưa chạy standalone build/Android hoặc benchmark FPS.
