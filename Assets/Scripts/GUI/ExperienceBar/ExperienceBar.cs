using System;
using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;
using TMPro;


// TODO: Implement logic for when EXP percent goes > 100, and there is a remainder of exp left to gain.
public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Transform _fillSpawnPoint;

    [SerializeField] private GameObject _barFillPrefab;
    [SerializeField] private float _fillDuration = 0.67f;
    [SerializeField] private float _fillMax = -641.87f;

    [HideInInspector] public Action OnBarFilled;

    private TextMeshProUGUI _expAmount;

    private Image _barFill;
    private int _percentage;
    [ShowInInspector] public int Percentage { get { return _percentage; } }

    private int _displayPercentage;
    private float _fillPerPercent;
    private float _startTime;
    private bool _timeSet = false;
    public bool IsFilling = false;
    private bool _barSpawned = false;
    

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

            _barFill.rectTransform.offsetMax = new Vector2(-(_fillPerPercent * unitExperience), _barFill.rectTransform.offsetMax.y);            
        }

        this.SetActive(true);
    }

    public void Reset()
    {
        this.SetActive(false);

        _timeSet = false;
        IsFilling = false;
        OnBarFilled = null;
        _barSpawned = false;

        _barFillPrefab.transform.SetParent(null);
        Destroy(_barFill.gameObject);
    }


    public void StartFilling(int percentage)
    {
        _percentage = percentage;
        IsFilling = true;

        if (!_timeSet)
        {
            _startTime =  Time.time;
            _timeSet = true;
        }
    }

    void Update() 
    {
        if (IsFilling)
            Fill();
    }


    private void Fill()
    {
        if (!_barSpawned)
            SpawnBar();

        
        float t = (Time.time - _startTime) / _fillDuration;
        var currentFill = -_barFill.rectTransform.offsetMax.x;

        var fillAmount = AnimationCurve.Linear(_startTime, currentFill, _startTime + _fillDuration, _fillPerPercent * Percentage).Evaluate(Time.time);


        _barFill.rectTransform.offsetMax = new Vector2(-fillAmount, _barFill.rectTransform.offsetMax.y);
        
        _displayPercentage = IncreaseDisplayPercentage();
        _expAmount.SetText($"{_displayPercentage}/100");

        if (Mathf.Abs(_barFill.rectTransform.offsetMax.x) >= Mathf.Abs(_fillMax))   /// 100 exp filled.
        {
            // TODO Level UP display.
            _percentage -= 100;
            _displayPercentage = 0;
            _barFill.rectTransform.offsetMax = new Vector2(0, _barFill.rectTransform.offsetMax.y);
        }

        if (Mathf.Abs(_barFill.rectTransform.offsetMax.x) >= Mathf.Abs(_fillPerPercent * Percentage) - 0.001f)
        {
            IsFilling = false;
            _displayPercentage = Percentage;
            _expAmount.SetText($"{_displayPercentage}/100");

            OnBarFilled.Invoke();
        }
    }

    private int IncreaseDisplayPercentage()
    {
        int newPercentage = (int)Mathf.Abs(Mathf.Round(_barFill.rectTransform.offsetMax.x / _fillPerPercent));

        if (newPercentage >  100) newPercentage -= 100;
        
        return newPercentage;
    }

    private void SpawnBar()
    {
        _barFill = _barFillPrefab.GetComponent<Image>();
        _barFill = Instantiate(_barFillPrefab, _fillSpawnPoint.position, _barFill.rectTransform.rotation, _fillSpawnPoint).GetComponent<Image>();
        _barSpawned = true;
    }
}
