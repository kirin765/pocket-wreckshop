using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace PocketWreckshop.Editor
{
    public static class LayeredTabletAssets
    {
        public const string Source="Assets/Wreckshop/Art/Devices/LayeredTablet/ipad.aseprite";
        public static void Import()
        {
            if(!System.IO.File.Exists(Source))return;
            var importer=(AsepriteImporter)AssetImporter.GetAtPath(Source);
            importer.importMode=FileImportModes.AnimatedSprite;
            importer.layerImportMode=LayerImportModes.IndividualLayers;
            importer.includeHiddenLayers=false;importer.spritePixelsPerUnit=100;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.pivotSpace=PivotSpaces.Canvas;importer.pivotAlignment=SpriteAlignment.Custom;
            // Shared center of the authored backing: x20..97, y3..106 in the 118x120 canvas.
            importer.customPivotPosition=new Vector2(58.5f/118f,65.5f/120f);
            importer.generateModelPrefab=false;importer.generateAnimationClips=false;importer.generateSpriteAtlas=false;
            importer.SaveAndReimport();
        }
        static Sprite Layer(int number)=>AssetDatabase.LoadAllAssetsAtPath(Source).OfType<Sprite>()
            .Single(s=>s.name=="Layer "+number);
        public static PhoneDefinition Create()
        {
            var front=Layer(6);var back=Layer(1);
            return new PhoneDefinition{
                title="Classic tablet",tag="Tablet",kind=DeviceKind.Tablet,integrity=1000,price=499,
                partsPerType=3,scrapCash=0,winChance=.7f,winDollars=800,color=Color.white,
                phaseNames=new[]{"Screen","Components","Backing"},
                stages=Enumerable.Range(0,GameCatalog.DamageFrameCount).Select(i=>i<6?front:back).ToArray(),
                // Source sprites supply the required fragment references; mounted devices do not emit particles.
                fragments=Enumerable.Range(0,12).Select(i=>i<4?front:i<8?back:Layer(2+i%4)).ToArray(),
                layerParts=Enumerable.Range(2,4).Select(n=>new DeviceLayerPart{
                    layer=1,sprite=Layer(n),sortingOrder=n-2
                }).ToArray(),
                screenCrackMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Wreckshop/ScreenCracks.mat"),
                screenCrackArea=new Rect(7,12,62,80)
            };
        }
        [MenuItem("Pocket Wreckshop/Add Classic tablet")]
        public static void Add()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before adding a device.");
            AssetDatabase.Refresh();Import();
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>(WorkshopBuilder.CatalogPath);
            var devices=catalog.phones.ToList();var device=Create();
            int existing=devices.FindIndex(d=>d.title==device.title);
            if(existing<0)devices.Add(device);else devices[existing]=device;
            catalog.phones=devices.ToArray();EditorUtility.SetDirty(catalog);
            DeviceAtlasBuilder.Pack(catalog);AssetDatabase.SaveAssets();
        }
    }
}
