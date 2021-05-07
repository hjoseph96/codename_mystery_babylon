using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ActionMenuOption : MenuOption<ActionSelectMenu>
{
    private Image _image;
    private Sprite _spriteNormal, _spriteSelected, _spritePressed;

    public void Init(ActionSelectMenu menu, Sprite normal, Sprite selected, Sprite pressed)
    {
        Menu = menu;
        _spriteNormal   = normal;
        _spriteSelected = selected;
        _spritePressed  = pressed;

        _image = GetComponent<Image>();
    }

    public override void SetSelected()
    {
        _image.sprite = _spriteSelected;
    }

    public override void SetNormal()
    {
        _image.sprite = _spriteNormal;
    }

    public override void SetPressed()
    {
        _image.sprite = _spritePressed;
    }
}