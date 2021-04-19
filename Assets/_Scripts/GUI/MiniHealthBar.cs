using System;
using System.Collections;
using UnityEngine;

public class MiniHealthBar : MonoBehaviour
{
    [SerializeField] private Transform _barfill;
    [SerializeField] private float _duration;
    [SerializeField] private float _destructionWaitDuration;

    private SpriteFlashTool _flasher;
    private bool _flashed = false;

    private bool _isTweening = false;
    private float _startTime;
    private float _startingPercentage;
    private float _targetPercentage;

    private Action _destroySelf;

    [HideInInspector] public Action OnComplete;


    void Start()
    {
        this.SetActive(false);
        _flasher = _barfill.GetComponent<SpriteFlashTool>();
    }

    public void Tween(float startPercentage, float endPercentage) 
    {
        // Attach Internal Finished Event
        _destroySelf += delegate ()
        {
            StartCoroutine(DestroySelf());
        };

        // Set Percentages & change initial fill amount
        _targetPercentage = startPercentage;
        _startingPercentage = _barfill.localScale.x;
        _barfill.localScale = new Vector3(_startingPercentage, 1, 1);

        // Set active and start Tweening
        this.SetActive(true);

        _startTime = Time.time;
        _isTweening = true;
    }


    void Update()
    {
        if (_isTweening)
            MoveFill();
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
        _barfill.localScale = new Vector3(currentfill, 1, 1);


        // End the tween once it reached it's target
        if (_barfill.localScale.x == _targetPercentage)
        {
            _isTweening = false;
            _destroySelf.Invoke();
        }
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSecondsRealtime(_destructionWaitDuration);

        Destroy(this.gameObject);
        OnComplete.Invoke();
    }
}
