"""Decode this static GLB into Unity-friendly mesh buffers without third-party packages."""
from pathlib import Path
import json, struct, shutil
import numpy as np
PROJECT_ROOT=Path(__file__).resolve().parents[1]

import sys
SOURCE=Path(sys.argv[1])
OUT=Path(sys.argv[2])
OUT.mkdir(parents=True, exist_ok=True)
data=SOURCE.read_bytes()
magic,version,length=struct.unpack_from('<III',data)
assert magic==0x46546C67 and version==2 and length==len(data)
offset=12; binary=None; gltf=None
while offset<len(data):
    size,kind=struct.unpack_from('<II',data,offset); chunk=data[offset+8:offset+8+size]; offset+=8+size
    if kind==0x4E4F534A: gltf=json.loads(chunk)
    elif kind==0x004E4942: binary=chunk
assert gltf is not None and binary is not None
types={5120:np.int8,5121:np.uint8,5122:np.int16,5123:np.uint16,5125:np.uint32,5126:np.float32}
dims={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}
def accessor(index):
    a=gltf['accessors'][index]; v=gltf['bufferViews'][a['bufferView']]; dtype=np.dtype(types[a['componentType']]).newbyteorder('<'); n=dims[a['type']]
    arr=np.ndarray((a['count'],n),dtype=dtype,buffer=binary,offset=v.get('byteOffset',0)+a.get('byteOffset',0),strides=(v.get('byteStride',dtype.itemsize*n),dtype.itemsize)).copy()
    if a.get('normalized') and np.issubdtype(dtype,np.integer): arr=arr.astype(np.float32)/np.iinfo(dtype).max
    return arr
def matrix(node):
    if 'matrix' in node: return np.array(node['matrix'],dtype=np.float64).reshape((4,4),order='F')
    x,y,z,w=node.get('rotation',[0,0,0,1]); m=np.eye(4)
    m[:3,:3]=np.array([[1-2*y*y-2*z*z,2*x*y-2*z*w,2*x*z+2*y*w],[2*x*y+2*z*w,1-2*x*x-2*z*z,2*y*z-2*x*w],[2*x*z-2*y*w,2*y*z+2*x*w,1-2*x*x-2*y*y]])@np.diag(node.get('scale',[1,1,1]));m[:3,3]=node.get('translation',[0,0,0]);return m
entries=[]
def walk(index,parent,group=None):
    node=gltf['nodes'][index]; world=parent@matrix(node)
    if 'mesh' in node:
        for pi,p in enumerate(gltf['meshes'][node['mesh']]['primitives']):
            assert p.get('mode',4)==4, 'Only triangle primitives supported'
            pos=accessor(p['attributes']['POSITION']); vertices=np.c_[pos,np.ones(len(pos))]@world.T;vertices=vertices[:,:3];vertices[:,2]*=-1
            if 'NORMAL' in p['attributes']:
                normals=accessor(p['attributes']['NORMAL'])@np.linalg.inv(world[:3,:3]);normals[:,2]*=-1;normals/=np.maximum(np.linalg.norm(normals,axis=1,keepdims=True),1e-12)
            else: normals=np.zeros_like(vertices)
            uv=accessor(p['attributes']['TEXCOORD_0']) if 'TEXCOORD_0' in p['attributes'] else np.zeros((len(vertices),2));uv[:,1]=1-uv[:,1]
            indices=accessor(p['indices']).reshape(-1).astype(np.int32) if 'indices' in p else np.arange(len(vertices),dtype=np.int32)
            # Reflect glTF right-handed Z into Unity. Account for any source reflection.
            if np.linalg.det(world[:3,:3])>0: indices=indices.reshape(-1,3)[:,[0,2,1]].reshape(-1)
            name=f'mesh_{len(entries):03d}.bytes'
            with (OUT/name).open('wb') as f:
                f.write(struct.pack('<II',len(vertices),len(indices)))
                for a in [vertices,normals,uv]: f.write(np.asarray(a,dtype='<f4').tobytes())
                f.write(np.asarray(indices,dtype='<i4').tobytes())
            entries.append({'name':node.get('name',str(index)),'file':name,'material':p.get('material',0),'node':index,'group':group if group is not None else index,'min':vertices.min(axis=0).tolist(),'max':vertices.max(axis=0).tolist(),'vertices':len(vertices),'triangles':len(indices)//3})
    for child in node.get('children',[]): walk(child,world,child if node.get('name')=='RootNode' else group)
for root in gltf['scenes'][gltf.get('scene',0)]['nodes']: walk(root,np.eye(4))
images=[]
for i,im in enumerate(gltf.get('images',[])):
    v=gltf['bufferViews'][im['bufferView']];extension='.png' if im.get('mimeType')=='image/png' else '.jpg';name=f'image_{i:02d}'+extension
    (OUT/name).write_bytes(binary[v.get('byteOffset',0):v.get('byteOffset',0)+v['byteLength']]);images.append(name)
materials=[]
for i,m in enumerate(gltf.get('materials',[])):
    p=m.get('pbrMetallicRoughness',{}); base=p.get('baseColorTexture',{}).get('index',-1);mr=p.get('metallicRoughnessTexture',{}).get('index',-1)
    normal=m.get('normalTexture',{}).get('index',-1)
    materials.append({'name':m.get('name',f'Material_{i}'),'color':p.get('baseColorFactor',[1,1,1,1]),'baseMap':images[gltf['textures'][base]['source']] if base>=0 else '', 'normalMap':images[gltf['textures'][normal]['source']] if normal>=0 else '', 'metalRough':images[gltf['textures'][mr]['source']] if mr>=0 else '', 'metallic':p.get('metallicFactor',1),'roughness':p.get('roughnessFactor',1),'doubleSided':m.get('doubleSided',False),'alphaMode':m.get('alphaMode','OPAQUE'),'alphaCutoff':m.get('alphaCutoff',.5)})
mins=np.min([e['min'] for e in entries],axis=0);maxs=np.max([e['max'] for e in entries],axis=0)
groups=[]
for group_id in sorted(set(e['group'] for e in entries)):
    subset=[e for e in entries if e['group']==group_id];lo=np.min([e['min'] for e in subset],axis=0);hi=np.max([e['max'] for e in subset],axis=0)
    groups.append({'id':group_id,'name':gltf['nodes'][group_id].get('name',str(group_id)),'min':lo.tolist(),'max':hi.tolist()})
(OUT/'manifest.json').write_text(json.dumps({'source':str(SOURCE),'meshes':entries,'groups':groups,'materials':materials,'min':mins.tolist(),'max':maxs.tolist()},indent=2),encoding='utf-8')
backup=PROJECT_ROOT/'Sources'/'OrbitalFrontier';backup.mkdir(parents=True,exist_ok=True);shutil.copy2(SOURCE,backup/SOURCE.name)
print(json.dumps({'bounds':[mins.tolist(),maxs.tolist()],'vertices':sum(e['vertices'] for e in entries),'triangles':sum(e['triangles'] for e in entries),'meshCount':len(entries),'groups':[{'id':g['id'],'name':g['name'],'size':[round(v,2) for v in (np.array(g['max'])-g['min'])]} for g in groups]},indent=2))
