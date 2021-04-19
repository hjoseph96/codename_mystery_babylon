using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GridAnimatedTile : SerializedMonoBehaviour
{
    protected readonly Dictionary<string, List<AnimatedTileConfigurationInfluenceZone>> tileStates = new Dictionary<string, List<AnimatedTileConfigurationInfluenceZone>>();

    protected Animator animator;
    protected Collider2D tileCollider;
    protected SpriteRenderer spriteRenderer;

    protected List<AnimatedTrigger> triggers = new List<AnimatedTrigger>();

    public bool IsTriggered => triggers.Any(trigger => trigger.IsWithinTrigger);

    private void Awake()
    {
        tileCollider    = GetComponent<BoxCollider2D>();
        animator        = GetComponent<Animator>();
        spriteRenderer  = GetComponent<SpriteRenderer>();

        foreach (var tileGroup in GetComponentsInChildren<AnimatedTileGroup>())
            tileStates[tileGroup.Name] = tileGroup.InfluenceZones;

        foreach (var trigger in GetComponentsInChildren<AnimatedTrigger>())
            triggers.Add(trigger);
    }    

    protected void SetTileInfluences(string stateName)
    {
        var influenceZones = tileStates[stateName];
        foreach (var zone in influenceZones)
            zone.Apply();
    }
}
