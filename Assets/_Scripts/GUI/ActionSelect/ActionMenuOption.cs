using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ActionMenuOption : MenuOption<ActionSelectMenu>
{
    protected Image _image;
    protected Sprite _spriteNormal, _spriteSelected, _spritePressed;
    protected TextMeshProUGUI _objectName;

    public virtual void Init(ActionSelectMenu menu, Sprite normal, Sprite selected, Sprite pressed, string objectText = null)
    {
        Menu = menu;
        _spriteNormal = normal;
        _spriteSelected = selected;
        _spritePressed = pressed;

        _image = GetComponent<Image>();
        if (objectText != null)
        {
            _objectName = GetComponentInChildren<TextMeshProUGUI>();
            if (_objectName)
                _objectName.text = objectText;
            else
                Debug.LogWarning("Error setting name in text, trying to use an object that's not set or missing!");
        }
    }

    public override void SetSelected()
    {
        if (Check())
            _image.sprite = _spriteSelected;
    }

    public override void SetNormal()
    {
        if (Check())
            _image.sprite = _spriteNormal;
    }

    public override void SetPressed()
    {
        if(Check())
            _image.sprite = _spritePressed;
    }

    private bool Check()
    {
        if (_image == null)
        {
            return false;
        }
        else return true;
    }
}