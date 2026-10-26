"""Check artwork for small local changes instead of abrupt whole-layer replacement."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json
root=Path(__file__).parent.parent
art=root/'Assets/Wreckshop/Art/Devices'
kinds=['phone','tablet','headphones','console','controller','speaker','monitor'];result={}
sheet=Image.new('RGB',(110*10,240*7),'#405c65');draw=ImageDraw.Draw(sheet)
for row,kind in enumerate(kinds):
 images=[Image.open(art/f'{kind}_{i:02}.png').convert('RGBA') for i in range(19)]
 assert len({im.size for im in images})==1,f'{kind}: alignment'
 assert len({hashlib.sha256(im.tobytes()).hexdigest() for im in images})==19,f'{kind}: duplicate states'
 area=sum(p[3]>0 for p in images[0].getdata());changes=[]
 for old,new in zip(images,images[1:]):changes.append(round(sum(a!=b for a,b in zip(old.getdata(),new.getdata()))/area*100,2))
 assert 0<changes[0]<1,f'{kind}: first crack should affect a small local area'
 assert changes[5]<10 and changes[11]<10,f'{kind}: step boundary replaces too much of the device'
 assert max(changes[:-1])<20,f'{kind}: a normal change replaces too much of the device'
 result[kind]={'first_crack_percent':changes[0],'step_boundaries_percent':[changes[5],changes[11]],'change_percent':changes}
 for col,index in enumerate([0,1,4,5,6,9,11,12,16,18]):
  im=images[index].copy();im.thumbnail((100,208),Image.Resampling.NEAREST);sheet.paste(im,(col*110+(110-im.width)//2,row*240+8),im);draw.text((col*110+6,row*240+218),f'{kind} {index}',fill='white')
(root/'Documentation/Damage-Detail-Stats.json').write_text(json.dumps(result,indent=2)+'\n')
sheet.save(root/'Documentation/Damage-Details.png')
print('PASS: all families have small first cracks, gentle step boundaries, aligned persistent damage and no full-surface swaps.')
print({k:v['step_boundaries_percent'] for k,v in result.items()})
