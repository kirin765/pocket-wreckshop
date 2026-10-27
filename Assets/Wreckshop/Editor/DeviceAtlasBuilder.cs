using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace PocketWreckshop.Editor
{
    public static class DeviceAtlasBuilder
    {
        public static void Pack(GameCatalog catalog)
        {
            const string path="Assets/Wreckshop/DeviceSprites.spriteatlas";
            var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if(!atlas){atlas=new SpriteAtlas();AssetDatabase.CreateAsset(atlas,path);}
            atlas.Remove(atlas.GetPackables());
            var packing=atlas.GetPackingSettings();packing.enableRotation=false;packing.enableTightPacking=false;packing.padding=4;atlas.SetPackingSettings(packing);
            var texture=atlas.GetTextureSettings();texture.filterMode=FilterMode.Point;texture.generateMipMaps=false;texture.readable=false;atlas.SetTextureSettings(texture);
            var sprites=catalog.phones.SelectMany(p=>p.stages.Concat(p.fragments)
                .Concat((p.layerParts??System.Array.Empty<DeviceLayerPart>()).Select(part=>part.sprite)))
                .Concat(catalog.impactSprites).Concat(catalog.chipSprites).Concat(catalog.dustSprites).Concat(catalog.hammerSprites).Append(catalog.matSprite).Distinct().Cast<Object>().ToArray();
            var platform=atlas.GetPlatformSettings("DefaultTexturePlatform");platform.maxTextureSize=4096;platform.textureCompression=TextureImporterCompression.Uncompressed;atlas.SetPlatformSettings(platform);
            atlas.Add(sprites);atlas.SetIncludeInBuild(true);EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget);
        }
    }
}
