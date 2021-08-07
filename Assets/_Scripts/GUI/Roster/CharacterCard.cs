using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MenuOption<RosterMenu>
{
    [SerializeField] private Color ColorWhenSelected;
    [SerializeField] private Color ColorWhenNotSelected;
    [SerializeField] private Image _renderer;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI classText;
    public Image portrait;

    public override void Execute()
    {
        Menu.SelectCard(this);
    }

    public void Highlight()
    {
        if(_renderer == null)
            _renderer = GetComponent<Image>();

        _renderer.color = ColorWhenSelected;
    }

    public void Fade()
    {
        if (_renderer == null)
            _renderer = GetComponent<Image>();

        _renderer.color = ColorWhenNotSelected;
    }

    public void AssignData(Unit unit)
    {
        nameText.text = unit.Name;
        genderText.text = unit.GetComponent<EntityReference>().AssignedEntity.Gender.ToString();
        ageText.text = unit.GetComponent<EntityReference>().AssignedEntity.Age.ToString();
        classText.text = unit.Class.Title;
        portrait.sprite = unit.Portrait.Default;
    }
}
