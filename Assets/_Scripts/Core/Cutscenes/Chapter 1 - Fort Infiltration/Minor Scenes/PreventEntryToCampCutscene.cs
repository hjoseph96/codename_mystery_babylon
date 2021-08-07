using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventEntryToCampCutscene : Cutscene
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
