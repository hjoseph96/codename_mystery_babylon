using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerForecast : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _health;
    [SerializeField] private TextMeshProUGUI _damage;
    [SerializeField] private TextMeshProUGUI _hitChance;
    [SerializeField] private TextMeshProUGUI _critChance;
    
    public void Populate(Unit unit, Unit enemyUnit)
    {
        _name.SetText(unit.Name);

        Dictionary<string, int> preview = unit.PreviewAttack(enemyUnit);
        _health.SetText($"{unit.CurrentHealth}");
        _damage.SetText($"{preview["ATK_DMG"]}");
        _hitChance.SetText($"{preview["ACCURACY"]}%");
        _critChance.SetText($"{preview["CRIT_RATE"]}%");
    }
}
