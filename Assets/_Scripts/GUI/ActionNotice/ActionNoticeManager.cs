using UnityEngine;

public class ActionNoticeManager : MonoBehaviour, IInitializable
{
    public static ActionNoticeManager Instance;

    [SerializeField] private ActionNotice _actionNotice;

    public bool IsShown => _actionNotice.gameObject.activeSelf;

    public void Init()
    {
        Instance = this;
    }

    public void ShowNotice(string noticeText = "", KeyCode actionButton = KeyCode.Z)
    {
        _actionNotice.Show(actionButton, noticeText);
    }

    public void HideNotice() => _actionNotice.SetActive(false);
}
