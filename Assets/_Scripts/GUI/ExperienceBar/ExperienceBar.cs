using System;
using UnityEngine;
using TMPro;


public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private RectTransform _fill;
    [SerializeField] private TextMeshProUGUI _expAmount;

    [SerializeField] private AnimationCurve _fillCurve;
    [SerializeField] private float _fillDuration = 0.67f;
    [SerializeField] private float _fillMax;

    [HideInInspector] public Action OnBarFilled;
    private int _initialExperience, _finalExperience;
    private float _fillStartTime, _fillCurrentDuration;
    private bool _isFilling;


    public void Show(int unitExperience)
    {
        _initialExperience = unitExperience;
        _expAmount.SetText($"{unitExperience}/100");
        FillBar(_initialExperience);

        this.SetActive(true);
    }

    private void Update()
    {
        if (_isFilling)
            Fill();
    }

    private void OnDisable()
    {
        OnBarFilled = null;
    }

    public void StartFilling(int finalExperience)
    {
        _finalExperience = finalExperience;
        _isFilling = true;
        _fillStartTime = Time.time;

        if (_finalExperience > 100)
            _fillCurrentDuration = _fillDuration * _finalExperience / 100f;
        else
            _fillCurrentDuration = _fillDuration;
    }

    private void FillBar(float exp)
    {
        _fill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, exp / 100f * _fillMax);
    }

    private void Fill()
    {
        var t = (Time.time - _fillStartTime) / _fillCurrentDuration;
        var currentFIll = Mathf.Lerp(_initialExperience, _finalExperience, _fillCurve.Evaluate(t)) % 100;

        // TODO: Level Up display

        if (t >= 1)
        {
            _isFilling = false;
            OnBarFilled.Invoke();
        }

        FillBar(currentFIll);
        _expAmount.SetText($"{Mathf.RoundToInt(currentFIll)}/100");
    }
}
