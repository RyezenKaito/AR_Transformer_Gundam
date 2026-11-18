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
for name in ('Saber_R','Saber_L','Beam_R','Beam_L','RifleMount','Sweep_R','Sweep_L','CrossFlare','Shockwave','ShieldMount'):
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
    drive(bone,Matrix.Identity(4))
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
    arc('Crimson_Sweep_'+s,'Sweep_'+s,1.03,-128,119,.23,soft)
    arc('Sweep_Hot_Edge_'+s,'Sweep_'+s,1.08,-129,108,.037,hot)
    arc('Sweep_Secondary_'+s,'Sweep_'+s,1.26,-122,94,.025,red)
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

controls=('Saber_R','Saber_L','Beam_R','Beam_L','RifleMount','Sweep_R','Sweep_L','CrossFlare','Shockwave','ShieldMount')
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



shield=bpy.data.objects['Unicorn_Donor_Shield']
shield_rest=shield.matrix_world.copy()
forearm_bind=rig_world(rig.pose.bones['Forearm.L']).inverted()
drive('ShieldMount',shield_rest)
attach(shield,'ShieldMount',shield_rest)

for action in list(bpy.data.actions):
    if action.name in ('Unicorn_BeamTonfa','Unicorn_FullBurst'):
        action.name='Source_'+action.name

def key(b,f,group):
    for prop in ('location','rotation_quaternion','scale'):
        b.keyframe_insert(prop,frame=f,group=group)

for clip in original.values():
    rig.animation_data.action=clip
    first,last=map(round,clip.frame_range)
    for f in range(first,last+1):
        scene.frame_set(f)
        back=rig_world(rig.pose.bones['Backpack']) @ pack_bind
        worlds={'Saber_'+s:back@rest[s] for s in sides}
        worlds['RifleMount']=rig_world(rig.pose.bones['Hand.R']) @ hand_bind @ gun_rest
        worlds['ShieldMount']=rig_world(rig.pose.bones['Forearm.L']) @ forearm_bind @ shield_rest
        for name in controls:
            drive(name,worlds[name] if name in worlds else hidden())
            key(rig.pose.bones[name],f,name)

# Timings are zero-based, at 30 FPS. The blade moves in a sagittal plane.
def strike(frame,impact):
    angle=sequence([(0,(90,0,0)),(impact-17,(96,0,0)),(impact-7,(122,0,0)),
                    (impact-3,(66,0,0)),(impact,(-27,0,0)),(impact+5,(-24,0,0)),
                    (impact+17,(-14,0,0)),(impact+29,(76,0,0))],frame).x
    a=math.radians(angle)
    return Vector((0,-math.cos(a),math.sin(a)))

def move_arm(frame,impact,grip,release,sign):
    return sequence([(0,(sign*.60,-.09,1.58)),(grip-5,(sign*.61,.18,2.17)),
                     (grip,(sign*.285,.297,2.62)),(grip+7,(sign*.47,.14,2.66)),
                     (impact-9,(sign*.39,-.14,2.76)),(impact-5,(sign*.33,-.36,2.69)),
                     (impact-2,(sign*.29,-.63,2.29)),(impact,(sign*.29,-.64,1.88)),
                     (impact+6,(sign*.30,-.64,1.91)),(impact+16,(sign*.66,-.33,2.09)),
                     (release-5,(sign*.54,-.04,2.54)),(release,(sign*.285,.297,2.62)),
                     (release+9,(sign*.60,-.09,1.58))],frame)

def impact_fx(frame,impact,side):
    if frame<impact or frame>=impact+16:return hidden()
    u=(frame-impact)/16
    # XY mesh rotated into the YZ plane. Its lower edge follows the cutting stroke.
    matrix=translate((side*.29,-.64,2.02)) @ rotation((0,1,0),90).to_matrix().to_4x4()
    matrix=matrix @ rotation((0,0,1),-41+u*22).to_matrix().to_4x4()
    strength=(.95+.25*u)*(1-weight(.30,1,u))
    return scaled(matrix,(strength,strength,strength))

