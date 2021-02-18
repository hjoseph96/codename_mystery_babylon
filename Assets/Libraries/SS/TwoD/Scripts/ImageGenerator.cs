using UnityEngine;
using System.Collections;
using System.IO;
using SS.Tools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SS.TwoD
{
    [System.Serializable]
    public class Size : System.Object
    {
        public int height;
    }

    public class ImageGenerator : MonoBehaviour
    {
        [SerializeField]
        public string imageName = "Image";

        [SerializeField]
        public string imagePath = "Textures/Gen";
        
        [SerializeField]
        public string spritePackingTag = "Image";

        [SerializeField]
        ScreenshotTools.RenderQuality m_RenderQuality = ScreenshotTools.RenderQuality.High;
        
        [SerializeField]
        TextureTools.ImageFilterMode m_FilterMode = TextureTools.ImageFilterMode.Average;

        [SerializeField]
        Size[] m_Sizes;

        int m_Index;

        IEnumerator Start()
        {
            bool validated = false;

            string outputImagePath = Path.Combine(imagePath, imageName);
            string path = Path.Combine(Application.dataPath, outputImagePath);
            string shortPath = Path.Combine("Assets", outputImagePath);

#if UNITY_EDITOR
            if (Directory.Exists(path))
            {
                if (EditorUtility.DisplayDialog("Warning!", "The output folder is exist. Do you really want to overrite?", "Yes", "No"))
                {
                    validated = true;
                }
            }
            else
            {
                validated = true;
            }
#endif
            if (!validated)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                yield break;
            }

            ScreenshotTools.renderQuality = m_RenderQuality;

            while (m_Index < m_Sizes.Length)
            {
                int h = m_Sizes[m_Index].height;
                int w = Mathf.RoundToInt((float)h * Screen.width / Screen.height);

                if (w > 0 && h > 0)
                {
                    yield return new WaitForEndOfFrame();

                    Texture2D screenShotTex = ScreenshotTools.ScreenShot();
                    Texture2D resizedTex = TextureTools.ResizeTexture(screenShotTex, m_FilterMode, (float)h / screenShotTex.height);
                    byte[] bytes = resizedTex.EncodeToPNG();

                    Destroy(screenShotTex);
                    Destroy(resizedTex);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string fileName = imageName + "_" + w.ToString() + "x" + h.ToString() + ".png";
                    string filePath = Path.Combine(path, fileName);
                    string shortFilePath = Path.Combine(shortPath, fileName);
                    File.WriteAllBytes(filePath, bytes);

#if UNITY_EDITOR
                    AssetDatabase.Refresh();
                    SettingSprite(shortFilePath);
#endif
                    
                    Debug.Log("Generated: " + filePath);
                }
                m_Index++;

                yield return 0;
            }

            Debug.Log("Done!");

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

#if UNITY_EDITOR
        void SettingSprite(string path)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.mipmapEnabled = false;
                textureImporter.spritePackingTag = spritePackingTag;
                textureImporter.SaveAndReimport();
            }
        }
#endif
    }
}