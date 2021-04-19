using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;


public class ActionNotice : SerializedMonoBehaviour
{
    private TextMeshProUGUI _notice;

    // TODO: We may want to create a seperate MonoBehaviour for `AnimatedKeyButton`s, in case we want to not only loop them being pressed and unpressed
    //       But have their state change along with the state of the User Input.
    //          IE: OnKeyDown it will remain in a pressed position && OnKeyUp it will go up

    private ActionButton _actionButton;


    // TODO: These definitions should live in a ScriptableObject or something -- only one place to store these references.
    public List<ActionButton> ActionButtons;

    // Start is called before the first frame update
    void Awake()
    {
        _notice = GetComponentInChildren<TextMeshProUGUI>();
        _actionButton = GetComponentInChildren<ActionButton>();
        this.SetActive(false);
    }

    private ActionButton GetActionButton(ActionButtonType buttonType) => ActionButtons.First((button) => button.ButtonType == buttonType);

    public void SetNoticeText(string noticeText)
    {
        this.SetActive(true);
        _notice.text = noticeText;
    }

    public void SetActionButton(ActionButtonType button)
    {
        var spawnPosition = _actionButton.transform.position;

        _actionButton.transform.SetParent(null);
        Destroy(_actionButton);
        _actionButton = null;

        var buttonPrefab = GetActionButton(button);
        _actionButton = Instantiate<ActionButton>(buttonPrefab, spawnPosition, Quaternion.identity, this.transform);
    }

    public void Show(ActionButtonType actionButton = ActionButtonType.Z, string noticeText = "")
    {
        SetNoticeText(noticeText);
        SetActionButton(actionButton);
        this.SetActive(true);
    }
}
