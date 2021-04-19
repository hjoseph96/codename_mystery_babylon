using UnityEngine;
using UnityEngine.UI;


public class HealthPoint : MonoBehaviour
{
    public bool IsFilled => _fill.enabled;

    [SerializeField] private Image _fill;

    public void Init(Sprite fillSprite)
    {
        _fill.sprite = fillSprite;
    }

    public void Fill()
    {
        _fill.enabled = true;
    }

    public void Clear()
    {
        _fill.enabled = false;
    }
}
