using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemSlot : MenuOption<UnitInventoryMenu>
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _title, _durability;
    [SerializeField] private GameObject _equippedIcon;


    public bool IsEmpty => Item == null;
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

        Menu = GetComponentInParent<UnitInventoryMenu>();
    }

    public void Populate(Item item)
    {
        Item = item;

        _icon.sprite = item.Icon;
        _icon.Show();
        
        _title.text = item.Name;

        // TODO: Use overloads, this is a TEMPORARY solution!
        if (item is Weapon weapon)
        {
            if (weapon.CurrentDurability < 0)
                _durability.text = "---";
            else
                _durability.SetText("{0}/{1}", weapon.CurrentDurability, weapon.MaxDurability);
        }
        else
            _durability.SetText("None");

        if (item.IsEquipped)
            ShowEquippedIcon();
    }

    public void Clear()
    {
        Item = null;

        _icon.sprite = null;
        _icon.Hide();

        _title.text = "";
        _durability.text = "";

        _equippedIcon.SetActive(false);
    }

    public override void Execute()
    {
        if (!IsEmpty)
            Menu.SelectItemSlot();
    }

    public override void SetSelected()
    {
        _allInOneMat.SetFloat(GhostTransparency, 0.413f);
        _allInOneMat.SetInt(GhostColorBoost, 5);
        _allInOneMat.EnableKeyword("GHOST_ON");

        _iconAllInOneMat.SetFloat(OutlineAlpha, 0.467f);
    }

    public override void SetNormal()
    {
        _allInOneMat.DisableKeyword("GHOST_ON");
        _iconAllInOneMat.SetFloat(OutlineAlpha, 0);
    }

    public void HideEquippedIcon() {
        if (_equippedIcon.activeSelf)
            _equippedIcon.SetActive(false);
    }

    
    public void ShowEquippedIcon() {
        if (!_equippedIcon.activeSelf)
            _equippedIcon.SetActive(true);
    }
}
