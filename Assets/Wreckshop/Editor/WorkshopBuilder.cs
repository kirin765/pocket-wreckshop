using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace PocketWreckshop.Editor
{
    public static class WorkshopBuilder
    {
        public const string CatalogPath="Assets/Wreckshop/GameCatalog.asset";
        public const string ScenePath="Assets/Wreckshop/Scenes/Wreckshop.unity";
        static Color C(string hex)=>WorkshopUI.Hex(hex);
        [MenuItem("Pocket Wreckshop/Refresh art only")]
        public static void RefreshArt()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before refreshing art.");
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if(!catalog)throw new InvalidOperationException("Open the existing project catalogue before refreshing art.");
            ImportSprites();
            DeviceAtlasBuilder.Pack(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("Artwork imported and atlas packed. Scene, catalogue values and PNG artwork were preserved.");
        }
        [MenuItem("Pocket Wreckshop/Create or rebuild scene")]
        public static void CreateScene()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before rebuilding.");
            Directory.CreateDirectory("Assets/Wreckshop/Scenes");
            ImportSprites();
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if(!catalog)
            {
                catalog=ScriptableObject.CreateInstance<GameCatalog>();
                catalog.hammers=new[]{
                    new HammerDefinition{title="Trusty Claw",damage=14,price=0,color=C("b6c3bd"),description="A little rusty. A lot of heart."},
                    new HammerDefinition{title="Club Hammer",damage=28,price=120,color=C("9bddc3"),description="Twice the punch. Half the taps."},
                    new HammerDefinition{title="Heavy Sledge",damage=55,price=360,color=C("efb777"),description="For very personal demolition."},
                    new HammerDefinition{title="Pixel Pulverizer",damage=110,price=950,color=C("c5abf6"),description="A one-hit wonder. Usually."}
                };
                catalog.phones=new[]{
                    new PhoneDefinition{title="Pocket One",tag="THE CLASSIC",integrity=100,price=25,partsPerType=1,scrapCash=12,color=C("a6cba5")},
                    new PhoneDefinition{title="Brick Pro",tag="BUILT LIKE A BRICK",integrity=220,price=75,partsPerType=2,scrapCash=38,color=C("b5a8d3")},
                    new PhoneDefinition{title="Gold Standard",tag="PREMIUM SALVAGE",integrity=420,price=155,partsPerType=4,scrapCash=80,color=C("ebc565")},
                    new PhoneDefinition{title="Scrap Special",tag="ONE PERSON’S TRASH",integrity=70,price=0,partsPerType=1,scrapCash=0,color=C("a1abaa")}
                };
                AssetDatabase.CreateAsset(catalog,CatalogPath);
            }
            ConfigureDevices(catalog);
            catalog.partNames=new[]{"Motherboard"};catalog.partPrices=new[]{60};
            string[] hammerNames={"Claw hammer","Club hammer","Sledgehammer","Power hammer"};
            for(int i=0;i<4;i++)catalog.hammers[i].title=hammerNames[i];
            catalog.autoPrices=new[]{150,300,600};catalog.autoIntervals=new[]{1.2f,.75f,.45f};
            catalog.impactSprites=Effects("impact",6);catalog.dustSprites=Effects("dust",4);
            catalog.chipSprites=new Sprite[12];
            for(int phase=0;phase<3;phase++)for(int i=0;i<4;i++)catalog.chipSprites[phase*4+i]=Sprite("Effects/chip_"+phase+"_"+i);
            catalog.damageStages=catalog.phones[0].stages;
            catalog.partSprites=new[]{Sprite("part_1")};
            catalog.hammerSprite=Sprite("hammer");catalog.hammerSprites=new[]{Sprite("hammer"),Sprite("hammer_1"),Sprite("hammer_2"),Sprite("hammer_3")};catalog.matSprite=Sprite("portrait_desk");
            catalog.bodyFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Wreckshop/Fonts/WorkshopBody.ttf");
            catalog.headingFont=AssetDatabase.LoadAssetAtPath<Font>("Assets/Wreckshop/Fonts/WorkshopPixel.ttf");
            EditorUtility.SetDirty(catalog);
            DeviceAtlasBuilder.Pack(catalog);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraGo=new GameObject("Portrait Camera",typeof(Camera),typeof(AudioListener),typeof(PortraitCamera));
            cameraGo.tag="MainCamera";cameraGo.transform.position=new Vector3(0,0,-10);
            var camera=cameraGo.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=C("303943");camera.nearClipPlane=.1f;camera.farClipPlane=30;
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if(!shader)throw new InvalidOperationException("URP 2D sprite shader is missing.");
            const string materialPath="Assets/Wreckshop/SpriteUnlit.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(!material){material=new Material(shader);AssetDatabase.CreateAsset(material,materialPath);}
            var bench=new GameObject("Workbench · top-down 2D",typeof(PhoneVisual));
            var visual=bench.GetComponent<PhoneVisual>();visual.catalog=catalog;bench.transform.position=new Vector3(0,-.08f,0);
            AddSprite("Wood desk and repair mat",bench.transform,catalog.matSprite,material,new Vector3(0,0,2),0);
            visual.phone=AddSprite("Phone · damage stages",bench.transform,catalog.damageStages[0],material,new Vector3(0,0,0),2);
            visual.phone.transform.localPosition=new Vector3(0,.18f,0);
            visual.phone.transform.localScale=Vector3.one*Mathf.Min(5f/(catalog.damageStages[0].rect.height/100f),3.6f/(catalog.damageStages[0].rect.width/100f));
            visual.phone.transform.localRotation=Quaternion.Euler(0,0,2);
            visual.hammer=AddSprite("Hammer · animated strike",bench.transform,catalog.hammerSprite,material,new Vector3(.7f,.6f,-3),9);
            visual.hammer.enabled=false;
            visual.effectsRoot=new GameObject("Physics debris").transform;visual.effectsRoot.SetParent(bench.transform,false);
            var root=new GameObject("Workshop UI · 450 × 800",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(WorkshopUI));
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=5;canvas.sortingOrder=100;canvas.pixelPerfect=true;
            var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(450,800);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=1;
            var ui=root.GetComponent<WorkshopUI>();ui.CreateShell(catalog);
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            var game=new GameObject("Pocket Wreckshop · Game",typeof(WorkshopGame)).GetComponent<WorkshopGame>();
            game.catalog=catalog;game.ui=ui;game.phoneVisual=visual;game.Preview();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            ConfigurePlayer();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject=game.gameObject;
            Debug.Log("Pocket Wreckshop scene created. Open Wreckshop.unity and press Play.");
        }
        static void ConfigureDevices(GameCatalog catalog)
        {
            // Keep the first four IDs stable so existing inventories and saves migrate safely.
            catalog.phones=new[]{
                Device("Phone",DeviceKind.Phone,72,25,1,12),
                Device("Phone Pro",DeviceKind.Phone,150,75,3,0),
                Device("Premium phone",DeviceKind.Phone,240,155,6,0),
                Device("Scrap phone",DeviceKind.Phone,180,0,1,0),
                Device("Tablet",DeviceKind.Tablet,100,60,2,26),
                Device("Headphones",DeviceKind.Headphones,70,40,1,20),
                Device("Game console",DeviceKind.Console,150,120,4,0),
                Device("Controller",DeviceKind.Controller,65,35,1,18),
                Device("Speaker",DeviceKind.Speaker,60,30,1,16),
                Device("Monitor",DeviceKind.Monitor,120,85,3,0)
            };
        }
        static PhoneDefinition Device(string name,DeviceKind kind,int hp,int cost,int parts,int cash)
        {
            string stem=kind.ToString().ToLowerInvariant();
            var stages=new Sprite[GameCatalog.DamageFrameCount];
            for(int i=0;i<stages.Length;i++)stages[i]=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Wreckshop/Art/Devices/"+stem+"_"+i.ToString("D2")+".png");
            var fragments=new Sprite[12];
            for(int phase=0;phase<3;phase++)for(int i=0;i<4;i++)fragments[phase*4+i]=Sprite("Devices/Fragments/"+stem+"_"+phase+"_"+i);
            string[] phases;
            switch(kind)
            {
                case DeviceKind.Headphones:phases=new[]{"Earpads","Headband","Drivers"};break;
                case DeviceKind.Console:phases=new[]{"Panels","Case","Mainboard"};break;
                case DeviceKind.Controller:phases=new[]{"Buttons","Grips","Circuit board"};break;
                case DeviceKind.Speaker:phases=new[]{"Grille","Shell","Driver"};break;
                case DeviceKind.Monitor:phases=new[]{"Screen","Housing","Power board"};break;
                default:phases=new[]{"Screen","Casing","Modules"};break;
            }
            return new PhoneDefinition{title=name,tag=kind.ToString(),kind=kind,integrity=hp,price=cost,partsPerType=parts,scrapCash=0,color=Color.white,stages=stages,fragments=fragments,phaseNames=phases};
        }
        static Sprite[] Effects(string prefix,int count){var sprites=new Sprite[count];for(int i=0;i<count;i++)sprites[i]=Sprite("Effects/"+prefix+"_"+i);return sprites;}
        static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Wreckshop/Art/"+name+".png");
        static SpriteRenderer AddSprite(string name,Transform parent,Sprite sprite,Material material,Vector3 position,int order)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=position;
            var sr=go.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.sortingOrder=order;return sr;
        }
        static void ImportSprites()
        {
            AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles("Assets/Wreckshop/Art","*.png",SearchOption.AllDirectories))
            {
                if(file.EndsWith("modern_phone_sheet.png"))continue;
                var importer=(TextureImporter)AssetImporter.GetAtPath(file);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;
                importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=512;importer.SaveAndReimport();
            }
        }
        public static void ConfigurePlayer()
        {
            PlayerSettings.companyName="Pocket Wreckshop";PlayerSettings.productName="Pocket Wreckshop";PlayerSettings.bundleVersion="0.6.1";
            PlayerSettings.defaultScreenWidth=450;PlayerSettings.defaultScreenHeight=800;PlayerSettings.defaultWebScreenWidth=450;PlayerSettings.defaultWebScreenHeight=800;
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"studio.pocketwreckshop.game");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;PlayerSettings.Android.bundleVersionCode=7;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3});
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.template="PROJECT:PocketWreckshop";
            PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.dataCaching=true;
            PlayerSettings.SplashScreen.show=false;
        }
        [MenuItem("Pocket Wreckshop/Build/WebGL")]
        public static void BuildWebGL()=>Build(BuildTarget.WebGL,"Builds/WebGL");
        [MenuItem("Pocket Wreckshop/Build/Android APK")]
        public static void BuildAndroid()=>Build(BuildTarget.Android,"Builds/Android/PocketWreckshop.apk");
        static void Build(BuildTarget target,string output)
        {
            ConfigurePlayer();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},target=target,locationPathName=output,options=BuildOptions.None});
            if(result.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException(target+" build failed: "+result.summary.result+"; "+result.summary.totalErrors+" errors.");
            Debug.Log(target+" BUILD SUCCEEDED: "+Path.GetFullPath(output));
        }
    }
}
