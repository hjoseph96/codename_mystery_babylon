using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class EyeController : MonoBehaviour
{
    [SerializeField] private bool _blinking = true;
    [ReadOnly] private string _defaultState = "open";

    [MinMaxSlider(1, 150)]
    public Vector2Int BlinkIntervalRange = new Vector2Int(5, 90);

    public float MinBlinkInterval { get => BlinkIntervalRange.x; }

    public float MaxBlinkInterval { get => BlinkIntervalRange.y; }

    [MinMaxSlider(1, 10)]
    public Vector2Int BlinkAmountRange = new Vector2Int(1, 7);

    public int MinBlinkAmount { get => (int)BlinkAmountRange.x; }

    public int MaxBlinkAmount { get => (int)BlinkAmountRange.y; }

    public static Dictionary<string, int> AnimationsAsHashes = new Dictionary<string, int>
    {
        { "open",  -1536130140 },
        { "squint", 1329022142 },
        { "closed", 80170468 },
        { "blink", 842569181 }
    };

    private Animator _animator;
    private bool _isBlinking = false;
    private int _currentBlinkAmount = 0;
    private int _targetBlinkAmount;

    public void Squint() => _defaultState = "squint";
    public void CloseEyes() => _defaultState = "closed";
    public void StopBlinking() => _blinking = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_blinking)
            StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        if (!_isBlinking)
        {
            _isBlinking = true;

            var waitTime = Random.Range(MinBlinkInterval, MaxBlinkInterval);

            _targetBlinkAmount = Random.Range(MinBlinkAmount, MaxBlinkAmount);

            yield return new WaitForSeconds(waitTime);

            _animator.Play(AnimationsAsHashes["blink"]);
        }
    }


    #region Animation Event Callbacks

    private void FinishBlinking()
    {
        _currentBlinkAmount += 1;

        if (_currentBlinkAmount == _targetBlinkAmount)
        {
            _animator.Play(_defaultState);
            _currentBlinkAmount = 0;
            _isBlinking = false;
        }

    }

    #endregion
}
