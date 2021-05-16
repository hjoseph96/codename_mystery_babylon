using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMoveTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CutsceneManager.Instance.SetMoveTarget(this.transform);
    }
}
