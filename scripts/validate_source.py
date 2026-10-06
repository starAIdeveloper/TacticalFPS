from pathlib import Path
import json,re
r=Path(__file__).resolve().parents[1]
files=list((r/'Assets').rglob('*.cs'))
for p in files:
 s=p.read_text(); assert s.count('{')==s.count('}'),p
for p in (r/'Assets').rglob('*'):
 if p.suffix!='.meta':assert Path(str(p)+'.meta').exists(),p
metas=list((r/'Assets').rglob('*.meta'))
g=[re.search(r'guid: (\w+)',p.read_text()).group(1) for p in metas]
assert len(set(g))==len(g)
for p in list((r/'Assets').rglob('*.asmdef'))+[r/'Packages/manifest.json']:json.loads(p.read_text())
for name in ['GameDirector','GameHud','WeaponController','EnemyAgent','Bootstrap']:assert (r/f'Assets/TacticalFPS/Runtime/{name}.cs').exists()
assert 'Training.unity' in (r/'ProjectSettings/EditorBuildSettings.asset').read_text()
print(f'Static checks passed: {len(files)} C# files, {len(metas)} metadata GUIDs. Unity execution remains unrun.')
