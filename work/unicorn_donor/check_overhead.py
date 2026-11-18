import bpy
from mathutils import Vector
from pathlib import Path
bpy.ops.wm.open_mainfile(filepath=str(Path.cwd()/'SourceArt/Blender/Unicorn/Unicorn_Combat_v03_Overhead.blend'))
s=bpy.context.scene
r=bpy.data.objects['Unicorn_PrototypeRig']
r.animation_data.action=bpy.data.actions['Unicorn_BackpackBeamSaber']
for f in (40,48,49,50):
 s.frame_set(f)
 b=r.pose.bones['Beam_R'];m=bpy.data.objects['Emissive_Blade_R']
 print('ACTUAL',f,'bone',b.matrix.translation[:],'local Rot',b.rotation_quaternion[:], 'mesh',m.matrix_world.translation[:], 'tip',(m.matrix_world@Vector((0,0,1.48)))[:],flush=True)
