from pathlib import Path

root = Path(__file__).resolve().parents[1]
folder = root / 'Prompts/2026-10-29-Camera-Flight-Weapons'
notice = '''> CẬP NHẬT ƯU TIÊN — yêu cầu bổ sung ngày 29/10/2026
> Kiếm phải là kiếm ánh sáng có lõi sáng, quầng, bật/tắt và vệt chém. Attack cận chiến bay tới rồi trở về; súng và các vũ khí tầm xa **đứng tại vị trí đội hình để rút/ngắm/bắn/cất**, không bay qua địch. Earth Prison vẫn giữ pha bay tới CastAnchor, thi triển và trở về. Camera phải zoom thực sự bằng con lăn/chụm và orbit 360° mượt; thanh máu địch nằm **trên đầu từng địch**, theo camera mỗi khung hình. Quy tắc này thay thế mọi yêu cầu “tất cả Skill đều bay tới” hoặc “thanh máu địch cố định bên màn hình” còn xuất hiện trong phần gốc bên dưới.

'''
parts = sorted(folder.glob('[0-5][0-9]_*.md'))
assert len(parts) == 6
for part in parts:
    body = part.read_text(encoding='utf-8')
    if not body.startswith('> CẬP NHẬT ƯU TIÊN'):
        body = notice + body
    part.write_text(body, encoding='utf-8')
    part.with_suffix('.txt').write_text(body, encoding='utf-8')
(folder / 'TAT_CA_PROMPT_VI.txt').write_text('\n\n'.join('====================\n'+p.name+'\n====================\n\n'+p.read_text(encoding='utf-8') for p in parts), encoding='utf-8')
print('Updated six prompts and copy-once bundle with the latest ranged/beam/HP/camera requirements.')
