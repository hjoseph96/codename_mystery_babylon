using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AnimatedPortrait : MonoBehaviour
{
    public string Name;

    public static Dictionary<string, int> AnimationsAsHashes = new Dictionary<string, int>
    {
        { "neutral", -1541085712 },
        { "talking", -1717018742 }
    };

    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _nightModeColor;

    private Animator _animator;
    private EyeController _eyes;

    private Image _bodyImg;
    private Image _eyesImg;

    // Start is called before the first frame update
    private void Awake()
    {
        _animator   = GetComponent<Animator>();
        _bodyImg    = GetComponent<Image>();

        _eyes       = GetComponentInChildren<EyeController>();
        _eyesImg    = _eyes.GetComponent<Image>();
    }

    public void SetNeutral() => _animator.Play("neutral");
    
    public void Talk() => _animator.Play("talking");

    public void SetDefaultColor()
    {
        if (_eyes == null)
            _eyes = GetComponentInChildren<EyeController>();

        _bodyImg = GetComponent<Image>();
        _eyesImg = _eyes.GetComponent<Image>();

        _bodyImg.color = _defaultColor;
        _eyesImg.color = _defaultColor;
    }
    public void SetNightModeColor()
    {
        if (_eyes == null)
            _eyes = GetComponentInChildren<EyeController>();

        _bodyImg = GetComponent<Image>();
        _eyesImg = _eyes.GetComponent<Image>();

        _bodyImg.color  = _nightModeColor;
        _eyesImg.color  = _nightModeColor;
    }
}
