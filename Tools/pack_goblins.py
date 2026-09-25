"""Pack the project's original CraftPix frames into three mobile sprite atlases.
No external artwork is downloaded. Run from the Unity project root.
"""
from pathlib import Path
from PIL import Image
import json

out=Path('Assets/EndlessHunt/Art/Goblins');out.mkdir(parents=True,exist_ok=True)
manifest=[]
for kind in ['Male Goblin','Female Goblin','Chief Goblin']:
    source=Path('Assets/DUSMAN_ASSETS')/kind/'PNG/PNG Sequences'
    atlas=Image.new('RGBA',(1536,1344))
    frames=[]
    for anim,count in [('Running',6),('Attacking',4),('Hurt',2),('Dying',6)]:
        for direction in (['Front','Back','Left','Right'] if anim!='Dying' else ['']):
            folder=source/(f'{direction} - {anim}' if direction else anim)
            paths=sorted(folder.glob('*.png'))
            assert len(paths)>=count,(folder,len(paths))
            for n in range(count):
                path=paths[round(n*(len(paths)-1)/(count-1))]
                im=Image.open(path).convert('RGBA').resize((192,192),Image.Resampling.LANCZOS)
                index=len(frames);atlas.paste(im,((index%8)*192,(index//8)*192))
                frames.append({'name':f'{anim}_{direction}_{n}','x':(index%8)*192,'y':1344-(index//8+1)*192,'height':(im.getbbox()[3]-im.getbbox()[1])/100,'source':str(path)})
    atlas.save(out/(kind.replace(' ','')+'.png'))
    manifest.append({'kind':kind,'frames':frames})
(out/'manifest.json').write_text(json.dumps({'goblins':manifest},ensure_ascii=False,indent=2),encoding='utf-8')
print('Packed',sum(len(g['frames']) for g in manifest),'frames into 3 atlases')
