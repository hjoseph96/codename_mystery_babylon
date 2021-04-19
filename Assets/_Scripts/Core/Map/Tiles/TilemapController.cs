using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
    using UnityEditor;
#endif

using Sirenix.OdinInspector;

public class TilemapController : SerializedMonoBehaviour
{
    public string Name;
    public TilemapRenderer Renderer;
    public TilemapCollider2D Collider;
    private string _startingLayer;

 
    public static TilemapController AddTilemapController(GameObject tilemapGameObject)
    {
        var controller = tilemapGameObject.AddComponent<TilemapController>();
        controller.Name = tilemapGameObject.name;
        
        controller.Collider = tilemapGameObject.GetComponent<TilemapCollider2D>();
        controller.Renderer = tilemapGameObject.GetComponent<TilemapRenderer>();

        #if UNITY_EDITOR
            EditorUtility.SetDirty(controller);
        #endif

        return controller;
    }

    private void Awake() => _startingLayer = Renderer.sortingLayerName;

    public void ApplyChanges(TileLayerRule rule)
    {
        if (!rule.IsColliderEnabled && Collider != null)
            DisableCollider();
        
        SetSortingLayer(rule.SortingLayer);
    }

    public void Revert() {
        if (Collider != null)
            EnableCollider();
        SetSortingLayer(_startingLayer);
    }
    
    private void SetSortingLayer(string sortingLayerName) => Renderer.sortingLayerName = sortingLayerName;


    public void DisableCollider() => Collider.enabled = false;
    public void EnableCollider() => Collider.enabled = true;
}
