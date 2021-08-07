using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveItemPrefab : MenuOption<SaveLoadMenu>
{
    public new SaveLoadMenu Menu;
    [Tooltip("item 0 is normal, item 1 is highlighted")]
    public List<Sprite> backgroundImages;
    public Image background;

    public TextMeshProUGUI locationText;
    public TextMeshProUGUI partText;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI fileNameText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI difficultyText;

    #region Methods
    /// <summary>
    /// Used to populate a save slot with data from the save file
    /// </summary>
    /// <param name="TEMPTOREMOVE"></param>
    public void Populate(bool TEMPTOREMOVE = false /*Save file Data*/)
    {
        Menu.OnSlotsChanged += RefreshItem;
        fileNameText.text = "Testing Save File";
    }

    /// <summary>
    /// Used to populate a new Save Slot, for writing a new file
    /// </summary>
    public void Populate()
    {
        fileNameText.text = "New Save Slot";
        Menu.OnSlotsChanged += RefreshItem;
    }

    private void RefreshItem(/*Save file Data*/)
    {
        Debug.Log("Refreshing Item");
    }

    private void SetHighlight(bool value)
    {
        background.sprite = backgroundImages[value ? 0 : 1];
    }
    #endregion

    #region MonoBehaviour
    private void OnDestroy()
    {
        Menu.OnSlotsChanged -= RefreshItem;
    }
    #endregion

    #region Inherited Methods
    public override void Execute()
    {
        base.Execute();
    }

    public override void SetNormal()
    {
        base.SetNormal();
        SetHighlight(true);
    }

    public override void SetPressed()
    {
        base.SetPressed();
    }

    public override void SetSelected()
    {
        base.SetSelected();
        SetHighlight(false);
    }
    #endregion
}
