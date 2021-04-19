using System;
using System.Collections;
using Animancer;
using UnityEngine;

public class ImageFlash : MonoBehaviour
{
    [Header("Flash settings")] 
    [SerializeField] private Color _flashColor;
    [SerializeField] private float _flashInterval;
    [SerializeField] private float _flashDuration;
    [SerializeField] private bool _startOnAwake;

    private Color _baseColor;

    protected virtual void Awake()
    {
        if (_startOnAwake)
            Init();
    }

    public void Init()
    {
        _baseColor = GetColor();
        StartCoroutine(FlashCoroutine());
    }

    protected virtual Color GetColor()
    {
        throw new NotImplementedException();
    }

    protected virtual void SetColor(Color color)
    {
        throw new NotImplementedException();
    }

    private IEnumerator FlashCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_flashInterval);
            var starTime = Time.time;
            while (true)
            {
                var t = (Time.time - starTime) / _flashDuration * 2f;
                if (t < 2)
                {
                    if (t < 1)
                        SetColor(Color.Lerp(_baseColor, _flashColor, Ease(t)));
                    else
                        SetColor(Color.Lerp(_flashColor, _baseColor, Ease(t - 1)));
                }
                else
                {
                    SetColor(_baseColor);
                    break;
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }

    private float Ease(float t)
    {
        return Easing.Quadratic.InOut(0, 1, t);
    }
}