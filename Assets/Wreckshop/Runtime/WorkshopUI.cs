using System;
using UnityEngine;
using UnityEngine.UI;

namespace PocketWreckshop
{
    public sealed class WorkshopUI : MonoBehaviour
    {
        public GameCatalog catalog;
        public RectTransform body,drawer;
        public Text walletLabel,soundLabel,toastLabel;
        public Button soundButton,helpButton,menuButton,menuClose,menuShade;
        public Button[] navigation;
        public GameObject helpPanel,toastPanel,menuLayer;
        float toastUntil;
        public bool MenuRequested {get;private set;}
        public bool MenuVisible => menuLayer&&menuLayer.activeSelf;
        static readonly Color Ink=Hex("303943"),Panel=Hex("3d4a55"),Muted=Hex("b3bfc4"),Yellow=Hex("e7c58f"),Mint=Hex("a0c7bd"),White=Hex("f1eadc");
        public static Color Hex(string v){ColorUtility.TryParseHtmlString("#"+v,out var c);return c;}
        public void CreateShell(GameCatalog data)
        {
            catalog=data;var root=(RectTransform)transform;
            Box(root,"Top bar",0,0,450,64,Ink);
            menuButton=Button(root,"Open menu","",12,10,48,44,Ink,White,16,null);
            for(int i=0;i<3;i++)Box(menuButton.transform,"Menu line "+i,13,13+i*8,22,2,White);
            walletLabel=Label(root,"Wallet","$100",78,12,225,37,20,White,true);
            soundButton=Button(root,"Sound","Off",324,10,48,44,Ink,Muted,11,null);soundLabel=soundButton.GetComponentInChildren<Text>();
            helpButton=Button(root,"Help","?",386,10,46,44,Ink,White,16,null);
            body=Rect(root,"Page",0,64,450,736);
            toastPanel=Box(root,"Toast",30,78,390,30,Panel).gameObject;
            toastLabel=Label(toastPanel.transform,"Message","",10,0,370,30,11,White);toastPanel.SetActive(false);
            menuLayer=Rect(root,"Side menu",0,0,450,800).gameObject;
            menuShade=Button(menuLayer.transform,"Close menu outside","",0,0,450,800,new Color(0,0,0,.48f),White,12,null);
            drawer=Box(menuLayer.transform,"Drawer",-272,0,272,800,Ink).rectTransform;drawer.GetComponent<Image>().raycastTarget=true;
            Label(drawer,"Menu title","Menu",22,12,176,44,18,White,true);
            menuClose=Button(drawer,"Close menu","X",210,10,48,44,Ink,White,17,null);
            navigation=new Button[3];string[] names={"Workbench","Used devices","Hammer"};
            for(int i=0;i<3;i++){navigation[i]=Button(drawer,"Menu destination "+i,names[i],12,92+i*60,248,48,Ink,White,14,null);navigation[i].GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;}
            menuLayer.SetActive(false);
            helpPanel=Box(root,"Instructions",0,0,450,800,new Color(.13f,.17f,.20f,.96f)).gameObject;helpPanel.GetComponent<Image>().raycastTarget=true;
            var modal=Box(helpPanel.transform,"Instruction card",26,180,398,410,Panel).rectTransform;
            Label(modal,"Title","How to play",22,18,354,37,17,White,true);
            Label(modal,"Instructions","Buy a used device for dollars.\n\nTap it to break it in three steps. Space works too.\n\nFinish all three steps to reveal your dollar payout. The Classic phone always pays $1. The tablet has a 70% chance of paying $800, otherwise $0.",22,72,354,205,14,White);
            Label(modal,"Free device","Out of money? Pick the free Classic phone.\nYour progress saves on this device.",22,284,354,46,11,Muted);
            Button(modal,"Close","Got it",22,350,354,42,Mint,Ink,13,null,true);helpPanel.SetActive(false);
        }
        public void Bind(WorkshopGame game)
        {
            for(int i=0;i<Math.Min(3,navigation.Length);i++){var tab=(WorkshopTab)i;navigation[i].onClick.RemoveAllListeners();navigation[i].onClick.AddListener(()=>game.ShowTab(tab));}
            // Older scenes still contain the retired sales destination and instruction text.
            for(int i=0;i<navigation.Length;i++)navigation[i].gameObject.SetActive(i<3);
            foreach(var text in helpPanel.GetComponentsInChildren<Text>(true)){
                if(text.name=="Instructions")text.text="Buy a device for dollars.\n\nTap to break three layers. Space works too.\n\nFinish to reveal your payout: Classic phone always pays $1. Classic tablet pays $800 with a 70% chance, or $0 with a 30% chance.";
                if(text.name=="Free device")text.text="The Classic phone is free.\nYour progress saves on this device.";
            }
            BindButton(menuButton,()=>game.SetMenu(!MenuRequested));BindButton(menuClose,()=>game.SetMenu(false));BindButton(menuShade,()=>game.SetMenu(false));
            BindButton(soundButton,game.ToggleSound);BindButton(helpButton,()=>game.SetHelp(true));
            BindButton(helpPanel.GetComponentInChildren<Button>(true),()=>game.SetHelp(false));RefreshHud(game.Session);
        }
        static void BindButton(Button b,Action action){b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>action());}
        public void SetMenu(bool open)
        {
            MenuRequested=open;
            if(open){toastPanel.SetActive(false);menuLayer.SetActive(true);menuLayer.transform.SetAsLastSibling();helpPanel.transform.SetAsLastSibling();}
            else if(!Application.isPlaying){drawer.anchoredPosition=new Vector2(-272,0);menuLayer.SetActive(false);}
        }
        public void RefreshHud(GameSession game){walletLabel.text="$"+game.State.money.ToString("N0");soundLabel.text=game.State.sound?"Sound":"Off";}
        public void Render(WorkshopGame game)
        {
            Clear(body);RefreshHud(game.Session);
            for(int i=0;i<navigation.Length;i++){navigation[i].GetComponentInChildren<Text>().color=(int)game.Tab==i?Yellow:White;navigation[i].targetGraphic.color=(int)game.Tab==i?Panel:Ink;}
            switch(game.Tab){case WorkshopTab.Smash:Smash(game);break;case WorkshopTab.Devices:Devices(game);break;case WorkshopTab.Tools:Tools(game);break;}
        }
        void Title(string title,string subtitle)
        {
            Label(body,"Page title",title,22,10,406,32,17,White,true);
            Label(body,"Page description",subtitle,22,45,406,29,12,Muted);
        }
        void Smash(WorkshopGame game)
        {
            var g=game.Session;var s=g.State;
            var hit=Button(body,"Tap device","",0,0,450,736,new Color(0,0,0,0),White,10,()=>{if(g.HasPhone)game.StrikeAtPointer();else game.MainAction();});hit.transition=Selectable.Transition.None;
            Box(body,"Tool bar",12,648,426,43,Ink);Icon(body,catalog.hammerSprites[s.hammer],21,655,28,28);
            Label(body,"Equipped tool",catalog.hammers[s.hammer].title,58,654,260,30,13,White);
            Button(body,"Change tool","Tools",334,650,98,40,Ink,Yellow,12,()=>game.ShowTab(WorkshopTab.Tools));
            if(s.lootPending){
                var result=Box(body,"Lottery result",45,465,360,126,Ink).rectTransform;
                Label(result,"Result",s.lotteryWon?"Success":"No win",16,8,328,26,15,s.lotteryWon?Mint:Muted,true,TextAnchor.MiddleCenter);
                Label(result,"Payout","You earned $"+s.dropCash.ToString("N0"),16,42,328,48,24,White,true,TextAnchor.MiddleCenter);
            }
            if(s.lootPending||!g.HasPhone)Button(body,"Main action",s.lootPending?(s.dropCash>0?"Collect $"+s.dropCash.ToString("N0"):"Continue"):"Buy a used device",18,692,414,40,Mint,Ink,13,game.MainAction,true);
        }
        public void RefreshIntegrity(GameSession g) {}
        void Tools(WorkshopGame game)
        {
            var s=game.Session.State;Title("Hammer","Power 1 per hit.");
            for(int i=0;i<catalog.hammers.Length;i++)
            {
                int index=i;var h=catalog.hammers[i];var card=Box(body,"Hammer "+i,22,90+i*114,406,102,Panel).rectTransform;
                Icon(card,catalog.hammerSprites[i],12,18,58,58);Label(card,"Name",h.title,83,14,220,30,14,White);
                Label(card,"Damage",h.damage+" damage per hit",83,48,220,29,11,Muted);
                Label(card,"Equipped","Using",310,34,84,36,12,Mint);
            }

        }
        void Devices(WorkshopGame game)
        {
            var s=game.Session.State;Title("Used devices",game.Session.HasPhone?"Collect the current device before buying another.":"Break all three layers to reveal your payout.");
            var devices=new System.Collections.Generic.List<int>{GameCatalog.ClassicPhoneId,GameCatalog.ClassicTabletId};
            var viewport=Rect(body,"Device list",0,82,450,642);
            var scrollSurface=viewport.gameObject.AddComponent<Image>();scrollSurface.color=Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();
            var content=Rect(viewport,"Device cards",0,0,450,Mathf.Max(642,Mathf.Ceil(devices.Count/2f)*146));
            scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;
            scroll.movementType=ScrollRect.MovementType.Clamped;
            for(int slot=0;slot<devices.Count;slot++)
            {
                int index=devices[slot];var p=catalog.phones[index];float x=22+(slot%2)*208,y=8+(slot/2)*222;
                var card=Box(content,"Device "+index,x,y,198,210,Panel).rectTransform;
                Label(card,"Name",p.title,10,4,178,24,12,White);Icon(card,p.stages[0],10,33,67,88);
                Label(card,"Details","Win "+(p.winChance*100).ToString("0")+"%: $"+p.winDollars+"\nLose "+((1-p.winChance)*100).ToString("0")+"%: $0",87,36,103,62,11,Muted);
                Label(card,"Layer HP",p.integrity.ToString("N0")+" HP per layer",10,128,178,26,11,Muted);
                var buy=Button(card,"Device buy "+index,(p.price==0?"Free":"Buy $"+p.price),10,164,178,34,Mint,Ink,12,()=>game.BuyPhone(index));buy.interactable=!game.Session.HasPhone&&s.money>=p.price;
            }
        }
        public void ShowToast(string text){toastLabel.text=text;toastUntil=Time.unscaledTime+1.2f;toastPanel.SetActive(true);toastPanel.transform.SetAsLastSibling();menuLayer.transform.SetAsLastSibling();helpPanel.transform.SetAsLastSibling();}
        void Update()
        {
            if(toastPanel&&toastPanel.activeSelf&&Time.unscaledTime>toastUntil)toastPanel.SetActive(false);
            if(MenuVisible)
            {
                var pos=drawer.anchoredPosition;pos.x=Mathf.MoveTowards(pos.x,MenuRequested?0:-272,Time.unscaledDeltaTime*1900);drawer.anchoredPosition=pos;
                if(!MenuRequested&&pos.x<=-272)menuLayer.SetActive(false);
            }
        }
        public RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var go=new GameObject(name,typeof(RectTransform));var rt=(RectTransform)go.transform;rt.SetParent(parent,false);rt.anchorMin=rt.anchorMax=new Vector2(0,1);rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;
        }
        public Image Box(Transform p,string name,float x,float y,float w,float h,Color color){var rt=Rect(p,name,x,y,w,h);var image=rt.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        public Text Label(Transform p,string name,string value,float x,float y,float w,float h,int size,Color color,bool heading=false,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var rt=Rect(p,name,x,y,w,h);var t=rt.gameObject.AddComponent<Text>();t.font=heading?catalog.headingFont:catalog.bodyFont;t.text=value;t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        public Image Icon(Transform p,Sprite sprite,float x,float y,float w,float h,Color? color=null){var image=Box(p,"Sprite",x,y,w,h,color??Color.white);image.sprite=sprite;image.preserveAspect=true;return image;}
        public Button Button(Transform p,string name,string value,float x,float y,float w,float h,Color bg,Color fg,int size,Action action,bool heading=false)
        {
            var image=Box(p,name,x,y,w,h,bg);image.raycastTarget=true;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;
            var c=b.colors;c.highlightedColor=new Color(1.05f,1.05f,1.05f);c.pressedColor=new Color(.8f,.85f,.85f);c.disabledColor=new Color(.5f,.5f,.5f,.6f);b.colors=c;
            Label(image.transform,"Label",value,7,0,w-14,h,size,fg,heading,TextAnchor.MiddleCenter);if(action!=null)b.onClick.AddListener(()=>action());return b;
        }
        static void Clear(Transform parent){for(int i=parent.childCount-1;i>=0;i--){var child=parent.GetChild(i).gameObject;child.SetActive(false);if(Application.isPlaying)Destroy(child);else DestroyImmediate(child);}}
    }
}
