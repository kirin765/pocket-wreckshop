using System;
using UnityEditor;
using UnityEngine;

namespace PocketWreckshop.Editor
{
    public static class WorkshopValidation
    {
        static int checks;
        static void Check(bool ok,string reason){checks++;if(!ok)throw new InvalidOperationException("Validation failed: "+reason);}
        static void Roundtrip(GameSession g)
        {
            var state=SaveStore.Decode(JsonUtility.ToJson(g.State),g.Catalog);
            Check(state!=null,"Save restores");Check(JsonUtility.ToJson(state)==JsonUtility.ToJson(g.State),"Save preserves state");
        }
        static void Wreck(GameSession g){int n=0;while(!g.State.lootPending){Check(g.Smash().success,"Hit accepted");if(++n>4000)throw new Exception("Unbreakable device");}}
        static string Legacy(SaveGame s,string extra)=>JsonUtility.ToJson(s).TrimEnd('}')+","+extra+"}";
        [MenuItem("Pocket Wreckshop/Validate gameplay & saves")]
        public static string Run()
        {
            checks=0;var c=AssetDatabase.LoadAssetAtPath<GameCatalog>(WorkshopBuilder.CatalogPath);
            Check(c&&c.phones.Length>=12&&c.hammers.Length==1&&c.hammerSprites.Length==1,"Stable device IDs and one hammer");
            string[] legacyTitles={"Phone","Phone Pro","Premium phone","Scrap phone","Tablet","Headphones","Game console","Controller","Speaker","Monitor"};
            for(int i=0;i<legacyTitles.Length;i++)Check(c.phones[i].title==legacyTitles[i],"Existing device IDs stay stable");
            Check(c.phones[10].title=="Classic phone"&&c.phones[10].kind==DeviceKind.Phone&&c.phones[10].stages[0]!=c.phones[0].stages[0],"Classic phone appends distinct art within Phone family");
            Check(c.phones[10].screenCrackMaterial&&c.phones[10].screenCrackMaterial.shader.isSupported&&
                c.phones[10].screenCrackArea.width>0,"Supplied front layer has a supported persistent glass-crack overlay");
            for(int i=0;i<GameCatalog.DamageFrameCount;i++)
                Check(AssetDatabase.GetAssetPath(c.phones[10].stages[i])==(i<12?ClassicPhoneAssets.Root+"phone"+(i/6)+".png":ClassicPhoneAssets.AsepritePath),"Supplied source is used directly as the phase layer");
            Check(c.phones[10].layerParts.Length==4,"Two mounted parts on each exposed layer");
            foreach(var file in new[]{"phone1-parts0","phone1-parts1"})
                Check(Array.Exists(c.phones[10].layerParts,p=>p.layer==(file.StartsWith("phone1")?1:2)&&
                    p.scale>0&&AssetDatabase.GetAssetPath(p.sprite)==ClassicPhoneAssets.Root+file+".png"),"Supplied part belongs to its named layer: "+file);
            foreach(var name in new[]{"Layer 1","Layer 2"})
                Check(Array.Exists(c.phones[10].layerParts,p=>p.layer==2&&p.sprite.name==name&&p.scale==1&&p.localPosition==Vector2.zero&&
                    AssetDatabase.GetAssetPath(p.sprite)==ClassicPhoneAssets.AsepritePath),"Native Aseprite part keeps authored canvas alignment: "+name);
            for(int i=8;i<12;i++)Check(AssetDatabase.GetAssetPath(c.phones[10].fragments[i])==ClassicPhoneAssets.Root+"phone2.png","Flying module chips are separate from mounted parts");
            var tablet=c.phones[11];
            Check(tablet.title=="Classic tablet"&&tablet.kind==DeviceKind.Tablet,"Layered tablet appends at ID 11");
            for(int i=0;i<GameCatalog.DamageFrameCount;i++)
                Check(AssetDatabase.GetAssetPath(tablet.stages[i])==LayeredTabletAssets.Source&&
                    tablet.stages[i].name==(i<6?"Layer 6":"Layer 1"),"Tablet front and backing use authored native layers");
            Check(tablet.layerParts.Length==4,"Tablet has four independent components");
            for(int n=2;n<=5;n++)Check(Array.Exists(tablet.layerParts,p=>p.layer==1&&p.sprite.name=="Layer "+n&&
                p.scale==1&&p.localPosition==Vector2.zero&&AssetDatabase.GetAssetPath(p.sprite)==LayeredTabletAssets.Source),
                "Tablet part keeps its source canvas registration");
            foreach(var device in c.phones)
            {
                Check(device.stages.Length==19&&device.phaseNames.Length==3,"Nineteen slots and three steps");
                Check(device.fragments.Length==12&&Array.TrueForAll(device.fragments,f=>f),"Native source fragment references remain valid");
                foreach(var sprite in device.stages)Check(sprite,"Every persistent slot has a sprite");
            }
            Check(c.hammers[0].damage==1&&c.hammers[0].price==0,"Only hammer has power 1");
            Check(c.phones[10].price==0&&c.phones[10].winChance==1&&c.phones[10].winDollars==1,"Free phone always pays one dollar");
            Check(c.phones[11].price==499&&c.phones[11].winChance==.7f&&c.phones[11].winDollars==800,"Tablet lottery terms");
            for(int id=0;id<c.phones.Length;id++)Check(c.IsAvailableDevice(id)==(id==10||id==11),"Only Classic devices are available");
            Check(new GameSession(c).State.activePhone==10,"Starter is Classic phone");
            foreach(int id in new[]{10,11})foreach(float roll in new[]{0f,.699999f,.7f,.999999f,1f})
            {
                int draws=0;var game=new GameSession(c,null,()=>{draws++;return roll;});var state=game.State;
                state.activePhone=-1;state.hp=0;state.money=1000;
                Check(game.BuyPhone(id)&&state.money==1000-c.phones[id].price,"Buy charges only purchase price");
                Check(!game.BuyPhone(id),"Cannot buy while occupied");
                int paid=state.money;int health=id==10?100:1000;
                for(int phase=0;phase<3;phase++)
                {
                    Check(c.PhaseIntegrity(id,phase)==health&&state.hp==health,"Every layer has exact requested HP");
                    for(int hit=0;hit<health;hit++)
                    {
                        var hitResult=game.Smash();
                        Check(hitResult.success&&hitResult.damage==1,"Each hammer hit deals exactly one damage");
                        Check(state.money==paid,"Breaking never credits before collection");
                        if(hit<health-1)Check(state.phase==phase&&state.hp==health-hit-1&&!state.lootPending,"No early layer clear");
                    }
                    if(phase<2)Check(state.phase==phase+1&&!state.lootPending&&!game.Collect()&&draws==0,"No early reward or lottery roll");
                    Roundtrip(game);
                }
                int reward=id==10?1:roll<.7f?800:0;
                Check(state.lootPending&&state.hp==0&&state.phase==2&&state.dropCash==reward&&state.lotteryWon==(reward>0),"Final hit fixes lottery outcome");
                Check(draws==(id==10?0:1),"Only one random draw per tablet, no critical hits");
                Check(state.totalHits==health*3&&!game.Smash().success,"Exactly three full layers then stop");
                var restored=new GameSession(c,SaveStore.Decode(JsonUtility.ToJson(state),c),()=>throw new Exception("Lottery rerolled"));
                Check(restored.State.dropCash==reward&&!restored.Smash().success,"Pending outcome persists across reload");
                Check(restored.Collect()&&restored.State.money==paid+reward&&restored.State.earned==reward,"Collect credits exact dollar payout");
                Check(!restored.Collect()&&restored.State.money==paid+reward,"No duplicate payout");Roundtrip(restored);
                Check(!JsonUtility.ToJson(restored.State).Contains("motherboards")&&!JsonUtility.ToJson(restored.State).Contains("boardDrops"),"Current saves contain dollars only");
            }
            int wins=0;
            for(int roll=0;roll<1000;roll++){
                float value=roll/1000f;var game=new GameSession(c,null,()=>value);game.State.activePhone=11;game.State.phase=2;game.State.hp=1;
                game.Smash();if(game.State.lotteryWon)wins++;
            }
            Check(wins==700,"Exactly 70 percent of evenly spaced rolls win");
            var poor=new GameSession(c);poor.State.activePhone=-1;poor.State.hp=0;poor.State.money=0;
            for(int id=0;id<10;id++)Check(!poor.BuyPhone(id),"Retired devices cannot be purchased");
            Check(!poor.BuyPhone(11)&&!poor.BuyPhone(-1)&&!poor.BuyPhone(99)&&poor.BuyPhone(10),"Free phone remains available at zero dollars");
            Wreck(poor);Check(poor.Collect()&&poor.State.money==1,"Free phone recovers dollars without sales");
            for(int version=1;version<=5;version++)
            {
                var old=OldSave(c,version);string extra=version<3?",\"parts\":[1,2,3,4],\"drops\":[0,0,0,0]":",\"motherboards\":2,\"boardDrops\":0";
                if(version==3)extra+=",\"ownedRecipes\":[true,false,false]";
                var migrated=SaveStore.Decode(WithExtra(old,extra),c);
                int amount=version<3?600:120;
                Check(migrated!=null&&migrated.version==6&&migrated.money==100+amount&&migrated.hp==100,"Old inventory converts to dollars and HP rescales");
                Check(SaveStore.Decode(JsonUtility.ToJson(migrated),c).money==100+amount,"Inventory conversion runs once");
            }
            var pending=OldSave(c,5);pending.hp=0;pending.phase=2;pending.lootPending=true;pending.dropCash=12;
            var migratedPending=SaveStore.Decode(WithExtra(pending,",\"motherboards\":3,\"boardDrops\":4"),c);
            Check(migratedPending!=null&&migratedPending.money==280&&migratedPending.dropCash==252,"Pending boards and cash retain full value");
            var pendingGame=new GameSession(c,migratedPending);Check(pendingGame.Collect()&&pendingGame.State.money==532&&!pendingGame.Collect(),"Legacy pending dollars collect once");
            var crafted=OldSave(c,3);
            var craftedSave=SaveStore.Decode(WithExtra(crafted,",\"motherboards\":8,\"ownedRecipes\":[true,true,true],\"hasFinished\":true,\"finished\":{\"model\":1,\"finish\":2,\"number\":1,\"price\":300}"),c);
            Check(craftedSave!=null&&craftedSave.money==1260,"Crafting refunds and existing inventory dollars preserved");
            var legacy=OldSave(c,5);legacy.activePhone=1;legacy.hp=75;legacy.hammer=3;legacy.ownedHammers=new[]{true,true,true,true};legacy.autoLevel=2;legacy.autoEnabled=true;
            var moved=SaveStore.Decode(JsonUtility.ToJson(legacy),c);
            Check(moved!=null&&moved.activePhone==10&&moved.hp==50&&moved.hammer==0&&moved.money==1605&&moved.autoLevel==2&&moved.autoEnabled,"Retired device and paid hammers refunded; progress and automation preserved");
            Check(SaveStore.Decode(JsonUtility.ToJson(moved),c).money==1605,"Retired purchases refunded once");
            var oldTablet=OldSave(c,5);oldTablet.activePhone=11;oldTablet.phase=1;oldTablet.hp=51;
            Check(SaveStore.Decode(JsonUtility.ToJson(oldTablet),c).hp==500,"Tablet progress scales to 1000 HP per layer");
            var bad=GameSession.NewSave(c);bad.money=-1;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Negative dollars rejected");
            bad=GameSession.NewSave(c);bad.phase=3;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Fourth step rejected");
            bad=GameSession.NewSave(c);bad.activePhone=0;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Retired active ID rejected in new saves");
            bad=GameSession.NewSave(c);bad.dropCash=800;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Unfinished reward rejected");
            Check(SaveStore.Decode("{bad",c)==null,"Invalid JSON rejected");
            var auto=new GameSession(c);auto.State.money=1050;auto.State.autoLevel=3;auto.State.autoEnabled=true;
            Check(!auto.CanAutoStrike&&!auto.BuyAutomation()&&!auto.ToggleAutomation(),"Hidden automation cannot run, purchase or toggle");
            Check(auto.State.money==1050&&auto.State.autoLevel==3&&auto.State.autoEnabled,"Hidden automation preserves dollars and saved ownership");
            Roundtrip(auto);Check(auto.Smash().damage==1,"Manual strikes remain available with legacy automation ownership");
            var result="PASS: "+checks+" checks: dollar lottery, exact layer HP, one-power hammer, Classic-only catalogue, save migration and manual-only hammer.";Debug.Log(result);return result;
        }
        static SaveGame OldSave(GameCatalog c,int version)=>new SaveGame{version=version,money=100,activePhone=10,hp=90,ownedHammers=new[]{true,false,false,false}};
        static string WithExtra(SaveGame s,string extra)=>JsonUtility.ToJson(s).TrimEnd('}')+extra+"}";
    }
}
