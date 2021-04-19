using UnityEditor;
using UnityEngine;
using System.IO;

namespace SS.TwoD
{
    public class SpriteGeneratorMultiChractersWindow : EditorWindow
    {
        public string characterName = string.Empty;
        public string spritesPath = "Textures/Gen";
        public string scenePath = "Scenes/Gen";
        public string prefabPath = "Prefabs/Gen";
        public string animationPath = "Animations/Gen";
        public string bodyRendererPath = "BodyRenderer";
        public string shadowRendererPath = "ShadowRenderer";
        public string colorRendererPath = "ColorRenderer";
        public string spritePackingTag = "Character";
        public bool includeAI = true;
        public bool movable = true;
        public bool genAnimClip = false;
        public SpriteGeneratorManager.ShadowType shadowType;
        public SpriteGeneratorManager.Directions maxDirection;
        public SpriteGeneratorManager.GamePlane gamePlane;

        GameObject prefab;

        [MenuItem("SS/TwoD/Multi Characters/Setup Window")]
        public static void ShowWindow()
        {
            SpriteGeneratorMultiChractersWindow win = (SpriteGeneratorMultiChractersWindow)(EditorWindow.GetWindow(typeof(SpriteGeneratorMultiChractersWindow)));
            win.minSize = new Vector2(500, 350);

            win.characterName = string.Empty;
            win.spritesPath = EditorPrefs.GetString("ss2d_spritesPath", "Textures/Gen");
            win.scenePath = EditorPrefs.GetString("ss2d_scenePath", "Scenes/Gen");
            win.prefabPath = EditorPrefs.GetString("ss2d_prefabPath", "Prefabs/Gen");
            win.animationPath = EditorPrefs.GetString("ss2d_animationPath", "Animations/Gen");
            win.bodyRendererPath = EditorPrefs.GetString("ss2d_rendererBodyRelativePath", "Body Renderer");
            win.shadowRendererPath = EditorPrefs.GetString("ss2d_rendererShadowRelativePath", "Shadow Renderer");
            win.colorRendererPath = EditorPrefs.GetString("ss2d_rendererColorRelativePath", "Color Renderer");
            win.spritePackingTag = EditorPrefs.GetString("ss2d_spritePackingTag", "Character");
            win.includeAI = EditorPrefs.GetBool("ss2d_includeAI", true);
            win.movable = EditorPrefs.GetBool("ss2d_movable", true);
            win.genAnimClip = EditorPrefs.GetBool("ss2d_genAnimClip", false);
            win.shadowType = (SpriteGeneratorManager.ShadowType)EditorPrefs.GetInt("ss2d_shadowType", 0);
            win.maxDirection = (SpriteGeneratorManager.Directions)EditorPrefs.GetInt("ss2d_maxDirection", 12);
            win.gamePlane = (SpriteGeneratorManager.GamePlane)EditorPrefs.GetInt("ss2d_gamePlane", 0);
        }

