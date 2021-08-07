using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using TMPro;
using UnityEngine.PlayerLoop;
using DarkTonic.MasterAudio;

public class StartScreen : Menu, IInitializable
{

    [SerializeField] private ProCamera2D _uiCamera;
    [SerializeField, SoundGroup] private string startMenuMusic;
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
    [SerializeField] private PauseMenu _pauseMenu;
    [SerializeField] private Vector2 _defaultPosition;
    [SerializeField] private Vector2 _moveToPosition;
    [SerializeField] private float _speedToMoveAt;

    public GameObject content;

    private ProCamera2DTransitionsFX _cameraTransitions;

    private Transform AudioListenerTransform => _uiCamera.transform;

    private bool _anyKeyPressed = false;
    private RectTransform rect;


    #region Monobehaviour
    void Start()
    {
        StartCoroutine(PlayMusic());
        _cameraTransitions.TransitionEnter();
        _pressAnyKeyText.DOFade(1f, 1.3f).SetLoops(-1, LoopType.Yoyo);
        rect = GetComponent<RectTransform>();
        _defaultPosition = rect.anchoredPosition;
    }

    private void Update()
    {
        if (!_anyKeyPressed && Input.anyKey)
        {
            _anyKeyPressed = true;
            Close();
        }
    }
    #endregion

    #region Methods
    public void Init()
    {
        _isInitialized = true;
        _cameraTransitions = _uiCamera.GetComponent<ProCamera2DTransitionsFX>();
    }
    #endregion

    #region Coroutines
    IEnumerator PlayMusic()
    {
        yield return new WaitForSecondsRealtime(1f);
        MasterAudio.PlaySound3DAtTransform(startMenuMusic, AudioListenerTransform);
    }

    private IEnumerator Move()
    {
        while(Vector2.Distance(rect.anchoredPosition, _moveToPosition) > 0.1f)
        {
            rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, _moveToPosition, _speedToMoveAt * Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Moving done");
        content.SetActive(false);
        _pauseMenu.PreviousMenu = this;
        _pauseMenu.Activate();
        yield return null;
    }

    private IEnumerator Return()
    {
        while (Vector2.Distance(rect.anchoredPosition, _defaultPosition) > 0.1f)
        {
            rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, _defaultPosition, _speedToMoveAt * Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }

        content.SetActive(true);
        _pauseMenu.Close();
        UserInput.Instance.InputTarget = this;
        _anyKeyPressed = false;
        yield return null;
    }
    #endregion

    #region Inherited Methods
    [ContextMenu("Open")]
    public override void Activate()
    {
        StartCoroutine(Return());
    }

    /// <summary>
    /// Triggers the transition to the Pause Menu for proceeding to the game
    /// </summary>
    public override void Close()
    {
        StartCoroutine(Move());
    }

    public override void Deactivate() { }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        return base.MoveSelection(input);
    }

    public override void ProcessInput(InputData input)
    {
        
    }
    #endregion

}
