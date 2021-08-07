using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedDisplayMenu : Menu
{
    public GameObject content;
    public GameObject tooltipContent;

    public List<ItemSlot> options = new List<ItemSlot>();
    public UICursor _cursor;

    private int _selectedOptionIndex;

    private bool _displayingTooltip = false;
    private bool _addedStats = false;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI classText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI movementText;

    public Image portrait;
    public Image experienceBar;

    public List<StatItemSlot> statSlots = new List<StatItemSlot>();
    public List<MagicItemSlot> magicSlots = new List<MagicItemSlot>();
    public List<InventoryItemSlot> inventorySlots = new List<InventoryItemSlot>();
    public List<SkillItemSlot> skillSlots = new List<SkillItemSlot>();
    public List<AbilityItemSlot> abilitySlots = new List<AbilityItemSlot>();
    public List<BonusItemSlot> bonusSlots = new List<BonusItemSlot>();

    // Exclusively used to set values
    public List<UnitStat> coreUnitStats = new List<UnitStat>();

    public void Show(Unit unit)
    {


        // Set unit text values
        nameText.text = unit.Name;
        genderText.text = unit.GetComponent<EntityReference>().AssignedEntity.Gender.ToString();
        ageText.text = unit.GetComponent<EntityReference>().AssignedEntity.Age.ToString();
        classText.text = unit.Class.Title;
        expText.text = unit.Experience.ToString();
        levelText.text = unit.Level.ToString();
        healthText.text = unit.MaxHealth.ToString();
        movementText.text = unit.MaxMoveRange.ToString();

        // set unit other values
        experienceBar.fillAmount = (float)unit.Experience / (float)100;
        portrait.sprite = unit.Portrait.Default;

        Activate();

        if (!_addedStats)
        {
            options.InsertRange(0, statSlots);
            _addedStats = true;
            _selectedOptionIndex += statSlots.Count;
        }

        for(int i = 0; i < options.Count; i++)
        {
            options[i].Clear();
        }

        var items = unit.Inventory.GetItems<Item>();
        for (int i = 0; i < items.Length; i++)
        {
            inventorySlots[i].Populate(items[i]);
        }

        for(int i = 0; i < statSlots.Count; i++)
        {
            statSlots[i].statValueTexts.text = unit.Stats[coreUnitStats[i]].ValueInt.ToString();
            statSlots[i].statFillValues.fillAmount = unit.Stats[coreUnitStats[i]].ValueInt / unit.EditorStats[coreUnitStats[i]].Value + 10;
        }
    }


    /// <summary>
    /// This is the main method to close the AdvancedDisplayMenu
    /// </summary>
    public override void Close()
    {
        base.Close();
        content.SetActive(false);
    }


    public override void Activate()
    {
        UserInput.Instance.InputTarget = this;
        gameObject.SetActive(true);
        content.SetActive(true);
        MoveSelectionToOption(_addedStats ? statSlots.Count: 0, true); 
        _selectedOptionIndex = _addedStats ? statSlots.Count : 0;
        _displayingTooltip = false;
    }


    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                    break;

                PressOption(SelectedOption);
                SelectedOption.Execute();
                
                break;
            case KeyCode.X:
                if (input.KeyState == KeyState.Down)
                    break;

                if (_displayingTooltip)
                {
                    HideInformation();
                }
                else
                {
                    Deactivate();
                    PreviousMenu.Activate();
                }
                break;

        }
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(options[index].transform, false);
        _cursor.MoveTo(new Vector2(-225, 0), instant);


        // Arbitrary hiding of the cursor from the scene when highlighting the stat items
        if (index >= 0 && index < statSlots.Count && _addedStats)
            _cursor.cursorImage.SetActive(false);
        else
            _cursor.cursorImage.SetActive(true);
        //MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }

    public void DisplayInformation()
    {
        tooltipContent.SetActive(true);
        _displayingTooltip = true;
    }

    public void HideInformation()
    {
        tooltipContent.SetActive(false);
        _displayingTooltip = false;
    }
}
