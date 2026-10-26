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
            var s=g.State;phone.enabled=g.HasPhone;if(!g.HasPhone)return;
            var device=catalog.phones[s.activePhone];
            int frame=catalog.DamageState(s);
            phone.sprite=device.stages[frame];var sprite=device.stages[0];
            phone.transform.localScale=Vector3.one*Mathf.Min(5f/(sprite.rect.height/sprite.pixelsPerUnit),3.6f/(sprite.rect.width/sprite.pixelsPerUnit));phone.color=Color.white;
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
