using TMPro;
using UnityEngine;

public class ItemStatDisplay : MonoBehaviour
{
    public string StatName { get; private set; }
    private TextMeshProUGUI _statName;
    private TextMeshProUGUI _statDisplay; 
    // Start is called before the first frame update
    void Awake()
    {
        TextMeshProUGUI[] fields = GetComponentsInChildren<TextMeshProUGUI>();
        _statName = fields[0];
        StatName = _statName.text.ToUpper();

        _statDisplay = fields[1];
    }

    public void SetStat(string textToDisplay) 
    {
        _statDisplay.SetText(textToDisplay);
    }
}
