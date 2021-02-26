using System.Collections;
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
        _hpRemaining.SetText($"{_unit.CurrentHealth}/{_unit.Stats[UnitStat.MaxHealth].ValueInt}");
    }

    public void DecreaseHealth(int amount)
    {
        StartCoroutine(DecrementBars(amount));
    }

    private IEnumerator DecrementBars(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int lastFilledBarIndex = _filledHPBars.Count - 1;

            var targetHPPiece = _filledHPBars[lastFilledBarIndex];

            var currentWidth = targetHPPiece.rectTransform.sizeDelta.x;
            var currentHeight = targetHPPiece.rectTransform.sizeDelta.y;
            var position = targetHPPiece.rectTransform.localPosition;

            targetHPPiece.rectTransform.sizeDelta = new Vector2(currentWidth + 2, currentHeight + 8);
            targetHPPiece.rectTransform.localPosition = new Vector2(position.x, position.y - 4);

            yield return new WaitForSeconds(0.03f);

            targetHPPiece.rectTransform.sizeDelta = new Vector2(currentWidth, currentHeight);
            targetHPPiece.rectTransform.localPosition = position;
            targetHPPiece.sprite = _clearHPBarPiece.sprite;

            _hpRemaining.SetText($"{_unit.CurrentHealth - (i + 1)}/{_unit.Stats[UnitStat.MaxHealth].ValueInt}");
            

            _filledHPBars.Remove(targetHPPiece);

            var insertPoint = _unfilledHPBars.Count - 1;
            if (insertPoint < 0) insertPoint = 0;
            _unfilledHPBars.Insert(insertPoint, targetHPPiece);
        }

        _unit.CurrentHealth -= amount;
    }


    public void IncreaseHealth(int amount)
    {

    }

    public void Reset()
    {
        _unitName.SetText("");
        _hpRemaining.SetText("");

        foreach (Image image in _filledHPBars)
            Destroy(image);
        
        foreach(Image image in _unfilledHPBars)
            Destroy(image);
        
        _filledHPBars.Clear();
        _unfilledHPBars.Clear();
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
