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
    [Serializable] public class DeviceLayerPart
    {
        public int layer;
        public Sprite sprite;
        public Vector2 localPosition;
        public float scale=1f;
        public int sortingOrder;
    }
    [Serializable] public class PhoneDefinition
    {
        public string title, tag;
        public int integrity, price;
        [HideInInspector] public int partsPerType, scrapCash; // Legacy catalogue data for save conversion.
        [Range(0,1)] public float winChance;
        public int winDollars;
        public DeviceKind kind;
        public Sprite[] stages, fragments;
        public DeviceLayerPart[] layerParts=Array.Empty<DeviceLayerPart>();
        public Rect screenCrackArea;
        public Material screenCrackMaterial;
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
        public Sprite[] damageStages,impactSprites,chipSprites,dustSprites;
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
            return Mathf.Max(1,phones[device].integrity);
        }
        public const int ClassicPhoneId=10, ClassicTabletId=11;
        public bool IsAvailableDevice(int index)=>index==ClassicPhoneId||index==ClassicTabletId;
        public string PhaseName(int device,int phase)
        {
            var names=phones[device].phaseNames;
            return names!=null&&names.Length==3?names[Mathf.Clamp(phase,0,2)]:new[]{"Surface","Casing","Modules"}[Mathf.Clamp(phase,0,2)];
        }
    }
}
