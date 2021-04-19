using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class TileLayerManipulator : SerializedMonoBehaviour
{
    [SerializeField] private TilemapManager _tilemapManager;
    private bool IsTilemapManagerSet => _tilemapManager != null;
    [ValueDropdown("GetTilemapControllerList"), ShowIf("IsTilemapManagerSet"), SerializeField] private string _selectedTileLayer;

    [SerializeField, ShowIf("IsTilemapManagerSet")] private bool colliderEnabled = false;
    [SerializeField, ShowIf("IsTilemapManagerSet")] private string sortingLayer;
    [SerializeField, ShowIf("IsTilemapManagerSet")] private List<Collider2D> _collidersToEnable;

    [InfoBox("Tile Layer Rules will be applied when Player enters trigger")]
    [Button("Add Tile Layer Rule")]
    private void AddTileLayerRule()
    {
        TileLayerRule rule = new TileLayerRule(colliderEnabled, sortingLayer, _collidersToEnable);

        var tileLayer = _tilemapManager.TileLayers.First((tileLayer) => tileLayer.Name == _selectedTileLayer);

        Debug.Log($"Tile Layer: {tileLayer}");
        UponEntryChanges[tileLayer] = rule;
    }

    public Dictionary<TilemapController, TileLayerRule> UponEntryChanges = new Dictionary<TilemapController, TileLayerRule>();

    private void OnTriggerEnter2D()
    {
        Debug.Log("Enter");
        AlterTileLayers();
    }

    private void OnTriggerExit2D()
    {
        Debug.Log("Exit");
        RevertTileLayers();
    }

    private void AlterTileLayers()
    {
        foreach(var entry in UponEntryChanges)
        {
            entry.Key.ApplyChanges(entry.Value);
            foreach (var collider in entry.Value.Obstacles)
                collider.enabled = true;
        }
    }

    private void RevertTileLayers()
    {
        foreach(var entry in UponEntryChanges)
        {
            entry.Key.Revert();

            foreach (var collider in entry.Value.Obstacles)
                collider.enabled = false;
        }
    }

    private List<string> GetTilemapControllerList() 
    {
        var names = new List<string>();
        
        foreach(var tileLayer in _tilemapManager.TileLayers)
            names.Add(tileLayer.Name);
        
        return names;
    }
}
