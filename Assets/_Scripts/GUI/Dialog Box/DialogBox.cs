using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

using Sirenix.OdinInspector;
using Febucci.UI;
using TMPro;

public class DialogBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextAnimatorPlayer _textAnimatorPlayer;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private GameObject _zButton;
    [SerializeField] private Transform _portraitSpawnPoint;
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
        UpdateListeners();

        _zButton.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsTyping)
                _textAnimatorPlayer.SkipTypewriter();
            else if (_currentTextIndex < _texts.Count)
                ShowNextText();
            else if (OnDialogueDisplayComplete != null)
            {
                // Fire event signaling the end of this dialogue fragment.
                OnDialogueDisplayComplete.Invoke();
            }
                
        }
    }

    /// <summary>
    /// Set text, split it and show the first part using typewriter
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        Split(text);

        _currentTextIndex = 0;
    }

    public void HideNextButton() => _zButton.SetActive(false);

    /// <summary>
    /// Set currently active portrait
    /// </summary>
    /// <param name="portrait"></param>
    public void SetActivePortrait(AnimatedPortrait portrait)
    {
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

    // Split text and store the result in _texts List
    private bool Split(string text)
    {
        _texts.Clear();

        _text.text = Regex.Replace(text, "<.*?>", string.Empty);
        _text.ForceMeshUpdate();
        if (!_text.isTextOverflowing)
        {
            _texts.Add(text);
            return false;
        }

        var sb = new StringBuilder(text);
        sb.Replace(". ", ".$$$")
            .Replace("! ", "!$$$")
            .Replace("? ", "?$$$")
            .Replace(": ", ":$$$")
            .Replace("; ", ";$$$")
            .Replace("… ", "…$$$")
            .Replace("\r\n", "\r\n$$$")
            .Replace("\r", "\r$$$")
            .Replace("\n", "\n$$$");

        var sentences = sb.ToString().Split(new[] { "$$$" }, StringSplitOptions.RemoveEmptyEntries);

        sb.Clear();

        foreach (var s in sentences)
        {
            var lastText = sb.ToString();

            sb.Append(s);
            _text.text = Regex.Replace(sb.ToString(), "<.*?>", string.Empty);
            _text.ForceMeshUpdate();

            if (_text.isTextOverflowing)
            {
                _texts.Add(lastText);
                _text.text = s;

                sb.Clear();
                sb.Append(s);
            }
        }

        if (!string.IsNullOrEmpty(_text.text))
            _texts.Add(sb.ToString());

        _text.text = string.Empty;

        return true;
    }

    public void ShowNextText()
    {
        this.SetActive(true);
        _textAnimatorPlayer.ShowText(_texts[_currentTextIndex++]);
    }

    // Clear listeners and add new ones
    private void UpdateListeners()
    {
        _textAnimatorPlayer.onTypewriterStart.RemoveAllListeners();
        _textAnimatorPlayer.onTextShowed.RemoveAllListeners();

        if (_currentPortrait != null)
        {
            _textAnimatorPlayer.onTypewriterStart.AddListener(_currentPortrait.Talk);
            _textAnimatorPlayer.onTextShowed.AddListener(_currentPortrait.SetNeutral);
        }

        BeginTyping();
        _textAnimatorPlayer.onTextShowed.AddListener(EndTyping);
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
