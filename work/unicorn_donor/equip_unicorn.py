import bpy
from pathlib import Path
from mathutils import Vector,Matrix
from math import radians
root=Path.cwd()
bpy.ops.wm.open_mainfile(filepath=str(root/'SourceArt/Blender/Unicorn/Unicorn_MotionPrototype_v01.blend'))
scene=bpy.context.scene
rig=bpy.data.objects['Unicorn_PrototypeRig']
scene.frame_set(1)
before=set(bpy.data.objects)
bpy.ops.import_scene.fbx(filepath=str(root/'Assets/Models/rx-0-full-armor-unicorn-gundam/source/RX-0 Full Armor Unicorn Gundam/untitled.fbx'))
donor=[o for o in bpy.data.objects if o not in before]
import bpy,json
from mathutils import Vector
def islands(o):
    verts=o.data.vertices
    parent=list(range(len(verts)))
    def find(i):
        while parent[i]!=i:
            parent[i]=parent[parent[i]];i=parent[i]
        return i
    def union(i,j):parent[find(i)]=find(j)
    keys={}
    for v in verts:
        k=tuple(round(x,5) for x in v.co)
        if k in keys:union(v.index,keys[k])
        else:keys[k]=v.index
    for e in o.data.edges:union(*e.vertices)
    parts={}
    for p in o.data.polygons:parts.setdefault(find(p.vertices[0]),[]).append(p.index)
    return sorted(parts.values(),key=lambda p:-len(p))

import bmesh
def extract(o, ids,name):
    selected={j for i,p in enumerate(islands(o)) if i in ids for j in p}
    m=o.data.copy()
    bm=bmesh.new();bm.from_mesh(m);bm.faces.ensure_lookup_table()
    bmesh.ops.delete(bm,geom=[f for f in bm.faces if f.index not in selected],context='FACES')
    bm.to_mesh(m);bm.free()
    clone=bpy.data.objects.new(name,m)
    bpy.context.scene.collection.objects.link(clone)
    clone.matrix_world=o.matrix_world.copy()
    return clone

# Keep both white shell sections, all four red fins, and the central mounting parts.
# The remaining islands are the two Gatling assemblies.
shield=extract(bpy.data.objects['1'],[0,1,4,5,8,11,12,13,14,15,34,35,46,47],'Unicorn_Donor_Shield')
gunSource=bpy.data.objects['gun']
gun=bpy.data.objects.new('Unicorn_Donor_Rifle',gunSource.data.copy())
scene.collection.objects.link(gun)
c=bpy.data.collections.new('Unicorn_Equipment')
scene.collection.children.link(c)
def attach(o,source,pivot,rotation,scale,loc,bone):
    world=Matrix.Translation(Vector(loc))@rotation@Matrix.Scale(scale,4)@Matrix.Translation(-Vector(pivot))@source
    o.data.transform(Matrix.Translation(-Vector(loc))@world)
    if world.to_3x3().determinant()<0:
        bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.reverse_faces(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    c.objects.link(o)
    for uc in list(o.users_collection):
        if uc!=c:uc.objects.unlink(o)
    o.parent=rig;o.parent_type='BONE';o.parent_bone=bone
    bpy.context.view_layer.update()
    o.matrix_world=Matrix.Translation(Vector(loc))
    o['equipment_source']='Full Armor Unicorn donor FBX'
    o.hide_set(False)
attach(gun,gunSource.matrix_world.copy(),(.752,-.124,1.807),Matrix.Rotation(radians(-28),4,'X')@Matrix.Rotation(radians(180),4,'Z'),.59,(-.585,-.145,1.49),'Hand.R')
attach(shield,shield.matrix_world.copy(),(.80,-.30,2.72),Matrix.Rotation(radians(-47),4,'Z')@Matrix.Rotation(radians(-12),4,'X'),.77,(.742,-.112,1.82),'Forearm.L')
bpy.data.batch_remove(donor)
for o in (gun,shield):
    for m in o.data.materials:
        for n in m.node_tree.nodes:
            if n.type=='TEX_IMAGE' and n.image and not n.image.packed_file:n.image.pack()
scene.frame_set(1)
p0=[o.matrix_world.translation.copy() for o in (gun,shield)]
action=rig.animation_data.action
rig.animation_data.action=bpy.data.actions['Unicorn_HitReact']
scene.frame_set(5)
drift=[(o.matrix_world.translation-p).length for o,p in zip((gun,shield),p0)]
assert min(drift)>.005,drift
rig.animation_data.action=action
scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
rig.select_set(True);bpy.context.view_layer.objects.active=rig
target=root/'SourceArt/Blender/Unicorn/Unicorn_Equipped_v02.blend'
assert not target.exists()
bpy.ops.wm.save_as_mainfile(filepath=str(target),compress=True)
print('VERIFIED',target,len(c.objects),drift, len(shield.data.polygons),flush=True)
