using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Febucci.UI;
using TMPro;
using UnityEngine;


public class DialogBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private TextAnimatorPlayer _textAnimatorPlayer;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private GameObject _zButton;
    [SerializeField] private AnimatedPortrait _currentPortrait;

    public AnimatedPortrait CurrentPortrait => _currentPortrait;
    public bool IsTyping { get; private set; }

    private readonly List<string> _texts = new List<string>();
    private int _currentTextIndex;
    
    private void Start()
    {
        UpdateListeners();

        _zButton.SetActive(false);
        SetText(_text.text);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsTyping)
                _textAnimatorPlayer.SkipTypewriter();
            else if (_currentTextIndex < _texts.Count)
                ShowNextText();
        }
    }

    /// <summary>
    /// Set text, split it and show the first part using typewriter
    /// </summary>
    /// <param name="text"></param>
    public void SetText(string text)
    {
        Split(text);

        if (CurrentPortrait != null)
            SetActivePortrait(CurrentPortrait);

        _currentTextIndex = 0;
        ShowNextText();
    }

    /// <summary>
    /// Set currently active portrait
    /// </summary>
    /// <param name="portrait"></param>
    public void SetActivePortrait(AnimatedPortrait portrait)
    {
        _currentPortrait = portrait;
        _name.text = portrait.Name;
        UpdateListeners();
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

    private void ShowNextText()
    {
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

        _textAnimatorPlayer.onTypewriterStart.AddListener(BeginTyping);
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
