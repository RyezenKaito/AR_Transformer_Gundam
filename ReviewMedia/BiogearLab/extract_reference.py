from pathlib import Path
import json
import cv2
from PIL import Image, ImageDraw, ImageFont

SOURCE = Path(r'D:\Downloads\YTSave.to_YouTube_Media__iGI-b0dEvY_Making-of-Biogear-Lab-Map-Mech-Arena_001_1080p.mp4')
OUT = Path(__file__).parent
cap = cv2.VideoCapture(str(SOURCE))
fps = cap.get(cv2.CAP_PROP_FPS)
duration = cap.get(cv2.CAP_PROP_FRAME_COUNT) / fps
times = list(range(3, 153, 6))
font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 20)
entries = []
for page in range(3):
    subset = times[page * 9:(page + 1) * 9]
    if not subset:
        continue
    sheet = Image.new('RGB', (1440, 3 * 294), '#18212b')
    draw = ImageDraw.Draw(sheet)
    for i, sec in enumerate(subset):
        cap.set(cv2.CAP_PROP_POS_MSEC, sec * 1000)
        ok, frame = cap.read()
        if not ok:
            continue
        im = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
        name = f'frame_{sec:03d}s.jpg'
        im.save(OUT / name, quality=94)
        im.thumbnail((480, 270))
        x, y = (i % 3) * 480, (i // 3) * 294
        sheet.paste(im, (x, y))
        draw.text((x + 8, y + 269), f'{sec // 60:02d}:{sec % 60:02d}', fill='white', font=font)
        entries.append({'seconds': sec, 'file': name})
    sheet.save(OUT / f'contact_sheet_{page + 1}.jpg', quality=93)
cap.release()
(OUT / 'reference_manifest.json').write_text(json.dumps({'source': str(SOURCE), 'fps': fps, 'duration_seconds': duration, 'sampled_frames': entries}, indent=2), encoding='utf-8')
print(json.dumps({'duration_seconds': duration, 'sampled_frames': len(entries), 'output': str(OUT)}))
