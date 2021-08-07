using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderDetailsView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _unitName;
    [SerializeField] private TextMeshProUGUI _unitLevel;
    [SerializeField] private TextMeshProUGUI _unitHealth;
    [SerializeField] private TextMeshProUGUI _unitClass;
    [SerializeField] private Image _unitPortrait;


    public void Populate(Unit unit)
    {
        _unitName.SetText(unit.Name);
        _unitHealth.SetText($"{unit.CurrentHealth}/{unit.MaxHealth}");
        _unitLevel.SetText($"{unit.Level}");
        _unitClass.SetText($"{unit.Class.Title}");
        _unitPortrait.sprite = unit.Portrait.Default;
    }

    public void PopulateSmall(Unit unit)
    {
        _unitName.SetText(unit.Name);
        _unitClass.SetText($"{unit.Class.Title}");
    }

    public void MakeSmall()
    {

    }

    public void MakeLarge()
    {

    }
}
