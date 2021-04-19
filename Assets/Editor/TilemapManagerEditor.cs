using System;
using System.Linq;
using System.Reflection;

using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditorInternal;

[CustomEditor( typeof( TilemapManager ) )]
public class TilemapManagerEditor : Editor
{
    private List<TilemapManager> _tilemapManagers = new List<TilemapManager>();

    void OnEnable() => ValidateTilemapManagers();
    
    void OnValidate() => ValidateTilemapManagers();

    private void OnSceneGUI() => ValidateTilemapManagers();
    
    private void ValidateTilemapManagers()
    {
        Debug.Log("Validating TilemapManagers");

        var objs = GameObject.FindGameObjectsWithTag("TilemapManager");
        
        if (objs.Length > _tilemapManagers.Count)
        for (int i = 0; i < objs.Length; i++)
            _tilemapManagers.Add(objs[i].GetComponent<TilemapManager>());

        var sortingLayerNames = GetSortingLayerNames();
        for (int i = 0; i < _tilemapManagers.Count; i++)
        {
            TilemapManager tilemapManager = _tilemapManagers[i];

            tilemapManager.TileLayers = tilemapManager.TileLayers.Where((controller) => controller != null).ToList();
            var tilemapControllers = tilemapManager.TilemapControllers();


            if (tilemapManager.TileLayers.Count != tilemapControllers.Count)
            {
                tilemapManager.TileLayers = tilemapControllers;
                
                if (tilemapManager.SortingLayers.Count != sortingLayerNames.Count)
                    tilemapManager.SortingLayers = sortingLayerNames;

                EditorUtility.SetDirty(tilemapManager);
            }
            
            Debug.Log($"TilemapManager '{tilemapManager.name}' has {tilemapControllers.Count} tile layers.");
        }

    }

    private List<string> GetSortingLayerNames()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        var listOfSortingLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);

        return new List<string>(listOfSortingLayerNames); 
    }
}