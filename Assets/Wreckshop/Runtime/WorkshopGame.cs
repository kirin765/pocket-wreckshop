using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PocketWreckshop
{
    public enum WorkshopTab { Smash, Devices, Tools }
    public sealed class WorkshopGame : MonoBehaviour
    {
        public GameCatalog catalog;
        public WorkshopUI ui;
        public PhoneVisual phoneVisual;
        public GameSession Session { get; private set; }
        public WorkshopTab Tab { get; private set; }
        public bool HelpOpen { get; private set; }
        public bool MenuOpen => ui.MenuVisible;
        AudioSource audioSource;
        AudioClip impactClip, breakClip, coinClip;
        Coroutine attack;
        bool queuedStrike;
        Vector3 queuedPoint;
        bool dirty;
        public bool AutoCanRun => false;
        void Awake()
        {
            Application.targetFrameRate = 60;
            Session = new GameSession(catalog, SaveStore.Load(catalog));
            Session.Changed += OnChanged;
            audioSource = gameObject.AddComponent<AudioSource>();
            impactClip = Tone(130,38,.1f,.24f);
            breakClip = Tone(85,18,.22f,.20f);
            coinClip = Tone(640,980,.14f,.1f);
            ui.Bind(this);
            ShowTab(WorkshopTab.Smash);
        }
#if UNITY_EDITOR
        public void Preview()
        {
            Session = new GameSession(catalog); Tab = WorkshopTab.Smash;
            ui.Bind(this); ui.Render(this);
        }
#endif
        void OnChanged() { dirty = true; ui.RefreshHud(Session); }
        void LateUpdate()
        {
            // Coalesce changes within a frame, but persist every completed action.
            if (dirty) { SaveStore.Save(Session.State); dirty = false; }
        }
        void OnApplicationPause(bool paused){CancelAttack();if(paused&&Session!=null)SaveStore.Save(Session.State);}
        void OnApplicationFocus(bool focused){if(!focused)CancelAttack();}
        void OnApplicationQuit() { if (Session != null) SaveStore.Save(Session.State); }
        void OnDestroy()
        {
            if (Session != null) Session.Changed -= OnChanged;
            if (impactClip) Destroy(impactClip);
            if (breakClip) Destroy(breakClip);
            if (coinClip) Destroy(coinClip);
        }
        void Update()
        {
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame){if(HelpOpen)SetHelp(false);else SetMenu(!ui.MenuRequested);}
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame &&
                Tab == WorkshopTab.Smash && !HelpOpen && !MenuOpen) Strike();

        }
        public void ShowTab(WorkshopTab next)
        {
            CancelAttack();
            ui.SetMenu(false);
            Tab = next;
            phoneVisual.gameObject.SetActive(next == WorkshopTab.Smash);
            if (next == WorkshopTab.Smash) phoneVisual.Refresh(Session);
            ui.Render(this);
        }
        public void Strike()=>RequestStrike(phoneVisual.phone.transform.position);
        public void StrikeAtPointer()
        {
            var point=phoneVisual.phone.transform.position;
            if(Pointer.current!=null&&Camera.main)
            {
                var screen=Pointer.current.position.ReadValue();var world=Camera.main.ScreenToWorldPoint(new Vector3(screen.x,screen.y,10));
                var bounds=phoneVisual.phone.bounds;
                point=new Vector3(Mathf.Clamp(world.x,bounds.min.x+.15f,bounds.max.x-.15f),Mathf.Clamp(world.y,bounds.min.y+.15f,bounds.max.y-.15f),0);
            }
            RequestStrike(point);
        }
        void RequestStrike(Vector3 point)
        {
            if(HelpOpen||MenuOpen||Tab!=WorkshopTab.Smash||!Session.HasPhone||Session.State.lootPending)return;
            if(attack!=null){queuedStrike=true;queuedPoint=point;return;}
            attack=StartCoroutine(Attack(point));
        }
        IEnumerator Attack(Vector3 point)
        {
            do
            {
                queuedStrike=false;
                phoneVisual.BeginSwing(Session.State.hammer,point);
                yield return new WaitForSecondsRealtime(.09f);
                if(Tab!=WorkshopTab.Smash||HelpOpen||MenuOpen||!Session.HasPhone||Session.State.lootPending)break;
                var hit=Session.Smash();
                if(!hit.success)break;
                phoneVisual.Refresh(Session);phoneVisual.Impact(hit,Session.State);
                Play(hit.broken?breakClip:impactClip);
                if(hit.broken)ui.Render(this);
                yield return new WaitForSecondsRealtime(.17f);
                point=queuedPoint;
            }while(queuedStrike&&!Session.State.lootPending&&Tab==WorkshopTab.Smash&&!HelpOpen&&!MenuOpen);
            phoneVisual.CancelSwing();attack=null;queuedStrike=false;
        }
        void CancelAttack()
        {
            if(attack!=null)StopCoroutine(attack);
            attack=null;queuedStrike=false;if(phoneVisual)phoneVisual.CancelSwing();
        }
        public void MainAction()
        {
            if (Session.State.lootPending)
            {
                Session.Collect(); phoneVisual.ClearEffects(); Play(coinClip);
                ShowTab(WorkshopTab.Smash);
            }
            else if (Session.HasPhone) Strike();
            else ShowTab(WorkshopTab.Devices);
        }
        public void BuyPhone(int index)
        {
            if (Session.BuyPhone(index)) { Play(coinClip); ShowTab(WorkshopTab.Smash); }
        }
        public void SetMenu(bool open){if(open)CancelAttack();ui.SetMenu(open);}
        public void ToggleSound() { Session.ToggleSound(); ui.RefreshHud(Session); Play(coinClip); }
        public void SetHelp(bool open) { if(open){CancelAttack();ui.SetMenu(false);}HelpOpen=open;ui.helpPanel.SetActive(open); }
        void Play(AudioClip clip) { if (Session.State.sound && clip) audioSource.PlayOneShot(clip); }
        static AudioClip Tone(float start, float end, float duration, float volume)
        {
            const int rate=22050;
            var clip=AudioClip.Create("Workshop synth",(int)(rate*duration),1,rate,false);
            float phase=0;var samples=new float[clip.samples];
            for(int i=0;i<samples.Length;i++) { float t=(float)i/samples.Length; phase+=Mathf.Lerp(start,end,t)*2*Mathf.PI/rate; samples[i]=Mathf.Sin(phase)*(1-t)*(1-t)*volume; }
            clip.SetData(samples,0);return clip;
        }
    }
}
