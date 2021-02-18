using UnityEditor;
using UnityEngine;
using System.IO;

namespace SS.TwoD
{
    public class SpriteImageGeneratorWindow : EditorWindow
    {
        public string imageName = string.Empty;
        public string imagePath = "Textures/Gen";
        public string scenePath = "Scenes/Gen";
        public string spritePackingTag = "Image";

        [MenuItem("SS/TwoD/Generate Still Images")]
        public static void ShowWindow()
        {
            SpriteImageGeneratorWindow win = (SpriteImageGeneratorWindow)(EditorWindow.GetWindow(typeof(SpriteImageGeneratorWindow)));
            win.minSize = new Vector2(500, 350);

            win.imageName = string.Empty;
            win.imagePath = EditorPrefs.GetString("ss2d_spritesPath", "Textures/Gen");
            win.scenePath = EditorPrefs.GetString("ss2d_scenePath", "Scenes/Gen");
            win.spritePackingTag = EditorPrefs.GetString("ss2d_imagePackingTag", "Image");
        }

        void OnGUI()
        {
            GUILayout.Label("Sprites Generator", EditorStyles.boldLabel);
            imageName = EditorGUILayout.TextField ("Image name", imageName);
            imagePath = EditorGUILayout.TextField ("Image path", imagePath);
            scenePath = EditorGUILayout.TextField ("Scene path", scenePath);
            spritePackingTag = EditorGUILayout.TextField("Sprite packing tag", spritePackingTag);

            if (GUILayout.Button("Create"))
            {
                SaveEditorPrefs();
                if (!string.IsNullOrEmpty(imageName))
                {
                    CreateScene();
                    SetupGenerator();
                    SS.Tools.SceneTools.SaveScene();
                    this.Close();
                }
                else
                {
                    Debug.Log("You have to input an unique name to 'Image Name'");
                }
            }
        }

        void SaveEditorPrefs()
        {
            EditorPrefs.SetString("ss2d_spritesPath", imagePath);
            EditorPrefs.SetString("ss2d_scenePath", scenePath);
            EditorPrefs.SetString("ss2d_imagePackingTag", spritePackingTag);
        }

        void CreateScene()
        {
			string targetPath = SS.IO.FileUtil.CopyFromTemplate("_GeneratorImageTemplate.unity", imageName, ".unity", scenePath);

            SS.Tools.SceneTools.OpenScene(targetPath);
        }

        void SetupGenerator()
        {
            ImageGenerator ig = FindObjectOfType<ImageGenerator>();
            ig.imageName = imageName;
            ig.imagePath = imagePath;
            ig.spritePackingTag = spritePackingTag;
        }
    }
}