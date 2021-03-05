using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;

[Serializable]
public class ItemData
{
    public string Filename;
    public string ItemName;
    public string ItemType;

    public string Description;
    public Dictionary<string, uint> Pricing = new Dictionary<string, uint>();
    public Dictionary<string, string> Icon = new Dictionary<string, string>();

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
}