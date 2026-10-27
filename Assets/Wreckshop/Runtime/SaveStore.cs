using System;
using UnityEngine;

namespace PocketWreckshop
{
    public static class SaveStore
    {
        public const string Key="PocketWreckshop.Unity.Save.v1";
        const string BackupKey=Key+".backup";
        // Retired inventory/crafting fields are read only when migrating older saves.
        [Serializable] sealed class LegacyPhone { public int model, finish, number, price; }
        [Serializable] sealed class LegacySave
        {
            public int[] parts,drops;
            public int motherboards,boardDrops;
            public bool[] ownedRecipes;
            public bool hasFinished;
            public LegacyPhone finished;
        }
        public static SaveGame Load(GameCatalog c)=>Decode(PlayerPrefs.GetString(Key,""),c)??Decode(PlayerPrefs.GetString(BackupKey,""),c)??GameSession.NewSave(c);
        public static SaveGame Decode(string json,GameCatalog c)
        {
            if(string.IsNullOrEmpty(json))return null;
            try
            {
                var s=JsonUtility.FromJson<SaveGame>(json);
                if(s==null||s.version<1||s.version>6||s.money<0||s.earned<0||s.wrecked<0||s.totalHits<0||
                    s.ownedHammers==null||s.hammer<0||s.hammer>=s.ownedHammers.Length||!s.ownedHammers[s.hammer]||
                    s.activePhone< -1||s.activePhone>=c.phones.Length||s.hp<0||s.dropCash<0)return null;
                if(s.version<6)
                {
                    if(s.ownedHammers.Length!=4)return null;
                    var old=JsonUtility.FromJson<LegacySave>(json);
                    if(s.version<3)
                    {
                        old.motherboards=Sum(old.parts);old.boardDrops=Sum(old.drops);
                        if(s.version==1){s.phase=s.lootPending?2:0;}
                        else if(s.lootPending)s.phase=2;
                        else if(s.activePhone>=0&&s.phase==3)
                        {
                            s.phase=2;s.hp=0;s.lootPending=true;
                            int[] yields={1,3,6,1,2,1,4,1,1,3,2,3};
                            old.boardDrops=yields[s.activePhone];s.wrecked++;
                        }
                    }
                    if(old.motherboards<0||old.boardDrops<0)return null;
                    if(s.phase<0||s.phase>=GameCatalog.DestructionSteps)return null;
                    if(s.activePhone==-1&&(s.hp!=0||s.lootPending))return null;
                    if(s.activePhone>=0&&((s.hp==0)!=s.lootPending||s.lootPending&&s.phase!=2))return null;
                    if(s.lootPending&&old.boardDrops<1||!s.lootPending&&(old.boardDrops!=0||s.dropCash!=0))return null;
                    int refund=checked(old.motherboards*60);
                    if(s.version==3)
                    {
                        if(old.ownedRecipes==null||old.ownedRecipes.Length!=3||!old.ownedRecipes[0])return null;
                        if(old.ownedRecipes[1])refund=checked(refund+120);
                        if(old.ownedRecipes[2])refund=checked(refund+260);
                    }
                    if(s.version<4&&old.hasFinished)
                    {
                        var phone=old.finished;int[] values={190,300,600};
                        if(phone==null||phone.model<0||phone.model>2||phone.finish<0||phone.finish>4||phone.number<1||
                            phone.price!=190&&phone.price!=values[phone.model])return null;
                        refund=checked(refund+phone.price);
                    }
                    // Removed paid hammers become dollars; everyone now uses the power-1 hammer.
                    int[] hammerPrices={0,120,360,950};
                    for(int i=1;i<4;i++)if(s.ownedHammers[i])refund=checked(refund+hammerPrices[i]);
                    s.hammer=0;s.ownedHammers=new[]{true};
                    if(s.lootPending)
                    {
                        s.dropCash=checked(s.dropCash+old.boardDrops*60);
                        s.lotteryWon=s.dropCash>0;
                    }
                    if(s.activePhone>=0)
                    {
                        // Map retired bench devices to the free Classic phone and refund their purchase.
                        int previous=s.activePhone;
                        int[] oldHealth={72,150,240,180,100,70,150,65,60,120,90,120};
                        float scale=s.phase==1?.85f:1f;
                        int oldMaximum=Math.Max(1,Mathf.RoundToInt(oldHealth[previous]*scale));
                        if(s.version>1&&s.hp>oldMaximum)return null;
                        if(!c.IsAvailableDevice(previous))
                        {
                            if(!s.lootPending)refund=checked(refund+c.phones[previous].price);
                            s.activePhone=GameCatalog.ClassicPhoneId;
                        }
                        if(!s.lootPending)s.hp=Mathf.Clamp(Mathf.CeilToInt((float)s.hp/oldMaximum*c.PhaseIntegrity(s.activePhone,s.phase)),1,c.PhaseIntegrity(s.activePhone,s.phase));
                    }
                    s.money=checked(s.money+refund);s.earned=checked(s.earned+refund);
                    if(s.version<5){s.autoLevel=0;s.autoEnabled=false;}
                    s.version=6;
                }
                if(s.ownedHammers.Length!=1||s.hammer!=0||!s.ownedHammers[0])return null;
                if(s.autoLevel<0||s.autoLevel>c.autoPrices.Length||s.autoEnabled&&s.autoLevel==0)return null;
                if(s.phase<0||s.phase>=GameCatalog.DestructionSteps)return null;
                if(s.activePhone==-1&&(s.hp!=0||s.lootPending))return null;
                if(s.activePhone>=0&&(!c.IsAvailableDevice(s.activePhone)||s.hp>c.PhaseIntegrity(s.activePhone,s.phase)||
                    (s.hp==0)!=s.lootPending||s.lootPending&&s.phase!=2))return null;
                if(!s.lootPending&&(s.dropCash!=0||s.lotteryWon))return null;
                if(s.lootPending&&s.lotteryWon!=(s.dropCash>0))return null;
                return s;
            }
            catch(Exception){return null;}
        }
        static int Sum(int[] parts){if(parts==null||parts.Length!=4)throw new ArgumentException("Invalid legacy inventory");int n=0;foreach(int p in parts){if(p<0)throw new ArgumentException("Negative inventory");n=checked(n+p);}return n;}
        public static void Save(SaveGame s)
        {
            var previous=PlayerPrefs.GetString(Key,"");if(!string.IsNullOrEmpty(previous))PlayerPrefs.SetString(BackupKey,previous);
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(s));PlayerPrefs.Save();
        }
    }
}
