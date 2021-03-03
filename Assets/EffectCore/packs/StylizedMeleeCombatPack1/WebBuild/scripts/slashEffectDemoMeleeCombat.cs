using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slashEffectDemoMeleeCombat : MonoBehaviour
{
   public int seq = 0;
    bool fired = false;
    float timer;
    public bool fire2 = false;
    public bool fire3 = false;
    public bool fire4 = false;
    public float timeLimit;

    public int meleefxVariation = 0;

    [System.Serializable]
    public class chainEffect
    {
        public bool isPlayed = false;
        public bool RotateRandomizer = false;
        public bool dimenson3_RotateRandomizer = false;
        public float activateTimer;
        public GameObject[] Effect;
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
        if (Input.GetKeyDown(KeyCode.Alpha1) && fire2 == false && fire3 == false && fire4 == false || Input.GetKeyDown(KeyCode.Alpha2) && fire2 == true && fire3 == false && fire4 == false || Input.GetKeyDown(KeyCode.Alpha3) && fire2 == false && fire3 == true && fire4 == false || Input.GetKeyDown(KeyCode.Alpha4) && fire2 == false && fire3 == false && fire4 == true)
        {
            fireEffect();
        }
        if (fired)
        {
            timer += Time.deltaTime;
            CheckTimer();
        }
        meleefxVariation = nextEffectMeleeCombat.effectVariation;
     }

    public void fireEffect()
    {
        if (fired == false)
        {
            fired = true;
        }
        else if (fired == true)
        {
            ResetTimers();
            CheckTimer();
        }
    }

    public void CheckTimer()
    {
        for (int i = 0; i < chainEffectList.Length; i++)
        {
            if (timer >= chainEffectList[i].activateTimer && chainEffectList[i].isPlayed == false)
            {
                Instantiate(chainEffectList[i].Effect[meleefxVariation], chainEffectList[i].effectLocator.transform.position, chainEffectList[i].effectLocator.transform.rotation);

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

                if (chainEffectList[i].dimenson3_RotateRandomizer)
                {
                    if (seq == 0)
                    {
                        seq++;

                        chainEffectList[i].effectLocator.transform.Rotate(5, 5, 0, Space.World);
                    }
                    else if (seq == 1)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(-5, -5, 0, Space.World);
                    }
                    else if (seq == 2)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(-5, -5, 0, Space.World);
                    }
                    else if (seq == 3)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(5, 5, 0, Space.World);
                    }
                    else if (seq == 4)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(-5, -5, 0, Space.World);
                    }
                    else if (seq == 5)
                    {
                        seq++;
                        chainEffectList[i].effectLocator.transform.Rotate(5, 5, 0, Space.World);
                    }
                    else if (seq == 6)
                    {
                        seq = 0;
                        chainEffectList[i].effectLocator.transform.Rotate(-5, -5, 0, Space.World);
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


    void ResetTimers()
    {
        for (int i = 0; i < chainEffectList.Length; i++)
        {
            chainEffectList[i].isPlayed = false;
        }
        timer = 0;
    }
}
