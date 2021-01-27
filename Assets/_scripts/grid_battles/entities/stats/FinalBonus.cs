using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Active Time Bonuses
public class FinalBonus : BaseAttribute
{
    public UnityEvent OnBonusExpiration;
    float _bonusDuration;
    bool _activated;
    Stat _parent;

    public FinalBonus(float timeLeft, int value = 0, float multiplier = 0) : base (value, multiplier) {
        _bonusDuration = timeLeft;
        _activated = false;
    }

    public void Activate(Stat parent) {
        _parent = parent;
        _activated = true;

        if (_activated) {
            _bonusDuration -= Time.deltaTime;

            if (_bonusDuration < 0)
            {
                _activated = false;
                _parent.RemoveFinalBonus(this);
                OnBonusExpiration.Invoke();
            }
        }
    }
}