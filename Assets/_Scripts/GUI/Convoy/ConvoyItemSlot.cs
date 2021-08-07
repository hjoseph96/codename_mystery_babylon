using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConvoyItemSlot : ItemSlot
{
    public new ConvoyMenu Menu;
    public List<GameObject> highlights;
    public Image unitEyes;
    public TMPro.TextMeshProUGUI amountText;


    public new Item Item { get; private set; }

    public bool changedOwner = false;

    #region Inherited Methods
    public void Populate(Item item, ConvoyMenu menu)
    {
        Item = item;
        Menu = menu;

        if (Item is Weapon weapon)
        {
            if (weapon.CurrentDurability < 0)
                _durability.text = "---";
            else
                _durability.SetText("{0}/{1}", weapon.CurrentDurability, weapon.MaxDurability);
        }
        else
            _durability.SetText("None");

        _icon.sprite = Item.Icon;
        _title.text = Item.Name;
        _equippedIcon.SetActive(Item.IsEquipped);

        SetPortrait();
        SetAmountText();

        Menu.OnSlotsChanged += CheckPortrait;
    }

    private void OnDestroy()
    {
        Menu.OnSlotsChanged -= CheckPortrait;
    }

    /// <summary>
    /// Checks to see if the unit portrait should be refreshed if it changed hands, and updates amount if has one
    /// </summary>
    private void CheckPortrait()
    {
        if (changedOwner)
        {
            changedOwner = false;
            SetPortrait();
            SetAmountText();
        }
    }

    private void SetAmountText()
    {
        if (Item.amount > 1)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = $"x {Item.amount}";
        }
        else
            amountText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the portrait to the unit eyes of the owned unit, or deactivates if in the convoy 
    /// </summary>
    private void SetPortrait()
    {
        if (Item.Unit != null)
        {
            unitEyes.gameObject.SetActive(true);
            unitEyes.sprite = Item.Unit.Portrait.Default;
        }
        else
            unitEyes.gameObject.SetActive(false);
    }

    public override void Execute()
    {
        base.Execute();
        Menu.OnConvoyItemSelected(this);
    }

    public override void SetNormal()
    {
        base.SetNormal();
        SetHighlight(false);
    }

    public override void SetPressed()
    {
        base.SetPressed();
    }

    public override void SetSelected()
    {
        base.SetSelected();
        SetHighlight(true);
    }
    #endregion

    #region Methods
    private void SetHighlight(bool value)
    {
        for(int i = 0; i < highlights.Count; i++)
        {
            highlights[i].SetActive(value);
        }
    }
    #endregion
}