for action_name, is_ult, length, impact in [('BackpackBeamSaber',False,96,48),('FullBurst',True,172,108)]:
    action=bpy.data.actions.new('Unicorn_'+action_name);action.use_fake_user=True
    action['impact_frame']=impact
    if is_ult:action['shield_release_frame']=25
    rig.animation_data.action=action
    grip,release=(51,151) if is_ult else (15,81)
    for frame in range(length+1):
        scene.frame_set(frame+1)
        chest=[(0,(0,0,0)),(grip,(-9,0,-6)),(impact-14,(-14,0,0)),(impact-5,(-8,0,0)),
               (impact,(23,0,0)),(impact+7,(18,0,0)),(impact+20,(4,0,0)),(length,(0,0,0))]
        leg=[(0,(0,0,0)),(impact-17,(0,0,0)),(impact-6,(-22,0,0)),(impact,(15,0,0)),
             (impact+10,(11,0,0)),(impact+25,(0,0,0))]
        head=[(0,(0,0,0)),(impact-10,(-5,0,0)),(impact,(-14,0,0)),(impact+16,(0,0,0))]
        pose(combine({'Chest':chest,'Thigh.R':leg,'Head':head}))
        back=rig_world(rig.pose.bones['Backpack'])@pack_bind
        worlds={}
        if is_ult and frame<43:
            left_goal=sequence([(0,(.60,-.09,1.58)),(8,(.52,-.42,1.94)),(18,(.32,-.49,2.15)),
                                (25,(.97,-.13,2.22)),(33,(.94,.03,2.02)),(42,(.63,.18,2.17))],frame)
            aim_arm('L',left_goal,(1,.12,-.02))

        for s in sides:
            rack=back@rest[s]
            if s=='L' and (not is_ult or frame<43):
                worlds['Saber_L']=rack;worlds['Beam_L']=hidden()
                continue
            sign=-1 if s=='R' else 1
            grip=15 if s=='R' else 51
            near=max(1-weight(0,6,abs(frame-grip)),1-weight(0,6,abs(frame-release)))
            hand_goal=move_arm(frame,impact,grip,release,sign).lerp(rack.translation,near)
            hand_point=aim_arm(s,hand_goal,(sign,.17,.035))
            blade_rotation=strike(frame,impact).to_track_quat('Z','Y')
            carried=translate(hand_point) @ blade_rotation.to_matrix().to_4x4()
            held=weight(grip,grip+6,frame)*(1-weight(release-6,release,frame))
            hand=rig.pose.bones['Hand.'+s]
            if held>.001:
                # Follow the sword through the wrist as well as the shoulder and elbow.
                ahead=(rig_world(hand).to_quaternion()@Vector((0,1,0))).normalized()
                axis=(blade_rotation@Vector((0,1,0))).normalized()
                apply_delta(hand,Quaternion().slerp(ahead.rotation_difference(axis),held*.74))
            worlds['Saber_'+s]=mix(rack,carried,held)
            glow=weight(grip+7,grip+13,frame)*(1-weight(release-16,release-9,frame))
            worlds['Beam_'+s]=scaled(carried,(max(.0001,glow),)*3) if glow>.001 else hidden()
            if frame in (impact-9,impact-1,impact,impact+1,impact+5):
                tip=carried@Vector((0,0,1.48))
                print(action_name,s,frame,'blade tip',tuple(round(x,3) for x in tip),flush=True)

        stow=weight(1,11,frame)*(1-weight(length-11,length,frame))
        worlds['RifleMount']=mix(rig_world(rig.pose.bones['Hand.R'])@hand_bind@gun_rest,back@gun_mount,stow)
        shield_held=rig_world(rig.pose.bones['Forearm.L']) @ forearm_bind @ shield_rest
        if not is_ult or frame<=25:
            worlds['ShieldMount']=shield_held
            if is_ult and frame==25: shield_release=shield_held.copy()
        else:
            u=min(1,(frame-25)/23)
            pos=shield_release.translation+Vector((2.35*u,.30*u,1.52*u-2.93*u*u))
            spin=rotation((0,0,1),198*u)@rotation((1,0,0),68*u)
            tossed=translate(pos)@spin.to_matrix().to_4x4()@shield_release.to_3x3().to_4x4()
            # Wait on the floor off to the side, then return to the mount during recovery.
            worlds['ShieldMount']=mix(tossed,shield_held,weight(143,160,frame))

        worlds['Sweep_R']=impact_fx(frame,impact,-1)
        worlds['Sweep_L']=impact_fx(frame,impact,1) if is_ult else hidden()
        pulse=(1-weight(impact+2,impact+14,frame)) if is_ult and impact<=frame<impact+14 else 0
        worlds['CrossFlare']=scaled(translate((0,-1.76,1.19)),(pulse,)*3) if pulse>.001 else hidden()
        ring=weight(impact-2,impact+17,frame)
        worlds['Shockwave']=scaled(translate((0,-1.57,.10)),(ring*1.95,ring*1.95,max(.0001,1-ring))) if is_ult and impact<=frame<impact+17 else hidden()
        for n,w in worlds.items():drive(n,w)
        for b in rig.pose.bones:key(b,frame+1,b.name)
    print('AUTHORED',action.name,tuple(action.frame_range),flush=True)

scene.frame_end=173
rig.animation_data.action=bpy.data.actions['Unicorn_BackpackBeamSaber']
scene.frame_set(49)
scene.timeline_markers.clear()
for label,frame in [('Grip',16),('Ignition',23),('Overhead Windup',40),('Impact',49),('Saber Dock',82)]:
    scene.timeline_markers.new(label,frame=frame)
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceArt/Blender/Unicorn/Unicorn_Combat_v03_Overhead.blend'),compress=True)
print('SAVED',len(gear.objects),flush=True)



