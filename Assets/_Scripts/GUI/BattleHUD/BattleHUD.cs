using System.Collections;
using TMPro;
using UnityEngine;


public class BattleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _unitName;
    [SerializeField] private TextMeshProUGUI _hpRemaining;
    [SerializeField] private Healthbar _healthbar;

    private Unit _unit;


    public void Populate(Unit unit)
    {
        _unit = unit;

        _unitName.SetText(_unit.Name);
        _hpRemaining.SetText($"{_unit.CurrentHealth}/{_unit.MaxHealth}");
        _healthbar.Fill(_unit.MaxHealth, _unit.CurrentHealth);
    }

    public void DecreaseHealth(int amount) => StartCoroutine(DecreaseHealthCoroutine(amount));

    private IEnumerator DecreaseHealthCoroutine(int amount)
    {
        amount = Mathf.Min(amount, _unit.CurrentHealth);
        var delay = 0.6f / amount;

        for (var i = 0; i < amount; i++)
        {
            var currentHealth =_unit.CurrentHealth - (i + 1);
            var rowId = Mathf.FloorToInt((float) currentHealth / Healthbar.HealthRowSize);
            var pointId = currentHealth % Healthbar.HealthRowSize;
            _healthbar.Clear(rowId, pointId);

            yield return new WaitForSeconds(delay);

            _hpRemaining.SetText($"{currentHealth}/{_unit.MaxHealth}");
        }

        _unit.DecreaseHealth(amount);
    }
}
