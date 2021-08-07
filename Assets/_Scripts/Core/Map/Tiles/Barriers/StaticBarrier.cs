using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBarrier : Barrier
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Scan();
    }
}
