from pathlib import Path
import tarfile,json,collections
package=Path(r'D:\Downloads\SphereCity_UnityPackage_HDRP_2022.2.16f1.unitypackage')
records=[]
with tarfile.open(package,'r:gz') as tar:
    for entry in tar:
        if entry.name.endswith('/pathname'):
            path=tar.extractfile(entry).read().decode('utf-8-sig').strip()
            records.append({'guid':entry.name.split('/')[0],'path':path})
out=Path(__file__).resolve().parents[1]/'Tools/SphereCityPackageInventory.json';out.write_text(json.dumps(records,indent=2))
print('Count',len(records));print(collections.Counter(Path(r['path']).suffix.lower() for r in records))
for r in records:
    if Path(r['path']).suffix.lower() in ['.prefab','.unity','.cs','.txt','.md','.asset']:
        print(r['path'])
