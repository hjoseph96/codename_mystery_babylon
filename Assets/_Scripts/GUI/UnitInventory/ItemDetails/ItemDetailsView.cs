using System.Collections.Generic;
using UnityEngine;

public class ItemDetailsView : MonoBehaviour
{
    [SerializeField] private GameObject weaponStatsView;
    Item _item;
    List<ItemStatDisplay> _weaponStats = new List<ItemStatDisplay>();
    ItemDescriptionView _itemDescription;
    
    public void Show(Item item, Vector2 spawnPoint) 
    {
        _item = item;
        this.SetActive(true);

        var isItemAWeapon = _item.ItemType == ItemType.Weapon; 
        if (isItemAWeapon)
        {
            foreach(var stat in GetComponentsInChildren<ItemStatDisplay>()) 
            {
                stat.SetText(item as Weapon);
                _weaponStats.Add(stat);
            }
        }
        weaponStatsView.SetActive(isItemAWeapon);

        _itemDescription = GetComponentInChildren<ItemDescriptionView>();
        _itemDescription.SetDescription(item.Description);

        SetPosition(spawnPoint);
    }

    public void Close() 
    {
        this.SetActive(false);
    }

    public void SetPosition(Vector2 localPosition) 
    {
        var tr = transform;
        var newPosition = new Vector2(tr.localPosition.x, localPosition.y - 147);
        tr.localPosition = newPosition;
    }
}
