using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDetailsView : MonoBehaviour
{
    [SerializeField] private GameObject weaponStatsView;
    Item _item;
    List<ItemStatDisplay> _weaponStats = new List<ItemStatDisplay>();
    ItemDescriptionView _itemDescription;
    
    public void Show(Item item, Vector2 spawnPoint) {
        _item = item;

        bool isItemAWeapon = _item.ItemType == ItemType.Weapon; 
        if (isItemAWeapon) {
            foreach(var stat in GetComponentsInChildren<ItemStatDisplay>()) {
                SetWeaponStat(stat, item as Weapon);

                _weaponStats.Add(stat);
            }
        }
        weaponStatsView.SetActive(isItemAWeapon);

        _itemDescription = GetComponentInChildren<ItemDescriptionView>();
        _itemDescription.SetDescription(item.Description);

        SetPosition(spawnPoint);

        this.SetActive(true);
    }

    public void Close() {
        this.SetActive(false);
    }

    public void SetPosition(Vector2 localPosition) {
        var newPosition = new Vector2(this.transform.localPosition.x, localPosition.y - 96);
        this.transform.localPosition = newPosition;
    }

    private void SetWeaponStat(ItemStatDisplay statDisplay, Weapon weapon) {
        switch (statDisplay.StatName) {
            case "DMG":
                statDisplay.SetStat($"{weapon.Stats[WeaponStat.Damage].ValueInt}");
                
                break;
            case "HIT":
                statDisplay.SetStat($"{weapon.Stats[WeaponStat.Hit].ValueInt}%");
                
                break;
            case "CRIT":
                statDisplay.SetStat($"{weapon.Stats[WeaponStat.CriticalHit].ValueInt}%");
                
                break;
            case "RNG":
                var minRange = weapon.Stats[WeaponStat.MinRange].ValueInt;
                var maxRange = weapon.Stats[WeaponStat.MaxRange].ValueInt;

                var onlyOneRange = minRange == maxRange;
                if (onlyOneRange)
                    statDisplay.SetStat($"{maxRange}");
                else 
                    statDisplay.SetStat($"{minRange}-{maxRange}");

                break;
            case "WT":
                statDisplay.SetStat($"{weapon.Weight}");
                
                break;
            case "RANK":
                var rank = (WeaponRank)weapon.RequiredRank;
                statDisplay.SetStat(rank.ToString());
                
                break;
        }
    }
}
