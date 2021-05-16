using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;
using TMPro;

public class DialogBox : MonoBehaviour
{

    [SerializeField] private Color DefaultColor   = new Color(229, 206, 206, 255);
    [SerializeField] private Color NightModeColor = new Color(178, 170, 170, 255);

    private TypewritingText _typewritingText;

    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private GameObject _zButton;
    [SerializeField] private Transform _portraitSpawnPoint;
    [SerializeField] private Image _dialogBubble;

    [SerializeField, ValueDropdown("LeftOrRight")] private Direction _orientation;
    private List<Direction> LeftOrRight() => new List<Direction> { Direction.Left, Direction.Right };

    private AnimatedPortrait _currentPortrait;
    [ShowInInspector]
    public AnimatedPortrait CurrentPortrait { get => _currentPortrait; }

    public bool IsTyping { get; private set; }

    public Action OnDialogueDisplayComplete;

    // For MapDialogue -- non animated Portrait dialogue
    private EntityReference _speaker;
    public EntityReference Speaker { get => _speaker; }

    private readonly List<string> _texts = new List<string>();
    private int _currentTextIndex;

    private void Start()
    {
        if (_typewritingText == null)
            _typewritingText = GetComponentInChildren<TypewritingText>();

        UpdateListeners();

        _zButton.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsTyping)
                _typewritingText.SetRevealSpeed(3.5f);
            else if (_currentTextIndex < _texts.Count)
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

    public void SetDefaultColor() => _dialogBubble.color = DefaultColor;

    public void SetNightModeColor() => _dialogBubble.color = NightModeColor;

    /// <summary>
    /// Set text, split it and show the first part using typewriter
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        if (_typewritingText == null)
            _typewritingText = GetComponentInChildren<TypewritingText>();

        _typewritingText.SetText(text);
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

    public void SetSpeaker(EntityReference entityReference)
    {
        _speaker = entityReference;
        _name.text = entityReference.EntityName;
    }

    
    public void ShowNextText()
    {
        this.SetActive(true);
        _typewritingText.ShowNextText();
    }

    // Clear listeners and add new ones
    private void UpdateListeners()
    {
        _typewritingText.RemoveTypewriterEvents();

        if (_currentPortrait != null)
            _typewritingText.AttachPortraitEvents(_currentPortrait.Talk, _currentPortrait.SetNeutral);

        _typewritingText.AttachOnStartTypingEvent(BeginTyping);

        _typewritingText.AttachOnTextShowedEvent(EndTyping);
    }

    // Callback for onTypewriterStart
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
}
