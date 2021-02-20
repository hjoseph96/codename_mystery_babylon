using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class ScriptableItem : SerializedScriptableObject
{
    [FoldoutGroup("Basic Properties"), PreviewField]
    public Sprite Icon;

    [FoldoutGroup("Basic Properties")] public string Name;
    [FoldoutGroup("Basic Properties")] public ItemType ItemType;

    [FoldoutGroup("Descriptions"), MultiLineProperty(5)]
    public string Description;

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