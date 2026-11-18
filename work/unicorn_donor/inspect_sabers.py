import bpy
from pathlib import Path
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath=str(Path.cwd()/'SourceArt/Blender/Unicorn/Unicorn_Combat_v01.blend'))
rig=bpy.data.objects['Unicorn_PrototypeRig']
bpy.context.scene.frame_set(1)
for b in rig.pose.bones:
 print('BONE',b.name,tuple(round(n,3) for n in rig.matrix_world@b.head),tuple(round(n,3) for n in rig.matrix_world@b.tail),flush=True)
for o in bpy.data.collections['Unicorn_Import'].objects:
 if o.type!='MESH':continue
 vs=[o.matrix_world@Vector(v) for v in o.bound_box]
 c=sum(vs,Vector())/len(vs)
 if ('saber' in o.name.lower() or 'sabre' in o.name.lower() or (c.y>.1 and c.z>1.92)):
  print('PART',o.name,len(o.data.vertices),[tuple(round(v[a],3) for a in range(3)) for v in (c,Vector(min(v[a] for v in vs) for a in range(3)),Vector(max(v[a] for v in vs) for a in range(3)))],flush=True)
