using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Portrait : MonoBehaviour
{
    public Sprite Default;

    [InfoBox("ImageColor is used to shade/tint the Portrait when it's assign in the GUI.")]
    public Color ImageColor;

    public RuntimeAnimatorController _animationController;

    private bool _isAnimated;
    [ShowInInspector] public bool IsAnimated { get => _isAnimated; }


    // Start is called before the first frame update
    void Start()
    {
        _animationController = GetComponent<RuntimeAnimatorController>();
        if (_animationController == null)
            _isAnimated = false;
        else
            _isAnimated = true;
    }
}
