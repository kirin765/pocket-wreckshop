"""Generate original pixel tools and shared part icons. Run generate_device_sprites.py afterward."""
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).parent.parent/'Assets'/'Wreckshop'/'Art'
ROOT.mkdir(exist_ok=True)
for key in range(4):
 im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im)
 if key==0:
  d.rectangle((8,2,24,29),fill='#a8ddbd');d.rectangle((10,5,22,25),fill='#345b59');d.polygon([(10,18),(22,8),(22,15),(10,25)],fill='#81bfa1');d.rectangle((14,27,18,28),fill='#567d66')
 elif key==1:
  d.rectangle((5,5,27,28),fill='#9cc58e');d.rectangle((11,11,21,21),fill='#304936')
  for x in [8,15,23]:d.line((x,2,x,7),fill='#dbbf6e',width=2);d.line((x,25,x,30),fill='#dbbf6e',width=2)
  for y in [11,20]:d.line((2,y,7,y),fill='#dbbf6e',width=2);d.line((25,y,30,y),fill='#dbbf6e',width=2)
 elif key==2:
  d.rectangle((9,5,23,29),fill='#e3ca7b');d.rectangle((13,2,19,4),fill='#a29155');d.rectangle((9,23,23,29),fill='#8a7951');d.polygon([(17,8),(12,17),(16,17),(15,22),(21,13),(17,13)],fill='#f8efbd')
 else:
  d.rectangle((7,2,25,30),fill='#b6a2ce');d.rectangle((10,5,16,12),fill='#544b6c');d.rectangle((11,6,14,9),fill='#e5d4ef');d.rectangle((14,24,19,26),fill='#ded2ea')
 im.save(ROOT/f'part_{key}.png')
im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
d.polygon([(25,21),(36,29),(15,61),(5,53)],fill='#9f714b');d.polygon([(27,26),(31,29),(13,55),(9,52)],fill='#cda172');d.polygon([(20,4),(29,0),(61,26),(53,39),(15,14)],fill='#adc2b3');d.polygon([(24,6),(29,4),(55,26),(50,29)],fill='#e1ead3');d.polygon([(53,33),(59,25),(63,30),(57,41)],fill='#4d655b');im.save(ROOT/'hammer.png')
for i in range(1,4):
 im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
 d.polygon([(26,19),(36,27),(14,62),(4,55)],fill='#4d433a');d.polygon([(27,23),(32,27),(12,57),(7,54)],fill='#b68450')
 if i==1:
  d.polygon([(18,1),(28,1),(62,27),(61,37),(52,43),(12,12)],fill='#273c3a');d.polygon([(20,4),(28,4),(58,28),(53,36),(15,11)],fill='#9ccabb');d.line((23,6,54,29),fill='#d7e9d3',width=3)
 elif i==2:
  d.polygon([(12,1),(30,0),(63,28),(63,42),(50,48),(4,14)],fill='#373937');d.polygon([(14,4),(29,3),(59,29),(50,42),(9,14)],fill='#a8a9a1');d.line((18,6,55,30),fill='#ded5ae',width=4);d.line((12,14,49,40),fill='#797e75',width=3)
 else:
  d.polygon([(12,0),(30,0),(63,29),(62,45),(47,49),(3,12)],fill='#332e4d');d.polygon([(15,3),(28,3),(59,30),(47,43),(8,12)],fill='#9880c3');d.line((19,6,54,30),fill='#ddc5f5',width=4)
  for x,y in [(21,15),(30,22),(39,30)]:d.rectangle((x,y,x+7,y+3),fill='#8be3d7')
 im.save(ROOT/f'hammer_{i}.png')
