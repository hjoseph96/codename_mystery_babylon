using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterCallerMeleeCombat : MonoBehaviour {

    public instantiateEffectCallerMeleeCombat EffectCaller;
    public string parameterCaller;
    public Animator thisAnimator;

	// Use this for initialization
	void Start ()
    {
        thisAnimator = gameObject.GetComponent<Animator>();
        EffectCaller = gameObject.GetComponent<instantiateEffectCallerMeleeCombat>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            thisAnimator.SetTrigger(parameterCaller);
            EffectCaller.ResetTimers();
            EffectCaller.fired = true;
        }
    }
}
