using TMPro;
using UnityEngine;


public class ItemStatDisplay : MonoBehaviour
{
    protected TextMeshProUGUI StatDisplay; 

    private void Awake()
    {
        StatDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    public virtual void SetText(Weapon weapon)
    {
        StatDisplay.SetText("ERROR");
    }
}