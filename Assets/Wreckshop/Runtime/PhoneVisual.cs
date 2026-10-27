using UnityEngine;

namespace PocketWreckshop
{
    public sealed class PhoneVisual : MonoBehaviour
    {
        public GameCatalog catalog;
        public SpriteRenderer phone,hammer;
        public Transform effectsRoot;
        Vector3 phoneHome,contact,grip,recoilDirection;
        float swingTime=-10,impactTime=-10,recoilStrength=1;
        bool swinging;
        SpriteRenderer[] trails;
        readonly System.Collections.Generic.List<SpriteRenderer> mountedParts=new System.Collections.Generic.List<SpriteRenderer>();
        sealed class PartMotion
        {
            public Vector3 home,center,direction;
            public float started=-10,frequency,damping,amplitude,rotation,duration;
            public Vector3 fromCenter,targetCenter,scatterCenter;
            public float fromAngle,targetAngle,scatterAngle,movedAt=-10,detachment=-1,scale;
            public float fadeStarted=-10;
            public int layer,order;
        }
        readonly System.Collections.Generic.List<PartMotion> partMotions=new System.Collections.Generic.List<PartMotion>();
        PhoneDefinition mountedDevice;
        int mountedLayer=-1;
        bool previousPending;
        Transform scatteredPartsRoot;
        ScreenCracks screenCracks;
        static readonly Vector3 GripLocal=new Vector3(-.22f,-.22f,0);
        static readonly Vector3 HeadLocal=new Vector3(-.14f,.23f,0);
        void Awake()
        {
            phoneHome=phone.transform.localPosition;trails=new SpriteRenderer[2];
            for(int i=0;i<trails.Length;i++)
            {
                var go=new GameObject("Hammer motion echo "+i);go.transform.SetParent(hammer.transform.parent,false);
                trails[i]=go.AddComponent<SpriteRenderer>();trails[i].sharedMaterial=hammer.sharedMaterial;trails[i].sortingOrder=hammer.sortingOrder-1;trails[i].enabled=false;
            }
        }
        public void Refresh(GameSession g)
        {
            var s=g.State;phone.enabled=g.HasPhone;
            if(!g.HasPhone){RefreshMountedParts(null,null);if(screenCracks)screenCracks.gameObject.SetActive(false);return;}
            var device=catalog.phones[s.activePhone];
            int frame=catalog.DamageState(s);
            phone.sprite=device.stages[frame];var sprite=device.stages[0];
            phone.transform.localScale=Vector3.one*Mathf.Min(5f/(sprite.rect.height/sprite.pixelsPerUnit),3.6f/(sprite.rect.width/sprite.pixelsPerUnit));phone.color=Color.white;
            RefreshMountedParts(device,s);
            bool cracked=device.screenCrackMaterial&&device.screenCrackArea.width>0&&s.phase==0&&!s.lootPending&&frame>0;
            if(cracked&&!screenCracks)
            {
                var go=new GameObject("Persistent screen cracks");go.transform.SetParent(phone.transform,false);
                screenCracks=go.AddComponent<ScreenCracks>();
            }
            if(screenCracks)
            {
                screenCracks.gameObject.SetActive(cracked);
                if(cracked)screenCracks.Refresh(phone,device.screenCrackArea,frame,device.screenCrackMaterial);
            }
        }
        void RefreshMountedParts(PhoneDefinition device,SaveGame state)
        {
            bool sameDevice=device==mountedDevice;
            bool animate=sameDevice&&state!=null&&(state.phase>mountedLayer||state.lootPending&&!previousPending);
            mountedDevice=device;mountedLayer=state?.phase??-1;previousPending=state?.lootPending??false;
            if(device?.layerParts==null||device.layerParts.Length==0||state==null)
            {
                foreach(var part in mountedParts)part.enabled=false;
                if(scatteredPartsRoot)scatteredPartsRoot.gameObject.SetActive(false);
                return;
            }
            if(!scatteredPartsRoot)
            {
                scatteredPartsRoot=new GameObject("Detached device parts").transform;
                scatteredPartsRoot.SetParent(phone.transform.parent,false);
            }
            scatteredPartsRoot.gameObject.SetActive(true);scatteredPartsRoot.localPosition=phoneHome;
            scatteredPartsRoot.localRotation=Quaternion.Euler(0,0,2);scatteredPartsRoot.localScale=phone.transform.localScale;
            if(!sameDevice)
            {
                foreach(var part in mountedParts)part.enabled=false;
                int index=0;var orders=new int[GameCatalog.DestructionSteps];
                foreach(var definition in device.layerParts)
                {
                    if(!definition.sprite)continue;
                    if(index==mountedParts.Count)
                    {
                        var go=new GameObject("Device part "+index);go.transform.SetParent(phone.transform,false);
                        mountedParts.Add(go.AddComponent<SpriteRenderer>());partMotions.Add(new PartMotion());
                    }
                    var part=mountedParts[index];var motion=partMotions[index++];
                    part.sprite=definition.sprite;part.sharedMaterial=phone.sharedMaterial;part.color=Color.white;
                    part.sortingLayerID=phone.sortingLayerID;part.sortingOrder=phone.sortingOrder+2+definition.sortingOrder;
                    motion.layer=definition.layer;motion.order=orders[definition.layer]++;motion.scale=definition.scale;
                    motion.home=new Vector3(definition.localPosition.x,definition.localPosition.y,0);
                    motion.started=-10;motion.detachment=-1;motion.fadeStarted=-10;
                    motion.frequency=53f+motion.order*17f;motion.damping=11f+motion.order*2f;
                    motion.amplitude=.016f/(1f+motion.order*.2f);motion.rotation=motion.order%2==0?1.5f:-1.2f;motion.duration=.38f+motion.order*.025f;
                    var vertices=part.sprite.vertices;var bounds=new Bounds(vertices[0],Vector3.zero);
                    foreach(var vertex in vertices)bounds.Encapsulate(vertex);
                    motion.center=bounds.center*definition.scale;
                    // Four separated tabletop locations, sized for the portrait bench. Keep the backing intact.
                    bool first=motion.order%2==0;
                    motion.scatterCenter=motion.layer==1?(first?new Vector3(.16f,.35f,0):new Vector3(-.14f,-.20f,0)):
                        (first?new Vector3(.20f,-.35f,0):new Vector3(-.23f,.47f,0));
                    motion.scatterAngle=motion.layer==1?(first?-18:15):(first?16:-22);
                }
                for(int i=index;i<mountedParts.Count;i++)mountedParts[i].enabled=false;
            }
            for(int i=0;i<device.layerParts.Length;i++)
            {
                var part=mountedParts[i];var motion=partMotions[i];bool wasVisible=part.enabled;
                part.enabled=motion.layer<=state.phase;
                if(!part.enabled){motion.detachment=-1;motion.started=-10;motion.fadeStarted=-10;continue;}
                // Damage vibrates mounted parts; only clearing their owner layer releases them.
                float progress=motion.layer<state.phase||state.lootPending?1:0;
                if(progress>0)
                {
                    if(motion.detachment>=1)
                    {
                        part.enabled=motion.fadeStarted>=0&&Time.unscaledTime-motion.fadeStarted<.28f;
                        if(!part.enabled)continue;
                    }
                    else if(animate&&wasVisible)motion.fadeStarted=Time.unscaledTime;
                    else
                    {
                        // Cleared layers restored from saves never leave old pieces on the next layer.
                        part.enabled=false;motion.detachment=1;motion.fadeStarted=-10;motion.started=-10;continue;
                    }
                }
                else {part.color=Color.white;motion.fadeStarted=-10;}
                if(Mathf.Approximately(progress,motion.detachment)&&wasVisible)continue;
                var parent=progress>0?scatteredPartsRoot:phone.transform;
                part.transform.SetParent(parent,wasVisible&&animate);part.transform.localScale=Vector3.one*motion.scale;
                motion.fromCenter=part.transform.localPosition+part.transform.localRotation*motion.center;
                motion.fromAngle=Mathf.DeltaAngle(0,part.transform.localEulerAngles.z);
                motion.targetCenter=Vector3.Lerp(motion.home+motion.center,motion.scatterCenter,progress);
                motion.targetAngle=motion.scatterAngle*progress;motion.detachment=progress;
                motion.movedAt=animate&&wasVisible&&progress>0?Time.unscaledTime:-10;
                if(motion.movedAt<0)
                {
                    motion.fromCenter=motion.targetCenter;motion.fromAngle=motion.targetAngle;motion.started=-10;
                    SetPartPose(part,motion,motion.targetCenter,motion.targetAngle,Vector3.zero);
                }
            }
        }
        static void SetPartPose(SpriteRenderer part,PartMotion motion,Vector3 center,float angle,Vector3 displacement)
        {
            var rotation=Quaternion.Euler(0,0,angle);part.transform.localRotation=rotation;
            part.transform.localPosition=center-rotation*motion.center+displacement;
        }
        public void BeginSwing(int tool,Vector3 worldPoint)
        {
            swinging=true;swingTime=Time.unscaledTime;contact=transform.InverseTransformPoint(worldPoint);contact.z=-3;
            hammer.sprite=catalog.hammerSprites[tool];hammer.color=Color.white;
            grip=contact-Quaternion.Euler(0,0,205)*(HeadLocal-GripLocal);
            SetPose(hammer,115);hammer.enabled=true;
            foreach(var trail in trails){trail.sprite=hammer.sprite;trail.enabled=false;}
        }
        void SetPose(SpriteRenderer sprite,float angle)
        {
            var rotation=Quaternion.Euler(0,0,angle);sprite.transform.localRotation=rotation;
            sprite.transform.localPosition=grip-rotation*GripLocal;
        }
        void Spawn(Sprite[] frames,Vector3 at,float scale,float life,Vector3 velocity,float spin,float drag,Color color)
        {
            var go=new GameObject("Break sprite");go.transform.SetParent(effectsRoot,false);go.transform.localPosition=at;go.transform.localScale=Vector3.one*scale;
            var sr=go.AddComponent<SpriteRenderer>();sr.sharedMaterial=phone.sharedMaterial;sr.sortingOrder=8;
            go.AddComponent<BreakEffect>().Play(sr,frames,life,velocity,spin,drag,color);
        }
        public void Impact(HitResult hit,SaveGame state)
        {
            impactTime=Time.unscaledTime;recoilStrength=hit.broken?2.2f:hit.phaseChanged?1.7f:1;
            recoilDirection=phoneHome-contact;recoilDirection.z=0;recoilDirection.Normalize();if(recoilDirection.sqrMagnitude<.01f)recoilDirection=new Vector3(-.6f,-.8f,0);
            int material=hit.phaseChanged?state.phase-1:state.phase;
            if(material==mountedLayer)
            {
                var localContact=phone.transform.InverseTransformPoint(transform.TransformPoint(contact));localContact.z=0;
                for(int i=0;i<mountedParts.Count;i++)
                {
                    if(!mountedParts[i].enabled||partMotions[i].layer!=material||partMotions[i].detachment>=1)continue;
                    var motion=partMotions[i];var away=motion.home+motion.center-localContact;away.z=0;
                    if(away.sqrMagnitude<.0001f)away=new Vector3(i%2==0?-1:1,.6f,0);
                    motion.direction=Quaternion.Euler(0,0,i%2==0?-28:32)*away.normalized;
                    motion.started=Time.unscaledTime;
                }
            }
            // Supplied layered devices show the real components separating, without particle substitutes.
            if(catalog.phones[state.activePhone].layerParts?.Length>0)return;
            var origin=new Vector3(contact.x,contact.y,-2);
            Spawn(catalog.impactSprites,origin,hit.phaseChanged||hit.broken?1.65f:1.1f,.20f,Vector3.zero,0,0,Color.white);
            Spawn(catalog.dustSprites,origin,hit.broken?2:1,.36f,new Vector3(.12f,.1f,0),0,2,new Color(1,1,1,.55f));
            for(int i=0;i<(hit.broken?16:hit.phaseChanged?10:6);i++)
            {
                var v=Random.insideUnitCircle.normalized*Random.Range(1.5f,3.5f);
                Spawn(new[]{catalog.chipSprites[material*4+Random.Range(0,4)]},origin,Random.Range(.35f,.7f),Random.Range(.5f,.85f),v,Random.Range(-230f,230f),5,Color.white);
            }
            if(hit.phaseChanged||hit.broken)
            {
                var device=catalog.phones[state.activePhone];
                for(int i=0;i<(hit.broken?7:4);i++)
                {
                    var v=Random.insideUnitCircle.normalized*Random.Range(2f,4.2f);
                    var at=phoneHome+new Vector3(Random.Range(-.6f,.6f),Random.Range(-.9f,.9f),-2);
                    Spawn(new[]{device.fragments[material*4+i%4]},at,phone.transform.localScale.x*Random.Range(.65f,.95f),1.1f,v,Random.Range(-180f,180f),4,Color.white);
                }
            }
        }
        void Update()
        {
            float age=Time.unscaledTime-impactTime;
            float recoil=age>=0&&age<.24f?Mathf.Sin(age/.24f*Mathf.PI*2)*Mathf.Exp(-age*16)*recoilStrength:0;
            phone.transform.localPosition=phoneHome+recoilDirection*(.035f*recoil);
            phone.transform.localRotation=Quaternion.Euler(0,0,2+.8f*recoil);
            for(int i=0;i<mountedParts.Count;i++)
            {
                var part=mountedParts[i];if(!part.enabled)continue;
                var motion=partMotions[i];float partAge=Time.unscaledTime-motion.started;
                if(motion.fadeStarted>=0)
                {
                    float fade=Mathf.Clamp01((Time.unscaledTime-motion.fadeStarted)/.28f);
                    part.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,fade));
                    if(fade>=1){part.enabled=false;continue;}
                }
                float shake=partAge>=0&&partAge<motion.duration?Mathf.Sin(partAge*motion.frequency)*Mathf.Exp(-partAge*motion.damping):0;
                float slide=Mathf.Clamp01((Time.unscaledTime-motion.movedAt)/(.36f+motion.order*.06f));
                float eased=1-Mathf.Pow(1-slide,3);
                var center=Vector3.Lerp(motion.fromCenter,motion.targetCenter,eased);
                float partAngle=Mathf.LerpAngle(motion.fromAngle,motion.targetAngle,eased);
                SetPartPose(part,motion,center,partAngle+motion.rotation*shake,motion.direction*(motion.amplitude*shake));
            }
            if(!swinging)return;
            float t=Time.unscaledTime-swingTime;float angle;
            if(t<.09f){float p=Mathf.Clamp01(t/.09f);angle=Mathf.Lerp(115,205,p*p);}
            else if(t<.125f)angle=205;
            else{float p=Mathf.Clamp01((t-.125f)/.135f);angle=Mathf.Lerp(205,140,1-(1-p)*(1-p));}
            SetPose(hammer,angle);
            for(int i=0;i<trails.Length;i++)
            {
                trails[i].enabled=t>.025f&&t<.11f;
                if(trails[i].enabled){SetPose(trails[i],angle-(i+1)*15);trails[i].color=new Color(.75f,.86f,.83f,.19f-i*.055f);}
            }
            if(t>.26f)CancelSwing();
        }
        public void CancelSwing(){swinging=false;if(hammer)hammer.enabled=false;if(trails!=null)foreach(var trail in trails)trail.enabled=false;}
        public void ClearEffects(){for(int i=effectsRoot.childCount-1;i>=0;i--)Destroy(effectsRoot.GetChild(i).gameObject);}
    }
}
