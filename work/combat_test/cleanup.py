from pathlib import Path
import re, zipfile
root=Path.cwd().resolve()
def path(s):
 p=root/s
 assert p.resolve().is_relative_to(root) and not any(x.is_symlink() or x.is_junction() for x in [p,*p.parents] if x!=root),p
 return p
C='Assets/Game/Features/Characters/'
K='Assets/Game/Features/Combat/'
S='Assets/Sandbox/'
U=S+'UnicornCombat/'
R=K+'Scripts/StrikeRougeDemo/'
L='Assets/GundamLobby/'
N='Assets/Game/Features/MainMenu/'
moves={
 U+'Runtime':C+'Unicorn/Runtime',U+'Unicorn Psycho-Frame Buff.asset':C+'Unicorn/Data/Unicorn Psycho-Frame Buff.asset',
 K+'UI/Unicorn':C+'Unicorn/UI',K+'Animations/StrikeRouge':C+'StrikeRouge/Animations',
 K+'UI/StrikeRouge/Prefabs':K+'UI/Shared/Prefabs',
 K+'UI/StrikeRouge/Art':C+'StrikeRouge/UI/Art',
 K+'UI/StrikeRouge/StrikeRougeHudArt.asset':C+'StrikeRouge/UI/StrikeRougeHudArt.asset',
 U+'Materials':S+'CombatTest/Materials',L+'Scenes/HomeLobby.unity':'Assets/Game/Scenes/HomeLobby.unity',
 S+'UnicornMotion':S+'Showcases/UnicornMotion'
}
for key in ('BlueFlame','HaoLing','SkyDefender'):
 for p in (root/f'Assets/Models/{key}/Scenes').glob('*.unity'):
  moves[p.relative_to(root).as_posix()]=f'{S}Showcases/{key}/Scenes/{p.name}'
for p in path(R).glob('*.cs'):
 dst='Assets/Game/Features/AR/Scripts/' if p.stem=='RougeArenaPlacement' else K+'Scripts/' if p.stem in ['DemoFighter','RougeTurnOrder','MechaBattleHUD','MechaPlateGraphic','RougeHudArt','RougeArenaCamera'] else C+'StrikeRouge/Runtime/'
 moves[R+p.name]=dst+p.name
doc=path('Docs/Planning/Combat Test Architecture and Cleanup.md').read_text(encoding='utf-8-sig')
deletes=re.findall(r'^- `([^`]+)`$',doc.split('### Nhóm A')[1].split('### Nhóm B')[0],re.M)
assert len(deletes)==39
for p in path(L).iterdir():
 if p.suffix=='.meta' or p.name in ['Backups','Previews','Scenes']:continue
 moves[L+p.name]=N+'Lobby/'+p.name
for p in path(L+'Previews').iterdir():
 key=p.relative_to(root).as_posix()
 if p.suffix=='.meta' or key in deletes:continue
 moves[key]='SourceArt/Reviews/Lobby/'+p.name
for a,b in moves.items():
 assert path(a).exists() and not path(b).exists(),(a,b)
archive=path('Archive/CombatCleanup/Approved-rollback.zip')
archive.parent.mkdir(parents=True,exist_ok=True)
assert not archive.exists()
with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=2) as z:
 for key in deletes:
  for name in [key,key+'.meta']:
   if path(name).is_file():z.write(path(name),name)
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
for source,dest in moves.items():
 a,b=path(source),path(dest)
 b.parent.mkdir(parents=True,exist_ok=True)
 a.rename(b)
 am=path(source+'.meta')
 if am.is_file():am.rename(path(dest+'.meta'))
pairs=dict(moves)
pairs[L]=N+'Lobby/'
pairs['Assets/GundamLobby']=N+'Lobby'
pairs[K+'UI/StrikeRouge']=C+'StrikeRouge/UI'
pairs[U+'Materials']=S+'CombatTest/Materials'
for folder in ('BlueFlame','HaoLing','SkyDefender'):
 # Editor builders concatenate Root + /Scenes; fix separately below.
 pass
pat=re.compile('|'.join(re.escape(k) for k in sorted(pairs,key=len,reverse=True)))
for base in ['Assets','ProjectSettings','Tools']:
 for f in path(base).rglob('*'):
  if f.suffix not in ['.cs','.ps1','.asset','.json','.md'] or not f.is_file():continue
  data=f.read_bytes()
  try:s=data.decode('utf-8')
  except UnicodeError:continue
  t=pat.sub(lambda m:pairs[m.group()],s)
  if f.name=='UnicornCombatAuthoring.cs':
   t=t.replace('Demo+"/Materials"','"Assets/Sandbox/CombatTest/Materials"').replace('Demo+"/Materials/','"Assets/Sandbox/CombatTest/Materials/').replace('Demo+"/Unicorn Psycho-Frame Buff.asset"','"Assets/Game/Features/Characters/Unicorn/Data/Unicorn Psycho-Frame Buff.asset"')
  if f.name=='RougeHudAuthoring.cs':t=t.replace('Folder+"/Prefabs','"Assets/Game/Features/Combat/UI/Shared/Prefabs')
  showcase={'BlueFlameBuilder.cs':'BlueFlame','HaoLingBuilder.cs':'HaoLing','SkyDefenderBuilder.cs':'SkyDefender'}.get(f.name)
  if showcase:
   t=re.sub(r'Root\s*\+\s*"/Scenes/',f'"Assets/Sandbox/Showcases/{showcase}/Scenes/',t)
  if 'HomeLobby.unity' in t:
   t=t.replace('Root+"/Scenes/HomeLobby.unity"','"Assets/Game/Scenes/HomeLobby.unity"').replace('Root + "/Scenes/HomeLobby.unity"','"Assets/Game/Scenes/HomeLobby.unity"').replace(N+'Lobby/Scenes/HomeLobby.unity','Assets/Game/Scenes/HomeLobby.unity')
  if t!=s:f.write_bytes(t.encode('utf-8'))
for key in deletes:
 for name in [key,key+'.meta']:
  p=path(name)
  if p.is_file():p.unlink()
print('Moved',len(moves),'approved groups; archived and removed',len(deletes),'files')
