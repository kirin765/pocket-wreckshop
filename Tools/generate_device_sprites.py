"""Original low-resolution device sprites, drawn from shapes; no photographic assets.
Design cues: iPhone 16 / iPad Air, AirPods Max, PS5 / DualSense,
a portable cylindrical speaker, and a silver thin-bezel desktop monitor.
Nineteen aligned destruction frames per family, plus matching flying fragments. All batteries are sealed rectangles.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import math
from PIL import ImageChops
OUT=Path(__file__).parent.parent/'Assets/Wreckshop/Art/Devices';OUT.mkdir(exist_ok=True)
W,H=192,224
INK='#1c272e'; EDGE='#829597'; SHELL='#e2e4dc'; LIGHT='#f3f1e7'; DARK='#303e47'; BLUE='#62afc0'; TEAL='#7cb6a1'; ORANGE='#d6a16b'
STEEL='#a3b2b5'; SHADOW='#52656e'; GLASS='#142832'; GREEN='#356b56'; COPPER='#cc9461'
def rr(d,b,fill,r=5,outline=INK,width=2):d.rounded_rectangle(b,radius=r,fill=fill,outline=outline,width=width)
def shade(color,factor):
 rgb=[max(0,min(255,round(int(color.lstrip('#')[i:i+2],16)*factor))) for i in [0,2,4]]
 return '#'+''.join(f'{v:02x}' for v in rgb)
def bevel(d,b,fill,r=5):
 rr(d,b,fill,r,INK,1);x,y,x2,y2=b;light=shade(fill,1.18);shadow=shade(fill,.69)
 d.arc((x+1,y+1,x+2*r+1,y+2*r+1),180,270,fill=light)
 d.arc((x2-2*r-1,y+1,x2-1,y+2*r+1),270,360,fill=light)
 d.arc((x2-2*r-1,y2-2*r-1,x2-1,y2-1),0,90,fill=shadow)
 d.arc((x+1,y2-2*r-1,x+2*r+1,y2-1),90,180,fill=shadow)
 d.line((x+r+1,y+1,x2-r-2,y+1),fill=light)
 if y2-y>2*r+3:
  d.line((x+1,y+r+1,x+1,y2-r-2),fill=light)
  d.line((x2-1,y+r+1,x2-1,y2-r-2),fill=shadow)
 if x2-x>2*r+4:d.line((x+r+1,y2-1,x2-r-2,y2-1),fill=shadow)
def curved_shell(d,points,fill):
 smooth=[];n=len(points)
 for i in range(n):
  p0,p1,p2,p3=[points[j%n] for j in [i-1,i,i+1,i+2]]
  for k in range(5):
   t=k/5
   smooth.append(tuple(round(.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t)) for a,b,c,d in zip(p0,p1,p2,p3)))
 d.polygon(smooth,fill=fill);d.line(smooth+[smooth[0]],fill=INK,width=2,joint='curve')
def driver(d,b,voice=False):
 x,y,x2,y2=b;inset=max(2,(x2-x)//11)
 d.ellipse(b,fill='#1b292f',outline=INK,width=2)
 d.ellipse((x+2,y+2,x2-2,y2-2),fill='#718083',outline='#a8b4b1')
 d.ellipse((x+inset,y+inset,x2-inset,y2-inset),fill='#38494e',outline=COPPER if voice else '#1e3038',width=2)
 d.arc((x+inset+1,y+inset+1,x2-inset-1,y2-inset-1),180,320,fill='#9dadaa')
 d.arc((x+inset+1,y+inset+1,x2-inset-1,y2-inset-1),5,155,fill='#1e3037')
 cx=(x+x2)//2;cy=(y+y2)//2;rw=max(3,(x2-x)//6);rh=max(2,(y2-y)//6)
 d.ellipse((cx-rw,cy-rh,cx+rw,cy+rh),fill='#1e3037',outline='#617477')
 d.arc((cx-rw+1,cy-rh+1,cx+rw-1,cy+rh-1),185,300,fill='#b0bcb6')
def bolt(d,x,y,c=ORANGE):d.polygon([(x+6,y),(x,y+9),(x+5,y+9),(x+3,y+17),(x+12,y+6),(x+7,y+6)],fill=c)
def screw(d,x,y):
 d.ellipse((x-2,y-2,x+2,y+2),fill='#34464c',outline=INK)
 d.ellipse((x-1,y-1,x+1,y+1),fill='#b1c0bc');d.line((x-1,y+1,x+1,y-1),fill='#51676e')
def chip(d,b):
 x,y,x2,y2=b;rr(d,b,'#24363d',1,INK,1)
 for xx in range(x+2,x2-1,4):d.line((xx,y-2,xx,y-1),fill=COPPER);d.line((xx,y2+1,xx,y2+2),fill=COPPER)
 d.line((x+2,y+2,x2-2,y+2),fill='#758b90');d.rectangle((x+2,y+3,x+3,y+4),fill='#adbbb3')
def battery(d,b):
 bevel(d,b,'#33464c',3);x,y,x2,y2=b;w=x2-x;h=y2-y
 d.polygon([(x+2,y+2),(x2-2,y+2),(x2-2,y+5),(x+2,y+9)],fill='#52676c')
 d.line((x+3,y+3,x2-3,y+3),fill='#a0b4b4');d.line((x+3,y+5,x+3,y2-4),fill='#677f85')
 d.rectangle((x+5,y+5,x+10,y+7),fill='#d1d9cd');d.rectangle((x2-10,y+5,x2-5,y+7),fill='#d1d9cd')
 if w>=45 and h>24:
  rr(d,(x+6,y+h//2-10,x2-6,y+h//2+11),'#3c5156',2,'#738985',1)
  d.text((x+9,y+h//2-8),'Li-ion',fill='#bfcbbf')
  for yy in [y+h//2+2,y+h//2+5]:d.line((x+9,yy,x2-10,yy),fill='#849b94')
 else:d.line((x+5,y+h//2,x2-5,y+h//2),fill='#829a97')
 if h>35:
  d.line((x+7,y2-9,x2-7,y2-9),fill='#8fa6a7');d.rectangle((x+7,y2-6,x+15,y2-5),fill=TEAL)
def board(d,b):
 rr(d,b,GREEN,2,INK,1);x,y,x2,y2=b;w=x2-x;h=y2-y
 d.line((x+2,y+2,x2-2,y+2),fill='#79b68e');d.line((x+2,y+2,x+2,y2-2),fill='#559b7d')
 for i,xx in enumerate(range(x+6,max(x+7,x2-5),11)):
  end=min(xx+6,x2-4);yy=y+10+(i%3)*7
  d.line([(xx,y+4),(xx,yy),(end,yy+5),(end,y2-6)],fill='#b0c480')
  d.point((xx,y+4),fill=ORANGE);d.point((end,y2-6),fill=TEAL)
 cw=max(5,min(14,w//4));ch=max(5,min(14,h//4))
 chip(d,(x+4,y+6,x+4+cw,y+6+ch));chip(d,(x2-cw-5,y2-ch-8,x2-5,y2-8))
 if w>42 and h>35:
  cx=(x+x2)//2;cy=(y+y2)//2;chip(d,(cx-8,cy-7,cx+8,cy+7))
  for xx,yy in [(x+6,y2-17),(x2-9,y+9),(x+w//3,y+7)]:
   d.rectangle((xx-2,yy-2,xx+2,yy+2),fill='#d5d9c5',outline=INK);d.point((xx,yy-1),fill=LIGHT)
 for xx in range(x+4,x2-3,5):d.rectangle((xx,y2-3,xx+2,y2),fill=ORANGE)
def pixel_time(d,x,y,scale=3):
 patterns={0:['111','101','101','101','111'],9:['111','101','111','001','111'],4:['101','101','111','001','001'],1:['010','110','010','010','111']}
 for j,n in enumerate([0,9,4,1]):
  for row,line in enumerate(patterns[n]):
   for col,v in enumerate(line):
    if v=='1':d.rectangle((x+j*5*scale+col*scale,y+row*scale,x+j*5*scale+(col+1)*scale-1,y+(row+1)*scale-1),fill=LIGHT)
 d.rectangle((x+9*scale,y+scale,x+9*scale+scale-1,y+2*scale-1),fill=LIGHT);d.rectangle((x+9*scale,y+3*scale,x+9*scale+scale-1,y+4*scale-1),fill=LIGHT)
def app_icon(d,x,y,c,symbol=0,size=10):
 rr(d,(x,y,x+size,y+size),c,2,None,1);d.line((x+2,y+1,x+size-2,y+1),fill=shade(c,1.18))
 if symbol%4==0:d.ellipse((x+3,y+4,x+size-3,y+size-2),outline=LIGHT)
 elif symbol%4==1:d.line((x+3,y+size-3,x+size//2,y+4,x+size-3,y+size-3),fill=LIGHT)
 elif symbol%4==2:d.rectangle((x+3,y+4,x+size-3,y+size-3),outline=LIGHT)
 else:d.line((x+3,y+5,x+size-3,y+5),fill=LIGHT);d.line((x+3,y+size-3,x+size-3,y+size-3),fill=LIGHT)
def screen(d,b,tablet=False,desktop=False):
 rr(d,b,GLASS,4,INK,1);x,y,x2,y2=b;h=y2-y;w=x2-x
 d.rectangle((x+2,y+2,x2-2,y+int(h*.30)),fill='#263e4b')
 d.polygon([(x+2,y+int(h*.55)),(x2-2,y+int(h*.19)),(x2-2,y2-2),(x+2,y2-2)],fill='#385660')
 d.polygon([(x+2,y+int(h*.72)),(x+int(w*.43),y+int(h*.43)),(x2-2,y+int(h*.64)),(x2-2,y2-2),(x+2,y2-2)],fill='#52737a')
 d.polygon([(x+2,y+int(h*.86)),(x+int(w*.65),y+int(h*.59)),(x2-2,y+int(h*.79)),(x2-2,y2-2),(x+2,y2-2)],fill='#7d9d96')
 d.polygon([(x+int(w*.72),y+2),(x2-2,y+2),(x2-2,y+int(h*.29)),(x+int(w*.89),y+int(h*.39))],fill='#314a55')
 d.line((x+3,y+3,x2-4,y+3),fill='#72878a')
 if desktop:
  wx=x+16;wy=y+12
  rr(d,(wx,wy,x2-31,y2-19),'#ccd4d1',2,'#405159',1);d.rectangle((wx+2,wy+2,x2-33,wy+8),fill='#526b76')
  for xx,c in [(wx+5,'#c68e7e'),(wx+10,ORANGE),(wx+15,TEAL)]:d.rectangle((xx,wy+4,xx+2,wy+6),fill=c)
  d.rectangle((wx+5,wy+13,wx+22,y2-25),fill='#94a6a7')
  for yy in range(wy+15,y2-27,6):d.line((wx+28,yy,x2-43,yy),fill='#82999e')
  d.rectangle((wx+28,y2-34,wx+49,y2-28),fill=BLUE)
  d.rectangle((x+3,y2-11,x2-3,y2-3),fill='#233a43')
  for i,c in enumerate([ORANGE,'#d3978e','#a0a8c0',TEAL]):app_icon(d,x+8+i*10,y2-10,c,i,7)
  d.rectangle((x2-21,y2-8,x2-7,y2-6),fill='#a5bcbd')
 else:
  d.line((x+7,y+7,x+13,y+7),fill='#c9d5d2');d.rectangle((x2-13,y+5,x2-7,y+8),outline=LIGHT);d.rectangle((x2-12,y+6,x2-10,y+7),fill=TEAL)
  pixel_time(d,x+7,y+19,3 if w>90 else 2)
  d.line((x+8,y+38,x+25,y+38),fill='#afc4c4');d.line((x+8,y+41,x+19,y+41),fill='#7e9c9f')
  rr(d,(x+6,y+49,x2-6,y+67),'#334e59',3,'#58727b',1)
  d.ellipse((x+10,y+54,x+18,y+62),fill='#bdab85');d.line((x+23,y+54,x2-12,y+54),fill='#c5d4d0');d.line((x+23,y+60,x2-18,y+60),fill='#87a9ac')
  colors=[ORANGE,'#ce958b','#9da8c0','#7fb5a5']
  for row in range(2):
   for i,c in enumerate(colors):app_icon(d,x+8+i*((w-16)//4),y+77+row*17,c,i+row,10)
  dock_y=y2-21;rr(d,(x+5,dock_y-3,x2-5,dock_y+14),'#354f58',3,None,1)
  for i,c in enumerate(colors):app_icon(d,x+8+i*((w-16)//4),dock_y,c,i+2,10)
  d.rectangle(((x+x2)//2-10,y2-4,(x+x2)//2+10,y2-3),fill='#d8e1da')
 if not tablet:rr(d,((x+x2)//2-12,y+4,(x+x2)//2+12,y+9),'#111d22',3,None,1)
 else:d.ellipse(((x+x2)//2-2,y+3,(x+x2)//2+2,y+7),fill='#111d22')
 d.line((x2-15,y+5,x2-5,y+15),fill='#5c7379',width=1)
 d.point((x2-5,y+16),fill='#7e9395')
def phone(d,s,tablet=False):
 b=(29,20,163,205) if tablet else (52,16,140,207);x,y,x2,y2=b
 rr(d,(x+5,y+5,x2+5,y2+4),'#273238',10,INK,2)
 bevel(d,b,'#879597' if tablet else '#758588',9)
 d.polygon([(x2-3,y+6),(x2,y+9),(x2,y2-7),(x2-4,y2-3)],fill='#3c4d54')
 rr(d,(x+2,y+2,x2-3,y2-3),'#b6c1bf' if tablet else '#9ca9a9',7,'#5e7075',1)
 rr(d,(x+3,y+3,x2-4,y2-4),'#202e34',6,INK,1)
 d.line((x+4,y+11,x+4,y2-15),fill='#d2dad5');d.line((x+7,y+3,x2-10,y+3),fill='#e4e7df')
 d.line((x2-3,y+15,x2-3,y2-18),fill='#43575f')
 for yy in [y+30,y2-35]:d.line((x+1,yy,x+4,yy),fill='#c2ceca');d.line((x2-4,yy,x2-1,yy),fill='#26363e')
 bevel(d,(x-2,y+39,x+1,y+57),'#899b9d',1);bevel(d,(x2-1,y+48,x2+2,y+70),'#71868a',1)
 d.line((x-1,y+41,x-1,y+53),fill='#d1dcd6');d.line((x2,y+51,x2,y+65),fill='#b3c5c3')
 if s<3:
  sb=(x+8,y+9,x2-8,y2-9) if tablet else (x+5,y+6,x2-5,y2-6);screen(d,sb,tablet)
 elif s<6:
  bevel(d,(x+6,y+8,x2-6,y2-9),'#4d6069',5)
  bevel(d,(x+9,y+11,x2-9,y2-13),'#9eadae',4)
  d.line((x+13,y+15,x2-13,y+15),fill='#d4dcd6')
  d.line((x+13,y+17,x2-13,y+17),fill='#b9c5c1')
  for yy in range(y+25,y2-23,9):
   d.line((x+13,yy,x2-13,yy),fill='#8b9c9f');d.line((x+14,yy+1,x2-14,yy+1),fill='#a8b6b5')
  for xx,yy in [(x+10,y+12),(x2-11,y+12),(x+10,y2-14),(x2-11,y2-14)]:screw(d,xx,yy)
  rr(d,((x+x2)//2-14,y2-12,(x+x2)//2+14,y2-7),'#24363e',2,'#74888e',1)
 elif s<9:
  bevel(d,(x+6,y+8,x2-6,y2-8),'#263940',4)
  board(d,(x+9,y+12,x2-9,y+65))
  rr(d,(x+11,y+68,x2-11,y+74),'#ac754d',1,INK,1)
  for xx in range(x+15,x2-14,7):d.line((xx,y+69,xx+3,y+73),fill=ORANGE)
  d.line((x+14,y+76,x2-14,y+76),fill=TEAL)
  battery(d,(x+13,y+81,x2-13,y2-30))
  bevel(d,(x+12,y2-26,x+35,y2-12),'#7d9197',2);bevel(d,(x2-35,y2-26,x2-12,y2-12),'#40545e',2)
  for xx in range(x+16,x+32,4):d.line((xx,y2-23,xx,y2-15),fill='#253840')
  d.ellipse((x2-29,y2-24,x2-18,y2-15),fill=STEEL,outline=INK);d.ellipse((x2-26,y2-21,x2-21,y2-18),fill=DARK)
  if not tablet:
   rr(d,(x+11,y+14,x+31,y+51),'#29383c',4,'#85938e',1)
   for yy in [y+18,y+35]:
    d.ellipse((x+15,yy,x+27,yy+12),fill='#111e25',outline='#8d9e9f')
    d.ellipse((x+18,yy+3,x+24,yy+9),fill='#304952');d.point((x+19,yy+3),fill='#bed0ca')
  if s==8:
   d.rectangle((x-2,y+83,x+7,y+121),fill=(0,0,0,0));d.rectangle((x2-7,y+49,x2+6,y+81),fill=(0,0,0,0));d.line((x+9,y2-22,x2-12,y2-24),fill=LIGHT,width=2)
 return b

def headphones(d,s):
 d.arc((28,14,164,175),180,360,fill=INK,width=19)
 d.arc((32,18,160,171),180,360,fill='#8f9e9f',width=13)
 d.arc((35,20,157,168),180,360,fill='#c4ceca',width=7)
 d.arc((37,22,155,166),180,360,fill='#e1e5dc',width=2)
 d.arc((44,30,148,151),184,356,fill='#35464c',width=8)
 for xx in range(54,140,8):
  d.line((xx,28,xx+4,28),fill='#6c7f82',width=1);d.point((xx+1,27),fill='#9caeaa')
 for x in [38,142]:
  bevel(d,(x,81,x+12,120),'#9baaaa',3);d.rectangle((x+2,85,x+3,112),fill='#d4ded7')
  d.line((x+6,104,x+6,125),fill=INK,width=4);d.line((x+5,105,x+5,121),fill=STEEL)
  d.ellipse((x-1,108,x+13,121),fill='#43565e',outline=INK);d.ellipse((x+3,112,x+9,117),fill='#7d9295')
 for b in [(21,105,68,196),(124,105,171,196)]:
  x,y,x2,y2=b;rr(d,(x+3,y+4,x2+3,y2+3),'#25343b',14,INK,2)
  bevel(d,b,'#91a3a5',14);rr(d,(x+2,y+3,x2-3,y2-4),'#a7b6b5',11,'#667c82',1)
  d.polygon([(x+4,y+14),(x+8,y+8),(x2-9,y+8),(x2-13,y+12),(x+8,y+16)],fill='#cbd6cf')
  d.line((x+4,y+20,x+4,y2-19),fill='#d1dcd4');d.line((x2-4,y+19,x2-4,y2-18),fill='#506970',width=2)
  rr(d,(x+8,y+10,x2-8,y2-10),'#23343b',9,INK,2)
  if s<3:
   rr(d,(x+11,y+14,x2-11,y2-14),'#405259',7,'#73878a',1)
   for yy in range(y+19,y2-17,4):
    d.line((x+14,yy,x2-14,yy),fill='#5c7176')
    for xx in range(x+15,x2-14,4):
     offset=1 if (yy//4)%2 else 0;d.point((xx+offset,yy+1),fill='#8d9f9f')
     d.point((xx+offset,yy+2),fill='#293d44')
   d.line((x+14,y+17,x2-15,y+17),fill='#98aaa8')
  elif s<6:
   bevel(d,(x+9,y+12,x2-9,y2-12),'#899b9e',6)
   for yy in range(y+20,y2-16,7):d.line((x+13,yy,x2-12,yy),fill='#bac7c2')
   d.line((x+12,y+16,x2-12,y+16),fill='#d7dfd5')
  else:
   driver(d,(x+7,y+26,x2-7,y2-24))
   if x<50:board(d,(x+9,y+8,x2-9,y+27))
   else:battery(d,(x+11,y+7,x2-11,y+28))
 if s>=8:d.rectangle((88,14,107,40),fill=(0,0,0,0));d.line((86,27,108,33),fill=ORANGE,width=2)
 if s>=7:d.rectangle((51,102,74,118),fill=(0,0,0,0));d.rectangle((121,165,133,184),fill=(0,0,0,0))
 return (20,13,173,198)

def console(d,s):
 d.polygon([(65,21),(123,15),(136,201),(61,206)],fill='#1e2c33',outline=INK)
 d.polygon([(70,27),(116,22),(127,195),(68,199)],fill='#2c3e47')
 d.polygon([(76,30),(89,27),(91,197),(73,198)],fill='#435861')
 d.polygon([(106,24),(116,22),(127,195),(116,197)],fill='#1d2d35')
 d.polygon([(58,18),(72,25),(69,198),(53,212),(43,197),(49,39)],fill='#d6ded8',outline=INK,width=2)
 d.polygon([(58,23),(64,28),(61,192),(52,202),(48,194),(53,41)],fill='#e8ebe2')
 d.polygon([(49,39),(53,41),(48,194),(43,197)],fill='#83999e')
 d.line([(57,31),(55,178),(51,192)],fill='#bcccca',width=1)
 d.polygon([(122,15),(143,25),(151,195),(139,210),(127,201)],fill='#dce2db',outline=INK,width=2)
 d.polygon([(132,24),(140,29),(146,191),(139,201),(134,197)],fill='#a8bcbe')
 d.polygon([(143,34),(148,184),(151,195),(146,191)],fill='#627d87')
 d.line([(128,26),(132,102),(135,184)],fill='#e4e9df',width=1)
 d.line((73,29,72,194),fill='#85c4cb',width=1);d.line((122,24,129,193),fill=BLUE,width=1)
 for yy in range(34,55,3):d.line((84,yy,112,yy-1),fill='#657d86')
 rr(d,(79,63,116,69),'#18272e',1,'#6d858b',1);d.rectangle((84,65,111,66),fill='#a6b9ba')
 rr(d,(86,80,116,84),'#14252d',1,'#6e858a',1)
 rr(d,(83,94,91,99),'#1e3038',1,'#6f858b',1);rr(d,(96,94,104,99),'#1e3038',1,'#6f858b',1)
 d.rectangle((110,94,113,100),fill=TEAL);d.ellipse((106,111,113,119),fill=INK,outline=EDGE);d.point((109,114),fill=LIGHT)
 d.line((137,131,141,184),fill='#526e78',width=1)
 bevel(d,(42,207,152,214),'#344952',3);d.line((54,208,140,208),fill='#a0b4b6');d.line((56,212,138,212),fill='#1c2e36')
 if s>=3:
  d.polygon([(52,40),(59,32),(66,34),(63,190),(54,201),(49,193)],fill='#879b9e',outline='#465e67')
  d.polygon([(130,34),(139,33),(145,188),(138,199),(132,195)],fill='#788f94',outline='#415a64')
  d.line((55,43,54,184),fill='#b3c3c0');d.line((134,41,139,185),fill='#a7babb')
 if 3<=s<6:
  bevel(d,(71,26,126,199),'#899da1',4)
  d.line((76,31,120,29),fill='#ccd6cf');d.line((76,33,120,31),fill='#abbcba')
  for yy in range(41,188,7):
   d.line((78,yy,119,yy),fill='#738b92');d.line((80,yy+1,117,yy+1),fill='#a0b2b3')
  for xx,yy in [(78,36),(119,35),(78,190),(119,189)]:screw(d,xx,yy)
 if s>=6:
  bevel(d,(71,26,126,199),'#354a52',4)
  board(d,(74,36,123,140));bevel(d,(77,146,121,190),'#536b75',2)
  for yy in range(151,185,4):d.line((82,yy,116,yy),fill='#9bb0b3')
  d.line((79,143,119,143),fill=COPPER,width=2);d.rectangle((84,140,97,144),fill=ORANGE)
 if s==8:d.rectangle((60,155,88,181),fill=(0,0,0,0));d.rectangle((116,63,138,87),fill=(0,0,0,0))
 return (41,14,154,216)

def controller(d,s):
 pts=[(42,65),(64,60),(78,68),(114,68),(129,60),(151,66),(164,84),(177,141),(169,158),(153,160),(126,128),(65,128),(39,160),(22,158),(15,143),(28,85)]
 curved_shell(d,pts,'#d3dcd6')
 d.line([(30,84),(44,70),(64,65),(76,73)],fill='#edf0e5',width=1);d.line([(117,73),(130,65),(148,71),(160,86)],fill='#e2e8df',width=1)
 d.polygon([(20,137),(35,111),(53,119),(61,130),(37,156),(24,154)],fill='#95a8ac')
 d.polygon([(23,137),(35,119),(41,121),(33,151),(25,151)],fill='#bdcbc8')
 d.polygon([(132,129),(143,117),(162,111),(173,141),(166,154),(154,156)],fill='#8a9fa4')
 d.polygon([(151,121),(161,117),(169,141),(165,150),(158,151)],fill='#b3c4c2')
 for xx,yy in [(31,137),(37,132),(42,139),(153,137),(160,132),(165,140)]:d.line((xx,yy,xx+2,yy+1),fill='#d2ded6')
 if s<3:
  d.polygon([(68,94),(123,94),(136,123),(122,135),(66,134),(55,123)],fill='#263840',outline=INK,width=2)
  bevel(d,(63,69,127,95),'#354850',5);rr(d,(67,72,123,91),'#4a6068',3,'#7d9295',1)
  d.polygon([(70,75),(120,75),(120,77),(70,80)],fill='#738b90')
  d.line((67,96,124,96),fill='#80bdc5',width=1);d.line((71,98,120,98),fill='#3b717c')
  for xx in range(83,110,4):d.point((xx,103),fill='#9aaeb0');d.point((xx,106),fill='#657e86')
  d.rectangle((40,81,50,107),fill='#20323a');d.rectangle((32,89,58,99),fill='#20323a')
  d.line((43,84,43,103),fill='#788d92');d.line((36,92,54,92),fill='#788d92')
  d.line((50,84,50,103),fill='#111f28');d.line((36,99,54,99),fill='#111f28')
  for i,(x,y,c) in enumerate([(145,81,'#91b4a8'),(158,94,'#c19c9a'),(132,94,'#a0abc0'),(145,107,'#c1b18d')]):
   d.ellipse((x-6,y-6,x+6,y+6),fill='#25373e',outline='#819496')
   d.ellipse((x-4,y-4,x+4,y+4),fill='#c2cdc7');d.arc((x-3,y-3,x+3,y+3),180,320,fill='#e2e8de')
   if i==0:d.polygon([(x,y-2),(x-2,y+2),(x+2,y+2)],fill=c)
   elif i==1:d.ellipse((x-2,y-2,x+2,y+2),outline=c)
   elif i==2:d.line((x-2,y-2,x+2,y+2),fill=c);d.line((x+2,y-2,x-2,y+2),fill=c)
   else:d.rectangle((x-2,y-2,x+2,y+2),outline=c)
  for x in [73,117]:
   d.ellipse((x-13,106,x+13,132),fill='#23343b',outline='#7c9398',width=1)
   d.ellipse((x-10,108,x+10,128),fill='#526a73',outline=INK)
   d.ellipse((x-7,110,x+7,126),fill='#2c4048');d.arc((x-6,111,x+6,125),180,345,fill='#8ca4a7')
   d.arc((x-5,112,x+5,124),15,160,fill='#1d3038');d.line((x-3,113,x+3,113),fill='#bbccc7')
   for xx,yy in [(x-6,113),(x+6,113),(x-6,122),(x+6,122)]:d.point((xx,yy),fill='#6e878c')
  d.ellipse((92,116,98,122),fill='#c8d2cb',outline=INK);rr(d,(86,100,104,104),'#1c3038',1,'#6f898e',1)
  for x,y in [(29,113),(159,119),(57,76),(133,75)]:screw(d,x,y)
 else:
  housing='#7e9296' if s<6 else '#30464d'
  curved_shell(d,pts,housing)
  d.polygon([(68,94),(123,94),(136,123),(122,135),(66,134),(55,123)],fill=housing,outline=INK,width=2)
  if s<6:
   bevel(d,(54,77,137,126),'#95a7a8',8)
   for yy in range(86,119,7):d.line((66,yy,125,yy),fill='#c0cdca')
   for yy in range(128,149,5):d.line((32,yy,47,yy+4),fill='#b5c5c2');d.line((146,yy+4,161,yy),fill='#acbfbd')
   for xx,yy in [(58,83),(133,83),(60,120),(130,120)]:screw(d,xx,yy)
  else:
   board(d,(55,76,138,113));battery(d,(78,116,114,139))
   for b in [(30,124,52,146),(140,124,162,146)]:
    bevel(d,b,'#809598',5);x,y,x2,y2=b
    for yy in range(y+4,y2-3,4):d.line((x+4,yy,x2-4,yy),fill='#c0ceca')
   d.line((54,112,72,122),fill=COPPER,width=2);d.line((119,122,140,113),fill=COPPER,width=2)
 if s>=7:d.rectangle((31,133,52,148),fill=(0,0,0,0));d.line((119,102,136,115),fill=LIGHT,width=2)
 if s==8:d.rectangle((139,116,157,135),fill=(0,0,0,0))
 return (13,58,180,164)

def speaker(d,s):
 bevel(d,(48,42,144,189),'#34454d',21);bevel(d,(52,45,140,185),'#50656b',18)
 d.polygon([(55,61),(61,57),(61,173),(56,165)],fill='#6f8588');d.polygon([(134,59),(139,65),(139,171),(134,178)],fill='#2c424b')
 d.ellipse((51,33,141,66),fill='#2c3d45',outline=INK,width=2);d.ellipse((58,38,134,59),fill='#708489',outline='#a1b4b4')
 d.ellipse((63,41,129,55),fill='#41575e');d.arc((64,39,130,55),180,350,fill='#bccbc6')
 rr(d,(85,41,107,47),'#23373f',2,INK,1);d.line((91,43,101,43),fill='#b6c6c1')
 for x in [72,116]:
  d.ellipse((x-5,42,x+5,52),fill='#2d4149',outline='#8fa6a8');d.line((x-2,47,x+2,47),fill='#c2d0ca')
 d.line((114,45,114,49),fill=LIGHT);d.point((96,52),fill=TEAL)
 if s<3:
  rr(d,(57,63,135,177),'#354b52',10,'#71878b',1)
  for yy in range(68,172,4):
   d.line((62,yy,130,yy),fill='#263e46')
   for xx in range(63,131,4):
    offset=2 if (yy//4)%2 else 0
    d.line((xx+offset,yy,xx+1+offset,yy+1),fill='#71898b')
    d.point((xx+offset,yy+2),fill='#516b70')
  d.line((59,70,59,161),fill='#9bb2af');d.line((61,174,131,174),fill='#263b43',width=3)
  d.line((65,177,127,177),fill='#81989a')
 elif s<6:
  bevel(d,(59,63,133,178),'#83999d',12)
  d.line((65,69,127,69),fill='#c8d3cc');d.line((65,71,127,71),fill='#a7b9b7')
  for yy in [91,149]:driver(d,(69,yy-21,123,yy+21))
 else:
  driver(d,(61,64,131,124),True)
  battery(d,(65,132,106,173));board(d,(110,130,134,174))
  d.line((106,126,122,132),fill=COPPER,width=2);d.rectangle((108,125,117,128),fill=ORANGE)
 if s>=8:d.rectangle((47,84,59,105),fill=(0,0,0,0))
 if s>=7:d.rectangle((124,143,145,162),fill=(0,0,0,0));d.line((85,105,90,116),fill=ORANGE,width=2)
 return (46,32,146,192)

def monitor(d,s):
 bevel(d,(9,34,183,155),'#7f9499',5)
 d.polygon([(12,37),(180,37),(180,43),(12,47)],fill='#c1cfcb')
 d.polygon([(179,42),(182,39),(182,150),(178,153)],fill='#425e69')
 rr(d,(14,39,178,145),'#1c3038',3,INK,1)
 d.line((18,41,173,41),fill='#a1babb');d.line((16,43,16,139),fill='#587681')
 if s<3:
  screen(d,(19,44,173,139),True,True)
 elif s<6:
  bevel(d,(20,46,172,140),'#8ea3a6',2)
  d.line((24,50,168,50),fill='#d0dad3');d.line((24,52,168,52),fill='#b0c1bd')
  for yy in range(60,134,8):
   d.line((29,yy,164,yy),fill='#7b949a');d.line((29,yy+1,164,yy+1),fill='#a4b7b7')
  for x,y in [(24,50),(168,50),(24,136),(168,136)]:screw(d,x,y)
 else:
  bevel(d,(20,46,172,140),'#334b53',2)
  board(d,(26,59,116,129));bevel(d,(122,60,164,128),'#506b75',2)
  for yy in range(67,113,6):d.line((128,yy,158,yy),fill='#95adb1')
  bevel(d,(130,82,153,109),'#738b91',2);bolt(d,136,88)
  d.line((115,119,126,112),fill=COPPER,width=2);d.rectangle((116,115,122,120),fill=ORANGE)
 d.rectangle((87,147,104,151),fill='#1c3038');d.rectangle((91,148,99,149),fill=TEAL);d.point((102,149),fill=LIGHT)
 d.polygon([(83,155),(109,155),(115,183),(133,190),(133,198),(57,198),(57,190),(77,183)],fill='#98adb1',outline=INK,width=2)
 d.polygon([(84,158),(92,158),(85,182),(66,190),(61,190),(79,181)],fill='#d7e0d7')
 d.polygon([(103,158),(109,158),(115,183),(128,190),(121,190),(108,183)],fill='#5c7983')
 d.line((63,192,127,192),fill='#d9e2d9');d.line((70,196,122,196),fill='#506d77')
 if s>=7:d.rectangle((11,109,25,126),fill=(0,0,0,0));d.line((63,97,103,108),fill=LIGHT,width=2)
 if s==8:d.rectangle((93,162,114,176),fill=(0,0,0,0))
 return (7,32,185,201)

renderers={'phone':lambda d,s:phone(d,s),'tablet':lambda d,s:phone(d,s,True),'headphones':headphones,'console':console,'controller':controller,'speaker':speaker,'monitor':monitor}
def layer_damage(im,bounds,kind,layer,level):
 if level==0:return im
 x,y,x2,y2=bounds;w=x2-x;h=y2-y
 spots=[(.35,.29),(.73,.62),(.27,.79),(.66,.19),(.52,.47)]
 if kind=='headphones':spots=[(.16,.65),(.83,.75),(.16,.84),(.84,.57),(.35,.13)]
 overlay=Image.new('RGBA',im.size);d=ImageDraw.Draw(overlay)
 for i,(nx,ny) in enumerate(spots[:level]):
  cx=int(x+nx*w);cy=int(y+ny*h);r=5+i
  path=[(cx-r,cy-3),(cx-2,cy),(cx+1,cy+3),(cx+r,cy+5)]
  branch=[(cx-2,cy),(cx+1,cy-r),(cx+4,cy-r-2)]
  if layer==0:
   d.line(path,fill=INK,width=2);d.line([(px,py-1) for px,py in path],fill='#c9d7d4',width=1)
   d.line(branch,fill='#b4c7c7',width=1)
   if i<level-1:d.line([(cx+1,cy+3),(cx-2,cy+r),(cx-6,cy+r+3)],fill='#8fa9af',width=1)
  elif layer==1:
   d.line(path,fill='#34434a',width=2);d.line([(px,py+2) for px,py in path],fill='#b5c0bc',width=1)
   d.line(branch,fill='#52646a',width=1)
  else:
   d.line(path,fill='#182a30',width=1);d.line(branch,fill='#233a3d',width=1)
   d.line([(cx-r+1,cy-2),(cx-2,cy+1)],fill='#aa9267',width=1)
  if level>=3 and i<2:
   d.polygon([(cx-2,cy),(cx+2,cy-2),(cx+4,cy+3),(cx,cy+5)],fill=INK)
   d.line((cx-1,cy+4,cx+2,cy+3),fill='#819399' if layer else '#d1dfd9')
 overlay.putalpha(ImageChops.multiply(overlay.getchannel('A'),im.getchannel('A')))
 im.alpha_composite(overlay);return im

def mount_layers(layers):
 silhouette=ImageChops.lighter(layers[0].getchannel('A'),layers[-1].getchannel('A'))
 for layer in layers:
  supported=ImageChops.darker(layer.getchannel('A'),silhouette)
  missing=ImageChops.subtract(silhouette,supported)
  layer.paste('#40565e',(0,0,W,H),missing)
  layer.putalpha(silhouette)

fragment_dir=OUT/'Fragments';fragment_dir.mkdir(exist_ok=True)
for kind,draw in renderers.items():
 layers=[];bounds=None
 for state in [0,3,6]:
  layer=Image.new('RGBA',(W,H));bounds=draw(ImageDraw.Draw(layer),state);layers.append(layer)
 mount_layers(layers)
 frames=[layer_damage(layer.copy(),bounds,kind,phase,progress) for phase,layer in enumerate(layers) for progress in range(6)]
 im=Image.new('RGBA',(W,H));draw(ImageDraw.Draw(im),8);source=im.copy();d=ImageDraw.Draw(im);x,y,x2,y2=bounds
 for yy in [y+(y2-y)//3,y+2*(y2-y)//3]:
  d.line([(x+3,yy),(x+19,yy+2),((x+x2)//2,yy-1),(x2-4,yy+3)],fill=(0,0,0,0),width=3)
 for xx,yy,dx,dy in [(x+4,y+12,-5,-3),(x2-17,y+39,5,-2),(x+6,y2-27,-4,4),(x2-21,y2-21,5,3)]:
  piece=source.crop((xx,yy,xx+15,yy+17));d.rectangle((xx,yy,xx+14,yy+16),fill=(0,0,0,0));im.alpha_composite(piece,(max(0,xx+dx),max(0,yy+dy)))
 d=ImageDraw.Draw(im)
 for xx,yy in [(max(2,x-6),y+54),(min(W-5,x2+3),y2-35),(x+23,min(H-6,y2+4))]:
  d.rectangle((xx,yy,xx+3,yy+2),fill=SHELL);d.point((xx,yy),fill=INK)
 frames.append(im)
 boxes=[im.getbbox() for im in frames];left=max(0,min(b[0] for b in boxes)-4);top=max(0,min(b[1] for b in boxes)-4);right=min(W,max(b[2] for b in boxes)+4);bottom=min(H,max(b[3] for b in boxes)+4)
 for i,frame in enumerate(frames):frame.crop((left,top,right,bottom)).save(OUT/f'{kind}_{i:02}.png')
 # Chunks cut from this model's own glass, case and internal frame.
 for phase in range(3):
  src=frames[phase*6+5];candidates=[]
  for yy in range(y+5,y2-20,14):
   for xx in range(x+3,x2-18,14):
    tile=src.crop((xx,yy,xx+20,yy+24))
    if sum(v>0 for v in tile.getchannel('A').getdata())>280:candidates.append((xx,yy))
  for j in range(4):
   xx,yy=candidates[min(len(candidates)-1,int((j+.5)*len(candidates)/4))]
   tile=src.crop((xx,yy,xx+20,yy+24));mask=Image.new('L',tile.size);md=ImageDraw.Draw(mask)
   md.polygon([(2,1),(15,0),(19,8),(15,21),(6,23),(0,14)],fill=255)
   from PIL import ImageChops
   tile.putalpha(ImageChops.multiply(tile.getchannel('A'),mask));tile.save(fragment_dir/f'{kind}_{phase}_{j}.png')
print('Upgraded seven device families with ordered layer damage and matching fragments.')
# A plain desk: no right-hand tray, bulletin, tools or other decorative side props.
im=Image.new('RGBA',(450,688),'#997a58');d=ImageDraw.Draw(im)
for y in [0,112,232,356,481,603]:d.line((0,y,449,y),fill='#836447',width=2);d.line((0,y+3,449,y+3),fill='#aa8862')
for x,y,l in [(10,49,95),(305,160,118),(28,300,101),(322,449,87),(18,560,77),(281,639,140)]:d.line((x,y,x+l,y),fill='#a5835d')
rr(d,(25,32,424,605),'#455e69',5,'#354752',2)
d.rectangle((30,37,419,600),outline='#70878b',width=1)
for x,y in [(38,45),(411,45),(38,592),(411,592)]:d.rectangle((x-1,y-1,x+1,y+1),fill='#96a7a1')
im.save(OUT.parent/'portrait_desk.png')
im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im)
rr(d,(7,4,25,29),'#344154',3,'#283346',2)
d.rectangle((12,9,17,11),fill='#f6f1e4');d.rectangle((15,9,20,11),fill='#f6f1e4')
bolt(d,10,8,'#e4b57e');d.rectangle((12,1,20,4),fill='#617388');im.save(OUT.parent/'part_2.png')
