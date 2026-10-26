using System;
using UnityEngine;

namespace PocketWreckshop
{
    [Serializable] public class SaveGame
    {
        public int version = 5, money = 100, hammer, activePhone = 0, hp = 100;
        public bool[] ownedHammers = { true, false, false, false };
        public int motherboards, boardDrops;
        public int dropCash, wrecked, earned, totalHits, phase;
        public bool lootPending, sound, autoEnabled;
        public int autoLevel;
    }
    public struct HitResult
    {
        public bool success, critical, broken, phaseChanged;
        public int damage;
    }

    // All economy rules live here; presentation never edits currency or inventory.
    public sealed class GameSession
    {
        public GameCatalog Catalog { get; }
        public SaveGame State { get; }
        readonly Func<float> random;
        public event Action Changed;
        public GameSession(GameCatalog catalog, SaveGame state = null, Func<float> random = null)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            State = state ?? NewSave(catalog);
            this.random = random ?? (() => UnityEngine.Random.value);
        }
        public static SaveGame NewSave(GameCatalog catalog)
        {
            var s = new SaveGame { hp = catalog.PhaseIntegrity(0,0),
                ownedHammers = new bool[catalog.hammers.Length] };
            s.ownedHammers[0] = true;
            return s;
        }
        public float AutoInterval => State.autoLevel>0?Catalog.autoIntervals[State.autoLevel-1]:Catalog.autoIntervals[0];
        public bool CanAutoStrike => State.autoLevel>0&&State.autoEnabled&&HasPhone&&!State.lootPending&&State.hp>0;
        public bool BuyAutomation()
        {
            if(State.autoLevel>=Catalog.autoPrices.Length||State.money<Catalog.autoPrices[State.autoLevel])return false;
            State.money-=Catalog.autoPrices[State.autoLevel];State.autoLevel++;State.autoEnabled=true;Changed?.Invoke();return true;
        }
        public bool ToggleAutomation()
        {
            if(State.autoLevel==0)return false;
            State.autoEnabled=!State.autoEnabled;Changed?.Invoke();return true;
        }
        public bool HasPhone => State.activePhone >= 0;
        public HitResult Smash()
        {
            if (!HasPhone || State.hp <= 0 || State.lootPending) return default;
            bool critical = random() < Catalog.criticalChance;
            int damage = Catalog.hammers[State.hammer].damage * (critical ? 2 : 1);
            State.hp = Math.Max(0, State.hp - damage);
            State.totalHits++;
            bool phaseChanged=State.hp==0&&State.phase<2;
            bool broken=State.hp==0&&State.phase==2;
            if(phaseChanged)
            {
                State.phase++;
                State.hp=Catalog.PhaseIntegrity(State.activePhone,State.phase);
            }
            if (broken)
            {
                State.boardDrops=Catalog.Salvage(State.activePhone)[0];
                State.dropCash=0;
                State.lootPending=true;
                State.wrecked++;
            }
            Changed?.Invoke();
            return new HitResult { success = true, damage = damage, critical = critical, broken = broken, phaseChanged = phaseChanged };
        }
        public bool Collect()
        {
            if (!State.lootPending) return false;
            State.motherboards+=State.boardDrops;
            State.boardDrops=0;
            Credit(State.dropCash);
            State.dropCash = 0;
            State.lootPending = false;
            State.activePhone = -1;
            State.hp = 0;
            State.phase = 0;
            Changed?.Invoke();
            return true;
        }
        void Credit(int amount) { State.money += amount; State.earned += amount; }
        public bool BuyPhone(int index)
        {
            if (HasPhone || index < 0 || index >= Catalog.phones.Length) return false;
            var p = Catalog.phones[index];
            if (State.money < p.price) return false;
            State.money -= p.price;
            State.activePhone = index;
            State.phase=0;
            State.hp = Catalog.PhaseIntegrity(index,0);
            Changed?.Invoke();
            return true;
        }
        public bool EquipHammer(int index)
        {
            if (index < 0 || index >= Catalog.hammers.Length || index == State.hammer) return false;
            if (!State.ownedHammers[index])
            {
                if (State.money < Catalog.hammers[index].price) return false;
                State.money -= Catalog.hammers[index].price;
                State.ownedHammers[index] = true;
            }
            State.hammer = index;
            Changed?.Invoke();
            return true;
        }
        public bool SellMotherboards(bool all)
        {
            if(State.motherboards<1)return false;
            int count=all?State.motherboards:1;
            State.motherboards-=count;Credit(count*Catalog.partPrices[0]);Changed?.Invoke();return true;
        }
        public void ToggleSound() { State.sound = !State.sound; Changed?.Invoke(); }
    }
}
