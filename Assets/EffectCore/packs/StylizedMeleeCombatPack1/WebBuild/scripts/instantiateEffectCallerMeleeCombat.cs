using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class instantiateEffectCallerMeleeCombat : MonoBehaviour
{
    int seq = 0;
    [System.NonSerialized]
    public bool fired = false;
    float timer;
    public bool rightClick = false;
    public float timeLimit;
    [System.Serializable]
    public class chainEffect
    {
        [System.NonSerialized]
        public bool isPlayed = false;
        public bool RotateRandomizer = false;
        public float activateTimer;
        public GameObject Effect;
        public Transform effectLocator;
    }
    public chainEffect[] chainEffectList;

    void Start()
    {
        //  print(chainEffectList.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (fired)
        {
            timer += Time.deltaTime;
            CheckTimer();
        }
    }
    void CheckTimer()
    {

        for (int i = 0; i < chainEffectList.Length; i++)
        {
            if (timer >= chainEffectList[i].activateTimer && chainEffectList[i].isPlayed == false)
            {
                Instantiate(chainEffectList[i].Effect, chainEffectList[i].effectLocator.transform.position, chainEffectList[i].effectLocator.transform.rotation);

                if(chainEffectList[i].RotateRandomizer)
                {
                    if (seq == 0)
                    {
                        seq++;

                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, 35);
                    }
                    else if (seq == 1)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, -65);
                    }
                    else if (seq == 2)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, 45);
                    }
                    else if (seq == 3)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, -20);
                    }
                    else if (seq == 4)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, 45);
                    }
                    else if (seq == 5)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, -15);
                    }
                    else if (seq == 6)
                    {
                        seq = 0;
                        chainEffectList[i].effectLocator.transform.Rotate(0, 0, -25);
                    }


                }
                // chainEffectList[i].Effect.Play();
                chainEffectList[i].isPlayed = true;
            }
        }
        if (timer >= timeLimit)
        {
            fired = false;
            ResetTimers();
        }
    }


    public void ResetTimers()
    {
        for (int i = 0; i < chainEffectList.Length; i++)
        {
            chainEffectList[i].isPlayed = false;
        }
        timer = 0;
    }
}
