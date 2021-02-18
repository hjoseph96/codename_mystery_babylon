using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SS.TwoD
{
    public class SpriteGeneratorEditorTools : MonoBehaviour
    {
        [MenuItem("SS/TwoD/Camera Setting/Default 2.5D")]
        public static void Default25D()
        {
            SetupCamera(new Vector3(0, 13.18f, -13.17f), new Vector3(45, 0, 0), new Vector3(0, 6.373f, 0), Vector3.zero);
        }

        [MenuItem("SS/TwoD/Camera Setting/Default 2D")]
        public static void Default2D()
        {
            SetupCamera(new Vector3(0, 0, -13.17f), new Vector3(0, 0, 0), new Vector3(0, 6.373f, 0), Vector3.zero);
        }

        [MenuItem("SS/TwoD/Camera Setting/Default Top-Down")]
        public static void DefaultTopDown()
        {
            SetupCamera(new Vector3(0, 30f, 0), new Vector3(90, 0, 0), new Vector3(0, 0, 1.4f), new Vector3(0, 0, -1.4f));
        }

        [MenuItem("SS/TwoD/Multi Characters/Generate output prefabs")]
        public static void GenerateOutputPrefabs()
        {
            SpriteGeneratorCharacterManager charMan = FindObjectOfType<SpriteGeneratorCharacterManager>();

            if (charMan != null)
            {
                // Prefabs
                GameObject[] temp = charMan.outputPrefabs;
                charMan.outputPrefabs = new GameObject[charMan.characters.Length];
                int maxLength = Mathf.Min(charMan.outputPrefabs.Length, temp.Length);

                for (int i = 0; i < maxLength; i++)
                {
                    charMan.outputPrefabs[i] = temp[i];
                }

                for (int i = maxLength; i < charMan.characters.Length; i++)
                {
                    string characterName = charMan.prefixName + i.ToString().PadLeft(charMan.indexPadLeftZero, '0');
                    charMan.outputPrefabs[i] = SS.TwoD.SpriteGeneratorEditorTools.CreatePrefab(charMan.includeAI, charMan.movable, characterName, charMan.prefabPath, false);
                }

                // Will generate
                bool[] temp2 = charMan.willGenerate;
                charMan.willGenerate = new bool[charMan.characters.Length];
                int maxLength2 = Mathf.Min(charMan.willGenerate.Length, temp2.Length);

                for (int i = 0; i < maxLength2; i++)
                {
                    charMan.willGenerate[i] = temp2[i];
                }

                for (int i = maxLength2; i < charMan.characters.Length; i++)
                {
                    charMan.willGenerate[i] = true;
                }

                SS.Tools.SceneTools.MarkCurrentSceneDirty();

                Debug.Log("Generated output prefabs!");
            }
            else
            {
                Debug.Log("You have to drag SpriteGeneratorCharacterManager to an object");
            }
        }

        static void SetupCamera(Vector3 pos, Vector3 rot, Vector3 topLinePos, Vector3 botLinePos)
        {
            SpriteGeneratorManager genMan = FindObjectOfType<SpriteGeneratorManager>();
            if (genMan == null)
            {
                Debug.Log("The current scene is not a 'Sprite Generator' scene.");
                return;
            }

            if (Camera.main != null)
            {
                Camera.main.transform.localPosition = pos;
                Camera.main.transform.localEulerAngles = rot;
            }

            if (genMan != null)
            {
                genMan.lineHorizontalTop.position = topLinePos;
                genMan.lineHorizontalBottom.position = botLinePos;
            }
        }

        public static GameObject CreatePrefab(bool includeAI, bool movable, string characterName, string prefabPath, bool replaceExistFile)
        {
            string templatePrefabFile = string.Empty;

            if (!includeAI)
            {
                templatePrefabFile = "_SpriteAnimatorTemplate.prefab";
            }
            else
            {
                if (movable)
                {
                    templatePrefabFile = "_MovingCharacterTemplate.prefab";
                }
                else
                {
                    templatePrefabFile = "_StaticCharacterTemplate.prefab";
                }
            }

            string targetPath = SS.IO.FileUtil.CopyFromTemplate(templatePrefabFile, characterName, ".prefab", prefabPath, replaceExistFile);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SS.IO.Path.GetRalativePath(targetPath));

            return prefab;
        }
    }
}
