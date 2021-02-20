using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _title, _durability;

    public bool IsEmpty => !_icon.IsActive();
    
    public Item Item { get; private set; }

    private Material _allInOneMat;
    private Material _iconAllInOneMat;

    private static readonly int GhostTransparency = Shader.PropertyToID("_GhostTransparency");
    private static readonly int GhostColorBoost = Shader.PropertyToID("_GhostColorBoost");
    private static readonly int OutlineAlpha = Shader.PropertyToID("_OutlineAlpha");

    private void Awake()
    {
        var image = GetComponent<Image>();
        _allInOneMat = new Material(image.material);
        _iconAllInOneMat = new Material(_icon.material);

        image.material = _allInOneMat;
        _icon.material = _iconAllInOneMat;
    }

    public void Populate(Item item)
    {
        Item = item;

        _icon.sprite = item.Icon;
        _icon.Show();
        
        _title.text = item.Name;

        if (item.CurrentDurability < 0)
           _durability.text = "---";
        else
           _durability.SetText("{0}/{1}", item.CurrentDurability, item.MaxDurability);
    }

    public void SetSelected()
    {
        _allInOneMat.SetFloat(GhostTransparency, 0.413f);
        _allInOneMat.SetInt(GhostColorBoost, 5);
        _allInOneMat.EnableKeyword("GHOST_ON");

        _iconAllInOneMat.SetFloat(OutlineAlpha, 0.467f);
    }

    public void SetDeselected()
    {
        _allInOneMat.DisableKeyword("GHOST_ON");
        _iconAllInOneMat.SetFloat(OutlineAlpha, 0);
    }
}
