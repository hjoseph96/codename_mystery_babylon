using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class ScriptableItem : SerializedScriptableObject
{
    [PreviewField]
    public Sprite Icon;

    public string Name;
    public ItemType ItemType;

    public abstract Item GetItem();

    private void OnValidate()
    {
        var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        var fileName = Path.GetFileNameWithoutExtension(assetPath);

        if (string.IsNullOrEmpty(Name))
        {
            Name = fileName;
        }
        else if (fileName != Name)
        {
            AssetDatabase.RenameAsset(assetPath, Name + ".asset");
        }
    }
}