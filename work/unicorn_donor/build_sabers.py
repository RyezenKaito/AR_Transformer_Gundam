from pathlib import Path
import math
import bpy
from mathutils import Matrix, Quaternion, Vector

ROOT = Path.cwd()
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'SourceArt/Blender/Unicorn/Unicorn_Combat_v01.blend'))
scene = bpy.context.scene
rig = bpy.data.objects['Unicorn_PrototypeRig']
gear = bpy.data.collections['Unicorn_Equipment']
armor = bpy.data.collections['Unicorn_Import']
scene.frame_set(1)
bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode='OBJECT')
for o in list(gear.objects):
    if o.name.startswith('Tonfa_'):
        bpy.data.objects.remove(o,do_unlink=True)

sides = ('R','L')
hilts = {s:next(o for o in armor.objects if 'light_saber_'+('b' if s=='R' else 'a')+' saber_G' in o.name) for s in sides}
rifle = bpy.data.objects['Unicorn_Donor_Rifle']
gun_rest = rifle.matrix_world.copy()
rest = {}
for s,o in hilts.items():
    v = [o.matrix_world @ a.co for a in o.data.vertices]
    z0,z1 = min(p.z for p in v),max(p.z for p in v)
    base_pts = [p for p in v if p.z < z0+.065]
    head_pts = [p for p in v if p.z > z1-.055]
    base = sum(base_pts,Vector())/len(base_pts)
    tip = sum(head_pts,Vector())/len(head_pts)
    direction = (tip-base).normalized()
    rest[s] = Matrix.Translation(base.lerp(tip,.45)) @ direction.to_track_quat('Z','Y').to_matrix().to_4x4()
    o.name='Backpack_Beam_Saber_'+s

VFX = bpy.data.collections.new('Unicorn Saber VFX')
scene.collection.children.link(VFX)
bpy.ops.object.mode_set(mode='EDIT')
for name in ('Saber_R','Saber_L','Beam_R','Beam_L','RifleMount','Sweep_R','Sweep_L','CrossFlare','Shockwave'):
    b=rig.data.edit_bones.new(name)
    b.head=(0,0,0)
    b.tail=(0,0,.12)
    b.use_deform=False
bpy.ops.object.mode_set(mode='OBJECT')
rig_inverse=rig.matrix_world.inverted()
def drive(name, world):
    b=rig.pose.bones[name]
    b.rotation_mode='QUATERNION'
    b.matrix=rig_inverse @ world
def attach(o,name,world):
    o.parent=rig
    o.parent_type='BONE'
    o.parent_bone=name
    bpy.context.view_layer.update()
    o.matrix_world=world

for s,o in hilts.items():
    world=o.matrix_world.copy()
    for mod in list(o.modifiers):
        if mod.type=='ARMATURE':o.modifiers.remove(mod)
    armor.objects.unlink(o);gear.objects.link(o)
    drive('Saber_'+s,rest[s])
    attach(o,'Saber_'+s,world)
drive('RifleMount',gun_rest)
attach(rifle,'RifleMount',gun_rest)

def material(name, color, strength):
    mat=bpy.data.materials.new(name)
    mat.diffuse_color=(*color,1)
    mat.use_nodes=True
    p=mat.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Emission Color'].default_value=(*color,1)
    p.inputs['Emission Strength'].default_value=strength
    return mat

red=material('UnicornSaberCrimson',(1,.006,.022),5)
hot=material('UnicornSaberHotEdge',(1,.15,.22),9)
soft=material('UnicornSaberRibbon',(.78,.005,.032),3)

def mesh(name, vertices, faces, mat, bone):
    data=bpy.data.meshes.new(name)
    data.from_pydata(vertices,[],faces);data.update()
    o=bpy.data.objects.new(name,data)
    VFX.objects.link(o);gear.objects.link(o)
    data.materials.append(mat)
    # The control bones all use a vertical rest axis. Store the effect in world-oriented mesh coordinates.
    attach(o,bone,Matrix.Identity(4))
    return o
def blade(name, mat, bone, widths):
    sections=zip((.12,.19,.70,1.29,1.48),widths)
    verts=[(w*math.cos(i*math.tau/12),w*math.sin(i*math.tau/12),z) for z,w in sections for i in range(12)]
    faces=[tuple((j*12+i,j*12+(i+1)%12,(j+1)*12+(i+1)%12,(j+1)*12+i)) for j in range(4) for i in range(12)]
    mesh(name,verts,faces,mat,bone)
