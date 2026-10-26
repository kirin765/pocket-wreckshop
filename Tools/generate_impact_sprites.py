"""Crisp pixel impact animations and material chips; no photographic textures."""
from pathlib import Path
from PIL import Image,ImageDraw
import math
out=Path(__file__).parent.parent/'Assets/Wreckshop/Art/Effects';out.mkdir(exist_ok=True)
for frame in range(6):
 im=Image.new('RGBA',(48,48));d=ImageDraw.Draw(im);r=4+frame*3
 for j in range(8):
  a=j*math.pi/4+.12;inner=max(0,r-6+frame);outer=r+2
  p=(round(24+math.cos(a)*inner),round(24+math.sin(a)*inner));q=(round(24+math.cos(a)*outer),round(24+math.sin(a)*outer))
  d.line((p,q),fill='#f6f1e4' if j%2 else '#a3cabb',width=3 if frame<2 else 2)
 if frame<2:d.polygon([(24,16),(27,22),(33,24),(27,27),(24,32),(21,27),(15,24),(21,21)],fill='#f6f1e4')
 im.save(out/f'impact_{frame}.png')
colors=[('#73a9bb','#d7eee2'),('#a0adba','#f6f1e4'),('#77a997','#e4b57e')]
for material,(base,edge) in enumerate(colors):
 for variant in range(4):
  im=Image.new('RGBA',(16,16));d=ImageDraw.Draw(im)
  points=[[(1,4),(12,2),(9,13)],[(3,1),(14,8),(3,13),(6,7)],[(2,3),(10,2),(14,10),(5,13)],[(2,7),(7,1),(13,5),(10,14)]][variant]
  d.polygon(points,fill=base);d.line(points[:2],fill=edge,width=2)
  if material==2:d.rectangle((6,6,9,9),fill='#283346')
  im.save(out/f'chip_{material}_{variant}.png')
for i in range(4):
 im=Image.new('RGBA',(24,24));d=ImageDraw.Draw(im)
 for j in range(5):
  x=4+(j*7+i*3)%16;y=4+(j*5+i*2)%16;s=max(1,3-i//2)
  d.rectangle((x,y,x+s,y+s),fill='#b3bfc4')
 im.save(out/f'dust_{i}.png')
print('Created 22 impact/material/dust sprites.')
