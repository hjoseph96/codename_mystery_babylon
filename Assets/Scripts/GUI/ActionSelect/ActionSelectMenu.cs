using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ActionSelectMenu : MonoBehaviour
{
    public Transform pointer;
    public Transform selectedHighlight;
    public List<RectTransform> menuOptions;
    public UnitInventoryMenu inventoryMenu;

    public UnityEvent OnMenuClose;
    private Unit _selectedUnit;
    private UICursor _pointer;
    private List<RectTransform> activeOptions = new List<RectTransform>();
    private RectTransform _selectedOption;
    private Vector2 _firstOptPos, _secondOptPos, _thirdOptPos, _fourthOptPos;
    private bool _open;

     // Start is called before the first frame update
    void Start()
    {
        _firstOptPos     = menuOptions[0].localPosition;
        _secondOptPos    = menuOptions[1].localPosition;
        _thirdOptPos     = menuOptions[2].localPosition;
        _fourthOptPos    = menuOptions[3].localPosition;
        
        this.SetActive(false);
        _open = false;
        _pointer = GetComponentInChildren<UICursor>();
    }

    public void ShowMenu(Unit unit) 
    {
        _selectedUnit = unit;

        var attackOption    = menuOptions[0];

        if (!_selectedUnit.CanAttack()) {   // TODO:Trying to Move the options up, if attack isn't an option. Also, resize the background.

            attackOption.gameObject.SetActive(false);

            var itemsOption = menuOptions[1];
            itemsOption.localPosition = _firstOptPos;
            _selectedOption = itemsOption;
            activeOptions.Add(itemsOption);

            var tradeOption = menuOptions[2];
            tradeOption.localPosition = _secondOptPos;
            activeOptions.Add(tradeOption);

            var waitOption = menuOptions[3];
            waitOption.localPosition = _thirdOptPos;
            activeOptions.Add(waitOption);
        }

        // TODO: Add CanTrade to Unit, is friendly Unit in an immediate neighbor cell?
        // TODO: If Unit Cannot Trade, remove Trade Option, move Wait option up in it's place

        // TODO: Resize Height of window based on # of active options. Get Smaller if not all options are in use.
        
        this.SetActive(true);
        _open = true;
    }
    
   

    // Update is called once per frame
    void Update()
    {
        if (_open) {
            // Feel free to refactor.
            var selectedIndex = activeOptions.IndexOf(_selectedOption);
            var lastOptionIndex = activeOptions.Count - 1;
            var vertAxis = Input.GetAxisRaw("Vertical");

            if (!_pointer.IsMoving) {

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
                        SelectAction(_selectedOption);
                        return;
                     }
                }

            }

            SetSelectedOption(selectedIndex);
            // TODO: Lerp alpha of selectedHighlight min: 103 max: 172 over a seconds duration.

            if (Input.GetKeyDown(KeyCode.X)) {
                CloseMenu();
                OnMenuClose.Invoke();
            }
        }
    }

    private void SelectAction(RectTransform selectedOption) {
        var option = selectedOption.name;

        switch (option) {
            case "Attack Option":
                break;
            case "Items Option":
                CloseMenu();
                inventoryMenu.ShowMenu(_selectedUnit);
                break;
            case "Trade Option":
                break;
            case "Wait Option":
                break;
        }
    }

    private void SetSelectedOption(int targetOptionIndex) {
        RectTransform targetOption = activeOptions[targetOptionIndex];

        // Move Pointer
        var newPointerLocation = new Vector2(pointer.localPosition.x, targetOption.localPosition.y - 10);
        _pointer.MoveTo(newPointerLocation);
       
        
        // Move Selected Highlight
        var newSelectedLocation = new Vector2(selectedHighlight.localPosition.x, targetOption.localPosition.y - 5);
        if (!_pointer.IsMoving) {
            selectedHighlight.localPosition = newSelectedLocation;
        } else {
            selectedHighlight.localPosition = Vector2.Lerp(
                selectedHighlight.localPosition, 
                newSelectedLocation,
                _pointer.moveSpeed - 1 * Time.smoothDeltaTime
            );
        }

        _selectedOption = targetOption;
    }

    private void ResetOptionPositions() {
        var attackOption = menuOptions[0];
        attackOption.localPosition = _firstOptPos;

        var itemsOption = menuOptions[1];
        itemsOption.localPosition = _secondOptPos;

        var tradeOption = menuOptions[2];
        tradeOption.localPosition = _thirdOptPos;

        var waitOption = menuOptions[3];
        waitOption.localPosition = _fourthOptPos;
    }

    private void CloseMenu() {
        _open = false;

        this.SetActive(false);
        
        ResetOptionPositions();
        activeOptions.Clear();
    }
    
}
