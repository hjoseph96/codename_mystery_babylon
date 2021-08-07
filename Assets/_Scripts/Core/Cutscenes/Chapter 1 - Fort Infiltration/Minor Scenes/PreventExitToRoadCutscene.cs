using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventExitToRoadCutscene : Cutscene
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
