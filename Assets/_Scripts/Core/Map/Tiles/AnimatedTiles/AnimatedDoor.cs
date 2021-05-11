using UnityEngine;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;


public class AnimatedDoor : GridAnimatedTile
{
    [InfoBox("AnimatedDoor requires AnimatedTileGroups named 'Open' and 'Closed' to update TileConfigurations based on animation state. If this door is not used in battle, it is optional.", InfoMessageType.Warning)]

    [InfoBox("If this AnimatedDoor open based on user input -- you must have a BoxCollider2D with isTrigger on this GameObject.", InfoMessageType.Warning)]

    [InfoBox("The 'Close' & 'Open' animations on this door MUST call the AnimationEvents in this script!", InfoMessageType.Warning)]


    [Header("Upon Start Settings")]
    [SerializeField] private bool _startClosed;
    [SerializeField] private bool _animateOnStart;

    [Header("Collider Settings")] 
    [SerializeField] private ColliderGroupSimple _colliderGroup;

    [Header("Sorting Layer Changes")]
    [SerializeField] private bool _changeSortingLayerUponOpening;
    [SerializeField, ShowIf("_changeSortingLayerUponOpening"), SortingLayer] private string _openSortingLayer;

    [SerializeField] private bool _changeSortingLayerUponClosing;
    [SerializeField, ShowIf("_changeSortingLayerUponClosing"), SortingLayer] private string _closedSortingLayer;

    [Header("Sound")]
    [SerializeField, SoundGroup] private string _openSound;
    [SerializeField, SoundGroup] private string _closeSound;


    public bool IsOpen { get; private set; }

    private bool _isAnimating;
    private ActionNoticeManager _noticeManager;


    // TODO: Add ability to close door and a boolean flag to allow closing/opening: ie: IsLocked & CanClose
    private void Start()
    {
        _noticeManager = ActionNoticeManager.Instance;

        if (_startClosed)
        {
            if (_animateOnStart)
            {
                Open(true);
                Close();
            }
            else
                Close(true);
        }
        else
        {
            if (_animateOnStart)
            {
                Close(true);
                Open();
            }
            else
                Open(true);
        }

        foreach (var trigger in triggers)
            trigger.OnLeftTrigger += _noticeManager.HideNotice;
    }

    private void Update()
    {
        if (IsTriggered && !IsOpen && !_isAnimating)
        {
            if (!_noticeManager.IsShown)
                _noticeManager.ShowNotice("To Open");

            if (Input.GetKeyDown(KeyCode.Z) && !_isAnimating)
            {
                _noticeManager.HideNotice();
                Open();
            }
        }
    }

    public void SetOpenImmediate()
    {
        animator.Play("Open");
        OpenAnimationEvent();
    }

    public void Open(bool instant = false)
    {
        if (instant)
        {
            OpenAnimationEvent();
            return;
        }

        animator.Play("Opening");
        _isAnimating = true;
        MasterAudio.PlaySound3DFollowTransform(_openSound, CampaignManager.AudioListenerTransform);
    }

    public void Close(bool instant = false)
    {
        if (instant)
        {
            CloseAnimationEvent();
            return;
        }

        animator.Play("Closing");
        _isAnimating = true;
        MasterAudio.PlaySound3DFollowTransform(_closeSound, CampaignManager.AudioListenerTransform);
    }

    private void OpenAnimationEvent()
    {
        _colliderGroup.Revert();
        tileCollider.enabled = false;

        if (_changeSortingLayerUponOpening)
            spriteRenderer.sortingLayerName = _openSortingLayer;

        IsOpen = true;
        _isAnimating = false;
    }

    private void CloseAnimationEvent()
    {
        _colliderGroup.Apply();
        tileCollider.enabled = true;

        if (_changeSortingLayerUponClosing)
            spriteRenderer.sortingLayerName = _closedSortingLayer;

        IsOpen = false;
        _isAnimating = false;
    }

}
