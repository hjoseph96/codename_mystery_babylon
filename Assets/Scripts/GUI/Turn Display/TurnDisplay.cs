using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;

public class TurnDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _turnNumber;
    private Image _background;

    [HideInInspector] public Action OnDisplayHidden;

    private bool _setupComplete = false;
    
    public void Show(int turnNumber)
    {
        Setup(turnNumber);
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        Fade(0, 0.5f).onComplete += delegate ()
        {
            this.SetActive(false);
            OnDisplayHidden.Invoke();
        };
    }

    public void ChangeNumber(int turnNumber)
    {
        _turnNumber.text = turnNumber.ToString();

        _turnNumber.DOScale(1.5f, 0.3f).onComplete += delegate ()
        {
            _turnNumber.DOScale(1, 0.2f);
        };
    }

    private void Setup(int turnNumber)
    {
        if (!_setupComplete)
        {
            _background = GetComponent<Image>();
            _setupComplete = true;
        }

        if (!this.IsActive())
        {
            this.SetActive(true);
        }

        if (_turnText == null || _turnNumber == null)
            throw new System.Exception("Unset TextMeshProUGUI serialized field in TurnDisplay...");

        _turnNumber.text = turnNumber.ToString();
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.2f);

        Fade(1, 0.6f, true);
    }

    private Tweener Fade(int opacity ,float duration, bool scaleNumber = false)
    {
        _background.DOFade(opacity, duration);
        _turnNumber.DOFade(opacity, duration);

        if (scaleNumber)
        {
            _turnNumber.DOScale(1.5f, 0.3f).onComplete += delegate()
            {
                _turnNumber.DOScale(1, 0.2f);
            };

        }

        return _turnText.DOFade(opacity, duration);
    }
}
