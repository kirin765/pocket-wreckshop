"""Check aligned persistent damage, opaque pixels, and discrete layer changes."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import hashlib,json
root=Path(__file__).parent.parent
art=root/'Assets/Wreckshop/Art/Devices'
kinds=['phone','tablet','headphones','console','controller','speaker','monitor'];result={}
sheet=Image.new('RGB',(110*10,240*7),'#405c65');draw=ImageDraw.Draw(sheet)
for row,kind in enumerate(kinds):
 images=[Image.open(art/f'{kind}_{i:02}.png').convert('RGBA') for i in range(19)]
 assert len({im.size for im in images})==1,f'{kind}: alignment'
 assert len({hashlib.sha256(im.tobytes()).hexdigest() for im in images})==19,f'{kind}: duplicate states'
 assert len({im.getchannel('A').tobytes() for im in images[:18]})==1,f'{kind}: layer silhouette or coverage must not change'
 for i,im in enumerate(images):
  assert set(im.getchannel('A').getdata())<={0,255},f'{kind} {i:02}: translucent pixels can create layer ghosting'
 area=sum(p[3]>0 for p in images[0].getdata());changed=[];deltas=[]
 for old,new in zip(images,images[1:]):
  changed.append(round(sum(a!=b for a,b in zip(old.getdata(),new.getdata()))/area*100,2))
  histogram=ImageChops.difference(old,new).histogram()
  deltas.append(round(sum(value*histogram[channel*256+value] for channel in range(4) for value in range(256))/(area*4*255)*100,3))
 assert 0<changed[0]<1,f'{kind}: first crack should affect a small local area'
 assert changed[5]>5 and changed[11]>5,f'{kind}: each boundary must replace the complete current layer'
 assert deltas[5]>1 and deltas[11]>1,f'{kind}: layer cuts must be visible, not gentle crossfades'
 cuts=[deltas[5],deltas[11]]
 result[kind]={'first_crack_percent':changed[0],'layer_cut_mean_rgba_percent':cuts,'mean_rgba_change_percent':deltas,'changed_pixels_percent':changed,'opaque_aligned_layers':True}
 for col,index in enumerate([0,1,5,6,7,11,12,13,17,18]):
  im=images[index].copy();im.thumbnail((100,208),Image.Resampling.NEAREST);sheet.paste(im,(col*110+(110-im.width)//2,row*240+8),im);draw.text((col*110+6,row*240+218),f'{kind} {index}',fill='white')
(root/'Documentation/Damage-Detail-Stats.json').write_text(json.dumps(result,indent=2)+'\n')
sheet.save(root/'Documentation/Damage-Details.png')
print('PASS: aligned persistent damage, fully opaque layers, and discrete layer cuts without fades.')
print({k:v['layer_cut_mean_rgba_percent'] for k,v in result.items()})
