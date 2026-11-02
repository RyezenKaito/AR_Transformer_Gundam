from pathlib import Path
import shutil

project = Path(__file__).resolve().parent.parent
folder = project / 'Prompts/2026-10-29-Camera-Flight-Weapons'
parts = sorted(folder.glob('[0-5][0-9]_*.md'))
assert len(parts) == 6, f'Expected master + five prompts, got {len(parts)}'
for part in parts:
    part.with_suffix('.txt').write_text(part.read_text(encoding='utf-8'), encoding='utf-8')
(folder / 'TAT_CA_PROMPT_VI.txt').write_text(
    '\n\n' + '\n\n' .join(['====================\n' + part.name + '\n====================\n\n' + part.read_text(encoding='utf-8') for part in parts]),
    encoding='utf-8')
shutil.copy2(Path('C:/Users/LENOVO/AppData/Local/Temp/codex-clipboard-79cb825c-8394-4550-b568-37bb466b4d4a.png'), folder/'References/DefaultCamera.png')

# Preserve earlier specifications as historical references and clearly redirect their readers.
older = [
    'Prompts/GUNDAM_TURN_BASED_MASTER_PROMPT_VI.md', 'Prompts/GUNDAM_TURN_BASED_MASTER_PROMPT_VI.txt',
    'Prompts/GUNDAM_MASTER_PROMPT_VI.md', 'Prompts/GUNDAM_MASTER_PROMPT_VI.txt',
    'Prompts/PHAN_TICH_CO_CHE_DANH_THEO_LUOT_VI.md', 'Prompts/HUONG_DAN_SU_DUNG_PROMPT.md',
    'Prompts/EarthPrison/EARTH_PRISON_MASTER_PROMPT_VI.md', 'Prompts/EarthPrison/EARTH_PRISON_MASTER_PROMPT_VI.txt',
    'Prompts/EarthPrison/README_VI.md',
]
marker = '> ĐẶC TẢ MỚI NHẤT — 29/10/2026'
for relative in older:
    file = project / relative
    original = file.read_text(encoding='utf-8-sig')
    if original.startswith(marker):
        continue
    link = ('../' if '/EarthPrison/' in relative else '') + '2026-10-29-Camera-Flight-Weapons/00_MASTER_PROMPT_VI.md'
    header = (marker + '\n> Đọc [master mới](' + link + ') và các prompt 01–05 cùng thư mục. '
              'Camera mặc định sau lưng nhưng xoay 360°; Attack, Skill và Ultimate đều bay tới phía địch rồi trở về; '
              'khớp chuyển động mượt và vũ khí chỉ rút khi dùng.\n> '
              'Phần bên dưới là tài liệu ngày trước được giữ để tham chiếu. Các ràng buộc camera fixed, '
              'root tuyệt đối đứng yên khi đánh, skill/ultimate đứng tại chỗ hoặc súng cầm sẵn đã được thay thế.\n\n---\n\n')
    file.write_text(header + original, encoding='utf-8')
print(f'Bundled {len(parts)} documents; copied camera reference; marked {len(older)} older documents as superseded.')
