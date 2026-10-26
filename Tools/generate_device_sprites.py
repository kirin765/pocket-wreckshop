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
INK='#283346'; EDGE='#617388'; SHELL='#dce2df'; LIGHT='#f6f1e4'; DARK='#344154'; BLUE='#73b8cd'; TEAL='#91c8b8'; ORANGE='#e4b57e'
def rr(d,b,fill,r=5,outline=INK,width=2):d.rounded_rectangle(b,radius=r,fill=fill,outline=outline,width=width)
def bolt(d,x,y,c=ORANGE):d.polygon([(x+6,y),(x,y+9),(x+5,y+9),(x+3,y+17),(x+12,y+6),(x+7,y+6)],fill=c)
def battery(d,b):
 rr(d,b,DARK,3);x,y,x2,y2=b;d.rectangle((x+5,y+5,x+10,y+7),fill=LIGHT);d.rectangle((x2-10,y+5,x2-5,y+7),fill=LIGHT);bolt(d,(x+x2)//2-6,(y+y2)//2-8)
def board(d,b):
 rr(d,b,'#77a997',3);x,y,x2,y2=b
 for xx in range(x+5,x2-4,12):d.line((xx,y+4,xx,y2-4),fill='#b4d2a9')
 for xx,yy in [(x+7,y+9),(x2-19,y2-23)]:rr(d,(xx,yy,xx+13,yy+13),DARK,1,INK,1)
 for xx in range(x+4,x2-3,6):d.rectangle((xx,y2-3,xx+2,y2),fill=ORANGE)
def screen(d,b,tablet=False):
 rr(d,b,'#38556e',5,INK,1);x,y,x2,y2=b
 d.polygon([(x+1,y+int((y2-y)*.60)),(x2-1,y+int((y2-y)*.20)),(x2-1,y2-2),(x+1,y2-2)],fill='#73a9bb')
 d.polygon([(x+1,y+int((y2-y)*.80)),(x2-1,y+int((y2-y)*.55)),(x2-1,y2-3),(x+1,y2-3)],fill='#a3cabb')
 for i,c in enumerate(['#e5c68c','#eab1a0','#a8a9cc','#8bbcc8']):
  xx=x+7+i*((x2-x-12)//4);yy=y2-20;rr(d,(xx,yy,xx+8,yy+8),c,2,INK,1)
 if not tablet:rr(d,((x+x2)//2-12,y+4,(x+x2)//2+12,y+9),INK,3,INK,1)
 else:d.ellipse(((x+x2)//2-2,y+3,(x+x2)//2+2,y+7),fill=INK)
def cracks(d,b,amount):
 x,y,x2,y2=b;cx=(x+x2)//2;cy=(y+y2)//2+12
 paths=[[(cx,cy),(cx-11,cy-8),(cx-7,cy-20),(x+5,y+6)],[(cx,cy),(cx+10,cy-11),(x2-4,y+15)],[(cx,cy),(cx+6,cy+15),(x2-8,y2-5)],[(cx,cy),(cx-17,cy+9),(x+3,y2-19)]]
 for p in paths[:amount]:d.line(p,fill='#eef0dd',width=1)
 d.rectangle((cx-2,cy-2,cx+2,cy+2),fill=INK)
def phone(d,s,tablet=False):
 b=(29,20,163,205) if tablet else (52,16,140,207);x,y,x2,y2=b
 rr(d,(x+4,y+4,x2+4,y2+3),'#2b3342',9,INK,2)
 rr(d,b,'#a0adba',9,INK,2);rr(d,(x+3,y+3,x2-3,y2-3),SHELL,8,EDGE,1)
 d.line((x+2,y+12,x+2,y2-13),fill=LIGHT);d.line((x2+1,y+43,x2+1,y+59),fill=INK,width=2)
 if s<3:
  sb=(x+7,y+8,x2-7,y2-8) if tablet else (x+4,y+5,x2-4,y2-5);screen(d,sb,tablet)
  # Block drawn clock avoids a photographic screen or a generated typeface.
  for j,n in enumerate([0,9,4,1]):
   xx=x+13+j*14;yy=y+35
   patterns={0:['111','101','101','101','111'],9:['111','101','111','001','111'],4:['101','101','111','001','001'],1:['010','110','010','010','111']}
   for row,line in enumerate(patterns[n]):
    for col,v in enumerate(line):
     if v=='1':d.rectangle((xx+col*2,yy+row*2,xx+col*2+1,yy+row*2+1),fill=LIGHT)
  if s:cracks(d,sb,2 if s==1 else 4)
  if s==2:d.rectangle((x+10,y2-44,x2-9,y2-40),fill='#afb7ce')
 elif s<6:
  rr(d,(x+8,y+10,x2-8,y2-11),'#8595a3',5,INK,2)
  d.rectangle((x+13,y+18,x2-13,y2-20),fill='#b7c2c2')
  for xx,yy in [(x+12,y+14),(x2-14,y+14),(x+12,y2-17),(x2-14,y2-17)]:d.rectangle((xx,yy,xx+2,yy+2),fill=INK)
  if s>=4:
   d.line((x+8,y+45,x+18,y+59,x+9,y+77),fill=INK,width=2);d.line((x2-7,y2-51,x2-21,y2-39),fill=INK,width=2)
  if s==5:d.rectangle((x+18,y+30,x2-17,y2-30),fill=DARK);d.rectangle((x+23,y+37,x2-23,y2-37),fill='#95a9ad')
 elif s<9:
  rr(d,(x+7,y+9,x2-7,y2-9),DARK,4)
  board(d,(x+12,y+15,x2-12,y+66));battery(d,(x+17,y+77,x2-17,y2-29))
  d.line((x+12,y+71,x2-11,y+71),fill=ORANGE,width=3);rr(d,(x+14,y2-24,x2-14,y2-13),EDGE,2)
  if s>=7:
   d.line((x+8,y+69,x+25,y+76),fill=LIGHT,width=2);d.rectangle((x+13,y+40,x+23,y+50),fill=DARK)
  if s==8:
   d.rectangle((x-2,y+83,x+7,y+121),fill=(0,0,0,0));d.rectangle((x2-7,y+49,x2+6,y+81),fill=(0,0,0,0));d.line((x+9,y2-22,x2-12,y2-24),fill=LIGHT,width=2)
 return b

def headphones(d,s):
 # AirPods-Max-like headband, rectangular ear cushions and silver yokes.
 d.arc((31,17,161,173),180,360,fill=INK,width=15);d.arc((35,21,157,171),180,360,fill='#adb7bf',width=8)
 d.arc((45,27,147,149),185,355,fill='#c9d8ce',width=5)
 for b in [(24,103,66,194),(126,103,168,194)]:
  rr(d,b,'#b6c3c6',12);rr(d,(b[0]+7,b[1]+9,b[2]-7,b[3]-9),DARK,9)
  if s<3:
   rr(d,(b[0]+11,b[1]+15,b[2]-11,b[3]-15),'#596b7d',7,INK,1)
   if s:d.line((b[0]+13,b[1]+24,b[2]-10,b[3]-21),fill=LIGHT,width=1)
  elif s<6:
   rr(d,(b[0]+8,b[1]+11,b[2]-8,b[3]-11),'#90a1ac',5)
   for yy in range(b[1]+19,b[3]-15,12):d.line((b[0]+14,yy,b[2]-13,yy),fill=EDGE,width=2)
  else:
   d.ellipse((b[0]+8,b[1]+24,b[2]-8,b[3]-24),fill=INK,outline=TEAL,width=3)
   if b[0]<50:board(d,(b[0]+8,b[1]+9,b[2]-8,b[1]+25))
   else:battery(d,(b[0]+10,b[1]+6,b[2]-10,b[1]+27))
 if s>=4:d.rectangle((88,16,107,38),fill=(0,0,0,0));d.line((86,27,108,33),fill=ORANGE,width=2)
 if s>=7:d.rectangle((52,101,74,117),fill=(0,0,0,0));d.rectangle((120,165,131,183),fill=(0,0,0,0))
 return (22,15,170,196)

def console(d,s):
 # The black center and flared white side plates are the PS5 silhouette cues.
 d.polygon([(66,20),(122,15),(135,201),(61,206)],fill=INK)
 d.polygon([(58,18),(72,25),(69,198),(53,212),(43,197),(49,39)],fill=SHELL,outline=INK,width=2)
 d.polygon([(122,15),(143,25),(151,195),(139,210),(127,201)],fill=LIGHT,outline=INK,width=2)
 d.line((74,28,73,194),fill=BLUE,width=2);d.line((122,25,129,193),fill=BLUE,width=2)
 d.rectangle((82,61,113,65),fill='#465365');d.rectangle((88,80,113,83),fill='#465365')
 d.line((136,130,140,184),fill=INK,width=2);rr(d,(43,207,151,213),DARK,3)
 if s in [1,2]:d.line((53,73,62,92,51,118),fill=EDGE,width=2)
 if s>=3:
  d.rectangle((48,32,67,191),fill=(0,0,0,0));d.rectangle((131,37,146,187),fill=(0,0,0,0))
  rr(d,(72,27,125,199),'#8b9aa5',4)
  for yy in range(35,182,7):d.line((78,yy,117,yy),fill=DARK,width=2)
 if s>=5:d.rectangle((77,53,119,177),fill=DARK)
 if s>=6:
  board(d,(76,38,121,139));rr(d,(78,146,120,189),EDGE,2)
  for yy in range(152,185,6):d.line((83,yy,115,yy),fill=INK,width=2)
 if s>=7:d.line((77,100,117,125),fill=LIGHT,width=2)
 if s==8:d.rectangle((60,155,88,181),fill=(0,0,0,0));d.rectangle((116,63,138,87),fill=(0,0,0,0))
 return (41,14,154,215)

def controller(d,s):
 pts=[(43,66),(65,61),(78,70),(114,70),(129,61),(150,67),(163,86),(175,142),(167,158),(151,156),(126,126),(65,126),(40,156),(24,158),(16,145),(28,86)]
 d.polygon(pts,fill=SHELL,outline=INK,width=3)
 d.polygon([(69,94),(122,94),(135,123),(122,132),(66,131),(55,122)],fill=DARK,outline=INK,width=2)
 rr(d,(65,71,125,93),'#a8b9bd',5,INK,2);d.line((68,94,123,94),fill=BLUE,width=2)
 # Direction pad, twin sticks, face buttons.
 d.rectangle((41,83,48,105),fill=INK);d.rectangle((34,90,55,97),fill=INK)
 for x,y,c in [(145,84,'#b2cbbd'),(155,95,'#d7adad'),(134,95,'#b0b7d6'),(145,106,'#cbbd97')]:d.ellipse((x-4,y-4,x+4,y+4),fill=c,outline=INK,width=1)
 for x in [73,117]:d.ellipse((x-11,107,x+11,129),fill=INK,outline=EDGE,width=2);d.ellipse((x-6,111,x+6,123),fill=DARK)
 d.ellipse((93,115,97,119),fill=LIGHT)
 if s in [1,2]:d.line((33,113,48,120,43,139),fill=EDGE,width=2)
 if s>=3:
  d.polygon([(28,113),(54,108),(66,130),(39,156),(24,157)],fill='#90a2af',outline=INK,width=2)
  d.polygon([(129,110),(160,112),(174,145),(165,157),(150,154)],fill='#90a2af',outline=INK,width=2)
 if s>=5:d.rectangle((62,70,127,103),fill=DARK)
 if s>=6:board(d,(57,78,136,111));battery(d,(79,115,113,137))
 if s>=7:d.rectangle((32,133,51,147),fill=(0,0,0,0));d.line((119,102,136,115),fill=LIGHT,width=2)
 if s==8:d.rectangle((139,116,156,134),fill=(0,0,0,0))
 return (14,59,178,162)

def speaker(d,s):
 rr(d,(49,38,143,187),DARK,21,INK,3);rr(d,(54,43,138,181),'#59717c',18,EDGE,2)
 if s<3:
  for yy in range(56,172,5):
   for xx in range(64,131,5):d.rectangle((xx+(yy%2),yy,xx+1+(yy%2),yy+1),fill='#8fa6a8')
  d.line((85,109,107,109),fill=LIGHT,width=3);d.line((96,98,96,120),fill=LIGHT,width=3)
  if s:d.line((64,147,88,138,111,149,133,136),fill=INK,width=2)
 elif s<6:
  rr(d,(62,52,130,176),'#b1c1c0',13)
  for yy in [78,147]:d.ellipse((74,yy-15,118,yy+15),fill=DARK,outline=INK,width=2)
 else:
  d.ellipse((64,58,128,112),fill=INK,outline=TEAL,width=3);d.ellipse((73,67,119,103),fill=EDGE,outline=INK,width=2)
  battery(d,(69,120,108,164));board(d,(110,121,130,165))
 if s>=4:d.rectangle((47,84,59,105),fill=(0,0,0,0))
 if s>=7:d.rectangle((124,143,145,162),fill=(0,0,0,0));d.line((85,105,90,116),fill=ORANGE,width=2)
 return (47,36,145,190)

def monitor(d,s):
 rr(d,(10,35,182,154),'#a4b2bd',5,INK,3)
 rr(d,(15,40,177,143),INK,3,INK,1)
 if s<3:
  screen(d,(19,44,173,139),True)
  # No phone-like status bar or apps on a desktop display.
  d.rectangle((22,46,170,53),fill='#38556e')
  if s:cracks(d,(20,45,173,138),2 if s==1 else 4)
 else:
  rr(d,(20,46,172,140),'#8e9eaa',2)
  for yy in range(53,136,12):d.line((29,yy,164,yy),fill='#b9c6c5')
 if s>=5:d.rectangle((27,55,166,132),fill=DARK)
 if s>=6:board(d,(28,61,115,127));rr(d,(123,62,162,126),EDGE,2);bolt(d,136,87)
 d.polygon([(83,155),(109,155),(115,183),(132,190),(132,197),(58,197),(58,190),(77,183)],fill='#bdc8c9',outline=INK,width=2)
 d.line((64,192,126,192),fill=LIGHT,width=2)
 if s>=4:d.line((13,62,23,74,11,87),fill=INK,width=2)
 if s>=7:d.rectangle((11,109,25,126),fill=(0,0,0,0));d.line((63,97,103,108),fill=LIGHT,width=2)
 if s==8:d.rectangle((93,162,114,176),fill=(0,0,0,0))
 return (8,33,184,200)

renderers={'phone':lambda d,s:phone(d,s),'tablet':lambda d,s:phone(d,s,True),'headphones':headphones,'console':console,'controller':controller,'speaker':speaker,'monitor':monitor}
def surface_damage(im,bounds,kind,level):
 # Small permanent cracks/chips. No repeated full-device X-shaped overlay.
 if level==0:return im
 x,y,x2,y2=bounds;w=x2-x;h=y2-y
 spots=[(.35,.29),(.73,.62),(.27,.79),(.66,.19),(.52,.47)]
 if kind=='headphones':spots=[(.16,.65),(.83,.75),(.16,.84),(.84,.57),(.35,.13)]
 overlay=Image.new('RGBA',im.size);d=ImageDraw.Draw(overlay)
 for i,(nx,ny) in enumerate(spots[:level]):
  cx=int(x+nx*w);cy=int(y+ny*h);r=6+i
  d.line([(cx-r,cy-3),(cx-2,cy),(cx+1,cy+3),(cx+r,cy+5)],fill=LIGHT,width=1)
  d.line([(cx-2,cy),(cx+1,cy-r),(cx+4,cy-r-2)],fill=LIGHT,width=1)
  if i<level-1:d.line([(cx+1,cy+3),(cx-2,cy+r),(cx-6,cy+r+3)],fill=EDGE,width=1)
  if level>=3 and i<2:
   d.polygon([(cx-2,cy),(cx+2,cy-2),(cx+4,cy+3),(cx,cy+5)],fill=INK)
 overlay.putalpha(ImageChops.multiply(overlay.getchannel('A'),im.getchannel('A')))
 im.alpha_composite(overlay);return im

def reveal_sections(start,under,bounds):
 # Partition only the pixels that really differ, so every release removes an
 # actual local section of glass/casing rather than replacing the entire layer.
 x,y,x2,y2=bounds;w=x2-x;h=y2-y
 coords=[(xx,yy) for yy in range(H) for xx in range(W) if start.getpixel((xx,yy))!=under.getpixel((xx,yy))]
 def distance(a,b):return ((a[0]-b[0])/w)**2+((a[1]-b[1])/h)**2
 # Slice a few irregular, contiguous shards with controlled areas. The first
 # section of each step is deliberately smaller, making boundaries gentle.
 def partition(points,weights,depth=0):
  if len(weights)==1:return [points]
  n=len(weights)//2;fraction=sum(weights[:n])/sum(weights)
  points=sorted(points,key=lambda p:((p[0]-x)/w+.31*(p[1]-y)/h) if depth%2==0 else ((p[1]-y)/h-.27*(p[0]-x)/w))
  cut=max(1,min(len(points)-1,round(len(points)*fraction)))
  return partition(points[:cut],weights[:n],depth+1)+partition(points[cut:],weights[n:],depth+1)
 groups=partition(coords,[1,1.5,1.5,1.5,1.5,1.5,2])
 masks=[Image.new('L',(W,H)) for _ in groups]
 for mask,points in zip(masks,groups):
  for point in points:mask.putpixel(point,255)
 # The small state change survives into every following state. No resets.
 images=[];current=start.copy()
 for mask in masks[:6]:
  current=Image.composite(under,current,mask);images.append(current.copy())
 return images

fragment_dir=OUT/'Fragments';fragment_dir.mkdir(exist_ok=True)
for kind,draw in renderers.items():
 pristine=Image.new('RGBA',(W,H));bounds=draw(ImageDraw.Draw(pristine),0)
 frames=[surface_damage(pristine.copy(),bounds,kind,progress) for progress in range(6)]
 case=Image.new('RGBA',(W,H));draw(ImageDraw.Draw(case),3)
 frames.extend(reveal_sections(frames[-1],case,bounds))
 internals=Image.new('RGBA',(W,H));draw(ImageDraw.Draw(internals),8)
 frames.extend(reveal_sections(frames[-1],internals,bounds))
 im=internals.copy();source=im.copy();d=ImageDraw.Draw(im);x,y,x2,y2=bounds
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
print('Reworked small persistent damage details across the existing three steps; asset counts unchanged.')
# A plain desk: no right-hand tray, bulletin, tools or other decorative side props.
im=Image.new('RGBA',(450,688),'#997a58');d=ImageDraw.Draw(im)
for y in [0,112,232,356,481,603]:d.line((0,y,449,y),fill='#836447',width=2);d.line((0,y+3,449,y+3),fill='#aa8862')
for x,y,l in [(10,49,95),(305,160,118),(28,300,101),(322,449,87),(18,560,77),(281,639,140)]:d.line((x,y,x+l,y),fill='#a5835d')
rr(d,(25,32,424,605),'#455e69',5,'#354752',2)
d.rectangle((30,37,419,600),outline='#70878b',width=1)
for x,y in [(38,45),(411,45),(38,592),(411,592)]:d.rectangle((x-1,y-1,x+1,y+1),fill='#96a7a1')
im.save(OUT.parent/'portrait_desk.png')
# Sealed, simple battery icon replaces every inventory representation as well.
im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im);battery(d,(7,4,25,29));d.rectangle((12,1,20,4),fill=EDGE);im.save(OUT.parent/'part_2.png')
