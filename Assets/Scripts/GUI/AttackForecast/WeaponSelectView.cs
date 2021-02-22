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
    bool _arrowsShown = false;

    public void Populate(Weapon selectedWeapon)
    {
        _weaponName.SetText(selectedWeapon.Name);
        _weaponIcon.sprite = selectedWeapon.Icon;

        var textLocalPosition = _weaponName.textInfo.characterInfo[0].bottomLeft;
        _weaponIcon.transform.localPosition =  new Vector2(
            textLocalPosition.x - 60, _weaponIcon.transform.localPosition.y
        );
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
