from pathlib import Path
import bpy
from mathutils import Vector

root = Path.cwd()
source = (root / 'work/unicorn_donor/equip_unicorn.py').read_text(encoding='utf8')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(root / 'Assets/Models/rx-0-full-armor-unicorn-gundam/source/RX-0 Full Armor Unicorn Gundam/untitled.fbx'))
ns = {}
exec('import bpy\n'+source[source.index('def islands('):source.index("shield=extract(")],ns)
donor=bpy.data.objects['1']
originals=list(bpy.context.scene.objects)
for o in originals:o.hide_render=True
for i in range(len(ns['islands'](donor))):
    obj=ns['extract'](donor,[i],f'Part_{i:02}')
    obj.data.transform(donor.matrix_world)
    points=[v.co.copy() for v in obj.data.vertices]
    lo=Vector(tuple(min(v[k] for v in points) for k in range(3)))
    hi=Vector(tuple(max(v[k] for v in points) for k in range(3)))
    from mathutils import Matrix
    obj.data.transform(Matrix.Translation(-(lo+hi)/2))
    obj.rotation_euler.z=0
    obj.scale=(.72/max(.23,(hi-lo).length),)*3
    obj.location=(0,(i%10)-4.5,2-(i//10)+.06)
    curve=bpy.data.curves.new(f'ID{i}','FONT')
    curve.body=str(i);curve.size=.15
    txt=bpy.data.objects.new(f'ID{i}',curve);bpy.context.scene.collection.objects.link(txt)
    txt.rotation_euler=(1.5707963,0,1.5707963)
    txt.location=(.2,(i%10)-4.85,2-i//10-.39)
scene=bpy.context.scene
cam=bpy.data.objects.new('Camera',bpy.data.cameras.new('Camera'))
scene.collection.objects.link(cam);scene.camera=cam
cam.location=(20,0,0)
cam.rotation_euler=(-cam.location).to_track_quat('-Z','Y').to_euler()
cam.data.type='ORTHO';cam.data.ortho_scale=10
scene.render.engine='BLENDER_WORKBENCH'
scene.display.shading.color_type='TEXTURE'
scene.display.shading.light='STUDIO'
scene.render.resolution_x=1700;scene.render.resolution_y=850;scene.render.resolution_percentage=100
scene.render.filepath=str(root/'work/unicorn_donor/shield_atlas.png')
import json
records=[]
for i, faces in enumerate(ns['islands'](donor)):
    data=donor.data
    records.append([[[list(donor.matrix_world@data.vertices[data.loops[j].vertex_index].co),list(data.uv_layers.active.data[j].uv)] for j in data.polygons[k].loop_indices] for k in faces])
(root/'work/unicorn_donor/shield_atlas.json').write_text(json.dumps(records))
