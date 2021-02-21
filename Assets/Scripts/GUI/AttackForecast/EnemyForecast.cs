using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyForecast : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TextMeshProUGUI _damage;
    [SerializeField] private TextMeshProUGUI _hitChance;
    [SerializeField] private TextMeshProUGUI _critChance;
    
    public void Populate(Unit unit, Unit playerUnit)
    {
        _name.SetText(unit.Name);
        _weaponName.SetText(unit.EquippedWeapon.Name);
        _weaponIcon.sprite = unit.EquippedWeapon.Icon;

        Dictionary<string, int> preview = unit.PreviewAttack(playerUnit);
        _health.SetText($"{unit.CurrentHealth}");
        _damage.SetText($"{preview["ATK_DMG"]}");
        _hitChance.SetText($"{preview["ACCURACY"]}%");
        _critChance.SetText($"{preview["CRIT_RATE"]}%");
    }
}
