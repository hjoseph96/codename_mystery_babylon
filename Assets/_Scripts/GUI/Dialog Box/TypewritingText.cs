using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using TMPro;
using Febucci.UI;
using DarkTonic.MasterAudio;

using Articy.Codename_Mysterybabylon;

public class TypewritingText : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private TextAnimatorPlayer _textAnimatorPlayer;
    private Sex _speakerGender;

    private List<string> _texts = new List<string>();
    public List<string> Texts { get => _texts;  }
    private int _currentTextIndex;
    public int CurrentTextIndex { get => _currentTextIndex; }

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

    public void SetSpeakerGender(Sex gender) => _speakerGender = gender;

    public void RemoveTypewriterEvents()
    {
        _textAnimatorPlayer.onTypewriterStart.RemoveAllListeners();
        _textAnimatorPlayer.onTextShowed.RemoveAllListeners();
        _textAnimatorPlayer.onCharacterVisible.RemoveAllListeners();
    }


    public void AttachPortraitEvents(UnityAction onTypewriterStart, UnityAction onTextShowed)
    {
        _textAnimatorPlayer.onTypewriterStart.AddListener(onTypewriterStart);
        _textAnimatorPlayer.onTextShowed.AddListener(onTextShowed);
    }

    public void AttachOnTextShowedEvent(UnityAction onTextShowed) => _textAnimatorPlayer.onTextShowed.AddListener(onTextShowed);

    public void AttachOnStartTypingEvent(UnityAction onTextStart) => _textAnimatorPlayer.onTypewriterStart.AddListener(onTextStart);


    public void AttachTextRevealSoundEvent( Action onCharVisible = null )
    {
        _textAnimatorPlayer.onCharacterVisible.AddListener(delegate (char characterShown)
        {
            if (characterShown != ' ')
            {
                if (_speakerGender == Sex.Male)
                    MasterAudio.PlaySound3DFollowTransform("character_reveal_male", CampaignManager.AudioListenerTransform);
                else if (_speakerGender == Sex.Female)
                    MasterAudio.PlaySound3DFollowTransform("character_reveal_female", CampaignManager.AudioListenerTransform);
            }

            if (onCharVisible != null)
                onCharVisible.Invoke();
        });

        _textAnimatorPlayer.onTextShowed.AddListener(delegate ()
        {
            MasterAudio.PlaySound3DFollowTransform("text_complete", CampaignManager.AudioListenerTransform);
        });
    }

    public void SkipText() => _textAnimatorPlayer.SkipTypewriter();

    public void SetRevealSpeed(float speed) => _textAnimatorPlayer.SetTypewriterSpeed(speed);

    /// <summary>
    /// used to return the text currently on the typewriter 
    /// </summary>
    /// <returns></returns>
    public TextData GetText()
    {
        return new TextData() { _currentIndex = _currentTextIndex, texts = _texts };
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
    public void SetText(TextData data)
    {
        _texts = data.texts;
        _currentTextIndex = data._currentIndex;
    }

    public struct TextData
    {
        public int _currentIndex;
        public List<string> texts;
    }

}
