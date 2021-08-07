using Febucci.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatText : MonoBehaviour
{
    /// <summary>
    /// Text to display the dodge, damage and heal amounts
    /// </summary>
    public TextMeshProUGUI displayText;
    public CanvasGroup canvasGroup;
    public Color32 dodge;
    public Color32 heal;
    public Color32 damage;
    public Color32 critical;

    public float timeToDisplay = 2f;
    public float fadeOutTime = 0.25f;
    private Vector2 _directionToFade;
    private Unit unit;

    public void SetRef(Unit u)
    {
        unit = u;
        u.OnLookDirectionChanged += ChangeFadeDirection;
    }

    private void OnDestroy()
    {
        if (unit != null)
            unit.OnLookDirectionChanged -= ChangeFadeDirection;
    }

    private void ChangeFadeDirection(Vector2 direction)
    {
        _directionToFade = direction*0.03f;
    }

    /// <summary>
    /// Displays text for a determined time on object, and then fades in the direction the unit is facing
    /// </summary>
    /// <param name="modifier"></param>
    /// <param name="textToSet"></param>
    /// <returns></returns>
    private IEnumerator DisplayText(TextModifier modifier = TextModifier.Normal, string textToSet = "TEXT NOT SET!")
    {
        // reset timer and add TextAnimator properties based on type of display
        float timer = 0;
        if (modifier == TextModifier.Critical)
            textToSet = string.Format("{0} {1}", "<shake>", textToSet);
        else
            textToSet = string.Format("{0} {1}", "<wave>", textToSet);

        // Assign Values and reset positioning.
        transform.localPosition = new Vector3(0, 2.2f, 0); // Truly don't know why we need the 1.7 but the positioning won't work without it (transform set up)
        displayText.text = textToSet;
        displayText.color = GetColor(modifier);
        canvasGroup.alpha = 1f;

        // Viewing time
        while (timer < timeToDisplay)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        timer -= timeToDisplay;

        // Fadeout Time
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = fadeOutTime - timer;
            transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + new Vector3(_directionToFade.x, _directionToFade.y, 0), timer/fadeOutTime);
            yield return new WaitForEndOfFrame();
        }

        // ensure no text is visible afterwards
        canvasGroup.alpha = 0;
        yield return null;
    }

    #region Testing Functions
    [ContextMenu("Start Display Loop")]
    public void StartDisplayLoop()
    {
        StopAllCoroutines();
        StartCoroutine(DisplayText(TextModifier.Normal, "Dodge!"));
    }

    [ContextMenu("Start Display Loop Damage")]
    public void StartDisplayLoopDamage()
    {
        StopAllCoroutines();
        StartCoroutine(DisplayText(TextModifier.Damage, "100"));
    }

    [ContextMenu("Start Display Loop Heal")]
    public void StartDisplayLoopHeal()
    {
        StopAllCoroutines();
        StartCoroutine(DisplayText(TextModifier.Heal, "50"));
    }

    [ContextMenu("Start Display Loop Critical")]
    public void StartDisplayLoopCritical()
    {
        StopAllCoroutines();
        StartCoroutine(DisplayText(TextModifier.Critical, "200"));
    }
    #endregion

    public void StartDisplay(TextModifier modifier, string textToDisplay)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayText(modifier, textToDisplay));
    }

    public Color32 GetColor(TextModifier modifier)
    {
        switch (modifier)
        {
            case TextModifier.Normal:
                return dodge;
            case TextModifier.Heal:
                return heal;
            case TextModifier.Damage:
                return damage;
            case TextModifier.Critical:
                return critical;
            default:
                return dodge;
        }
    }

    public enum TextModifier { Normal, Damage, Heal, Critical }
}
