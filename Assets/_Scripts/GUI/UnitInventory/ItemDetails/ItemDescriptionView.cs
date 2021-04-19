using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemDescriptionView : MonoBehaviour
{
    private TextMeshProUGUI _description;

    public void SetDescription(string textToDisplay) 
    {
        _description = GetComponentInChildren<TextMeshProUGUI>();
        _description.SetText(textToDisplay);
    }
}
