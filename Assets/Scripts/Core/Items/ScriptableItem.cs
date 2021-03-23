using System;
using System.IO;
using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.U2D;
 #if UNITY_EDITOR
using UnityEditor;
#endif
public abstract class ScriptableItem : SerializedScriptableObject
{
    [FoldoutGroup("Basic Properties"), PreviewField]
    public Sprite Icon;

    [FoldoutGroup("Basic Properties")] public string Name;
    [FoldoutGroup("Basic Properties")] public ItemType ItemType;

    [FoldoutGroup("Descriptions"), MultiLineProperty(5)]
    public string Description;

    public uint Cost;
    public uint SaleValue;



    public abstract Item GetItem();

    #if UNITY_EDITOR
    public void SetFromItemData(ItemData item)
    {
        Name = item.ItemName;
        Description = item.Description;
        ItemType = (ItemType)Enum.Parse(typeof(ItemType), item.ItemType);
        
        Cost = item.Pricing["Cost"];
        SaleValue = item.Pricing["SaleValue"];
        
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(item.Icon["AtlasPath"]);
        Icon = atlas.GetSprite(item.Icon["SpriteName"]);
    }

    private void OnValidate()
    {
        var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        var fileName = Path.GetFileNameWithoutExtension(assetPath);

        if (string.IsNullOrEmpty(Name))
            Name = fileName;
    }
    #endif
}