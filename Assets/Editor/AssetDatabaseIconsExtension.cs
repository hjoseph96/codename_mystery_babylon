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

        var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
        if (monoScript != null)
        {
            var type = monoScript.GetClass();
            if (type != typeof(NavigationTile) && type != typeof(ScriptableWeapon))
                return;

            var csIcon = EditorGUIUtility.IconContent("cs Script Icon");
            rect.height *= 0.85f;
            GUI.DrawTexture(rect, csIcon.image);
            return;
        }

        Sprite sprite = null;
        if (scriptableObject is NavigationTile navTile && navTile.sprite != null)
        {
            sprite = navTile.sprite;
        }

        if (scriptableObject is ScriptableWeapon weapon && weapon.Icon != null)
        {
            sprite = weapon.Icon;
        }

        if (sprite == null)
            return;

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

        const float sizeReduction = 0.5f;
        var newWidth = spriteRect.width * factor * sizeReduction;
        var newHeight = spriteRect.height * factor * sizeReduction;
        rect = new Rect(rect.x + rect.width / 2 - newWidth / 2, rect.y + rect.height / 2 - newHeight / 2, newWidth, newHeight);

        GUI.DrawTextureWithTexCoords(rect, tex,
            new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));
    }
}