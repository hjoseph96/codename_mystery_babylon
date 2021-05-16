using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Febucci.UI;
using TMPro;

public class TypewritingText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private TextAnimatorPlayer _textAnimatorPlayer;

    private readonly List<string> _texts = new List<string>();
    private int _currentTextIndex;


    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _textAnimatorPlayer = GetComponent<TextAnimatorPlayer>();
    }


    public void ShowNextText() => _textAnimatorPlayer.ShowText(_texts[_currentTextIndex++]);


    public void SetText(string text)
    {
        Split(text);

        _currentTextIndex = 0;
    }


    public void RemoveTypewriterEvents()
    {
        _textAnimatorPlayer.onTypewriterStart.RemoveAllListeners();
        _textAnimatorPlayer.onTextShowed.RemoveAllListeners();
    }


    public void AttachPortraitEvents(UnityAction onTypewriterStart, UnityAction onTextShowed)
    {
        _textAnimatorPlayer.onTypewriterStart.AddListener(onTypewriterStart);
        _textAnimatorPlayer.onTextShowed.AddListener(onTextShowed);
    }

    public void AttachOnTextShowedEvent(UnityAction onTextShowed) => _textAnimatorPlayer.onTextShowed.AddListener(onTextShowed);

    public void AttachOnStartTypingEvent(UnityAction onTextShowed) => _textAnimatorPlayer.onTypewriterStart.AddListener(onTextShowed);

    public void SkipText() => _textAnimatorPlayer.SkipTypewriter();

    public void SetRevealSpeed(float speed) => _textAnimatorPlayer.SetTypewriterSpeed(speed);



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
}
