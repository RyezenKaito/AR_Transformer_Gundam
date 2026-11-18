import bpy, math, json, bmesh
from pathlib import Path
from mathutils import Vector, Quaternion, Matrix
root=Path.cwd()
source=root/'SourceArt/Blender/Unicorn/Unicorn_Equipped_v02.blend'
bpy.ops.wm.open_mainfile(filepath=str(source))
scene=bpy.context.scene
rig=bpy.data.objects['Unicorn_PrototypeRig']
scene.render.fps=30
rig.animation_data.action=bpy.data.actions['Unicorn_Idle']
scene.frame_set(1)
gear=bpy.data.collections['Unicorn_Equipment']
rifle=bpy.data.objects['Unicorn_Donor_Rifle']
shield=bpy.data.objects['Unicorn_Donor_Shield']
rifle.data.materials[0].name='UnicornEquipmentAtlas'

def socket(name,point,parent,bone=None):
    o=bpy.data.objects.new(name,None)
    gear.objects.link(o);o.empty_display_size=.045
    o.parent=parent
    if bone:o.parent_type='BONE';o.parent_bone=bone
    bpy.context.view_layer.update()
    o.matrix_world=Matrix.Translation(Vector(point))
    return o

verts=[rifle.matrix_world@v.co for v in rifle.data.vertices]
grip=rifle.matrix_world.translation
aim0=Vector((-.29,-.90,-.39)).normalized()
maxproj=max((v-grip).dot(aim0) for v in verts)
lip=[v for v in verts if (v-grip).dot(aim0)>maxproj-.025]
muzzle=socket('Muzzle',sum(lip,Vector())/len(lip),rifle)
socket('Eject',grip+Vector((-.115,-.13,.10)),rifle)
socket('ShieldCore',(.86,-.19,1.71),shield)
socket('Vulcan_L',(.078,-.129,2.48),rig,'Head')
socket('Vulcan_R',(-.078,-.129,2.48),rig,'Head')
socket('Impact_Chest',(0,-.275,2.15),rig,'Chest')
socket('CameraFocus_Head',(0,-.04,2.48),rig,'Head')
socket('CameraFocus_Body',(0,-.05,1.77),rig,'Chest')
socket('Ground',(0,0,-.049),rig,'Root')
tonfa=socket('Tonfa_Pivot',(.68,-.085,1.72),rig,'Forearm.L')
socket('Tonfa_Base',(.68,-.13,1.67),tonfa)
socket('Tonfa_Tip',(.68,-.70,1.41),tonfa)

# Compact mechanical hilt aligned with the beam sockets.
material=bpy.data.materials.new('UnicornTonfaMetal');material.diffuse_color=(.13,.17,.21,1)
for name,point,size in [('Tonfa_Housing',(.68,-.083,1.725),(.09,.14,.10)),('Tonfa_Emitter',(.68,-.145,1.67),(.08,.045,.076))]:
    bpy.ops.mesh.primitive_cube_add(size=1,location=point)
    o=bpy.context.object;o.name=name;o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bevel=o.modifiers.new('Chamfer','BEVEL');bevel.width=.012;bevel.segments=2
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    o.data.materials.append(material)
    gear.objects.link(o)
    for c in list(o.users_collection):
        if c!=gear:c.objects.unlink(o)
    world=o.matrix_world.copy();o.parent=tonfa;bpy.context.view_layer.update();o.matrix_world=world

def rest_axis(b,axis):
    return b.bone.matrix_local.to_3x3().inverted()@Vector(axis)

def pose(frame, entries, raise_rifle=False, recoil=0, deploy=0):
    scene.frame_set(frame)
    for b in rig.pose.bones:
        b.rotation_mode='QUATERNION';b.rotation_quaternion=Quaternion()
        angles=entries.get(b.name,(0,0,0))
        q=Quaternion()
        for a,axis in zip(angles,((1,0,0),(0,1,0),(0,0,1))):
            q=q@Quaternion(rest_axis(b,axis),math.radians(a))
        b.rotation_quaternion=q
        b.location=(0,0,0)
    tonfa.rotation_mode='XYZ';tonfa.rotation_euler=(math.radians(deploy),0,0)
    bpy.context.view_layer.update()
    if raise_rifle:
        h=rig.pose.bones['Hand.R']
        current=(muzzle.matrix_world.translation-rifle.matrix_world.translation).normalized()
        target=Vector((0,-1,.09+recoil)).normalized()
        world=(rig.matrix_world@h.matrix).to_quaternion()
        delta=current.rotation_difference(target)
        h.rotation_quaternion=h.rotation_quaternion@(world.inverted()@delta@world)
    for b in rig.pose.bones:
        b.keyframe_insert('rotation_quaternion',frame=frame,group=b.name)
        b.keyframe_insert('location',frame=frame,group=b.name)
    if deploy:
        # Hilt rotation is driven by Unity from the same authored cue times.
        tonfa.rotation_euler=(0,0,0)

