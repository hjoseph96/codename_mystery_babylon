using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;
using DarkTonic.MasterAudio;



public class PhaseDisplay : SerializedMonoBehaviour
{
    [SerializeField] private Dictionary<TurnPhase, Color> turnColors = new Dictionary<TurnPhase, Color>();
    [SerializeField, SoundGroup] private string playerPhaseSound;
    [SerializeField, SoundGroup] private string enemyPhaseSound;
    [SerializeField, SoundGroup] private string otherEnemyPhaseSound;
    [SerializeField, SoundGroup] private string allyPhaseSound;
    [SerializeField, SoundGroup] private string neutralPhaseSound;


    [HideInInspector] public Action OnDisplayComplete;

    private TurnPhase _phase;
    private Image _phaseNameHolder;
    private TextMeshProUGUI _phaseText;

    private bool _isDisplaying = false;
    public bool IsDisplaying { get { return _isDisplaying; } }
    private bool _setupComplete = false;

    public void Show(TurnPhase phase)
    {
        _isDisplaying = true;

        _phase = phase;
        
        Setup();
        _phaseText.DOFade(0, 0.01f);
        _phaseNameHolder.DOFade(0, 0.001f);

        _phaseNameHolder.color = turnColors[_phase];
        _phaseText.text = $"{_phase.ToString()} Phase";
        _phaseText.ForceMeshUpdate(true, true);
        
        StartCoroutine(AnimatePhaseTextIn());
    }

    private void Setup()
    {
        this.SetActive(true);

        if (!_setupComplete)
        {
            _phaseNameHolder = GetComponentInChildren<Image>();
            _phaseText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (!_phaseText.IsActive())
            _phaseText.SetActive(true);

        _setupComplete = true;
    }
   

    private IEnumerator AnimatePhaseTextIn()
    {
        _phaseNameHolder.DOFade(1, 0.2f);

        yield return new WaitForSeconds(0.21f);

        _phaseText.DOFade(1, 0.2f);

        StartCoroutine(AnimatePhaseTextOut());
    }


    private IEnumerator AnimatePhaseTextOut()
    {
        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(_phaseText);
        Sequence sequence = DOTween.Sequence();

        
        float originalVolume = MasterAudio.GrabBusByName("Music").volume;
        sequence.onComplete += delegate() {
            StartCoroutine(FadeOut(originalVolume));
        };

        MasterAudio.FadeBusToVolume("Music", .2f, 0.5f);
        MasterAudio.PlaySound3DFollowTransform(PhaseSound(), CampaignManager.AudioListenerTransform);
        yield return new WaitForSecondsRealtime(3f);

        for (int i = 0; i < animator.textInfo.characterCount; ++i) {
            if (!animator.textInfo.characterInfo[i].isVisible) continue;
        
            var tweenTarget = new Vector3(_phaseText.rectTransform.rect.min.x - (40 + (50 * i)), _phaseText.rectTransform.rect.center.y, 0);
            yield return new WaitForSeconds(0.1f);

            sequence.Join(animator.DOOffsetChar(i, tweenTarget, 0.4f));
        }
    }

    private IEnumerator FadeOut(float originalVolume)
    {
        yield return new WaitForSeconds(5f);
        MasterAudio.FadeBusToVolume("Music", originalVolume, 1.5f);

        _phaseText.SetActive(false);
        _phaseNameHolder.DOFade(0, 0.5f).onComplete += delegate ()
        {
            OnDisplayComplete.Invoke();
            _isDisplaying = false;
        };
    }


    private string PhaseSound()
    {
        switch(_phase)
        {
            case TurnPhase.Player:
                return playerPhaseSound;
            case TurnPhase.Enemy:
                return enemyPhaseSound;
            case TurnPhase.OtherEnemy:
                return otherEnemyPhaseSound;
            case TurnPhase.Ally:
                return allyPhaseSound;
            case TurnPhase.Neutral:
                return neutralPhaseSound;            
        }

        throw new Exception($"Invalid TurnPhase: {_phase.ToString()}");
    }
}
