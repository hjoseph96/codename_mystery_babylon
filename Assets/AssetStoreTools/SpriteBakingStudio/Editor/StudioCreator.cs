using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBS
{
    public class StudioCreator : EditorWindow
    {
        private string studioName;

        [MenuItem("Assets/Sprite Baking Studio/Create New Studio")]
        public static void Init()
        {
            StudioCreator window = GetWindow<StudioCreator>("Studio Creator");
            window.Show();
            window.studioName = "NewStudio";
        }

        void OnGUI()
        {
            studioName = EditorGUILayout.TextField("Studio Name", studioName);
        
            if(GUILayout.Button("Create"))
                CheckAndCreateStudio();
        }

        protected void CheckAndCreateStudio()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Cannot create scenes while in play mode. Exit play mode first.");
                return;
            }

            if (string.IsNullOrEmpty(studioName))
            {
                Debug.LogWarning("Please enter a scene name before creating a studio.");
                return;
            }

            Scene currentActiveStudio = SceneManager.GetActiveScene();

            if (currentActiveStudio.isDirty)
            {
                string title = currentActiveStudio.name + " Has Been Modified";
                string message = "Do you want to save the changes you made to " + currentActiveStudio.path + "?\nChanges will be lost if you don't save them.";
                int option = EditorUtility.DisplayDialogComplex(title, message, "Save", "Don't Save", "Cancel");

                if (option == 0)
                {
                    EditorSceneManager.SaveScene(currentActiveStudio);
                }
                else if (option == 2)
                {
                    return;
                }
            }
        
            CreateScene();
        }

        protected void CreateScene()
        {
            string[] result = AssetDatabase.FindAssets("_StudioTemplate");

            if (result.Length > 0)
            {
                string newScenePath = "Assets/" + studioName + ".unity";
                AssetDatabase.CopyAsset(AssetDatabase.GUIDToAssetPath(result[0]), newScenePath);
                EditorSceneManager.OpenScene(newScenePath, OpenSceneMode.Single);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error",
                    "The scene _StudioTemplate was not found in SpriteBakingStudio/Template folder. This scene is required by the New Studio Creator.",
                    "OK");
            }
        }
    }
}