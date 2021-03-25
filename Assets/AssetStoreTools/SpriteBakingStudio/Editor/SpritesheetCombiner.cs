using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace SBS
{
    public class SpritesheetCombiner
    {
        private class Sprite
        {
            public Texture2D tex;
            public string name;
            public Vector2 pivot;

            public Sprite(Texture2D tex, string name, Vector2 pivot)
            {
                this.tex = tex;
                this.name = name;
                this.pivot = pivot;
            }
        }

        [MenuItem("Assets/Sprite Baking Studio/Combine Spritesheets/Only Sprite Name")]
        private static void Combine_OnlySpriteName()
        {
            Combine(true);
        }

        [MenuItem("Assets/Sprite Baking Studio/Combine Spritesheets/File Name + Sprite Name")]
        private static void Combine_FileNamePlusSpriteName()
        {
            Combine(false);
        }

        private static void Combine(bool onlySpriteName)
        {
            if (Selection.objects.Length < 2)
                return;

            List<Sprite> sprites = new List<Sprite>();
            List<Texture2D> textures = new List<Texture2D>();

            foreach (Object obj in Selection.objects)
            {
                if (!EditorUtility.IsPersistent(obj))
                    continue;

                string assetPath = AssetDatabase.GetAssetPath(obj);
                
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                    continue;

                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath);

                Texture2D spritesheetTex = obj as Texture2D;
                if (spritesheetTex == null)
                    continue;

                Color[] originalColors = spritesheetTex.GetPixels();

                for (int metai = 0; metai < importer.spritesheet.Length; ++metai)
                {
                    SpriteMetaData metaData = importer.spritesheet[metai];

                    int spriteWidth = (int)metaData.rect.width;
                    int spriteHeight = (int)metaData.rect.height;

                    Color[] resultColors = new Color[spriteWidth * spriteHeight];

                    for (int y = 0; y < spriteHeight; y++)
                    {
                        for (int x = 0; x < spriteWidth; x++)
                        {
                            int index = ((int)metaData.rect.y + y) * spritesheetTex.width + ((int)metaData.rect.x + x);
                            Color color = originalColors[index];
                            resultColors[y * spriteWidth + x] = color;
                        }
                    }

                    Texture2D spriteTex = new Texture2D(spriteWidth, spriteHeight, TextureFormat.ARGB32, false);
                    spriteTex.SetPixels(resultColors);

                    string spriteName = onlySpriteName ? metaData.name : spritesheetTex.name + "_" + metaData.name;
                    Sprite sprite = new Sprite(spriteTex, spriteName, metaData.pivot);
                    sprites.Add(sprite);
                    textures.Add(spriteTex);
                }
            }

            Texture2D newSpritesheet = new Texture2D(8192, 8192, TextureFormat.ARGB32, false);
            Rect[] texRects = newSpritesheet.PackTextures(textures.ToArray(), 2, 8192);
            for (int i = 0; i < sprites.Count; i++)
            {
                Texture2D tex = sprites[i].tex;
                float newX = texRects[i].x * newSpritesheet.width;
                float newY = texRects[i].y * newSpritesheet.height;
                texRects[i] = new Rect(newX, newY, (float)tex.width, (float)tex.height);
            }

            string filePath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            string dirPath = filePath.Remove(filePath.LastIndexOf('/'));
            string fileName = "CombinedSpritesheet_" + StudioUtility.MakeDateTimeString();
            filePath = TextureUtils.SaveTexture(dirPath, fileName, newSpritesheet);
            AssetDatabase.ImportAsset(filePath);

            TextureImporter texImporter = (TextureImporter)AssetImporter.GetAtPath(filePath);
            if (texImporter != null)
            {
                texImporter.textureType = TextureImporterType.Sprite;
                texImporter.spriteImportMode = SpriteImportMode.Multiple;
                texImporter.maxTextureSize = 4096;

                int texCount = sprites.Count;
                SpriteMetaData[] metaData = new SpriteMetaData[texCount];
                for (int i = 0; i < texCount; i++)
                {
                    metaData[i].name = sprites[i].name;
                    metaData[i].rect = texRects[i];
                    metaData[i].alignment = (int)SpriteAlignment.Custom;
                    metaData[i].pivot = sprites[i].pivot;
                }
                texImporter.spritesheet = metaData;

                AssetDatabase.ImportAsset(filePath);
            }

            foreach (Object obj in Selection.objects)
            {
                if (!EditorUtility.IsPersistent(obj))
                    continue;

                string assetPath = AssetDatabase.GetAssetPath(obj);

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
                if (importer == null)
                    continue;

                importer.isReadable = false;
                AssetDatabase.ImportAsset(assetPath);
            }
        }

        [MenuItem("Assets/Sprite Baking Studio/Combine Spritesheets/Only Sprite Name", true)]
        private static bool Validate_OnlySpriteName()
        {
            return Validate();
        }

        [MenuItem("Assets/Sprite Baking Studio/Combine Spritesheets/File Name + Sprite Name", true)]
        private static bool Validate_FileNamePlusSpriteName()
        {
            return Validate();
        }

        private static bool Validate()
        {
            if (Selection.objects.Length < 2)
                return false;

            foreach (Object obj in Selection.objects)
            {
                if (!EditorUtility.IsPersistent(obj))
                    return false;
                if (!(obj is Texture2D))
                    return false;
            }

            return true;
        }
    }
}
