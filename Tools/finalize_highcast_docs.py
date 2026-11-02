from pathlib import Path
root=Path(__file__).resolve().parents[1]
folder=root/'Prompts/2026-10-29-Camera-Flight-Weapons'
notice='''> BẢN HIỆN HÀNH — CHẠY SẢI DÀI / R BAY CAO (đã triển khai)
> Ưu tiên `PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt` và báo cáo `Assets/TurnBasedBattle/Reports/HighCast_20261029_VI.md`. Kiếm chạy chậm, stride 2.10, chu kỳ hai bước khoảng 0.92s trên đường chuẩn; chạy đi 0.75–5.45, hit 5.90, về 6.60–11.50, complete 12.45s. Súng đứng tại chỗ. R giữ khoảng hở bàn chân 1.15×H, lên cao/ổn định trước VfxStart=2.55, VFX đủ 8s, complete 13.10s; damage global 5.90/7.65/8.05/8.45/8.85. Các thông số khác bên dưới là lịch sử nếu mâu thuẫn.

'''
parts=sorted(folder.glob('[0-5][0-9]_*.md'))
for p in parts:
    body=p.read_text(encoding='utf-8')
    if not body.startswith('> BẢN HIỆN HÀNH'):body=notice+body
    p.write_text(body,encoding='utf-8');p.with_suffix('.txt').write_text(body,encoding='utf-8')
(folder/'TAT_CA_PROMPT_VI.txt').write_text('\n\n'.join('====================\n'+p.name+'\n====================\n\n'+p.read_text(encoding='utf-8') for p in parts)+'\n\n'+(folder/'PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.txt').read_text(encoding='utf-8'),encoding='utf-8')
p=root/'Assets/TurnBasedBattle/README_VI.md'
body=p.read_text(encoding='utf-8')
header='''> Bản hiện hành: xem [thông số chạy chậm và R bay cao](Reports/HighCast_20261029_VI.md). **31/31 kiểm tra đạt.** Clip mới: `ReviewMedia/HighCastReview_20261029.webm` tại gốc project. Các phần timeline 6.90s/10.90s bên dưới mô tả bản trước; bản mới dùng Attack 12.45s, R 13.10s và VfxStart 2.55s. Cách điều khiển không đổi.

'''
if not body.startswith('> Bản hiện hành:'):p.write_text(header+body,encoding='utf-8')
for suffix in ['md','txt']:
    p=folder/f'PROMPT_TINH_CHINH_CHAY_VA_R_BAY_CAO_VI.{suffix}'
    body=p.read_text(encoding='utf-8').replace('Trạng thái: đặc tả đề xuất để xem trước, CHƯA triển khai thay đổi này.', 'Trạng thái: đã được người dùng yêu cầu thực hiện và đã triển khai; thông số thực tế xem HighCast_20261029_VI.md. Các bảng trong prompt là đề xuất ban đầu.')
    p.write_text(body,encoding='utf-8')
print('Saved implementation report and updated prompt priorities.')
