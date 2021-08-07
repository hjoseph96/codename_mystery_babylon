using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadMenu : Menu
{
    public GameObject content;
    public Transform prefabParent;
    public SaveItemPrefab prefab;
    public ScrollRect scroll;
    public bool saving;
    public List<SaveItemPrefab> options = new List<SaveItemPrefab>();
    public UICursor _cursor;
    private int _selectedOptionIndex;
    public SaveLoadActionMenu actionMenu;
    public System.Action OnSlotsChanged;

    #region Monobehaviour
    public void Start()
    {
        actionMenu.PerformRebuildCheck += RefreshItems;
        actionMenu.PerformedChange += delegate
        {
            Debug.Log("Refreshing slots");
            OnSlotsChanged?.Invoke();
            scroll.content.position = Vector3.zero;
        };
    }

    private void OnDestroy()
    {
        actionMenu.PerformRebuildCheck -= RefreshItems;
    }
    #endregion

    #region List Related Methods
    /// <summary>
    /// Loads save data from the save files and creates the prefabs showing the data from the file
    /// </summary>
    public void CreateItemsList()
    {
        //TEMPORARY // TODO: Get All save files and add them to the list 
        for(int i = 0; i < 6; i++)
        {
            CreateSlot().Populate(false);
        }

        if (saving)
        {
            CreateSlot().Populate();
            Debug.Log("Detected Saving, creating empty prefab for saving new slot");
        }
    }

    /// <summary>
    /// Prefabs are inserted at 0 in options, and transforms sibling index is set to 0
    /// <br>therefore, create recent slots after older slots, and new save at 0</br>
    /// </summary>
    /// <returns></returns>
    public SaveItemPrefab CreateSlot()
    {
        var slot = GameObject.Instantiate(prefab, prefabParent).GetComponent<SaveItemPrefab>();
        slot.Menu = this;
        options.Insert(0, slot);
        slot.transform.SetAsFirstSibling();
        return slot;
    }

    /// <summary>
    /// Used explicitly when selecting the new save slot by selecting the first index
    /// </summary>
    private void SaveNewSlot()
    {
        var slot = CreateSlot();
        //slot.Populate(/*Current Save Game Data*/);
        Save();
    }

    /// <summary>
    /// Called from the SaveLoadActionMenu, when confirming overwriting of a slot
    /// </summary>
    public void OverwriteExistingSlot()
    {
        var slotToOverwrite = options[_selectedOptionIndex];
        Save();
    }

    private void SnapTo(RectTransform child)
    {
        var contentPos = (Vector2)scroll.transform.InverseTransformPoint(scroll.content.position);
        var childPos = (Vector2)scroll.transform.InverseTransformPoint(child.position);
        var endPos = contentPos - childPos;
        // If no horizontal scroll, then don't change contentPos.x
        if (!scroll.horizontal) endPos.x = contentPos.x;
        // If no vertical scroll, then don't change contentPos.y
        if (!scroll.vertical) endPos.y = contentPos.y;
        scroll.content.anchoredPosition = endPos;
    }

    private void RefreshItems()
    {
        TakeControl();
    }
    #endregion

    #region Core Saving and Loading calls
    public void Save()
    {
        // Perform Save operation

        // Create new prefab with that information
        // Alternatively close out of Menu

    }

    public void Load()
    {
        // Perform Load Operation

        // Close out of all menus?
    }
    #endregion

    #region Inherited and related Methods
    /// <summary>
    /// Assign control to this object for the input manager used from the Save Load context menu and Through Activate
    /// <br>KeepPos will not reset the _selectedOptionIndex to 0</br>
    /// </summary>
    public void TakeControl(bool keepPos = false)
    {
        UserInput.Instance.InputTarget = this;
        if(!keepPos)
            _selectedOptionIndex = 0;
        MoveSelectionToOption(_selectedOptionIndex, true);
        scroll.content.position = Vector3.zero;
    }

    /// <summary>
    /// Recreates item list, and assigns control to this item
    /// <br>saving value should be set before calling this function to activate window</br>
    /// </summary>
    public override void Activate()
    {
        CreateItemsList();
        content.SetActive(true);
        TakeControl();
        actionMenu.saving = saving;
        actionMenu.PreviousMenu = this;
    }


    public override void Close() { }

    /// <summary>
    /// This is Main method to close Save Load Menu
    /// <br></br>Destroys the items list, allowing refreshing of them after changing data.
    /// </summary>
    public override void Deactivate()
    {
        UserInput.Instance.InputTarget = PreviousMenu;
        PreviousMenu.Activate();
        content.SetActive(false);
        _cursor.transform.SetParent(transform, false);
        for (int i = 0; i < options.Count; i++)
        {
            Destroy(options[i].gameObject);
        }
        options.Clear();
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, options.Count - 1);


        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(options[index].transform, false);
        _cursor.MoveTo(new Vector2(-200, 0), instant);
        SnapTo(options[index].GetComponent<RectTransform>());
    }


    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                    break;

                if (!saving || _selectedOptionIndex != 0)
                {
                    actionMenu.Activate();
                }
                else
                    SaveNewSlot();

                break;

            case KeyCode.X:
                if (input.KeyState == KeyState.Down)
                    break;

                Deactivate();
                break;
        }
    }
    #endregion
}
