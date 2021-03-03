using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour {


    public float difference = 0.01f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {

        if(Input.GetKey(KeyCode.A))
        {
            Time.timeScale -= difference;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Time.timeScale += difference;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Time.timeScale = 1;
        }

    }
}
