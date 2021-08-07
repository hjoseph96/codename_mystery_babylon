using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatItemSlot : ItemSlot
{
    public TextMeshProUGUI statValueTexts;
    public Image statFillValues;
    public AdvancedDisplayMenu menu;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    public override void Clear()
    {
        
    }

    public override void Execute()
    {
        menu.DisplayInformation();
    }

    public override void SetNormal()
    {
        
    }

    public override void SetSelected()
    {
        
    }

    public override void SetPressed()
    {
        
    }

    public override void Populate(Item item)
    {
        
    }
}
