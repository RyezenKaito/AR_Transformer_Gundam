from pathlib import Path
import math
import bpy
from mathutils import Matrix, Quaternion, Vector

root=Path.cwd()
bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_Combat_v01.blend'))
scene=bpy.context.scene
rig=bpy.data.objects['Unicorn_PrototypeRig']
gear=bpy.data.collections['Unicorn_Equipment']
scene.frame_set(1)
for o in list(gear.objects):
    if o.name.startswith('Tonfa_'):bpy.data.objects.remove(o,do_unlink=True)
controls=('RifleMount','Magnum_BackpackSocket','Magnum_RightHandSocket')
bpy.context.view_layer.objects.active=rig
bpy.ops.object.mode_set(mode='EDIT')
for name in controls:
    b=rig.data.edit_bones.new(name)
    b.head=(0,0,0);b.tail=(0,0,.12);b.use_deform=False
bpy.ops.object.mode_set(mode='OBJECT')
# World-space axes are consistent across the rigid body and the control bones.
def translate(v):
    return Matrix.Translation(Vector(v))
def rotation(axis, angle):
    return Quaternion(Vector(axis),math.radians(angle))
def rig_world(b):
    return rig.matrix_world @ b.matrix
def apply_delta(b, delta):
    orientation=rig_world(b).to_quaternion()
    b.rotation_quaternion=b.rotation_quaternion @ (orientation.inverted() @ delta @ orientation)
    bpy.context.view_layer.update()
def pose(entries):
    for b in rig.pose.bones:
        if b.name in controls:continue
        b.rotation_mode='QUATERNION'
        b.location=(0,0,0);b.scale=(1,1,1)
        q=Quaternion()
        for a,axis in zip(entries.get(b.name,(0,0,0)),((1,0,0),(0,1,0),(0,0,1))):
            local=b.bone.matrix_local.to_3x3().inverted()@Vector(axis)
            q=q@rotation(local,a)
        b.rotation_quaternion=q
    bpy.context.view_layer.update()

def aim_chain(names, goal, outward):
    upper=rig.pose.bones[names[0]]
    fore=rig.pose.bones[names[1]]
    hand=rig.pose.bones[names[2]]
    a,b,c=(rig.matrix_world @ x.head for x in (upper,fore,hand))
    l1,l2=(b-a).length,(c-b).length
    axis=(goal-a).normalized()
    reach=max(abs(l1-l2)+.002,min((goal-a).length,(l1+l2)*.997))
    end=a+axis*reach
    pole=Vector(outward);pole=(pole-axis*pole.dot(axis)).normalized()
    projected=(l1*l1+reach*reach-l2*l2)/(2*reach)
    elbow=a+axis*projected+pole*math.sqrt(max(0,l1*l1-projected*projected))
    apply_delta(upper,(b-a).rotation_difference(elbow-a))
    b,c=(rig.matrix_world @ x.head for x in (fore,hand))
    apply_delta(fore,(c-b).rotation_difference(end-b))
    return rig.matrix_world @ hand.head

def sequence(frames, t):
    if t<=frames[0][0]:return Vector(frames[0][1])
    for (af,a),(bf,b) in zip(frames,frames[1:]):
        if t<=bf:
            u=(t-af)/(bf-af)
            u=u*u*(3-2*u)
            return Vector(a).lerp(Vector(b),u)
    return Vector(frames[-1][1])
def weight(a,b,t):
    x=min(1,max(0,(t-a)/(b-a)))
    return x*x*(3-2*x)
def combine(entries):
    result={}
    for name,keys in entries.items():
        result[name]=sequence(keys,frame)
    return result



def arm(side,goal):
    sign=1 if side=='L' else -1
    return aim_chain(('UpperArm.'+side,'Forearm.'+side,'Hand.'+side),Vector(goal),(sign,.13,-.28))
def leg(side,goal):
    return aim_chain(('Thigh.'+side,'Shin.'+side,'Foot.'+side),Vector(goal),(0,-1,.12))
def tm(pos,rot):
    return translate(pos) @ rot.to_matrix().to_4x4()
def mix(a,b,t):
    ap,ar,_=a.decompose();bp,br,_=b.decompose()
    return tm(ap.lerp(bp,t),ar.slerp(br,t))
def set_bone(name,world,frame):
    b=rig.pose.bones[name];b.rotation_mode='QUATERNION'
    b.matrix=rig.matrix_world.inverted()@world
    key(b,frame)
def key(b,frame):
    for prop in ('location','rotation_quaternion','scale'):
        b.keyframe_insert(prop,frame=frame,group=b.name)

