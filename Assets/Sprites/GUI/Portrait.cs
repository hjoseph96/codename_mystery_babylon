using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Portrait : MonoBehaviour
{
    public Sprite Default;

    private bool _isAnimated;
    [ShowInInspector] public bool IsAnimated { get => _isAnimated; }

    public RuntimeAnimatorController _animationController;

    // Start is called before the first frame update
    void Start()
    {
        _animationController = GetComponent<RuntimeAnimatorController>();
        if (_animationController == null)
            _isAnimated = false;
        else
            _isAnimated = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
