using UnityEditor;
using UnityEngine;

namespace SBS
{
    public class ConfigurationWindow : EditorWindow
    {
        [MenuItem("Assets/Sprite Baking Studio/Configuration")]
        public static void Init()
        {
            ConfigurationWindow window = GetWindow<ConfigurationWindow>("Configuration");
            window.Show();
        }

        void OnGUI()
        {
            SpriteBakingStudio studio = FindObjectOfType<SpriteBakingStudio>();
            if (studio == null)
            {
                EditorGUILayout.HelpBox("SpriteBakingStudio object needed", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();
            studio.folding = EditorGUILayout.Toggle(Global.FOLDING_KEY, studio.folding);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(Global.FOLDING_KEY, studio.folding);

                if (Selection.activeGameObject == studio.gameObject)
                    EditorUtility.SetDirty(studio);
            }
        }
    }
}
