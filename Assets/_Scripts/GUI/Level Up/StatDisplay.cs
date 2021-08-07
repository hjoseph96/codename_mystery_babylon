using ca.HenrySoftware.Rage;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    // UI values
    public float timeBetweenStats = 0.075f;
    public Image statHighlight; 
    public Image statNameHighlight;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI upgradeAmountText;
    public Image statFlash;

    // keeping updates of items 
    private int upgradeAmount;
    private int currentLevel;

    private float _currentTime = 0f;
    private float _highlightDisplayTime = .25f;
    private float _flashEffectTime = .1f;

    /// <summary>
    /// Called when first opening up the stats window to update the value of this stat plus the new amount, upgradeAmount should not be fed through if there is no value
    /// </summary>
    /// <param name="currentLevel"></param>
    /// <param name="upgradeAmount"></param>
    public void DisplayValues(int currentLevel)
    {
        // reset stat display items
        statHighlight.fillAmount = 0;
        upgradeAmountText.text = "";
        statNameHighlight.gameObject.SetActive(false);

        this.currentLevel = currentLevel;

        // set current level text of Stat
        currentLevelText.text = string.Format("{0}", currentLevel);
    }

    public void SetUpgradeAmount(int newStatValue)
    {
        upgradeAmount = Mathf.Abs(newStatValue - currentLevel);
        Debug.Log($"Set Upgrade Amount {newStatValue} - {currentLevel} = {upgradeAmount}");
    }

    /// <summary>
    /// Performs a flash and fill effect onto the stat, and a very brief delay before returning
    /// </summary>
    /// <returns></returns>
    public IEnumerator PerformVisualEffect()
    {
        // If there is no upgrade on this stat, we want to simply return
        if (upgradeAmount != 0)
        {
            // Makes the stat bar highlight fill over quick time
            while (_currentTime < _highlightDisplayTime)
            {
                //Debug.Log(_currentTime + " " + _highlightDisplayTime);
                _currentTime += Time.deltaTime;
                statHighlight.fillAmount = _currentTime / _highlightDisplayTime;
                yield return null;
            }
            // set to 1 to ensure full visibility
            statHighlight.fillAmount = 1;

            // reset timer
            _currentTime -= _highlightDisplayTime;

            // Make a white overlay quickly flash into visibility
            while (_currentTime < _flashEffectTime)
            {
                //Debug.Log(_currentTime + " " + _flashEffectTime);
                _currentTime += Time.deltaTime;
                statFlash.color = new Color(statFlash.color.r, statFlash.color.g, statFlash.color.b, _currentTime / _flashEffectTime * 255);
                yield return null;
            }

            // While the flash is happening update the value to showcase the new value
            upgradeAmountText.text = string.Format("+ {0}", upgradeAmount);
            currentLevelText.text = string.Format("{0}", currentLevel + upgradeAmount);
            statNameHighlight.gameObject.SetActive(true);

            // Ensure flash is full set
            statFlash.color = new Color(statFlash.color.r, statFlash.color.g, statFlash.color.b, 255);

            // reset timer again
            _currentTime -= _flashEffectTime;

            // remove flash effect
            while (_currentTime < _flashEffectTime)
            {
                _currentTime += Time.deltaTime;
                statFlash.color = new Color(statFlash.color.r, statFlash.color.g, statFlash.color.b, (_flashEffectTime - _currentTime) * 255);
                yield return null;
            }

            // set back to 0 to ensure complete invisibility
            statFlash.color = new Color(statFlash.color.r, statFlash.color.g, statFlash.color.b, 0);

            // arbitrary yield for effect
            yield return new WaitForSeconds(timeBetweenStats);
        }
        yield return null;
    }
}
