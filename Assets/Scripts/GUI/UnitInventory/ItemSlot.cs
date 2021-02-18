using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Image itemIcon;
    public TMPro.TMP_Text title;
    public TMPro.TMP_Text durability;

    
    private Material _allInOneMat;
    private Material _iconAllInOneMat;
    
    public void Populate(Item item) {
       itemIcon.sprite = item.Icon;
       itemIcon.SetActive(true);

        _allInOneMat = this.GetComponent<Image>().material;
        _iconAllInOneMat = itemIcon.material;

       title.text = item.Name;

       string displayDurability;
       if (item.CurrentDurability < 0) {
           displayDurability = "---";
       } else {
           displayDurability = $"{item.CurrentDurability}/{item.MaxDurability}";
       }

       durability.text = displayDurability;
    }

    
    public void SetSelected() {
        _allInOneMat.SetFloat("_GhostTransparency", 0.413f);
        _allInOneMat.SetInt("_GhostColorBoost", 5);
        _allInOneMat.EnableKeyword("GHOST_ON");

        _iconAllInOneMat.SetFloat("_OutlineAlpha", 0.467f);
    }

    public void SetDeselected() {
        _allInOneMat.DisableKeyword("GHOST_ON");
        _iconAllInOneMat.SetFloat("_OutlineAlpha", 0);
    }
}
