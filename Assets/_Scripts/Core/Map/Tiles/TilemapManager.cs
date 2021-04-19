
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Sirenix.OdinInspector;

[InfoBox("The 'TilemapManager' Tag is REQUIRED!!!", InfoMessageType.Warning)]

[ExecuteInEditMode]
public class TilemapManager : SerializedMonoBehaviour
{

    public List<string> SortingLayers = new List<string>();

    public Dictionary<string, int>  TileLayerIds = new Dictionary<string, int>();

    public List<TilemapController> TileLayers = new List<TilemapController>();


    public List<TilemapController> TilemapControllers()
    {
        List<TilemapController> tilemapControllers = new List<TilemapController>();
        foreach(TilemapRenderer layer in GetComponentsInChildren<TilemapRenderer>())
        {
            var controller = layer.gameObject.GetComponent<TilemapController>();
            
            if (controller == null)
                tilemapControllers.Add(TilemapController.AddTilemapController(layer.gameObject));
            else
                tilemapControllers.Add(controller);
        }

        
        return tilemapControllers;
    }
}

