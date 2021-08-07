using System;
using System.Linq;

using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;


public class AnimatedDoor : GridAnimatedTile
{
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

    [Header("Inside Door Spawn Point")]
    [SerializeField] private Transform _insideDoorSpawnPoint;
    public Transform InsideDoorSpawnPoint { get => _insideDoorSpawnPoint; }

    [Header("On Open/Close Dialogue")]
    [SerializeField] private MapDialogue _onOpenDialogue;
    [SerializeField] private MapDialogue _onLockedDialogue;
    [SerializeField] private MapDialogue _onCloseDialogue;

    [HideInInspector] public Action OnDoorOpened;
    [HideInInspector] public Action OnDoorClosed;


    public bool IsOpen { get; private set; }

    private ActionNoticeManager _noticeManager;
    private SpriteRenderer _renderer;
    private DoorLock _doorLock;

    private bool _isAnimating;
    private bool _isFlipped;
    public bool IsFlipped { get => _isFlipped; }

    public bool IsLocked()
    {
        if (_doorLock != null)
            return _doorLock.IsLocked;
        else
            return false;
    }


    // TODO: Add ability to close door and a boolean flag to allow closing/opening: ie: IsLocked & CanClose
    protected void Start()
    {
        _noticeManager  = ActionNoticeManager.Instance;
        _renderer       = GetComponent<SpriteRenderer>();
        _doorLock       = GetComponentInChildren<DoorLock>();

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

    protected virtual void Update()
    {
        if (IsTriggered && !IsOpen && !_isAnimating)
        {
            if (!_noticeManager.IsShown)
                _noticeManager.ShowNotice("To Open");

            if (Input.GetKeyDown(KeyCode.Z) && !_isAnimating )
            {
                var isDoorLocked = IsLocked();
                if (!isDoorLocked)
                {
                    OpenDoor();
                }
                else
                {
                    var trigger = triggers.Where((trigger) => trigger.FoundObject != null).First();
                    var unit = trigger.FoundObject.GetComponent<Unit>();

                    // For now, find the unit in the trigger. this will fail if non-unit people enter the trigger...
                    if (_doorLock.CanOpen(unit))
                    {
                        _doorLock.UponUnlocked += delegate () { OpenDoor(); };
                        _doorLock.Unlock();
                    } else
                    {
                        MasterAudio.PlaySound3DFollowTransform("door_locked", CampaignManager.AudioListenerTransform);

                        if (_onLockedDialogue != null)
                            _onLockedDialogue.StartDialogue();
                    }

                }

            } 
        }
    }

    private void OpenDoor()
    {
        _noticeManager.HideNotice();

        if (_onOpenDialogue != null)
            OnDoorOpened += delegate () { _onOpenDialogue.StartDialogue(); };

        Open();
    }
    public void FlipX()
    {
        _renderer.flipX = !_renderer.flipX;
        _isFlipped = _renderer.flipX;
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
        animator.Play("Open");

        _colliderGroup.Revert();
        tileCollider.enabled = false;

        if (_changeSortingLayerUponOpening)
            spriteRenderer.sortingLayerName = _openSortingLayer;

        IsOpen = true;
        _isAnimating = false;

        if (OnDoorOpened != null)
        {
            OnDoorOpened.Invoke();
            OnDoorOpened = null;
        }
    }

    private void CloseAnimationEvent()
    {
        animator.Play("Closed");

        _colliderGroup.Apply();
        tileCollider.enabled = true;

        if (_changeSortingLayerUponClosing)
            spriteRenderer.sortingLayerName = _closedSortingLayer;

        IsOpen = false;
        _isAnimating = false;

        if (OnDoorClosed != null)
        {
            OnDoorClosed.Invoke();
            OnDoorClosed = null;
        }
    }

}
