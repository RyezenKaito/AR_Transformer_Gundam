from pathlib import Path
p=Path('Assets/TurnBasedBattle/Scripts/Runtime')
f=p/'BattleDefinition.cs'
s=f.read_text(encoding='utf-8')
a=s.index('    [CreateAssetMenu(menuName = "Turn Battle/Action")]')
b=s.index('    [CreateAssetMenu(menuName = "Turn Battle/Tuning")]')
c=s.index('    /// <summary>Pure per-hit')
for name,body in [('BattleActionDefinition',s[a:b]),('BattleTuning',s[b:c])]:
    (p/(name+'.cs')).write_text('using System;\nusing UnityEngine;\nnamespace TurnBasedBattle\n{\n'+body+'}\n',encoding='utf-8')
f.write_text(s[:a]+s[c:],encoding='utf-8')
print('Split configuration script assets.')
