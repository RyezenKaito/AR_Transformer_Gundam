from pathlib import Path
import re
root=Path('Assets/TurnBasedBattle')
for name in ['BattleTuning','BasicAttack','EnergyCannon','Guard','EarthPrison','EnemyStrike','EnemyCombo']:
    script='BattleTuning' if name=='BattleTuning' else 'BattleActionDefinition'
    guid=re.search(r'guid: (\w+)',(root/f'Scripts/Runtime/{script}.cs.meta').read_text()).group(1)
    path=root/f'Config/{name}.asset'
    text=path.read_text(encoding='utf-8')
    text=re.sub(r'm_Script: \{[^}]+\}',f'm_Script: {{fileID: 11500000, guid: {guid}, type: 3}}',text)
    path.write_text(text,encoding='utf-8')
    print(name,guid)
