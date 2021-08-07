using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;
using TMPro;
using Articy.Codename_Mysterybabylon;

public class DialogBox : MonoBehaviour
{

    [SerializeField] private Color DefaultColor   = new Color(229, 206, 206, 255);
    [SerializeField] private Color NightModeColor = new Color(178, 170, 170, 255);

    private TypewritingText _typewritingText;
    private Transform _followTransform;
    private Transform _transformToFlip;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private GameObject _zButton;
    [SerializeField] private Transform _portraitSpawnPoint;
    [SerializeField] private Image _dialogBubble;

    private AnimatedPortrait _currentPortrait;
    [ShowInInspector]
    public AnimatedPortrait CurrentPortrait { get => _currentPortrait; }

    public bool IsTyping { get; private set; }
    public bool IsActive { get; private set; }

    public Action OnDialogueDisplayComplete;

    // For MapDialogue -- non animated Portrait dialogue
    private EntityReference _speaker;
    public EntityReference Speaker { get => _speaker; }

    private Sex _speakerGender;

    public Action<Direction> OnFlipInvoked;
    [SerializeField]
    public Direction direction = Direction.Left;

    private bool _flipping = false;
    private bool _paused = false;

    #region MonoBehaviour
    private void Start()
    {
        if (_typewritingText == null)
            _typewritingText = GetComponentInChildren<TypewritingText>();

        IsActive = true;
        UpdateListeners();

        _zButton.SetActive(false);
        PauseMenu.OnGamePaused += SetPausedState;
    }

    private void OnDestroy()
    {
        PauseMenu.OnGamePaused -= SetPausedState;
    }

    private void Update()
    {
        if (_paused)
            return;

        if ((Input.GetKeyDown(KeyCode.Z) && !DialogueManager.Instance.IsRunningAnAction) && IsActive)
        {
           if (IsTyping)
                _typewritingText.SetRevealSpeed(3.5f);
            else if (_typewritingText.CurrentTextIndex < _typewritingText.Texts.Count)
                ShowNextText();
            else if (OnDialogueDisplayComplete != null)
            {
                // Fire event signaling the end of this dialogue fragment.
                OnDialogueDisplayComplete.Invoke();
            }       
        }

        if (Input.GetKeyDown(KeyCode.Space))
            if (IsTyping)
                _typewritingText.SkipText();

        if (Input.GetKeyUp(KeyCode.Z))
            if (IsTyping)
                _typewritingText.SetRevealSpeed(1f);
    }

    private void FixedUpdate()
    {
        if (_paused)
            return;

        if (_followTransform)
        {
            if (transform.position != _followTransform.transform.position)
            {
                transform.position = _followTransform.position;
            }
        }
        
        if (_flipping)
            return;

        if(_transformToFlip)
        {
            if (direction == Direction.Right) 
            {
                if (transform.position.x > _transformToFlip.position.x)
                {
                    OnFlipInvoked?.Invoke(Direction.Right);
                    _flipping = true;
                }
            }
            else if (direction == Direction.Left)
            {
                if(transform.position.x < _transformToFlip.position.x)
                {
                    OnFlipInvoked?.Invoke(Direction.Left);
                    _flipping = true;
                }
            }
        }
    }
    #endregion

    #region Lambdas
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDefaultColor() => _dialogBubble.color = DefaultColor;
    public void SetNightModeColor() => _dialogBubble.color = NightModeColor;
    #endregion
    private void SetPausedState(bool isPaused) 
    {
        _paused = isPaused;
    }

    /// <summary>
    /// Set text, split it and show the first part using typewriter
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        CheckData();

        _typewritingText.SetText(text);
    }

    private void CheckData()
    {
        if (_typewritingText == null)
            _typewritingText = GetComponentInChildren<TypewritingText>();
    }

    public void SetText(TypewritingText.TextData data)
    {
        CheckData();
        _typewritingText.SetText(data);
    }

    public void HideNextButton() => _zButton.SetActive(false);

    /// <summary>
    /// Set currently active portrait
    /// </summary>
    /// <param name="portrait"></param>
    public void SetActivePortrait(AnimatedPortrait portrait)
    {
        if (_typewritingText == null)
            _typewritingText = GetComponentInChildren<TypewritingText>();

        if (CurrentPortrait != null)
            Destroy(CurrentPortrait.gameObject);

        if (portrait == CurrentPortrait)
            return;

        var newPortrait = Instantiate(
            portrait,
            _portraitSpawnPoint.position,
            Quaternion.identity,
            _portraitSpawnPoint
        );

        _currentPortrait    = newPortrait;
        _name.text          = portrait.Name;

        UpdateListeners();
    }

    /// <param name="entityReference">The Entity speaking</param>
    /// <param name="followTransform">The transform that the dialogue box will follow<br>Should be a child object of the BubblePositionController</br></param>
    /// <param name="transformToFlip">The transform that when it passes, will flip directions</param>
    public void SetSpeaker(EntityReference entityReference, Transform followTransform, Transform transformToFlip)
    {
        _speaker = entityReference;
        _name.text = entityReference.EntityName;
        _followTransform = followTransform;
        _transformToFlip = transformToFlip;
    }

    public void SetSpeakerGender(Sex gender) => _speakerGender = gender;

    
    public void ShowNextText()
    {
        this.SetActive(true);
        _typewritingText.ShowNextText();
    }

    // Clear listeners and add new ones
    private void UpdateListeners()
    {
        _typewritingText.RemoveTypewriterEvents();

        BeginTyping();

        _typewritingText.AttachOnStartTypingEvent(BeginTyping);

        _typewritingText.SetSpeakerGender(_speakerGender);

        _typewritingText.AttachTextRevealSoundEvent(delegate ()
        {
            IsTyping = true;
        });

        if (_currentPortrait != null)
            _typewritingText.AttachPortraitEvents(_currentPortrait.Talk, _currentPortrait.SetNeutral);

        _typewritingText.AttachOnTextShowedEvent(EndTyping);

    }


    private void BeginTyping()
    {
        _zButton.SetActive(false);
        IsTyping = true;
    }

    // Callback for onTextShowed
    private void EndTyping()
    {
        _zButton.SetActive(true);
        IsTyping = false;
    }

    public TypewritingText.TextData GetTextData()
    {
        return _typewritingText.GetText();
    }
}
