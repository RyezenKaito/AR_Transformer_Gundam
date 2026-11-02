from pathlib import Path
import shutil,json
from PIL import Image,ImageOps
source=Path(r'D:\Downloads\Modular SciFi MegaKit[Standard]\Modular SciFi MegaKit[Standard]')
dest=Path(__file__).resolve().parents[1]/'Assets/ThirdParty/Quaternius/ModularSciFiMegaKit'
dest.mkdir(parents=True,exist_ok=True)
for a,b in [('FBX (Unity)','Models'),('Textures','Textures')]:
    shutil.copytree(source/a,dest/b,dirs_exist_ok=True)
shutil.copy2(source/'License_Standard.txt',dest/'License_Standard.txt')
generated=dest/'Textures/URP';generated.mkdir(exist_ok=True)
for p in (dest/'Textures').glob('*_ORM.png'):
    ao,rough,metal=Image.open(p).convert('RGB').split()
    blank=Image.new('L',metal.size,0)
    Image.merge('RGBA',(metal,blank,blank,ImageOps.invert(rough))).save(generated/(p.stem.replace('_ORM','_MetallicSmoothness')+'.png'))
    Image.merge('RGB',(ao,ao,ao)).save(generated/(p.stem.replace('_ORM','_Occlusion')+'.png'))
materials={}
for p in (source/'glTF').rglob('*.gltf'):
    data=json.loads(p.read_text())
    for m in data.get('materials',[]):
        pbr=m.get('pbrMetallicRoughness',{})
        def texture(info):
            if not info:return ''
            uri=data['images'][data['textures'][info['index']]['source']].get('uri','')
            return Path(uri.replace('\\','/')).name
        materials[m['name']]={'name':m['name'],'color':pbr.get('baseColorFactor',[1,1,1,1]),'baseMap':texture(pbr.get('baseColorTexture')),'normal':texture(m.get('normalTexture')),'orm':texture(pbr.get('metallicRoughnessTexture')),'emissionMap':texture(m.get('emissiveTexture')),'emission':m.get('emissiveFactor',[0,0,0]),'metallic':pbr.get('metallicFactor',1),'roughness':pbr.get('roughnessFactor',1),'alpha':m.get('alphaMode','OPAQUE')}
(dest/'SourceMaterialReference.json').write_text(json.dumps(materials,indent=2))
(dest/'MaterialImport.json').write_text(json.dumps({'materials':list(materials.values())},indent=2))
print(json.dumps({'models':len(list((dest/'Models').rglob('*.fbx'))),'textures':len(list((dest/'Textures').glob('*.png'))),'materials':list(materials),'mb':round(sum(p.stat().st_size for p in dest.rglob('*') if p.is_file())/1e6,1)}))
