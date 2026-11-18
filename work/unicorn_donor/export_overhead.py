import bpy
from pathlib import Path
root=Path.cwd()
bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_Combat_v03_Overhead.blend'))
rig=bpy.data.objects['Unicorn_PrototypeRig']
gear=bpy.data.collections['Unicorn_Equipment']
muzzle=bpy.data.objects['Muzzle']
for img in bpy.data.images:
    if img.source=='FILE':
        img.buffers_free()
# Group original rigid panels by bone, while retaining the two donor meshes.
from collections import defaultdict
groups=defaultdict(list)
for o in bpy.data.collections['Unicorn_Import'].objects:
    if o.type=='MESH':
        group=o.get('prototype_bone') or o.vertex_groups[0].name
        groups[group].append(o)
bpy.ops.object.select_all(action='DESELECT')
combined=[]
for bone,parts in groups.items():
    for o in parts:o.hide_set(False);o.select_set(True)
    bpy.context.view_layer.objects.active=parts[0]
    if len(parts)>1:bpy.ops.object.join()
    parts[0].name='Armor_'+bone.replace('.','_')
    parts[0].select_set(False);combined.append(parts[0])
for a in list(bpy.data.actions):
    if not a.name.startswith('Unicorn_'):bpy.data.actions.remove(a)
for o in [rig,*combined,*list(gear.objects)]:o.select_set(True)
bpy.context.view_layer.objects.active=rig
out=root/'work/unicorn_donor/overhead_export'
out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out/'Unicorn_Base.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,mesh_smooth_type='FACE')
print('COMBAT_EXPORT',len(combined),len(bpy.data.actions),muzzle.name)



