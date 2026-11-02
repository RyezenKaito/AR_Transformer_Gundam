from pathlib import Path,PurePosixPath
import tarfile,json,shutil,collections
root=Path(__file__).resolve().parents[1]
records=json.loads((root/'Tools/SphereCityPackageInventory.json').read_text())
mapping={r['guid']:r['path'] for r in records}
raw=root/'Sources/SphereCity';dest=root/'Assets/ThirdParty/SphereCity'
raw.mkdir(parents=True,exist_ok=True);dest.mkdir(parents=True,exist_ok=True)
with tarfile.open(r'D:\Downloads\SphereCity_UnityPackage_HDRP_2022.2.16f1.unitypackage','r:gz') as tar:
    for entry in tar:
        bits=entry.name.split('/')
        if len(bits)!=2 or bits[0] not in mapping or bits[1] not in ('asset','asset.meta') or not entry.isfile():continue
        original=PurePosixPath(mapping[bits[0]])
        if original.is_absolute() or '..' in original.parts or original.parts[0]!='Assets':raise ValueError(str(original))
        relative=Path(*original.parts[1:]);suffix='' if bits[1]=='asset' else '.meta'
        target=raw/(str(relative)+suffix);target.parent.mkdir(parents=True,exist_ok=True)
        with tar.extractfile(entry) as src,target.open('wb') as out:shutil.copyfileobj(src,out)
        # Preserve package source separately; import only geometry/material/texture data, never project HDRP settings or scripts.
        allowed=relative.suffix.lower() in ('.fbx','.mat','.tga','.png','.jpg','.bmp') and str(relative).replace('\\','/').startswith('Art/')
        if allowed:
            output=dest/(str(relative)+suffix);output.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(target,output)
print('Imported',dict(collections.Counter(p.suffix for p in dest.rglob('*') if p.is_file() and p.suffix!='.meta')))
print('Raw source scene:',raw/'Scenes/Main_SphereCity.unity')
