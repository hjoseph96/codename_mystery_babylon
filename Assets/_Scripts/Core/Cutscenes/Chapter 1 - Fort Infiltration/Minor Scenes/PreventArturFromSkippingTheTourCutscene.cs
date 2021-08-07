using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PreventArturFromSkippingTheTourCutscene : Cutscene
{
    // Update is called once per frame
    void Update()
    {
        if (IsTriggered)
        {
            Play();

            IsTriggered = false;
        }
    }
}