rifle=bpy.data.objects['Unicorn_Donor_Rifle']
muzzle=bpy.data.objects['Muzzle']
gun_origin=rifle.matrix_world.copy()
origin=gun_origin.translation
bore=(muzzle.matrix_world.translation-origin).normalized()
stowed_direction=Vector((.62,.02,-.78)).normalized()
stow_rotation=bore.rotation_difference(stowed_direction)@gun_origin.to_quaternion()
def depth(q):
    return max((q@v.co).y for v in rifle.data.vertices)-min((q@v.co).y for v in rifle.data.vertices)
stow_rotation=min((Quaternion(stowed_direction,math.radians(a))@stow_rotation for a in range(-78,79,3)),key=depth)
# Center the full gun behind the thrusters, including the stock.
mesh_points=[v.co for v in rifle.data.vertices]
center=sum(mesh_points,Vector())/len(mesh_points)
gun_front=min((stow_rotation@(p-center)).y for p in mesh_points)
dock=tm(Vector((0,.485-gun_front,2.02))-stow_rotation@center,stow_rotation)
pack_bind=rig_world(rig.pose.bones['Backpack']).inverted()
hand=rig.pose.bones['Hand.R']
hand_bind=rig_world(hand).inverted()
rifle.parent=rig;rifle.parent_type='BONE';rifle.parent_bone='RifleMount'
set_bone('RifleMount',gun_origin,1)
bpy.context.view_layer.update()
rifle.matrix_world=gun_origin

# Small rigid rack clamps bridge the backpack armor to the gun stow position.
pack=rig.pose.bones['Backpack']
bracket_material=rifle.data.materials[0]
for name,location,size in [('Magnum_Rack_Base',(0,.455,2.06),(.22,.095,.135)),
                           ('Magnum_Rack_Upper',(-.068,.492,2.145),(.10,.115,.059)),
                           ('Magnum_Rack_Lower',(.068,.492,1.98),(.10,.115,.059))]:
    bpy.ops.mesh.primitive_cube_add(size=1,location=location)
    o=bpy.context.object;o.name=name;o.dimensions=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    bevel=o.modifiers.new('Rack edge','BEVEL');bevel.width=.014;bevel.segments=2
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    o.data.materials.append(bracket_material)
    for loop in o.data.uv_layers.active.data:loop.uv=(.04,.04)
    world=o.matrix_world.copy()
    gear.objects.link(o)
    for col in list(o.users_collection):
        if col!=gear:col.objects.unlink(o)
    o.parent=rig;o.parent_type='BONE';o.parent_bone=pack.name
    bpy.context.view_layer.update();o.matrix_world=world

def sockets(f,weapon=None):
    back=rig_world(rig.pose.bones['Backpack'])@pack_bind@dock
    held=weapon if weapon is not None else rig_world(hand)@hand_bind@gun_origin
    set_bone('Magnum_BackpackSocket',back,f)
    set_bone('Magnum_RightHandSocket',held,f)
    set_bone('RifleMount',held if weapon is not None else back,f)
    return back

replaced=('Unicorn_BeamTonfa','Unicorn_BeamMagnum','Unicorn_FullBurst')
for action in list(bpy.data.actions):
    if action.name in replaced:action.name='Source_'+action.name
for name in ('Idle','HitReact','TwinVulcan','Deploy','Dodge','Parry','Defeat'):
    clip=bpy.data.actions['Unicorn_'+name]
    rig.animation_data.action=clip
    first,last=map(round,clip.frame_range)
    for f in range(first,last+1):
        scene.frame_set(f);sockets(f)

def body(chest,head,crouch):
    pose({'Chest':chest,'Head':head})
    pelvis=rig.pose.bones['Pelvis']
    pelvis.location=pelvis.bone.matrix_local.to_3x3().inverted()@Vector((0,0,-crouch))
    bpy.context.view_layer.update()

def write_clip(name,last,animate):
    clip=bpy.data.actions.new('Unicorn_'+name);clip.use_fake_user=True
    rig.animation_data.action=clip
    for f in range(last+1):
        scene.frame_set(f+1)
        held=animate(f)
        for b in rig.pose.bones:
            if b.name not in controls:key(b,f+1)
        sockets(f+1,held)
    print('BAKED',name,last,'at 30 FPS',flush=True)

