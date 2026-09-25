"""Copy only reviewed assets from the isolated Unity validation project."""
from pathlib import Path
import hashlib
import shutil
import re

root=Path('D:/OyunProjelerim/Endless_Hunt')
clone=Path('D:/OyunProjelerim/EndlessHunt_Validation_20260909')
backup=root/'Logs/DemoRevisionBackup-20260909'
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def copy(relative):
    src=clone/relative;dst=root/relative
    dst.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(src,dst)

scenes=['Assets/Scenes/MainMenu.unity','Assets/Scenes/GameScene.unity','Assets/Scenes/GameScene2.unity','Assets/EndlessHunt/UI/Screens/UI_MobileMenu.prefab']
for rel in scenes:
    assert digest(root/rel)==digest(backup/Path(rel).name),'User file changed during verification: '+rel
assert 'ERRORS: 0' in (clone/'Logs/demo-verification.txt').read_text(encoding='utf-8')
assert (clone/'Logs/android-script-check.txt').read_text(encoding='utf-8').startswith('PASS:')

# Generated scene references use these new scripts' GUIDs from the validation editor.
new_scripts=['Core/LocalPlayerStore','UI/DemoTheme','UI/DemoUI','UI/DemoTypography','UI/DemoMenu','UI/DemoBattle','Gameplay/GoblinSet','Gameplay/RunArsenal','Gameplay/HuntProjectile']
for name in new_scripts:
    rel='Assets/EndlessHunt/'+name+'.cs'
    assert digest(root/rel)==digest(clone/rel),'Source differs from tested code: '+rel
    copy(rel+'.meta')
for name in ['EndlessHuntDemoInspect','EndlessHuntDemoRevision','EndlessHuntDemoVerification','EndlessHuntAndroidScriptCheck']:
    copy('Assets/Editor/'+name+'.cs.meta')
for folder in ['Assets/EndlessHunt/Resources','Assets/EndlessHunt/Art/Goblins']:
    for src in (clone/folder).rglob('*'):
        if src.is_file():copy(src.relative_to(clone))
    if (clone/(folder+'.meta')).exists():copy(folder+'.meta')
copy('Assets/Fonts/myriad-pro/MYRIADPRO-BOLD SDF.asset')
for rel in scenes:copy(rel)
for name in ['demo-verification.txt','android-script-check.txt','topbar-preservation.txt']:
    copy('Logs/'+name)
for src in (clone/'Logs/DemoPreviews').glob('*.png'):copy(src.relative_to(clone))

# Verify every non-builtin GUID referenced by the delivered scenes/prefab/theme/goblins.
known=set()
for folder in [root/'Assets',root/'Packages',root/'Library/PackageCache']:
    if not folder.exists():continue
    for meta in folder.rglob('*.meta'):
        found=re.search(r'^guid: ([a-f0-9]{32})',meta.read_text(encoding='utf-8',errors='ignore'),re.M)
        if found:known.add(found.group(1))
targets=[root/p for p in scenes]+list((root/'Assets/EndlessHunt/Resources').glob('*.asset'))+list((root/'Assets/EndlessHunt/Resources').glob('*.mat'))
refs=set()
for p in targets:refs.update(re.findall(r'guid: ([a-f0-9]{32})',p.read_text(encoding='utf-8')))
missing=sorted(g for g in refs if g not in known and not g.startswith('0000000000000000'))
report=f'Referenced GUIDs: {len(refs)}\nMissing GUIDs: {len(missing)}\n'+'\n'.join(missing)
(root/'Logs/demo-asset-integrity.txt').write_text(report,encoding='utf-8')
assert not missing,report
(root/'Logs/MobilePolishReload.request').write_text('Load verified demo UI revision',encoding='utf-8')
print('Delivered reviewed scenes, resources, script metadata and captures. '+report)
