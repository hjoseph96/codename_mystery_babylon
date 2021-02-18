using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class UnitInventoryMenu : MonoBehaviour
{
    public RectTransform pointer;
    public List<ItemSlot> itemSlots;
    public UnityEvent OnMenuClose;

    private bool _open;
    private Unit _activePlayer;
    private ItemSlot _selectedSlot;

    // Start is called before the first frame update
    void Start()
    {
        if (itemSlots.Count > UnitInventory.MaxSize)
            throw new System.Exception("Given more item slots than allowed per user.");
        
        this.SetActive(false);
        _open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_open) {
            if (Input.GetKeyDown(KeyCode.X))
                CloseMenu();
        }
    }

    public void ShowMenu(Unit player) {
        GridCursor.Instance.ClearAll();
        GridCursor.Instance.SetLockedMode();

        _activePlayer = player;
        
        Item[] items = player.Inventory.GetItems<Item>();


        for(int i = 0; i < items.Length; i++) {
            var itemSlot = itemSlots[i];
            var inventoryItem = items[i];

            itemSlot.Populate(inventoryItem);
        }
        
        _selectedSlot = itemSlots[0];
        _selectedSlot.SetSelected();

        this.SetActive(true);
        _open = true;
    }

    void CloseMenu() {
        this.SetActive(false);
        _open = false;

        GridCursor.Instance.SetFreeMode();
        OnMenuClose.Invoke();
    }

}