for s in sides:
    blade('Emissive_Blade_'+s,red,'Beam_'+s,(.035,.062,.054,.038,.001))
    blade('Hot_Blade_Core_'+s,hot,'Beam_'+s,(.016,.023,.020,.015,.001))

def arc(name, bone, radius, start, span, width, mat):
    verts=[]
    for i in range(37):
        u=i/36
        a=math.radians(start+span*u)
        w=width * (math.sin(math.pi*u)**.72)
        jag=.035*math.sin(u*24)*math.sin(math.pi*u)
        verts.extend([((radius-w)*math.cos(a),(radius-w)*math.sin(a),jag),((radius+w*.26)*math.cos(a),(radius+w*.26)*math.sin(a),0)])
    faces=[(2*i,2*i+1,2*i+3,2*i+2) for i in range(36)]
    mesh(name,verts,faces,mat,bone)
for s in sides:
    arc('Crimson_Sweep_'+s,'Sweep_'+s,1.03,-103,203,.28,soft)
    arc('Sweep_Hot_Edge_'+s,'Sweep_'+s,1.08,-82,156,.038,hot)
    arc('Sweep_Secondary_'+s,'Sweep_'+s,1.26,-63,123,.032,red)
for start in range(0,360,60):
    arc('Shockwave_Segment_'+str(start),'Shockwave',1.17,start,44,.071,red)

verts=[];faces=[]
for i in range(21):
    a=i*2.399963
    z=(i/20-.5)*1.48
    xy=math.sqrt(1-z*z)
    d=Vector((xy*math.cos(a),xy*math.sin(a),z))
    side=d.cross(Vector((0,0,1))).normalized()
    foot=d*(.21+(i%4)*.12);tip=d*(.84+(i%6)*.12)
    n=len(verts)
    verts.extend((foot,d*.43+side*.029,tip,d*.42-side*.029))
    faces.append((n,n+1,n+2,n+3))
mesh('Cross_Impact_Shards',verts,faces,hot,'CrossFlare')

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

def aim_arm(side, goal, outward):
    upper=rig.pose.bones['UpperArm.'+side]
    fore=rig.pose.bones['Forearm.'+side]
    hand=rig.pose.bones['Hand.'+side]
    a,b,c=(rig.matrix_world @ x.head for x in (upper,fore,hand))
    l1,l2=(b-a).length,(c-b).length
    axis=(goal-a).normalized()
    reach=min((goal-a).length,(l1+l2)*.997)
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

controls=('Saber_R','Saber_L','Beam_R','Beam_L','RifleMount','Sweep_R','Sweep_L','CrossFlare','Shockwave')
original = {name:bpy.data.actions['Unicorn_'+name] for name in ('Idle','HitReact','TwinVulcan','BeamMagnum','Deploy','Dodge','Parry','Defeat')}
pack_bind=rig_world(rig.pose.bones['Backpack']).inverted()
hand_bind=rig_world(rig.pose.bones['Hand.R']).inverted()
gun_mount=translate((-.14,.52,1.69)) @ rotation((0,1,0),-75).to_matrix().to_4x4()
gun_mount=gun_mount @ gun_rest.to_3x3().to_4x4()
def mix(a,b,t):
    ap,ar,_=a.decompose();bp,br,_=b.decompose()
    return translate(ap.lerp(bp,t)) @ ar.slerp(br,t).to_matrix().to_4x4()
def scaled(w,s):
    return w @ Matrix.Diagonal((*s,1))
def hidden():
    return scaled(translate((0,0,-8)),(.0001,.0001,.0001))

