using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using Articy.Unity;

public class SingleDialogChoice : MonoBehaviour
{
    [SerializeField] private Image actionButton;
    [SerializeField] private KeyCode keyToPress;
    [SerializeField] private Sprite keyUpSprite;
    [SerializeField] private Sprite keyDownSprite;

    [HideInInspector] public Action<Branch> OnChoiceSelected;
    private TextMeshProUGUI _choiceText;
    private Branch _choice;
    

    public void Populate(Branch choice, string choiceText)
    {
        _choice = choice;

        _choiceText = GetComponentInChildren<TextMeshProUGUI>();
        _choiceText.text = choiceText;

        this.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
            actionButton.sprite = keyDownSprite;

        if (Input.GetKeyUp(keyToPress))
        {
            actionButton.sprite = keyUpSprite;
            OnChoiceSelected.Invoke(_choice);
        }
    }
}
