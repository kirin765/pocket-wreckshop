using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

namespace PocketWreckshop.Editor
{
    // Sprite rectangles reference the supplied PNGs directly. No image synthesis or rewriting.
    public static class ClassicPhoneAssets
    {
        public const string Root="Assets/Wreckshop/Art/Devices/ClassicPhone/";
        public const string AsepritePath=Root+"phone2.aseprite";
        static readonly Rect[] LayerRects={new Rect(0,0,73,150),new Rect(33,8,73,150),new Rect(76,6,73,150)};
        static SpriteRect Slice(string name,Rect rect)=>new SpriteRect
        {
            name=name,rect=rect,pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,
            spriteID=new GUID(System.BitConverter.ToString(System.Security.Cryptography.MD5.Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes("classic_phone/"+name))).Replace("-",""))
        };
        static void Import(string file,params SpriteRect[] rects)
        {
            string path=Root+file+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();importer.SaveAndReimport();
        }
        public static void Import()
        {
            // Each full-device slice is 73 x 150; padding is selected in metadata only.
            Import("phone0",Slice("front",new Rect(0,0,73,150)),
                Slice("glass0",new Rect(8,96,18,22)),Slice("glass1",new Rect(39,78,18,22)),
                Slice("glass2",new Rect(9,36,18,22)),Slice("glass3",new Rect(43,16,18,22)));
            Import("phone1",Slice("casing",new Rect(33,8,73,150)),
                Slice("case0",new Rect(40,111,18,22)),Slice("case1",new Rect(80,91,18,22)),
                Slice("case2",new Rect(42,32,18,22)),Slice("case3",new Rect(79,16,18,22)));
            Import("phone2",Slice("internals",LayerRects[2]),
                Slice("module0",new Rect(80,124,18,18)),Slice("module1",new Rect(121,121,18,18)),
                Slice("module2",new Rect(84,62,18,22)),Slice("module3",new Rect(123,18,18,22)));
            foreach(var path in Directory.GetFiles(Root,"phone*-parts*.png").OrderBy(p=>p,StringComparer.Ordinal))
            {
                var match=Regex.Match(Path.GetFileNameWithoutExtension(path),@"^phone(\d+)-parts(\d+)$");
                if(!match.Success)continue;
                int layer=int.Parse(match.Groups[1].Value);
                if(layer>=LayerRects.Length)throw new InvalidOperationException("Part has no matching device layer: "+path);
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var part=Slice(Path.GetFileNameWithoutExtension(path),new Rect(0,0,texture.width,texture.height));
                // Use the owner's source-canvas center so the supplied part stays at its authored position.
                part.alignment=SpriteAlignment.Custom;
                part.pivot=new Vector2(LayerRects[layer].center.x/texture.width,LayerRects[layer].center.y/texture.height);
                if(Path.GetFileNameWithoutExtension(path)=="phone2-parts0")
                {
                    // This supplied board has its own larger canvas, rather than the layer's canvas.
                    // Trim only in sprite metadata; its source PNG stays untouched.
                    part.rect=new Rect(113,134,566,351);part.alignment=SpriteAlignment.Center;part.pivot=new Vector2(.5f,.5f);
                }
                Import(Path.GetFileNameWithoutExtension(path),part);
            }
            ImportAseprite();
        }
        static void ImportAseprite()
        {
            if(!File.Exists(AsepritePath))return;
            var importer=(AsepriteImporter)AssetImporter.GetAtPath(AsepritePath);
            importer.importMode=FileImportModes.AnimatedSprite;importer.layerImportMode=LayerImportModes.IndividualLayers;
            importer.includeHiddenLayers=false;importer.spritePixelsPerUnit=100;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.pivotSpace=PivotSpaces.Canvas;importer.pivotAlignment=SpriteAlignment.Custom;
            importer.customPivotPosition=new Vector2(LayerRects[2].center.x/228f,LayerRects[2].center.y/168f);
            importer.generateModelPrefab=false;importer.generateAnimationClips=false;importer.generateSpriteAtlas=false;
            importer.SaveAndReimport();
        }
        static Sprite AsepriteLayer(string name)=>AssetDatabase.LoadAllAssetsAtPath(AsepritePath).OfType<Sprite>().Single(s=>s.name==name);
        static Sprite Get(string file,string name)=>AssetDatabase.LoadAllAssetsAtPath(Root+file+".png")
            .OfType<Sprite>().Single(s=>s.name==name);
        public static void Assign(PhoneDefinition device)
        {
            const string materialPath="Assets/Wreckshop/ScreenCracks.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material)
            {
                material=new Material(Shader.Find("PocketWreckshop/ScreenCracks"));
                AssetDatabase.CreateAsset(material,materialPath);
            }
            device.screenCrackMaterial=material;device.screenCrackArea=new Rect(6,22,61,108);
            var layers=new[]{Get("phone0","front"),Get("phone1","casing"),Get("phone2","internals")};
            bool native=File.Exists(AsepritePath);
            if(native)layers[2]=AsepriteLayer("Layer");
            // Retain health-selected slots and three steps using only the supplied layer pixels.
            device.stages=Enumerable.Range(0,GameCatalog.DamageFrameCount)
                .Select(i=>layers[Math.Min(2,i/GameCatalog.FramesPerStep)]).ToArray();
            device.fragments=Enumerable.Range(0,12).Select(i=>i<4?Get("phone0","glass"+i):
                i<8?Get("phone1","case"+(i-4)):Get("phone2","module"+(i-8))).ToArray();
            device.layerParts=Directory.GetFiles(Root,"phone*-parts*.png").OrderBy(p=>p,StringComparer.Ordinal)
                .Select(path=>new{file=Path.GetFileNameWithoutExtension(path),match=Regex.Match(Path.GetFileNameWithoutExtension(path),@"^phone(\d+)-parts(\d+)$")})
                .Where(p=>p.match.Success&&(!native||int.Parse(p.match.Groups[1].Value)!=2))
                .Select(p=>new DeviceLayerPart{layer=int.Parse(p.match.Groups[1].Value),sprite=Get(p.file,p.file)}).ToArray();
            if(native)
                device.layerParts=device.layerParts.Concat(new[]{
                    new DeviceLayerPart{layer=2,sprite=AsepriteLayer("Layer 1"),sortingOrder=0},
                    new DeviceLayerPart{layer=2,sprite=AsepriteLayer("Layer 2"),sortingOrder=1}
                }).ToArray();
            foreach(var part in device.layerParts)
            {
                if(part.sprite.name=="phone1-parts1")part.sortingOrder=-1;
                if(part.sprite.name=="phone2-parts0")
                {
                    part.localPosition=new Vector2(0,.45f);part.scale=65f/566f;
                }
            }
        }
    }
}
