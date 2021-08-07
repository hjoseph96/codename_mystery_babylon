using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpGUI : Menu, IInitializable
{
    public static LevelUpGUI Instance;

    public Image unitPortrait;
    public List<StatDisplay> statDisplays;
    public TextMeshProUGUI unitName;
    public TextMeshProUGUI unitClass;
    public TextMeshProUGUI unitLevel;

    public List<UnitStat> levelUpStats;

    public bool finishedDisplay = true;

    private Unit u;

    public void Init()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public static void BeginDisplay(Unit u)
    {
        Instance.ShowLevelUpGUI(u);
    }

    public static void VisualEffects()
    {
        Instance.PerformVisualEffects();
    }

    private void PerformVisualEffects()
    {
        // go over the displays and set the initial values
        for (int i = 0; i < statDisplays.Count; i++)
        {
            statDisplays[i].SetUpgradeAmount(u.Stats[levelUpStats[i]].ValueInt);
        }
        unitLevel.text = string.Format("{0}", u.Level);
        StartCoroutine(LevelUpDisplay());
    }

    public void ShowLevelUpGUI(Unit u)
    {
        finishedDisplay = false;
        this.u = u;

        // Sets values to the level up window related to the unit
        unitName.text = u.Name;
        unitClass.text = u.UnitClass.Title;
        unitLevel.text = string.Format("{0}", u.Level);

        // go over the displays and set the initial values
        for(int i = 0; i < statDisplays.Count; i++)
        {
            statDisplays[i].DisplayValues(u.Stats[levelUpStats[i]].ValueInt);
        }

        Activate();
    }

    /// <summary>
    /// This goes over the Visual effects used to process the fancy effects for the GUI screen
    /// </summary>
    /// <returns></returns>
    private IEnumerator LevelUpDisplay()
    {
        // go over the displays and set the initial values
        for (int i = 0; i < statDisplays.Count; i++)
        {
            yield return StartCoroutine(statDisplays[i].PerformVisualEffect());
        }

        u = null;
        yield return null;
    }

    public override void ProcessInput(InputData input)
    {
        switch (input.KeyCode)
        {
            case KeyCode.X:
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                    break;

                Debug.Log("Processing Input Finished display is true");
                finishedDisplay = true;
                MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                Close();
                break;
        }
    }
}
