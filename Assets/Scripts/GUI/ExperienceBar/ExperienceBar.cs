using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Sirenix.OdinInspector;
using TMPro;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Transform _fillSpawnPoint;
    [SerializeField] private Image _barFill;
    [SerializeField] private float _fillDuration = 5f;
    [SerializeField] private float _fillMax = -641.87f;
    [SerializeField] private float _fillMin = -2.5f;


    [HideInInspector] public UnityEvent OnBarFilled;

    private TextMeshProUGUI _expAmount;

    private int _percentage;
    [ShowInInspector] public int Percentage { get { return _percentage; } }

    private int _displayPercentage;
    private float _fillPerPercent;
    private bool _isFilling = false;
    private bool _barSpawned = false;
    
    private float _startTime;
    private bool _timeSet = false;

    public void Show(int unitExperience)
    {
        _percentage = unitExperience;
        _displayPercentage = Percentage;
        _expAmount = GetComponentInChildren<TextMeshProUGUI>();

        _fillPerPercent = _fillMax / 100;
        _expAmount.SetText($"{_displayPercentage}/100");

        if (Percentage > 0)
        {
            SpawnBar();
            StartFilling(Percentage);
        }

        this.SetActive(true);
    }

    public void Deactivate()
    {
        this.SetActive(false);
    }


    public void StartFilling(int percentage)
    {
        _percentage = percentage;
        _isFilling = true;

        if (!_timeSet)
        {
            _startTime =  Time.time;
            _timeSet = true;
        }
    }

    void Update() 
    {
        if (_isFilling)
            Fill();
    }

    private void Fill()
    {
        if (!_barSpawned)
            SpawnBar();

        
        float t = (Time.time - _startTime) / _fillDuration;

        var fillAmount = Mathf.SmoothStep(_barFill.rectTransform.offsetMax.x, _fillPerPercent * Percentage, t);

        _displayPercentage = IncreaseDisplayPercentage();
        _expAmount.SetText($"{_displayPercentage}/100");

        _barFill.rectTransform.offsetMax = new Vector2(-fillAmount, _barFill.rectTransform.offsetMax.y);

        if (Mathf.Abs(_barFill.rectTransform.offsetMax.x) >= Mathf.Abs(_fillPerPercent * Percentage) - 0.001f)
        {
            _isFilling = false;
            _displayPercentage = Percentage;
            _expAmount.SetText($"{_displayPercentage}/100");

            OnBarFilled.Invoke();
        }
    }

    private int IncreaseDisplayPercentage()
    {
        float displayPercentage = (float)_displayPercentage;
        float percentage = (float)Percentage;

        float slerpedPercentage =  Mathf.SmoothStep(displayPercentage, percentage, _fillDuration * Time.deltaTime);

        return (int)Mathf.Round(slerpedPercentage);
    }

    private void SpawnBar()
    {
        _barFill = Instantiate(_barFill, _fillSpawnPoint.position, _barFill.rectTransform.rotation, _fillSpawnPoint);
        _barSpawned = true;
    }
}
