using System;
using System.Collections;
using UnityEngine;

using Sirenix.OdinInspector;

public class MiniHealthBar : MonoBehaviour
{
    [SerializeField] private Transform _barFill;
    [SerializeField] private float _duration;

    private SpriteFlashTool _flasher;
    private bool _flashed = false;

    private bool  _isTweening = false;
    private float _startTime;
    private float _startingPercentage;
    private float _targetPercentage;

    private Action _destroySelf;


    [HideInInspector] public Action OnComplete;


    public void Show(Unit unit)
    {
        if (!this.IsActive())
        {
            var targetPercentage = unit.CurrentHealth / unit.MaxHealth;
            _barFill.localScale = new Vector3(targetPercentage, 1, 1);

            this.SetActive(true);
        }
    }

    public void Hide() => this.SetActive(false);

    void Start()
    {
        this.SetActive(false);
        _flasher = _barFill.GetComponent<SpriteFlashTool>();
    }

    void Update()
    {
        if (_isTweening)
            MoveFill();
    }


    public void Tween(float targetPercentage) 
    {
        // Attach Internal Finished Event
        _destroySelf += delegate ()
        {
            StartCoroutine(WaitAndDeactivate());
        };

        // Set Percentages & change initial fill amount
        _targetPercentage   = targetPercentage;
        _startingPercentage = _barFill.localScale.x;
        _barFill.localScale = new Vector3(_startingPercentage, 1, 1);

        // Set active and start Tweening
        this.SetActive(true);

        _startTime = Time.time;
        _isTweening = true;
    }



    void MoveFill()
    {
        if (_targetPercentage == _startingPercentage)
            return;


        // Flash the HP Bar once
        if (!_flashed)
        {
            _flashed = true;
            _flasher.Flash();
        }

        // Tween to target fill amount
        float t = (Time.time - _startTime) / _duration;
        float currentfill = Mathf.SmoothStep(_startingPercentage, _targetPercentage, t);
        _barFill.localScale = new Vector3(currentfill, 1, 1);


        // End the tween once it reached it's target
        if (_barFill.localScale.x == _targetPercentage)
        {
            _isTweening = false;
            _destroySelf.Invoke();
        }
    }

    IEnumerator WaitAndDeactivate()
    {
        yield return new WaitForSecondsRealtime(2f);

        this.SetActive(false);

        if (OnComplete != null)
            OnComplete.Invoke();
    }
}
