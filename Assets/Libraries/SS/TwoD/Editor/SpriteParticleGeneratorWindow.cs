using UnityEditor;
using UnityEngine;
using System.IO;

namespace SS.TwoD
{
    public class SpriteParticleGeneratorWindow : EditorWindow
    {
        public string characterName = string.Empty;
        public string spritesPath = "Textures/Gen";
        public string scenePath = "Scenes/Gen";
        public string prefabPath = "Prefabs/Gen";
        public string animationPath = "Animations/Gen";
        public string bodyRendererPath = "Renderer";
        public string spritePackingTag = "Effect";
        public bool genAnimClip = false;
        public Object characterModel = null;
        public SpriteGeneratorManager.Directions maxDirection;
        public SpriteGeneratorManager.GamePlane gamePlane;
        
        string shadowRendererPath = "";
        string colorRendererPath = "";
        bool includeAI = false;
        bool movable = false;
        SpriteGeneratorManager.ShadowType shadowType = SpriteGeneratorManager.ShadowType.NoShadow;
        GameObject outputPrefab;

        [MenuItem("SS/TwoD/Generate Effect Sprites")]
        public static void ShowWindow()
        {
            SpriteParticleGeneratorWindow win = (SpriteParticleGeneratorWindow)(EditorWindow.GetWindow(typeof(SpriteParticleGeneratorWindow)));
            win.minSize = new Vector2(500, 350);

            win.characterName = string.Empty;
            win.spritesPath = EditorPrefs.GetString("ss2d_spritesPath", "Textures/Gen");
            win.scenePath = EditorPrefs.GetString("ss2d_scenePath", "Scenes/Gen");
            win.prefabPath = EditorPrefs.GetString("ss2d_prefabPath", "Prefabs/Gen");
            win.animationPath = EditorPrefs.GetString("ss2d_animationPath", "Animations/Gen");
            win.bodyRendererPath = EditorPrefs.GetString("ss2d_rendererBodyRelativePath_ps", "Renderer");
            win.spritePackingTag = EditorPrefs.GetString("ss2d_spritePackingTag_ps", "Effect");
            win.genAnimClip = EditorPrefs.GetBool("ss2d_genAnimClip", false);
            win.maxDirection = (SpriteGeneratorManager.Directions)EditorPrefs.GetInt("ss2d_maxDirection", 12);
            win.gamePlane = (SpriteGeneratorManager.GamePlane)EditorPrefs.GetInt("ss2d_gamePlane", 0);
            win.characterModel = null;
        }

        void OnGUI()
        {
            GUILayout.Label("Sprites Generator", EditorStyles.boldLabel);
            characterName = EditorGUILayout.TextField ("Effect name", characterName);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Effect prefab");
            characterModel = EditorGUILayout.ObjectField(characterModel, typeof(Object), false);
            EditorGUILayout.EndHorizontal();

            maxDirection = (SpriteGeneratorManager.Directions)EditorGUILayout.EnumPopup("Max Direction", maxDirection);
            gamePlane = (SpriteGeneratorManager.GamePlane)EditorGUILayout.EnumPopup("Game Plane", gamePlane);

            spritesPath = EditorGUILayout.TextField ("Sprites path", spritesPath);
            scenePath = EditorGUILayout.TextField ("Scene path", scenePath);
            prefabPath = EditorGUILayout.TextField("Prefab path", prefabPath);
            spritePackingTag = EditorGUILayout.TextField("Sprite packing tag", spritePackingTag);

            bodyRendererPath = EditorGUILayout.TextField("Renderer Child Path", bodyRendererPath);

            genAnimClip = EditorGUILayout.BeginToggleGroup("Generate Animation Clip", genAnimClip);
            animationPath = EditorGUILayout.TextField("Animation Path", animationPath);
            EditorGUILayout.EndToggleGroup();

            if (GUILayout.Button("Create"))
            {
                SaveEditorPrefs();
                if (!string.IsNullOrEmpty(characterName) && characterModel != null)
                {
                    CreateScene();
                    CreatePrefab();
                    SetupGenerator();
                    SS.Tools.SceneTools.SaveScene();
                    this.Close();
                }
                else
                {
                    Debug.Log("You have to input an unique name to 'Effect Name', and drag your effect prefab to 'Effect prefab'");
                }
            }
        }

        void SaveEditorPrefs()
        {
            EditorPrefs.SetString("ss2d_spritesPath", spritesPath);
            EditorPrefs.SetString("ss2d_scenePath", scenePath);
            EditorPrefs.SetString("ss2d_prefabPath", prefabPath);
            EditorPrefs.SetString("ss2d_animationPath", animationPath);
            EditorPrefs.SetString("ss2d_rendererBodyRelativePath_ps", bodyRendererPath);
            EditorPrefs.SetString("ss2d_spritePackingTag_ps", spritePackingTag);
            EditorPrefs.SetBool("ss2d_genAnimClip", genAnimClip);
            EditorPrefs.SetInt("ss2d_gamePlane", (int)gamePlane);
            EditorPrefs.SetInt("ss2d_maxDirection", (int)maxDirection);
        }

        void CreateScene()
        {
            string targetPath = SS.IO.FileUtil.CopyFromTemplate("_GeneratorPsTemplate.unity", characterName, ".unity", scenePath);

            SS.Tools.SceneTools.OpenScene(targetPath);

            switch (shadowType)
            {
                case SpriteGeneratorManager.ShadowType.RealShadow:
                    break;

                case SpriteGeneratorManager.ShadowType.FakeShadowEllipse:
                case SpriteGeneratorManager.ShadowType.NoShadow:
                    OffShadowOfLights();
                    break;
            }
        }

        void OffShadowOfLights()
        {
            Light[] lights = FindObjectsOfType<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].shadows = LightShadows.None;
            }
        }

        void CreatePrefab()
        {
			outputPrefab = SpriteGeneratorEditorTools.CreatePrefab(includeAI, movable, characterName, prefabPath, true);
        }

        void SetupGenerator()
        {
            SpriteGeneratorManager sgm = FindObjectOfType<SpriteGeneratorManager>();
            sgm.characterName = characterName;
            sgm.spritePackingTag = spritePackingTag;
            sgm.spritePath = spritesPath;
            sgm.spritePrefabs = new GameObject[1];
            sgm.spritePrefabs[0] = outputPrefab;
            sgm.animationPath = (genAnimClip) ? animationPath : string.Empty;
            sgm.bodyRendererPath = bodyRendererPath;
            sgm.shadowRendererPath = shadowRendererPath;
            sgm.colorRendererPath = colorRendererPath;
            sgm.maxDirection = maxDirection;
            sgm.gamePlane = gamePlane;
            sgm.shadowType = shadowType;

            SpriteParticleGenerator spg = FindObjectOfType<SpriteParticleGenerator>();
            spg.particleSystemPrefab = (GameObject)characterModel;
            spg.animationName = characterName;

            ParticleSystem ps = spg.particleSystemPrefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                #if UNITY_5_5_OR_NEWER
                spg.originalAnimationDuration = ps.main.duration;
                spg.animationDuration = ps.main.duration;
                #else
                spg.originalAnimationDuration = ps.duration;
                spg.animationDuration = ps.duration;
                #endif
            }
        }
    }
}