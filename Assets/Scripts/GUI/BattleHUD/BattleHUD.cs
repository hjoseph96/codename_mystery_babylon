using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] private Image _filledHPBarPiece;
    [SerializeField] private Image _clearHPBarPiece;
    [SerializeField] private RectTransform _hpBarSpawnPoint;
    [SerializeField] private TextMeshProUGUI _unitName;
    [SerializeField] private TextMeshProUGUI _hpRemaining;

    private Unit _unit;
    private List<Image> _filledHPBars = new List<Image>();
    private List<Image> _unfilledHPBars = new List<Image>();


    public void Populate(Unit unit)
    {
        _unit = unit;

        CreateHPBar();
        _unitName.SetText(_unit.Name);
        _hpRemaining.SetText($"{unit.CurrentHealth}/{unit.Stats[UnitStat.MaxHealth].ValueInt}");
    }

    public void DecreaseHealth(int amount)
    {

    }


    public void IncreaseHealth(int amount)
    {

    }


    // Start is called before the first frame update
    private void CreateHPBar()
    {
        var filledBarCount = _unit.CurrentHealth;
        var unfilledBarCount = _unit.Stats[UnitStat.MaxHealth].ValueInt - _unit.CurrentHealth;

        var initialSpawnPoint = _hpBarSpawnPoint.transform.position;
        var localStartPosition = _hpBarSpawnPoint.transform.localPosition;
        Vector3 whereToSpawn = initialSpawnPoint;
        for(int i = 0; i <= filledBarCount; i++)
        {
            var filledBar = Instantiate(_filledHPBarPiece, whereToSpawn, Quaternion.identity, _hpBarSpawnPoint.parent);

            filledBar.transform.localPosition = new Vector2(localStartPosition.x + (16f * i), localStartPosition.y);
            _filledHPBars.Add(filledBar);
        }

        if (unfilledBarCount > 0)
        {
            for(int i = 0; i <= filledBarCount; i++)
            {
                var unfilledBar = Instantiate(_clearHPBarPiece, whereToSpawn, Quaternion.identity, _hpBarSpawnPoint.parent);
                unfilledBar.transform.localPosition = new Vector2(localStartPosition.x + (16f * (i + filledBarCount)), localStartPosition.y);

                _unfilledHPBars.Add(unfilledBar);
            }
        }
    }
}