def buff(f):
    wind=weight(2,19,f)*(1-weight(48,78,f))
    opening=weight(41,51,f)*(1-weight(60,80,f))
    shake=math.sin(f*2.1)*wind
    body((9*wind-18*opening,shake*1.4,shake*1.7),(-5*wind+15*opening,0,0),.23*wind+.012*shake)
    for side,sign in (('R',-1),('L',1)):
        arm(side,(sign*(.603-.238*wind+.23*opening),-.09-.46*wind-.13*opening,1.577+.31*wind+.28*opening))
        leg(side,(sign*(.39+.07*wind),-.035,.345))
    return None

def magnum(f):
    open_draw=weight(0,14,f)*(1-weight(78,91,f))
    recoil=weight(48,51,f)*(1-weight(51,62,f))
    body((-3*open_draw-12*recoil,0,-8*open_draw+6*recoil),(3*open_draw,0,5*open_draw),.04*open_draw)
    back=rig_world(rig.pose.bones['Backpack'])@pack_bind@dock
    mount=back.translation
    aim=Vector((-.42,-.63+recoil*.14,2.14+recoil*.06))
    rest_hand=Vector((-.603,-.09,1.577))
    draw=f<=14
    returning=f>=78
    reach=weight(2,14,f) if draw else 1-weight(78,92,f) if returning else 1
    trip=weight(14,36,f)*(1-weight(58,78,f))
    hand_goal=mount.lerp(aim,trip)
    hand_goal.z+=.50*math.sin(math.pi*trip)
    hand_point=arm('R',rest_hand.lerp(hand_goal,reach))
    arm('L',(.602-.19*trip,-.09-.30*trip,1.58+.36*trip))
    aim_direction=Vector((0,-1,-.27+recoil*.26)).normalized()
    firing=bore.rotation_difference(aim_direction)@gun_origin.to_quaternion()
    in_hand=tm(hand_point,back.to_quaternion().slerp(firing,trip))
    return mix(back,in_hand,weight(14,21,f)*(1-weight(72,78,f))) if 14<=f<78 else None

def burst(f):
    crouch=sequence([(0,(0,0,0)),(19,(.13,0,0)),(31,(.09,0,0)),(58,(.06,0,0)),(71,(.21,0,0)),
                    (85,(.02,0,0)),(151,(.03,0,0)),(178,(.17,0,0)),(195,(0,0,0))],f).x
    chest=sequence([(0,(0,0,0)),(20,(23,0,-18)),(30,(18,-5,27)),(36,(12,5,-22)),(45,(13,0,-31)),
                    (54,(11,9,16)),(63,(-14,15,-9)),(72,(18,-3,0)),(84,(-14,0,0)),
                    (110,(-8,0,2)),(132,(16,0,0)),(146,(27,0,0)),(155,(14,0,0)),(195,(0,0,0))],f)
    body(chest,(-chest.x*.35,0,-chest.z*.35),crouch)
    for side,s in (('R',-1),('L',1)):
        guard=(s*.34,-.43,2.13)
        rest=(s*.603,-.09,1.577)
        high=(s*.25,-.55,2.22)
        charge=(s*.18,-.67,2.12)
        push=(s*.24,-.76,1.96)
        attack=[(25,guard),(30,(s*.22,-.89,2.10)),(35,(s*.40,-.53,2.03)),(44,guard)] if side=='R' else [(35,guard),(44,(s*.18,-.89,2.20)),(50,guard)]
        arm(side,sequence([(0,rest),(13,guard),(22,guard),*attack,(57,(s*.56,-.05,2.11)),(64,(s*.61,.08,1.97)),
                   (73,guard),(91,(s*.73,-.07,2.23)),(109,high),(131,charge),(139,push),(145,push),(156,charge),(172,guard),(195,rest)],f))
        foot=(s*.39,-.035,.345)
        kick=[(46,foot),(54,(s*.23,-.57,.94)),(61,(s*.19,-1.30,1.18)),(65,(s*.25,-.87,.95)),(73,foot)] if side=='R' else [(73,foot)]
        leg(side,sequence([(0,foot),*kick,(90,(s*.33,.12,.73)),(106,(s*.32,.21,.70)),(147,(s*.34,.26,.62)),(178,foot),(195,foot)],f))
    return None

write_clip('PsychoFrameAwakening',84,buff)
write_clip('BeamMagnum',92,magnum)
write_clip('FullBurst',195,burst)
rig.animation_data.action=bpy.data.actions['Unicorn_Idle']
scene.frame_set(1);scene.frame_end=196
print('STOWED', tuple(round(x,3) for x in dock.translation),flush=True)
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_Combat_v04_PsychoFrame.blend'),compress=True)

