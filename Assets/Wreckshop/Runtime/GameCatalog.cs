using System;
using UnityEngine;

namespace PocketWreckshop
{
    [Serializable] public class HammerDefinition
    {
        public string title, description;
        public int damage, price;
        public Color color = Color.white;
    }
    public enum DeviceKind { Phone, Tablet, Headphones, Console, Controller, Speaker, Monitor }
    [Serializable] public class PhoneDefinition
    {
        public string title, tag;
        public int integrity, price, partsPerType, scrapCash;
        public DeviceKind kind;
        public Sprite[] stages, fragments;
        public string[] phaseNames;
        public Color color = Color.white;
    }
    [CreateAssetMenu(menuName = "Pocket Wreckshop/Game Catalog")]
    public class GameCatalog : ScriptableObject
    {
        public HammerDefinition[] hammers;
        public PhoneDefinition[] phones;
        public const int DestructionSteps=3, FramesPerStep=6, DamageFrameCount=DestructionSteps*FramesPerStep+1;
        public int[] autoPrices={150,300,600};
        public float[] autoIntervals={1.2f,.75f,.45f};
        public string[] partNames = { "Motherboard" };
        public int[] partPrices = { 60 };
        [Range(0, 1)] public float criticalChance = .12f;
        public Sprite[] damageStages,impactSprites,chipSprites,dustSprites;
        public Sprite[] partSprites;
        public Sprite hammerSprite, matSprite;
        public Sprite[] hammerSprites;
        public Font bodyFont, headingFont;
        // Persistent damage state selected from health, never advanced by animation time.
        public int DamageState(SaveGame state)
        {
            if(state.lootPending)return DamageFrameCount-1;
            float damage=1f-(float)state.hp/PhaseIntegrity(state.activePhone,state.phase);
            return state.phase*FramesPerStep+Mathf.Clamp(Mathf.FloorToInt(damage*FramesPerStep),damage>0?1:0,FramesPerStep-1);
        }
        public int PhaseIntegrity(int device,int phase)
        {
            float[] scales={1f,.85f,1f};
            return Mathf.Max(1,Mathf.RoundToInt(phones[device].integrity*scales[Mathf.Clamp(phase,0,2)]));
        }
        public string PhaseName(int device,int phase)
        {
            var names=phones[device].phaseNames;
            return names!=null&&names.Length==3?names[Mathf.Clamp(phase,0,2)]:new[]{"Surface","Casing","Modules"}[Mathf.Clamp(phase,0,2)];
        }
        public int[] Salvage(int device) => new[]{Mathf.Max(1,phones[device].partsPerType)};
    }
}
