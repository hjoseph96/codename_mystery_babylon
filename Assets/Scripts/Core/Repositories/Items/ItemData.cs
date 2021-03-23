using System;
using System.IO;
using System.Collections.Generic;
 
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ItemData
{
    public string Filename;
    public string ItemName;
    public string ItemType;

    public string Description;
    public Dictionary<string, uint> Pricing = new Dictionary<string, uint>();
    public Dictionary<string, string> Icon = new Dictionary<string, string>();

    #if UNITY_EDITOR
    public void AssignItemFields(ScriptableItem source)
    {
        Filename = source.name;
        ItemName = source.Name;
        Description = source.Description;
        ItemType    = source.ItemType.ToString();
        
        Pricing.Add("Cost", source.Cost);
        Pricing.Add("SaleValue", source.SaleValue);

        var path = AssetDatabase.GetAssetPath(source.Icon);
        path = Path.ChangeExtension(path, ".spriteatlas");
        
        Icon.Add("AtlasPath", path);
        Icon.Add("SpriteName", source.Icon.name);
    }
    #endif
}