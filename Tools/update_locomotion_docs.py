from pathlib import Path
root=Path(__file__).resolve().parents[1]
folder=root/'Prompts/2026-10-29-Camera-Flight-Weapons'
notice='''> CẬP NHẬT CHUYỂN ĐỘNG — 29/10/2026
> Mỗi action có kiểu tiếp cận riêng: **Run / Fly / Stationary**. Attack kiếm và đòn cận chiến của địch dùng **chạy bộ áp sát**, chân đổi nhịp theo quãng đường, có chân trụ, lấy đà, hãm và bước quay người trước khi chạy về. Không dùng đường nhảy cho chạy bộ. Earth Prison dùng **bay** với nâng lên–lướt–hạ cánh mượt, có booster và tư thế toàn thân. Súng/tầm xa vẫn đứng tại chỗ. Đây là quy tắc ưu tiên mới nhất, thay thế các dòng cũ yêu cầu mọi đòn kiếm bay tới.

'''
parts=sorted(folder.glob('[0-5][0-9]_*.md'))
for p in parts:
    body=p.read_text(encoding='utf-8')
    if not body.startswith('> CẬP NHẬT CHUYỂN ĐỘNG'): body=notice+body
    p.write_text(body,encoding='utf-8');p.with_suffix('.txt').write_text(body,encoding='utf-8')
(folder/'TAT_CA_PROMPT_VI.txt').write_text('\n\n'.join('====================\n'+p.name+'\n====================\n\n'+p.read_text(encoding='utf-8') for p in parts),encoding='utf-8')
p=root/'Assets/TurnBasedBattle/README_VI.md'
body=p.read_text(encoding='utf-8').replace('kiếm ánh sáng, bay áp sát/chém/trở về/cất kiếm','kiếm ánh sáng, chạy áp sát/chém/chạy trở về/cất kiếm')
body=body.replace('CombatReview_20261029.webm','LocomotionReview_20261029.webm').replace('Reports/Review_01…13','Reports/Review_01…15')
body+='''
## Cập nhật chạy bộ / bay theo từng chiêu

`BattleActionDefinition.travelStyle`: Stationary, Run, Fly. `approachTarget` cho phép tiếp cận; Stationary đi cùng approachTarget=false. `runStride` là quãng đường mỗi chu kỳ chân, mặc định 1.65 đơn vị bàn đấu. Nhịp chân dựa trên quãng đường thực, không chạy animation cố định trong khi root đi với tốc độ khác. `BattleLocomotionPose` ghép chuyển động thân/tay và IK chân vào action pose, trước bước gắn vũ khí.

Attack / EnemyStrike mới: chuẩn bị+rút 0–0.65s; chạy đi 0.65–2.65; hit 3.05; quay/chạy về 3.65–5.95; cất và ổn định đến 6.90s. Blade bật 2.48, tắt 3.35. Cửa sổ né đòn cận chiến địch 2.80–3.00s. Sát thương, thanh ultimate và cooldown không đổi. Earth Prison giữ các marker damage/VFX cũ; đường bay mới có đoạn lướt giữ độ cao thay vì một cung nhảy.

Ảnh `Review_14_Run.png` và `Review_15_Flight.png` minh họa hai loại di chuyển. Clip LocomotionReview là bản mới; CombatReview lưu bản trước để đối chiếu. Chất lượng chuyển động còn có thể tinh chỉnh theo cảm nhận khi chơi, nhất là khi thay model/tỉ lệ chân khác.
'''
p.write_text(body,encoding='utf-8')
print('Updated bundle and runtime guide for per-action Run/Fly/Stationary locomotion.')
