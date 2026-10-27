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
            navigation=new Button[4];string[] names={"Workbench","Used devices","Hammers","Sell motherboards"};
            for(int i=0;i<4;i++){navigation[i]=Button(drawer,"Menu destination "+i,names[i],12,92+i*60,248,48,Ink,White,14,null);navigation[i].GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;}
            menuLayer.SetActive(false);
            helpPanel=Box(root,"Instructions",0,0,450,800,new Color(.13f,.17f,.20f,.96f)).gameObject;helpPanel.GetComponent<Image>().raycastTarget=true;
            var modal=Box(helpPanel.transform,"Instruction card",26,180,398,410,Panel).rectTransform;
            Label(modal,"Title","How to play",22,18,354,37,17,White,true);
            Label(modal,"Instructions","Buy a used device for dollars.\n\nTap it to break it in three steps. Space works too.\n\nCollect the motherboard, then sell it for dollars. Buy devices, hammers and Auto Hammer from the side menu.",22,72,354,205,14,White);
            Label(modal,"Free device","Out of money? Pick the $0 scrap phone.\nYour progress saves on this device.",22,284,354,46,11,Muted);
            Button(modal,"Close","Got it",22,350,354,42,Mint,Ink,13,null,true);helpPanel.SetActive(false);
        }
        public void Bind(WorkshopGame game)
        {
            for(int i=0;i<navigation.Length;i++){var tab=(WorkshopTab)i;navigation[i].onClick.RemoveAllListeners();navigation[i].onClick.AddListener(()=>game.ShowTab(tab));}
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
            switch(game.Tab){case WorkshopTab.Smash:Smash(game);break;case WorkshopTab.Devices:Devices(game);break;case WorkshopTab.Tools:Tools(game);break;case WorkshopTab.Sell:Sell(game);break;}
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
            if(s.lootPending||!g.HasPhone)Button(body,"Main action",s.lootPending?"Collect motherboard x"+s.boardDrops:"Buy a used device",18,692,414,40,Mint,Ink,13,game.MainAction,true);
        }
        public void RefreshIntegrity(GameSession g) {}
        void Tools(WorkshopGame game)
        {
            var s=game.Session.State;Title("Hammers","Choose your tool.");
            for(int i=0;i<catalog.hammers.Length;i++)
            {
                int index=i;var h=catalog.hammers[i];var card=Box(body,"Hammer "+i,22,90+i*114,406,102,Panel).rectTransform;
                Icon(card,catalog.hammerSprites[i],12,18,58,58);Label(card,"Name",h.title,83,14,220,30,14,White);
                Label(card,"Damage",h.damage+" damage per hit",83,48,220,29,11,Muted);
                bool owned=s.ownedHammers[i];var buy=Button(card,"Hammer buy "+i,s.hammer==i?"Using":owned?"Equip":"$"+h.price,310,34,84,36,owned?Ink:Mint,owned?White:Ink,12,()=>game.EquipHammer(index));
                buy.interactable=s.hammer!=i&&(owned||s.money>=h.price);
            }
            var auto=Box(body,"Auto hammer",22,552,406,160,Panel).rectTransform;
            Label(auto,"Title","Auto hammer",16,8,374,30,15,White);
            string speed=s.autoLevel==0?"Hits the current device automatically.":"One hit every "+game.Session.AutoInterval.ToString("0.##")+" seconds.";
            Label(auto,"Details",speed+"\nPauses while menus are open.",16,42,374,42,12,Muted);
            bool max=s.autoLevel>=catalog.autoPrices.Length;
            string buyText=max?"Fully upgraded":(s.autoLevel==0?"Unlock $":"Faster $")+catalog.autoPrices[s.autoLevel];
            Button(auto,"Buy automation",buyText,16,105,200,40,Mint,Ink,13,game.BuyAutomation).interactable=!max&&s.money>=catalog.autoPrices[s.autoLevel];
            Button(auto,"Toggle automation",s.autoEnabled?"Auto: On":"Auto: Off",228,105,162,40,s.autoEnabled?Mint:Ink,s.autoEnabled?Ink:White,13,game.ToggleAutomation).interactable=s.autoLevel>0;
        }
        void Devices(WorkshopGame game)
        {
            var s=game.Session.State;Title("Used devices",game.Session.HasPhone?"Collect the current device before buying another.":"Buy it. Break it. Sell the motherboard.");
            var devices=new System.Collections.Generic.List<int>{0,4,5,6,7,8,9,3};
            for(int i=10;i<catalog.phones.Length;i++)devices.Add(i);
            var viewport=Rect(body,"Device list",0,82,450,642);
            var scrollSurface=viewport.gameObject.AddComponent<Image>();scrollSurface.color=Color.clear;
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();
            var content=Rect(viewport,"Device cards",0,0,450,Mathf.Max(642,Mathf.Ceil(devices.Count/2f)*146));
            scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;
            scroll.movementType=ScrollRect.MovementType.Clamped;
            for(int slot=0;slot<devices.Count;slot++)
            {
                int index=devices[slot];var p=catalog.phones[index];float x=22+(slot%2)*208,y=8+(slot/2)*146;
                var card=Box(content,"Device "+index,x,y,198,134,Panel).rectTransform;
                Label(card,"Name",p.title,10,4,178,24,12,White);Icon(card,p.stages[0],10,33,67,88);
                int count=catalog.Salvage(index)[0];
                Label(card,"Details","Boards x"+count+"\nSell for $"+(count*catalog.partPrices[0]),87,34,103,38,10,Muted);
                var buy=Button(card,"Device buy "+index,"Buy $"+p.price,87,89,100,34,Mint,Ink,12,()=>game.BuyPhone(index));buy.interactable=!game.Session.HasPhone&&s.money>=p.price;
            }
        }
        void Sell(WorkshopGame game)
        {
            var s=game.Session.State;int price=catalog.partPrices[0];Title("Sell motherboards","$"+price+" per motherboard");
            var card=Box(body,"Motherboard inventory",22,90,406,210,Panel).rectTransform;
            Icon(card,catalog.partSprites[0],27,23,79,89);
            Label(card,"Name","Motherboards",128,25,256,30,16,White);
            Label(card,"Count","In storage: "+s.motherboards,128,62,256,30,13,Muted);
            Label(card,"Total value","Total value: $"+(s.motherboards*price),128,99,256,30,13,Mint);
            Button(card,"Sell one","Sell one $"+price,16,154,178,40,Mint,Ink,13,()=>game.SellMotherboards(false)).interactable=s.motherboards>0;
            Button(card,"Sell all","Sell all $"+(s.motherboards*price),208,154,182,40,Mint,Ink,13,()=>game.SellMotherboards(true)).interactable=s.motherboards>0;
            Button(body,"Buy devices","Buy used devices",22,326,406,44,Panel,White,14,()=>game.ShowTab(WorkshopTab.Devices));
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
