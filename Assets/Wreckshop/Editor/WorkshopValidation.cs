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
        static void Wreck(GameSession g){int n=0;while(!g.State.lootPending){Check(g.Smash().success,"Hit accepted");if(++n>160)throw new Exception("Unbreakable device");}}
        static string Legacy(SaveGame s,string extra)=>JsonUtility.ToJson(s).TrimEnd('}')+","+extra+"}";
        [MenuItem("Pocket Wreckshop/Validate gameplay & saves")]
        public static string Run()
        {
            checks=0;var c=AssetDatabase.LoadAssetAtPath<GameCatalog>(WorkshopBuilder.CatalogPath);
            Check(c&&c.phones.Length==10&&c.partSprites.Length==1&&c.partPrices.Length==1,"Single motherboard inventory");
            Check(c.partPrices[0]==60,"Motherboard dollar sale value");
            var kinds=new System.Collections.Generic.HashSet<DeviceKind>();
            foreach(var device in c.phones)
            {
                kinds.Add(device.kind);Check(device.stages.Length==GameCatalog.DamageFrameCount&&device.phaseNames.Length==3,"Nineteen aligned frames and three steps");
                Check(device.partsPerType*c.partPrices[0]>device.price&&device.scrapCash==0,"Paid devices profitable through sales only");
                Check(device.fragments.Length==12&&Array.TrueForAll(device.fragments,f=>f!=null),"Device-specific glass, case and module fragments");
                foreach(var sprite in device.stages)Check(sprite&&sprite.rect.size==device.stages[0].rect.size,"Frame bounds");
            }
            Check(c.impactSprites.Length==6&&c.chipSprites.Length==12&&c.dustSprites.Length==4,"Impact flipbooks and material chips");
            Check(kinds.Count==7,"Seven families");
            for(int p=0;p<c.phones.Length;p++)for(int h=0;h<c.hammers.Length;h++)
            {
                var g=new GameSession(c,null,()=>.5f);var s=g.State;s.activePhone=-1;s.hp=0;s.money=1000;s.hammer=h;s.ownedHammers[h]=true;
                Check(g.BuyPhone(p)&&s.money==1000-c.phones[p].price,"Buy used device with dollars");
                Check(!g.BuyPhone(p),"Only one device at a time");
                int paid=s.money;
                for(int phase=0;phase<GameCatalog.DestructionSteps;phase++)
                {
                    int count=0;
                    while(s.phase==phase&&!s.lootPending){var hit=g.Smash();Check(hit.success&&hit.damage==c.hammers[h].damage,"Tool damage");if(++count>100)throw new Exception("Unbreakable step");}
                    if(phase<2)
                    {
                        Check(s.phase==phase+1&&!s.lootPending&&s.hp==c.PhaseIntegrity(p,phase+1),"Independent steps and no overflow");
                        Check(!g.Collect()&&!g.SellMotherboards(true)&&s.money==paid,"No early rewards");
                    }
                    Roundtrip(g);
                }
                Check(s.phase==2&&s.hp==0&&s.lootPending&&s.wrecked==1&&s.totalHits>=3,"Third step completes");
                Check(s.boardDrops==c.Salvage(p)[0]&&s.dropCash==0&&s.money==paid,"Breaking yields boards only");
                Check(!g.Smash().success,"No post-completion hit");int boards=s.boardDrops;
                Check(g.Collect()&&s.motherboards==boards&&s.money==paid,"Collect does not mint dollars");
                Check(!g.Collect(),"No double collection");Roundtrip(g);
                Check(g.SellMotherboards(true)&&s.motherboards==0&&s.money==paid+boards*60&&s.money>1000,"Sell for net dollar profit");
                Check(!g.SellMotherboards(true)&&!g.SellMotherboards(false),"No double sale");Roundtrip(g);
            }
            var game=new GameSession(c,null,()=>.5f);var state=game.State;
            Check(!game.BuyPhone(0)&&!game.SellMotherboards(false),"Busy bench and empty sales rejected");
            state.motherboards=3;int cash=state.money;
            Check(game.SellMotherboards(false)&&state.motherboards==2&&state.money==cash+60,"Sell one");
            Check(game.SellMotherboards(true)&&state.motherboards==0&&state.money==cash+180,"Sell remaining");
            Check(!game.BuyPhone(-1)&&!game.BuyPhone(100)&&!game.EquipHammer(-1),"Invalid indices rejected");
            var poor=new GameSession(c,null,()=>.5f);poor.State.activePhone=-1;poor.State.hp=0;poor.State.money=0;
            Check(!poor.BuyPhone(0)&&!poor.EquipHammer(1)&&poor.BuyPhone(3),"No overspend and zero-dollar recovery");
            Wreck(poor);Check(poor.Collect()&&poor.State.money==0,"Recovery still requires sale");Check(poor.SellMotherboards(true)&&poor.State.money==60,"Recover dollars by selling");
            cash=state.money;Check(game.EquipHammer(1)&&state.money==cash-c.hammers[1].price,"Buy tool with dollars");
            cash=state.money;Check(game.EquipHammer(0)&&game.EquipHammer(1)&&state.money==cash,"Owned tools equip without charge");Roundtrip(game);
            var old=GameSession.NewSave(c);old.version=1;old.hp=90;
            var migrated=SaveStore.Decode(Legacy(old,"\"parts\":[1,2,3,4],\"drops\":[0,0,0,0]"),c);
            Check(migrated!=null&&migrated.version==5&&migrated.hp==72&&migrated.motherboards==10,"v1 inventory migration");
            old.version=2;old.phase=3;old.hp=5;old.wrecked=2;
            migrated=SaveStore.Decode(Legacy(old,"\"parts\":[1,2,3,4],\"drops\":[0,0,0,0]"),c);
            Check(migrated!=null&&migrated.lootPending&&migrated.phase==2&&migrated.wrecked==3,"Removed fourth stage migration");Roundtrip(new GameSession(c,migrated));
            old.hp=0;old.lootPending=true;old.dropCash=12;old.wrecked=3;
            migrated=SaveStore.Decode(Legacy(old,"\"parts\":[1,2,3,4],\"drops\":[1,2,0,1]"),c);
            Check(migrated!=null&&migrated.boardDrops==4&&migrated.dropCash==12&&migrated.wrecked==3,"Old pending rewards retained");
            var pending=new GameSession(c,migrated);Check(pending.Collect()&&pending.State.money==112&&!pending.Collect(),"Old cash collected once");
            old=GameSession.NewSave(c);old.version=3;old.motherboards=8;
            migrated=SaveStore.Decode(Legacy(old,"\"ownedRecipes\":[true,true,true],\"hasFinished\":true,\"finished\":{\"model\":1,\"finish\":2,\"number\":1,\"price\":300}"),c);
            Check(migrated!=null&&migrated.money==780&&migrated.motherboards==8&&migrated.version==5,"Removed recipes refunded and completed phone sold");
            var again=SaveStore.Decode(JsonUtility.ToJson(migrated),c);Check(again!=null&&again.money==780,"Migration credit never repeats");
            Check(!JsonUtility.ToJson(again).Contains("Recipes")&&!JsonUtility.ToJson(again).Contains("finished"),"Current save removes crafting state");
            migrated=SaveStore.Decode(Legacy(old,"\"ownedRecipes\":[true,false,false],\"hasFinished\":false"),c);
            Check(migrated!=null&&migrated.money==100,"Starter recipe adds no refund");
            Check(SaveStore.Decode("{bad",c)==null,"Bad JSON rejected");
            var bad=GameSession.NewSave(c);bad.phase=3;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Fourth stage rejected");
            bad=GameSession.NewSave(c);bad.motherboards=-1;Check(SaveStore.Decode(JsonUtility.ToJson(bad),c)==null,"Negative inventory rejected");
            Check(new GameSession(c,null,()=>0f).Smash().damage==28,"Critical hit");
            var auto=new GameSession(c,null,()=>.5f);auto.State.money=149;
            Check(!auto.BuyAutomation()&&!auto.ToggleAutomation()&&!auto.CanAutoStrike,"Locked automation cannot run or overspend");
            auto.State.money=1050;
            for(int level=1;level<=3;level++)
            {
                int before=auto.State.money;Check(auto.BuyAutomation()&&auto.State.autoLevel==level&&auto.State.autoEnabled&&auto.State.money==before-c.autoPrices[level-1],"Sequential dollar automation upgrade");
                Check(auto.AutoInterval==c.autoIntervals[level-1]&&auto.CanAutoStrike,"Configured automation cadence");
                if(level>1)Check(c.autoIntervals[level-1]<c.autoIntervals[level-2],"Upgrade is faster");
                Roundtrip(auto);
            }
            Check(!auto.BuyAutomation()&&auto.State.money==0,"No charge beyond max automation");
            Check(auto.ToggleAutomation()&&!auto.CanAutoStrike&&auto.ToggleAutomation()&&auto.CanAutoStrike,"Automation on/off");
            Wreck(auto);Check(!auto.CanAutoStrike,"Auto stops at finished wreck");auto.Collect();Check(!auto.CanAutoStrike,"Auto stops on empty bench");
            Check(auto.BuyPhone(3)&&auto.CanAutoStrike,"Owned automation applies to next purchased device");
            var v4=GameSession.NewSave(c);v4.version=4;v4.money=345;v4.motherboards=7;v4.hammer=1;v4.ownedHammers[1]=true;
            var moved=SaveStore.Decode(JsonUtility.ToJson(v4),c);Check(moved!=null&&moved.version==5&&moved.money==345&&moved.motherboards==7&&moved.hammer==1&&moved.autoLevel==0&&!moved.autoEnabled,"v4 migration retains economy and leaves automation locked");
            var invalid=GameSession.NewSave(c);invalid.autoLevel=4;Check(SaveStore.Decode(JsonUtility.ToJson(invalid),c)==null,"Invalid auto level rejected");invalid.autoLevel=0;invalid.autoEnabled=true;Check(SaveStore.Decode(JsonUtility.ToJson(invalid),c)==null,"Unowned automation flag rejected");
            foreach(var device in c.phones)
            {
                int id=Array.IndexOf(c.phones,device);var damageGame=new GameSession(c,null,()=>.5f);var ds=damageGame.State;
                ds.activePhone=id;ds.phase=0;ds.hp=c.PhaseIntegrity(id,0);
                Check(c.DamageState(ds)==0,"New device has no damage");damageGame.Smash();
                Check(c.DamageState(ds)>0,"First hit always leaves visible permanent damage");
                for(int phase=0;phase<3;phase++)
                {
                    ds.phase=phase;ds.lootPending=false;int full=c.PhaseIntegrity(id,phase);ds.hp=full;
                    Check(c.DamageState(ds)==phase*6,"Step boundary continues the ordered damage progression");
                    int previous=c.DamageState(ds);
                    foreach(float fraction in new[]{.05f,.20f,.40f,.60f,.80f,.99f})
                    {
                        ds.hp=Math.Max(1,Mathf.CeilToInt(full*(1-fraction)));int next=c.DamageState(ds);
                        Check(next>=previous&&next/6==phase,"Health damage never restores a previous surface");previous=next;
                        var recovered=SaveStore.Decode(JsonUtility.ToJson(ds),c);Check(recovered!=null&&c.DamageState(recovered)==next,"Damage detail persists through save/reload");
                    }
                }
                ds.phase=2;ds.hp=0;ds.lootPending=true;ds.boardDrops=c.Salvage(id)[0];
                Check(c.DamageState(ds)==18,"Completed state is the whole wreck");
            }
            var result="PASS: "+checks+" checks: buy used devices, three steps, motherboard sales, dollar economy, tools, automation and save migration.";Debug.Log(result);return result;
        }
    }
}
