using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is a base, and should not be used as a component directly
/// </summary>
[RequireComponent(typeof(Image))]
public class ItemSlot : MenuOption<Menu>
{
    [SerializeField] protected Image _icon;
    [SerializeField] protected TextMeshProUGUI _title, _durability;
    [SerializeField] protected GameObject _equippedIcon;


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
    }

    private void Start()
    {
        Menu = GetComponentInParent<UnitInventoryMenu>();
    }

    public virtual void Populate(Item item)
    {
        Item = item;

        _icon.sprite = item.Icon;
        _icon.Show();
        _title.text = item.Name;

        if (item.IsEquipped)
            ShowEquippedIcon();
    }

    public virtual void Clear()
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
        Debug.Log("Execute is not set up to display anything, ensure child class overrides Execute!");
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
