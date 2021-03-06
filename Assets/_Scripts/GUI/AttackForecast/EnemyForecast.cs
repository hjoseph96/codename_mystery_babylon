using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyForecast : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _damage;
    [SerializeField] private TextMeshProUGUI _hitChance;
    [SerializeField] private TextMeshProUGUI _critChance;
    [SerializeField] private TextMeshProUGUI _multiAttack;

    private Unit _enemyUnit;
    private Unit _playerUnit;
    
    public void Populate(Unit unit, Unit playerUnit)
    {
        _enemyUnit = unit;
        _playerUnit = playerUnit;

        _name.SetText(unit.Name);
        _portrait.sprite = unit.Portrait.Default;

        if (unit.EquippedWeapon != null)
        {
            _weaponName.SetText(unit.EquippedWeapon.Name);
            _weaponIcon.sprite = unit.EquippedWeapon.Icon;
        }
        else {
            _weaponName.SetText("---");
            
            var invisibleColor = _weaponIcon.color;
            invisibleColor.a = 0;
            _weaponIcon.color = invisibleColor;
        }
        

        Dictionary<string, int> preview = unit.PreviewAttack(playerUnit, unit.EquippedWeapon);
        _health.SetText($"{unit.CurrentHealth}");
        
        _damage.SetText(PreviewValue(preview["ATK_DMG"]));
        _hitChance.SetText(PreviewValue(preview["ACCURACY"], true));
        _critChance.SetText(PreviewValue(preview["CRIT_RATE"], true));

        bool showDoubleAttack = unit.CanDoubleAttack(playerUnit, unit.EquippedWeapon);
        _multiAttack.SetActive(showDoubleAttack);
    }

    private string PreviewValue(int value, bool percentage = false)
    {
        string displayString;

        if (_enemyUnit.CanAttack(_playerUnit))
        {
            displayString = $"{value}";
            if (percentage) displayString += "%";
            return displayString;
        } else {
            displayString = "---";
        }

        return displayString;
    }

    private void ShowDoubleAttack()
    {
        var damageTextInfo = _damage.textInfo;
        var textLocalPosition = damageTextInfo.characterInfo[damageTextInfo.characterCount - 1].bottomLeft;
        _multiAttack.transform.localPosition = new Vector2(
            textLocalPosition.x + 60, _multiAttack.transform.localPosition.y 
        );
        _multiAttack.SetActive(true);
    }
}