        void OnGUI()
        {
            GUILayout.Label("Sprites Generator", EditorStyles.boldLabel);
            characterName = EditorGUILayout.TextField ("Character prefix", characterName);

            maxDirection = (SpriteGeneratorManager.Directions)EditorGUILayout.EnumPopup("Max Direction", maxDirection);
            shadowType = (SpriteGeneratorManager.ShadowType)EditorGUILayout.EnumPopup("Shadow Type", shadowType);
            gamePlane = (SpriteGeneratorManager.GamePlane)EditorGUILayout.EnumPopup("Game Plane", gamePlane);

            spritesPath = EditorGUILayout.TextField ("Sprites path", spritesPath);
            scenePath = EditorGUILayout.TextField ("Scene path", scenePath);
            prefabPath = EditorGUILayout.TextField("Prefab path", prefabPath);
            spritePackingTag = EditorGUILayout.TextField("Sprite packing tag", spritePackingTag);

            bodyRendererPath = EditorGUILayout.TextField("Body Renderer Child Path", bodyRendererPath);
            shadowRendererPath = EditorGUILayout.TextField("Shadow Renderer Child Path", shadowRendererPath);
            colorRendererPath = EditorGUILayout.TextField("Color Renderer Child Path", colorRendererPath);

            genAnimClip = EditorGUILayout.BeginToggleGroup("Generate Animation Clip", genAnimClip);
            animationPath = EditorGUILayout.TextField("Animation Path", animationPath);
            EditorGUILayout.EndToggleGroup();

            includeAI = EditorGUILayout.BeginToggleGroup("Include AI (for 2.5d RTS game)", includeAI);
            movable = EditorGUILayout.Toggle("Movable", movable);
            EditorGUILayout.EndToggleGroup();

            if (GUILayout.Button("Create"))
            {
                SaveEditorPrefs();
                if (!string.IsNullOrEmpty(characterName))
                {
                    CreateScene();
                    CreateModel();
                    SetupGenerator();
					SetupCharacterManager();
                    SS.Tools.SceneTools.SaveScene();
					this.Close();
                }
                else
                {
                    Debug.Log("You have to input an unique name to 'Character Prefix'");
                }
            }
        }

        void SaveEditorPrefs()
        {
            EditorPrefs.SetString("ss2d_spritesPath", spritesPath);
            EditorPrefs.SetString("ss2d_scenePath", scenePath);
            EditorPrefs.SetString("ss2d_prefabPath", prefabPath);
            EditorPrefs.SetString("ss2d_animationPath", animationPath);
            EditorPrefs.SetString("ss2d_rendererBodyRelativePath", bodyRendererPath);
            EditorPrefs.SetString("ss2d_rendererShadowRelativePath", shadowRendererPath);
            EditorPrefs.SetString("ss2d_rendererColorRelativePath", colorRendererPath);
            EditorPrefs.SetString("ss2d_spritePackingTag", spritePackingTag);
            EditorPrefs.SetBool("ss2d_includeAI", includeAI);
            EditorPrefs.SetBool("ss2d_movable", movable);
            EditorPrefs.SetBool("ss2d_genAnimClip", genAnimClip);
            EditorPrefs.SetInt("ss2d_shadowType", (int)shadowType);
            EditorPrefs.SetInt("ss2d_gamePlane", (int)gamePlane);
            EditorPrefs.SetInt("ss2d_maxDirection", (int)maxDirection);
        }

        void CreateScene()
        {
			string targetPath = SS.IO.FileUtil.CopyFromTemplate("_GeneratorTemplate.unity", characterName, ".unity", scenePath);

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

        void CreateModel()
        {
			string targetPath = SS.IO.FileUtil.CopyFromTemplate("_BaseAnimator.controller", characterName, ".controller", scenePath);

			AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SS.IO.Path.GetRalativePath(targetPath));
        }

        void SetupGenerator()
        {
            SpriteGeneratorManager sgm = FindObjectOfType<SpriteGeneratorManager>();
            sgm.spritePackingTag = spritePackingTag;
            sgm.spritePath = spritesPath;
            sgm.animationPath = (genAnimClip) ? animationPath : string.Empty;
            sgm.bodyRendererPath = bodyRendererPath;
            sgm.shadowRendererPath = shadowRendererPath;
            sgm.colorRendererPath = colorRendererPath;
            sgm.maxDirection = maxDirection;
            sgm.gamePlane = gamePlane;
            sgm.shadowType = shadowType;
        }

		void SetupCharacterManager()
		{
			GameObject go = new GameObject("Character Manager");
			SpriteGeneratorCharacterManager charMan = go.AddComponent<SpriteGeneratorCharacterManager>();

			charMan.prefixName = characterName;
			charMan.includeAI = includeAI;
			charMan.movable = movable;
			charMan.prefabPath = prefabPath;
		}
    }
}