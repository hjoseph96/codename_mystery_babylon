using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBS;

public class ShootCustomizer : FrameUpdater
{
    public GameObject arrow;
    public Transform bowString;
    public Transform stringShot;
    public Transform rightHandBone;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void UpdateFrame(int frame, float time = 0.0f) {
        if (frame > 14) {
            arrow.SetActive(false);
            
            bowString.parent = null;

            if (frame <= 30)
                bowString.position = Vector3.Slerp(bowString.position, stringShot.position, 0.3f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
