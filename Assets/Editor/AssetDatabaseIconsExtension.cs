using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public static class AssetDatabaseIconsExtension
{
    [DidReloadScripts]
    static AssetDatabaseIconsExtension()
    {
        EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemOnGUI;
        EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
    }

    private static void ProjectWindowItemOnGUI(string guid, Rect rect)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

        if (scriptableObject is NavigationTile tile && tile.sprite != null)
        {
            //Debug.Log(rect);
            var sprite = tile.sprite;
            var spriteRect = sprite.rect;
            var tex = sprite.texture;

            var rectRatio = rect.width / rect.height;
            var spriteRectRatio = spriteRect.width / spriteRect.height;

            float factor;
            if (spriteRectRatio > rectRatio)
            {
                factor = rect.width / spriteRect.width;
            }
            else
            {
                factor = rect.height / spriteRect.height;
            }

            var sizeReduction = 0.5f;
            var newWidth = spriteRect.width * factor * sizeReduction;
            var newHeight = spriteRect.height * factor * sizeReduction;
            rect = new Rect(rect.x + rect.width / 2 - newWidth / 2, rect.y + rect.height / 2 - newHeight / 2, newWidth, newHeight);
            //Debug.Log(rect);

            GUI.DrawTextureWithTexCoords(rect, tex,
                new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));

        }
    }
}