# S2: detach the right hilt from the backpack, ignite, low draw-cut, then reverse cut.
s2_hand=[(0,(-.603,-.09,1.58)),(8,(-.70,.12,2.04)),(15,(-.32,.25,2.58)),(22,(-.66,.05,2.60)),(28,(-.82,-.23,2.25)),(35,(-.84,-.27,2.00)),(41,(-.20,-.53,2.14)),(47,(.04,-.44,2.37)),(54,(-.73,-.47,2.02)),(63,(-.78,-.23,1.98)),(73,(-.54,-.03,2.43)),(82,(-.32,.25,2.58)),(87,(-.59,.02,2.25)),(96,(-.603,-.09,1.58))]
s2_dir=[(0,(0,0,1)),(15,(0,.13,1)),(24,(-.92,.14,.38)),(35,(-1,-.15,-.14)),(41,(.12,-1,-.13)),(47,(.93,-.28,.32)),(54,(-.19,-1,.12)),(60,(-.94,-.16,.34)),(70,(-.3,.11,1)),(82,(0,.13,1))]
# S4: both draw, first cut, countercut, spinning cross and downward twin finisher.
ult_right=[(0,(-.60,-.09,1.58)),(15,(-.65,.11,2.05)),(25,(-.32,.25,2.58)),(35,(-.72,-.04,2.55)),(49,(-.79,-.30,2.05)),(55,(-.58,-.47,2.10)),(62,(-.06,-.52,2.34)),(70,(-.16,-.38,2.55)),(77,(-.73,-.29,2.03)),(86,(-.78,-.28,2.18)),(94,(-.72,-.39,2.49)),(100,(-.22,-.48,2.36)),(110,(-.54,-.03,2.68)),(118,(-.53,-.20,2.68)),(126,(-.42,-.54,1.99)),(136,(-.64,-.30,1.80)),(148,(-.56,.03,2.55)),(158,(-.32,.25,2.58)),(165,(-.60,-.09,1.58))]
ult_left=[(f,(-p[0],p[1],p[2])) for f,p in ult_right]
ult_left[5:10]=[(55,(.79,-.20,2.45)),(62,(.82,-.25,2.58)),(70,(.52,-.50,2.18)),(77,(.08,-.46,1.98)),(86,(.63,-.37,2.28))]
ult_dir=[(0,(0,0,1)),(25,(0,.14,1)),(40,(-.94,-.16,.44)),(54,(-.8,-.5,-.18)),(60,(.22,-1,.26)),(67,(.94,-.12,.42)),(77,(-.91,-.32,.20)),(88,(-.86,-.33,.58)),(99,(.58,-.7,-.20)),(112,(-.26,.06,1)),(118,(-.32,-.3,.93)),(126,(-.08,-.89,-.38)),(137,(-.87,-.42,.14)),(158,(0,.13,1))]
left_dir=[(f,(-p[0],p[1],p[2])) for f,p in ult_dir]
left_dir[4:8]=[(54,(.93,-.15,.37)),(62,(.85,-.13,.55)),(70,(.02,-1,-.35)),(77,(-.93,-.21,-.10))]

def vfx_slice(f, intervals):
    for start,end,yaw,tilt in intervals:
        if start<=f<end:
            u=(f-start)/(end-start)
            loc=(0,-.57,2.13)
            rot=rotation((0,0,1),yaw+u*58)@rotation((0,1,0),tilt)@rotation((1,0,0),58)
            width=(.54+u*.58)*max(.06,1-weight(.60,1,u))
            return scaled(translate(loc)@rot.to_matrix().to_4x4(),(width,width,width))
    return hidden()

for action in list(bpy.data.actions):
    if action.name in ('Unicorn_BeamTonfa','Unicorn_FullBurst'):
        action.name='Source_'+action.name

def key(b,f,group):
    for prop in ('location','rotation_quaternion','scale'):
        b.keyframe_insert(prop,frame=f,group=group)

for key_name,clip in original.items():
    rig.animation_data.action=clip
    first,last=map(round,clip.frame_range)
    for f in range(first,last+1):
        scene.frame_set(f)
        back=rig_world(rig.pose.bones['Backpack']) @ pack_bind
        worlds={'Saber_'+s:back@rest[s] for s in sides}
        worlds['RifleMount']=rig_world(rig.pose.bones['Hand.R']) @ hand_bind @ gun_rest
        for name in controls:
            drive(name,worlds[name] if name in worlds else hidden())
            key(rig.pose.bones[name],f,name)

