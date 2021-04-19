using System;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public class AnimatedTrigger : MonoBehaviour
{
    [InfoBox("AnimatedTrigger should have a Collder2D that isTrigger and be parented to a GridAnimatedTile")]

    private bool _isWithinTrigger = false;
    public bool IsWithinTrigger { get => _isWithinTrigger; }

    [HideInInspector] public Action OnLeftTrigger;

    [ValueDropdown("GetTagList")]
    public List<string> tagsToScanFor;
    
    #if UNITY_EDITOR
    private List<string> GetTagList()
    {
        var tagList = new List<string> { "Any" };

        foreach (var tag in InternalEditorUtility.tags)
            tagList.Add(tag);

        return tagList;
    }
    #endif

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tagsToScanFor.Contains("Any"))
        {
            _isWithinTrigger = true;
            return;
        }

        if (tagsToScanFor.Contains(collision.tag))
            _isWithinTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_isWithinTrigger && tagsToScanFor.Contains(collision.tag))
            _isWithinTrigger = false;

        if (OnLeftTrigger != null)
            OnLeftTrigger.Invoke();
    }
}
