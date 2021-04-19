using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WeaponSelectView : MonoBehaviour
{
    [SerializeField] private Image _leftArrow; 
    [SerializeField] private Image _rightArrow;
    [SerializeField] private Sprite _normalArrow;
    [SerializeField] private Sprite _activatedArrow;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private float _maxWidth = 175f;

    private float _defaultFontSize = -1;
    private bool _arrowsShown;

    public void Populate(Weapon selectedWeapon)
    {
        // If size is negative (first time this method is called), then we set default font size value
        if (_defaultFontSize <= 0)
            _defaultFontSize = _weaponName.fontSize;
        else
        // Otherwise we reset wont size back to normal
            _weaponName.fontSize = _defaultFontSize;

        _weaponIcon.sprite = selectedWeapon.Icon;
        _weaponName.SetText(selectedWeapon.Name);
        _weaponName.SetAllDirty();
        _weaponName.ForceMeshUpdate(true);

        var tr = _weaponIcon.transform;
        // Modify font size if text is too wide
        if (_weaponName.textBounds.size.x >= _maxWidth)
            _weaponName.fontSize = _defaultFontSize * _maxWidth / _weaponName.textBounds.size.x;

        _weaponName.ForceMeshUpdate(true);
        // Set icon position
        tr.localPosition =  new Vector2(_weaponName.textBounds.min.x - 32, tr.localPosition.y);
    }

    public void HideArrows() 
    {
        if (_arrowsShown)
        {
            _leftArrow.SetActive(false);
            _rightArrow.SetActive(false);
            _arrowsShown = false;
        }
    }

    public void ShowArrows()
    {
        if (!_arrowsShown)
        {
            _leftArrow.SetActive(true);
            _rightArrow.SetActive(true);
            _arrowsShown = true;
        }
    }

    public void ActivateLeftArrow()
    {
        DeactivateRightArrow();
        _leftArrow.sprite = _activatedArrow;
    }

    public void ActivateRightArrow()
    {
        DeactivateLeftArrow();
        _rightArrow.sprite = _activatedArrow;
    }

    public void DeactivateLeftArrow()  => _leftArrow.sprite = _normalArrow;
    public void DeactivateRightArrow()  => _rightArrow.sprite = _normalArrow;
}
