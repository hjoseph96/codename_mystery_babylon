using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VikingExample : MonoBehaviour {

    [SerializeField] private int vikingNumber = 0;

	void Update () {
        if (Input.GetMouseButtonDown(vikingNumber))
            GetComponent<SpriteFlashTool>().FlashAll();
    }
}
