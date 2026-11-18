from pathlib import Path
import hashlib
import re
import json
from collections import defaultdict
base=Path.cwd()
assets=base/'Assets'
roots=['Assets','ProjectSettings','Packages','SourceArt','Docs','Tools','GeneratedAssets']
paths=[p for loc in roots for p in (base/loc).rglob('*') if p.is_file() and not any(x.is_symlink() or x.is_junction() for x in p.parents if x!=base)]
scene_paths=[p for p in assets.rglob('*.unity')]
ext={'.unity','.prefab','.asset','.mat','.anim','.controller','.overridecontroller','.meta','.rendertexture','.playable','.shadergraph','.spriteatlas'}
incoming=defaultdict(set)
guid_pattern=re.compile(rb'guid:\s*([a-f0-9]{32})')
defined={}
for p in paths:
 if p.suffix.lower() not in ext:continue
 data=p.read_bytes()
 if b'\0' in data[:1000]:continue
 refs=guid_pattern.findall(data)
 if p.suffix=='.meta' and refs:defined[Path(str(p)[:-5])]=refs.pop(0)
 for g in refs:incoming[g].add(p.relative_to(base).as_posix())

scope=(assets/'GundamLobby/Backups',assets/'Sandbox/StrikeRougeCombat/PreservedScenes',assets/'Models/HaoLing/Backups')
groups=[p for folder in scope for p in folder.rglob('*') if p.is_file() and p.suffix!='.meta']
groups += list((base/'SourceArt').rglob('*.blend1'))
groups += [assets/p for p in ['Sandbox/UnicornCombat/Scenes/Unicorn Demo.unity','Sandbox/StrikeRougeCombat/Scenes/StrikeRougeCombatDemo.unity']]
for p in groups:
 found=sorted(incoming.get(defined.get(p),set()))
 print('CANDIDATE',p.relative_to(base).as_posix(),'|',len(found),','.join(found[:10]))
sizes=defaultdict(list)
for p in paths:
 if p.is_relative_to(assets) and p.suffix not in {'.meta','.cs','.md'} and p.stat().st_size>0:sizes[p.stat().st_size].append(p)
collisions=defaultdict(list)
for values in sizes.values():
 if len(values)>1:
  for p in values:collisions[hashlib.file_digest(p.open('rb'),'sha256').hexdigest()].append(p)
pairs=[v for v in collisions.values() if len(v)>1]
print('COUNTS',len(paths),len(scene_paths),len(groups),len(pairs))
for pp in pairs:
 print('MATCH',' | '.join(p.relative_to(base).as_posix()+' ('+str(len(incoming.get(defined.get(p),set())))+' refs)' for p in pp))
