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
    private UICursor _pointer;
    private ItemSlot _selectedSlot;

    // Start is called before the first frame update
    void Start()
    {
        if (itemSlots.Count > UnitInventory.MaxSize)
            throw new System.Exception("Given more item slots than allowed per user.");
        
        _pointer = GetComponentInChildren<UICursor>();

        this.SetActive(false);
        _open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_open) {
            if (!_pointer.IsMoving) {
                var selectedIndex = itemSlots.IndexOf(_selectedSlot);
                var lastOptionIndex = itemSlots.Count - 1;
                var vertAxis = Input.GetAxisRaw("Vertical");

                if (vertAxis < 0) {
                    if (selectedIndex == lastOptionIndex) {
                        selectedIndex = 0;
                    } else {
                        selectedIndex += 1;
                    }
                }

                if (vertAxis > 0) {
                    if (selectedIndex == 0) {
                        selectedIndex = lastOptionIndex;
                    } else {
                        selectedIndex -= 1;
                    }
                }

                if (vertAxis == 0) {
                     if (Input.GetKeyDown(KeyCode.Z)) {
                        // SelectAction(_selectedOption);
                        return;
                     }
                }

                MoveCursorTo(selectedIndex);
            }

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

        this.SetActive(true);
        _open = true;
    }

    void CloseMenu() {
        this.SetActive(false);
        _open = false;

        GridCursor.Instance.SetFreeMode();
        OnMenuClose.Invoke();
    }

    void MoveCursorTo(int itemSlotIndex) {
        var itemSlot = itemSlots[itemSlotIndex];

        Vector2 cursorDestination = new Vector2(
            _pointer.transform.localPosition.x, itemSlot.transform.localPosition.y
        );

        _pointer.MoveTo(cursorDestination);
        if (!_pointer.IsMoving)
            _selectedSlot = itemSlot;
    }
}