aim={'Chest':(3,0,-11),'Head':(-3,0,10),'UpperArm.R':(-47,-11,2),'Forearm.R':(-40,0,0),'UpperArm.L':(-17,4,-4)}
hit={'Chest':(-7,0,-16),'Head':(1,0,14),'UpperArm.R':(-44,-14,0),'Forearm.R':(-35,0,0),'UpperArm.L':(-20,5,-5)}
guard={'Chest':(7,0,10),'Head':(-4,0,-9),'UpperArm.L':(-39,0,-10),'Forearm.L':(-53,4,0),'UpperArm.R':(-9,0,0)}
wind={'Chest':(7,-5,-27),'Head':(-5,2,23),'UpperArm.L':(-25,0,32),'Forearm.L':(-55,0,0),'UpperArm.R':(-8,0,10)}
slash={'Chest':(12,4,31),'Head':(-9,-3,-24),'UpperArm.L':(-56,-7,-23),'Forearm.L':(-18,0,0),'UpperArm.R':(-16,5,-10)}
reverse={'Chest':(8,-4,-27),'Head':(-7,3,21),'UpperArm.L':(-38,8,32),'Forearm.L':(-29,0,0)}
specs={
 'TwinVulcan':[(0,{}),(6,{'Head':(-4,0,0),'Chest':(3,0,0)}),(10,{'Head':(-1,0,0),'Chest':(1,0,0)}),(12,{'Head':(-3,0,0),'Chest':(2,0,0)}),(16,{'Head':(-1,0,0)}),(27,{})],
 'BeamTonfa':[(0,{}),(10,wind),(16,wind),(20,slash),(24,slash),(27,slash),(32,reverse),(36,reverse),(43,guard),(51,{})],
 'BeamMagnum':[(0,{}),(9,{'UpperArm.R':(-24,-6,1),'Forearm.R':(-22,0,0)}),(15,aim,True),(20,aim,True),(23,hit,True,.23),(27,hit,True,.1),(34,aim,True),(48,{})],
 'FullBurst':[(0,{}),(23,{'Chest':(2,0,-9),'Head':(8,0,9),'UpperArm.L':(-8,0,12),'UpperArm.R':(-8,0,-12)}),(45,{'Chest':(-3,0,2),'Head':(-8,0,-2),'UpperArm.L':(-16,0,14),'UpperArm.R':(-18,0,-14)}),(58,{'Chest':(5,0,-12),'Head':(-3,0,8),'UpperArm.R':(-22,0,-7)}),(78,aim,True),(96,aim,True),(101,hit,True,.37),(106,hit,True,.18),(115,aim,True),(138,{})],
 'Deploy':[(0,{'Chest':(13,0,0),'Head':(-10,0,0),'UpperArm.L':(-13,0,15),'UpperArm.R':(-13,0,-15)}),(15,{'Chest':(17,0,0),'Head':(-14,0,0),'UpperArm.L':(-18,0,20),'UpperArm.R':(-20,0,-16)}),(21,{'Chest':(10,0,0),'Head':(-8,0,0)}),(36,{})],
 'Dodge':[(0,{}),(5,{'Chest':(6,-14,0),'Head':(-4,10,0),'UpperArm.L':(-10,0,8)}),(11,{'Chest':(7,13,0),'Head':(-5,-10,0),'UpperArm.R':(-11,0,-8)}),(18,{})],
 'Parry':[(0,{}),(5,guard),(8,{'Chest':(-5,0,8),'UpperArm.L':(-34,0,-8),'Forearm.L':(-48,4,0)}),(12,guard),(17,{})],
 'Defeat':[(0,{}),(7,{'Chest':(-11,3,-3),'Head':(8,0,5)}),(17,{'Chest':(23,8,6),'Head':(-17,0,-6),'UpperArm.L':(-12,0,14),'UpperArm.R':(11,0,-13)}),(30,{'Chest':(34,13,8),'Head':(-21,0,-8),'UpperArm.L':(-16,0,12),'UpperArm.R':(15,0,-15)})]
}
for name,keys in specs.items():
    action=bpy.data.actions.new('Unicorn_'+name)
    rig.animation_data.action=action
    for key in keys:pose(key[0]+1,*key[1:])
    action.use_fake_user=True

rig.animation_data.action=bpy.data.actions['Unicorn_Idle']
for b in rig.pose.bones:
    b.rotation_mode='XYZ'
    b.rotation_quaternion=Quaternion()
    b.rotation_euler=(0,0,0)
    b.location=(0,0,0)
# Use quaternion curves for every clip, including the two existing Euler actions.
for name in ('Idle','HitReact'):
    old=bpy.data.actions['Unicorn_'+name]
    rig.animation_data.action=old
    first,last=map(int,old.frame_range)
    baked=[]
    for frame in range(first,last+1):
        scene.frame_set(frame)
        baked.append([(b.rotation_euler.to_quaternion(),b.location.copy()) for b in rig.pose.bones])
    old.name='Source_'+old.name
    old.use_fake_user=True
    action=bpy.data.actions.new('Unicorn_'+name)
    rig.animation_data.action=action
    for i,values in enumerate(baked):
        for b,(q,loc) in zip(rig.pose.bones,values):
            b.rotation_mode='QUATERNION';b.rotation_quaternion=q;b.location=loc
            b.keyframe_insert('rotation_quaternion',frame=i+1,group=b.name)
            b.keyframe_insert('location',frame=i+1,group=b.name)
    action.use_fake_user=True
rig.animation_data.action=bpy.data.actions['Unicorn_Idle']
scene.frame_start=1;scene.frame_end=139;scene.frame_set(1)
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D': area.spaces.active.shading.type='SOLID';area.spaces.active.shading.color_type='TEXTURE'
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_Combat_v01.blend'),compress=True)

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
out=root/'Assets/Game/Features/Characters/Unicorn/Art'
out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out/'Unicorn_Base.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',add_leaf_bones=False,bake_anim=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,bake_anim_force_startend_keying=True,bake_anim_simplify_factor=0,mesh_smooth_type='FACE')
print('COMBAT_EXPORT',len(combined),len(bpy.data.actions),muzzle.name)
