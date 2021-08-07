using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Lamp : MonoBehaviour
{
    [InfoBox("The first SpriteRenderer should be hte Lightbulb sprite")]

    [Header("Lightbulb Sprite")]
    [SerializeField] private SpriteRenderer _lightbulb;
    
    [Header("On and Off Sprites")]
    [SerializeField] private Sprite _onSprite;
    [SerializeField] private Sprite _offSprite;

    private bool _isLightOn;
    private Light2D _light;
    
    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponentInChildren<Light2D>();
    }

    [Button("Set Light On")]
    private void TurnOn()
    {
        _isLightOn = true;

        if (_light == null)
            _light = GetComponentInChildren<Light2D>();

        _light.enabled = true;

        if (_lightbulb == null)
            _lightbulb = GetComponentInChildren<SpriteRenderer>();

        _lightbulb.sprite = _onSprite;
    }

    [Button("Set Light Off")]
    private void TurnOff()
    {
        _isLightOn = false;

        if (_light == null)
            _light = GetComponentInChildren<Light2D>();

        _light.enabled = false;

        if (_lightbulb == null)
            _lightbulb = GetComponentInChildren<SpriteRenderer>();

        _lightbulb.sprite = _offSprite;
    }
}
