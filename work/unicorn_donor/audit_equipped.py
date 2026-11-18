import bpy, json
from pathlib import Path
root=Path.cwd()
bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_Equipped_v02.blend'))
rig=bpy.data.objects['Unicorn_PrototypeRig']
for b in rig.data.bones:
    print(b.name, tuple(round(x,3) for x in b.head_local), tuple(round(x,3) for x in b.tail_local))
for o in bpy.data.collections['Unicorn_Equipment'].objects:
    pts=[o.matrix_world@v.co for v in o.data.vertices]
    print(o.name, list(o.matrix_world.translation), 'tip',list(min(pts,key=lambda v:v.z)), 'mat',[(m.name,[(n.image.name,n.image.filepath) for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]) for m in o.data.materials])
print('BINDING', next(o for o in bpy.data.collections['Unicorn_Import'].objects if o.type=='MESH').items())