for action_name, is_ult, length in [('BackpackBeamSaber',False,96),('FullBurst',True,172)]:
    action=bpy.data.actions.new('Unicorn_'+action_name);action.use_fake_user=True
    rig.animation_data.action=action
    for frame in range(length+1):
        scene.frame_set(frame+1)
        if is_ult:
            chest=[(0,(0,0,0)),(25,(-4,0,0)),(45,(12,0,-18)),(54,(21,-12,-32)),(61,(6,4,32)),(69,(15,0,20)),(74,(8,10,-25)),(85,(-4,0,0)),(98,(13,0,11)),(111,(-15,0,0)),(118,(-6,0,1)),(127,(25,0,-2)),(141,(6,0,0)),(172,(0,0,0))]
            leg=[(0,(0,0,0)),(45,(12,0,4)),(60,(-19,0,-4)),(81,(5,0,4)),(109,(-30,0,0)),(119,(-51,0,0)),(130,(6,0,3)),(151,(12,0,0)),(172,(0,0,0))]
        else:
            chest=[(0,(0,0,0)),(15,(-7,0,-8)),(31,(11,-7,-22)),(41,(12,5,35)),(49,(12,0,18)),(55,(10,-4,-24)),(71,(5,0,-6)),(96,(0,0,0))]
            leg=[(0,(0,0,0)),(32,(-25,0,0)),(41,(-11,0,0)),(61,(8,0,0)),(96,(0,0,0))]
        pose(combine({'Chest':chest,'Thigh.R':leg,'Shin.R':[(0,(0,0,0)),(33,(22,0,0)),(62,(6,0,0)),(90,(0,0,0))],
                      'Head':[(0,(0,0,0)),(24,(-4,0,2)),(46,(-7,0,-8)),(63,(-7,0,8)),(length,(0,0,0))]}))
        back=rig_world(rig.pose.bones['Backpack'])@pack_bind
        worlds={}
        for s in sides:
            if s=='L' and not is_ult:
                worlds['Saber_L']=back@rest['L'];worlds['Beam_L']=hidden()
                continue
            sign=-1 if s=='R' else 1
            hands=ult_right if is_ult and s=='R' else ult_left if is_ult else s2_hand
            dirs=ult_dir if is_ult and s=='R' else left_dir if is_ult else s2_dir
            start, release=(25,158) if is_ult else (15,82)
            rack=back@rest[s]
            near=max(1-weight(start-4,start+7,abs(frame-start)+start-4),1-weight(0,8,abs(frame-release)))
            hand_goal=sequence(hands,frame).lerp(rack.translation,near)
            hand_point=aim_arm(s,hand_goal,(sign,.20,.10))
            blade_direction=sequence(dirs,frame).normalized()
            carried=translate(hand_point) @ blade_direction.to_track_quat('Z','Y').to_matrix().to_4x4()
            held=weight(start,start+6,frame)*(1-weight(release-6,release,frame))
            # Hands reach the rack before the original hilt leaves it.
            worlds['Saber_'+s]=mix(rack,carried,held)
            glow=(weight(35,42,frame)*(1-weight(142,150,frame))) if is_ult else (weight(23,29,frame)*(1-weight(66,74,frame)))
            worlds['Beam_'+s]=scaled(carried,(max(.0001,glow),)*3) if glow>.001 else hidden()
        stow=weight(2,12,frame)*(1-weight(length-12,length,frame))
        worlds['RifleMount']=mix(rig_world(rig.pose.bones['Hand.R'])@hand_bind@gun_rest,back@gun_mount,stow)
        worlds['Sweep_R']=vfx_slice(frame,[(54,67,-38,-20),(93,107,-20,44),(120,138,-24,62)] if is_ult else [(37,48,-31,-28),(51,63,162,30)])
        worlds['Sweep_L']=vfx_slice(frame,[(65,80,142,28),(93,107,155,-42),(120,138,104,-63)]) if is_ult else hidden()
        pulse=weight(123,127,frame)*(1-weight(130,145,frame)) if is_ult else 0
        worlds['CrossFlare']=scaled(translate((0,-1.18,1.54)),(pulse,)*3) if pulse>.001 else hidden()
        ring=weight(126,147,frame)
        worlds['Shockwave']=scaled(translate((0,-1.18,.07)),(ring*1.64,ring*1.64,max(.0001,1-ring))) if is_ult and 127<=frame<147 else hidden()
        for n,w in worlds.items():drive(n,w)
        for b in rig.pose.bones:key(b,frame+1,b.name)
    print('AUTHORED',action.name,tuple(action.frame_range),flush=True)

scene.frame_end=173
rig.animation_data.action=bpy.data.actions['Unicorn_BackpackBeamSaber']
scene.frame_set(42)
scene.timeline_markers.clear()
for label,frame in [('Grip',16),('Ignition',24),('Cut A',42),('Cut B',55),('Saber Dock',83)]:
    scene.timeline_markers.new(label,frame=frame)
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt/Blender/Unicorn/Unicorn_Combat_v02_Sabers.blend'),compress=True)
print('SAVED',len(gear.objects),flush=True)

