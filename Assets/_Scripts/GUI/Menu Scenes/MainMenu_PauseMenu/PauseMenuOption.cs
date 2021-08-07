using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PauseMenuOption : MenuOption<PauseMenu>
{
    public Color normalColor;
    public Color pressedColor;
    public Color selectedColor;

    public float normalSize;
    public float pressedSize;
    public float selectedSize;

    public TextMeshProUGUI wordText;
    public GameObject wordUnderline;

    public override void SetNormal()
    {
        // The normal sized and gold colored text
        wordText.color = normalColor;
        wordText.fontSize = normalSize;
        wordUnderline.SetActive(false);
    }

    public override void SetPressed()
    {
        // Slightly smaller text, all black 
        wordText.color = pressedColor;
        wordText.fontSize = pressedSize;
        wordUnderline.SetActive(false);
    }

    public override void SetSelected()
    {
        // Purpleish text larger with an underline on it 
        wordText.color = selectedColor;
        wordText.fontSize = selectedSize;
        wordUnderline.SetActive(true);
    }
}
