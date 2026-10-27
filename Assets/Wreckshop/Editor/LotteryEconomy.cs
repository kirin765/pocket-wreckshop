using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PocketWreckshop.Editor
{
    public static class LotteryEconomy
    {
        public static void Configure(GameCatalog catalog)
        {
            var phone=catalog.phones[GameCatalog.ClassicPhoneId];
            phone.integrity=100;phone.price=0;phone.winChance=1;phone.winDollars=1;
            var tablet=catalog.phones[GameCatalog.ClassicTabletId];
            tablet.integrity=1000;tablet.price=499;tablet.winChance=.7f;tablet.winDollars=800;
            catalog.hammers=new[]{new HammerDefinition{title="Hammer",description="Power 1",damage=1,price=0,color=Color.white}};
            catalog.hammerSprites=new[]{catalog.hammerSprites[0]};catalog.hammerSprite=catalog.hammerSprites[0];
        }
        [MenuItem("Pocket Wreckshop/Apply dollar lottery")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before changing the catalogue.");
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>(WorkshopBuilder.CatalogPath);
            Configure(catalog);EditorUtility.SetDirty(catalog);DeviceAtlasBuilder.Pack(catalog);
            var game=UnityEngine.Object.FindFirstObjectByType<WorkshopGame>();
            if(game){
                var ui=game.ui;
                for(int i=ui.transform.childCount-1;i>=0;i--)UnityEngine.Object.DestroyImmediate(ui.transform.GetChild(i).gameObject);
                ui.CreateShell(catalog);game.Preview();
                EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
