import bpy, json, os
from mathutils import Vector
root=os.getcwd()
src=os.path.join(root,'Assets','Models','rx-0-full-armor-unicorn-gundam','source','RX-0 Full Armor Unicorn Gundam','untitled.fbx')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=src)
rows=[]
for o in bpy.context.scene.objects:
    d={'name':o.name,'type':o.type,'parent':o.parent.name if o.parent else None}
    if o.type=='MESH':
        pts=[o.matrix_world@Vector(v) for v in o.bound_box]
        d.update(vertices=len(o.data.vertices), faces=len(o.data.polygons), bounds=[[round(f(v[i] for v in pts),4) for i in range(3)] for f in (min,max)], materials=[m.name if m else None for m in o.data.materials])
    rows.append(d)
with open('work/unicorn_donor/objects.json','w',encoding='utf8') as f: json.dump(rows,f,ensure_ascii=False,indent=2)
print('DONOR',len(rows))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'work','unicorn_donor','donor.blend'))
