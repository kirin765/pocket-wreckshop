using System;
using UnityEngine;

namespace PocketWreckshop
{
    public static class SaveStore
    {
        public const string Key="PocketWreckshop.Unity.Save.v1";
        const string BackupKey=Key+".backup";
        // Removed systems are read only during migration; current saves contain no crafting state.
        [Serializable] sealed class LegacyPhone { public int model, finish, number, price; }
        [Serializable] sealed class LegacySave
        {
            public int[] parts,drops;
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
                if(s==null||s.version<1||s.version>5||s.money<0||s.earned<0||s.wrecked<0||s.totalHits<0||
                    s.ownedHammers==null||s.ownedHammers.Length!=c.hammers.Length||s.hammer<0||s.hammer>=c.hammers.Length||!s.ownedHammers[s.hammer]||
                    s.activePhone< -1||s.activePhone>=c.phones.Length||s.hp<0)return null;
                if(s.version<4)
                {
                    var old=JsonUtility.FromJson<LegacySave>(json);
                    if(s.version<3)
                    {
                        s.motherboards=Sum(old.parts);s.boardDrops=Sum(old.drops);
                        if(s.version==1){s.phase=s.lootPending?2:0;if(s.activePhone>=0&&!s.lootPending)s.hp=Math.Min(s.hp,c.PhaseIntegrity(s.activePhone,0));}
                        else if(s.lootPending)s.phase=2;
                        else if(s.activePhone>=0&&s.phase==3)
                        {
                            s.phase=2;s.hp=0;s.lootPending=true;s.boardDrops=c.Salvage(s.activePhone)[0];s.wrecked++;
                        }
                    }
                    // Credit removed purchases once. The next persisted save is v4.
                    int refund=0;
                    if(s.version==3)
                    {
                        if(old.ownedRecipes==null||old.ownedRecipes.Length!=3||!old.ownedRecipes[0])return null;
                        if(old.ownedRecipes[1])refund+=120;if(old.ownedRecipes[2])refund+=260;
                    }
                    if(old.hasFinished)
                    {
                        var phone=old.finished;int[] values={190,300,600};
                        if(phone==null||phone.model<0||phone.model>2||phone.finish<0||phone.finish>4||phone.number<1||
                            phone.price!=190&&phone.price!=values[phone.model])return null;
                        refund=checked(refund+phone.price);
                    }
                    s.money=checked(s.money+refund);s.earned=checked(s.earned+refund);s.version=4;
                }
                if(s.version<5){s.autoLevel=0;s.autoEnabled=false;s.version=5;}
                if(s.autoLevel<0||s.autoLevel>c.autoPrices.Length||s.autoEnabled&&s.autoLevel==0)return null;
                if(s.phase<0||s.phase>=GameCatalog.DestructionSteps||s.motherboards<0||s.boardDrops<0||s.dropCash<0)return null;
                if(s.activePhone==-1&&(s.hp!=0||s.lootPending))return null;
                if(s.activePhone>=0&&(s.hp>c.PhaseIntegrity(s.activePhone,s.phase)||(s.hp==0)!=s.lootPending||s.lootPending&&s.phase!=2))return null;
                if(s.lootPending&&s.boardDrops<1||!s.lootPending&&(s.boardDrops!=0||s.dropCash!=0))return null;
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
