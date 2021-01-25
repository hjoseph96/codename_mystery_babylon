using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS;

public class DrawCustomizer : FrameUpdater
{
    public GameObject arrow;
    public Transform bowString;
    public Transform rightHandBone;

    // Start is called before the first frame update
    void Start()
    {
    }

    public override void UpdateFrame(int frame, float time = 0.0f) {
        if (frame == 17)
            arrow.SetActive(true);
        
        if (frame == 38) {
            bowString.SetParent(rightHandBone);
            arrow.transform.SetParent(bowString);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
