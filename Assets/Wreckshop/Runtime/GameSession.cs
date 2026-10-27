using System;
using UnityEngine;

namespace PocketWreckshop
{
    [Serializable] public class SaveGame
    {
        public int version = 6, money = 100, hammer, activePhone = GameCatalog.ClassicPhoneId, hp = 100;
        public bool[] ownedHammers = { true };
        public bool lotteryWon;
        public int dropCash, wrecked, earned, totalHits, phase;
        public bool lootPending, sound, autoEnabled;
        public int autoLevel;
    }
    public struct HitResult
    {
        public bool success, broken, phaseChanged;
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
            var s = new SaveGame { hp = catalog.PhaseIntegrity(GameCatalog.ClassicPhoneId,0),
                ownedHammers = new bool[catalog.hammers.Length] };
            s.ownedHammers[0] = true;
            return s;
        }
        public float AutoInterval => State.autoLevel>0?Catalog.autoIntervals[State.autoLevel-1]:Catalog.autoIntervals[0];
        // Retain saved ownership for compatibility while automation is hidden and disabled.
        public bool CanAutoStrike => false;
        public bool BuyAutomation() => false;
        public bool ToggleAutomation() => false;
        public bool HasPhone => State.activePhone >= 0;
        public HitResult Smash()
        {
            if (!HasPhone || State.hp <= 0 || State.lootPending) return default;
            int damage = Catalog.hammers[0].damage;
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
                var device=Catalog.phones[State.activePhone];
                State.lotteryWon=device.winChance>=1f||(device.winChance>0f&&random()<device.winChance);
                State.dropCash=State.lotteryWon?device.winDollars:0;
                State.lootPending=true;
                State.wrecked++;
            }
            Changed?.Invoke();
            return new HitResult { success = true, damage = damage, broken = broken, phaseChanged = phaseChanged };
        }
        public bool Collect()
        {
            if (!State.lootPending) return false;
            Credit(State.dropCash);
            State.dropCash = 0;
            State.lotteryWon=false;
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
            if (HasPhone || index < 0 || index >= Catalog.phones.Length || !Catalog.IsAvailableDevice(index)) return false;
            var p = Catalog.phones[index];
            if (State.money < p.price) return false;
            State.money -= p.price;
            State.activePhone = index;
            State.phase=0;
            State.hp = Catalog.PhaseIntegrity(index,0);
            Changed?.Invoke();
            return true;
        }
        public void ToggleSound() { State.sound = !State.sound; Changed?.Invoke(); }
    }
}
