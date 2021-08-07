using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MiniHealthBar : MonoBehaviour
{
    [SerializeField] private float _duration;
    public Image healthBarFill;
    public Image healthBarHighlight;
    private bool  _isTweening = false;
    private float _startTime;
    private float _startingPercentage;
    private float _targetPercentage;

    [HideInInspector] public Action OnComplete;
    private bool _flashed;
    private bool _flashReverse;
    private float _flashDuration = 0.125f;

    private void Awake()
    {
        //healthBarFill.material = new Material(healthBarFill.material);
    }

    public void Show(Unit unit)
    {
        var targetPercentage = (float)unit.CurrentHealth / unit.MaxHealth;

        healthBarFill.fillAmount = targetPercentage;
        healthBarHighlight.fillAmount = targetPercentage;

        gameObject.SetActive(true);
    }

    public void Refresh(Unit u) 
    { 
        healthBarFill.fillAmount = u.CurrentHealth / u.MaxHealth;
        Hide();
    }

    public void Hide() => gameObject.SetActive(false);

    void Start()
    {
        healthBarFill.material.SetFloat("_HitEffectBlend", 0);
        healthBarFill.material.DisableKeyword("HITEFFECT_ON");
        _startingPercentage = healthBarFill.fillAmount;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (_isTweening)
            MoveFill();
    }


    public void Tween(float targetPercentage) 
    {
        gameObject.SetActive(true);
        // Set Percentages & change initial fill amount
        _targetPercentage   = targetPercentage;

        // Set active and start Tweening

        _startTime = Time.time;
        _isTweening = true;
        _flashed = false;

        healthBarHighlight.gameObject.SetActive(true);
        healthBarHighlight.material.EnableKeyword("HITEFFECT_ON");
    }



    void MoveFill()
    {
        if (_targetPercentage == _startingPercentage)
            return;

        // Flash the HP Bar once
        if (!_flashed)
        {
            float flashTime;

            if (_flashReverse)
            {
                flashTime = (Time.time - _startTime - _flashDuration) / _flashDuration;
                healthBarFill.material.SetFloat("_HitEffectBlend", Mathf.SmoothStep(1, 0, flashTime));

                if(healthBarFill.material.GetFloat("_HitEffectBlend") == 0)
                {
                    _flashed = true;
                    healthBarHighlight.gameObject.SetActive(false);
                    healthBarFill.material.DisableKeyword("HITEFFECT_ON");
                }
            }
            else
            {
                flashTime = (Time.time - _startTime) / _flashDuration;
                healthBarFill.material.SetFloat("_HitEffectBlend", Mathf.SmoothStep(0, 1, flashTime));

                if (healthBarFill.material.GetFloat("_HitEffectBlend") == 1)
                {
                    _flashReverse = true;
                }
            }
        }

        // Tween to target fill amount
        float t = (Time.time - _startTime) / _duration;
        float currentfill = Mathf.SmoothStep(_startingPercentage, _targetPercentage, t);
        healthBarFill.fillAmount = currentfill;


        // End the tween once it reached it's target
        if (healthBarFill.fillAmount == _targetPercentage)
        {
            _isTweening = false;
            _startingPercentage = _targetPercentage;
            StartCoroutine(WaitAndDeactivate());
        }
    }

    IEnumerator WaitAndDeactivate()
    {
        yield return new WaitForSecondsRealtime(2f);
        gameObject.SetActive(false);

        if (OnComplete != null)
        {
            OnComplete.Invoke();
            OnComplete = null;
        }
    }
}
