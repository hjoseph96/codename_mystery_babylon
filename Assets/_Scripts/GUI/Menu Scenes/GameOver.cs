using Com.LuisPedroFonseca.ProCamera2D;
using DarkTonic.MasterAudio;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : Menu, IInitializable
{

    [SerializeField] private ProCamera2D _uiCamera;
    [SerializeField, SoundGroup] private string startMenuMusic;
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;

    private Transform AudioListenerTransform => _uiCamera.transform;

    private bool _anyKeyPressed = false;


    #region Monobehaviour
    void Start()
    {
        StartCoroutine(PlayMusic());
        _pressAnyKeyText.DOFade(1f, 1.3f).SetLoops(-1, LoopType.Yoyo);
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
    }

    public void GoToStartScreen()
    {
        StartCoroutine(ReturnToStartScreen());
    }
    #endregion

    #region Coroutines
    IEnumerator PlayMusic()
    {
        yield return new WaitForSecondsRealtime(1f);
        MasterAudio.PlaySound3DAtTransform(startMenuMusic, AudioListenerTransform);
    }

    public IEnumerator ReturnToStartScreen()
    {
        UserInput.Instance.InputTarget = null;
        _pressAnyKeyText.DOPause();
        _pressAnyKeyText.color = new Color(_pressAnyKeyText.color.r, _pressAnyKeyText.color.g, _pressAnyKeyText.color.b, 0);
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(SceneLoader.Instance.LoadScene("StartScreen"));
        SceneManager.UnloadSceneAsync("GameOverScreen");
        yield return null;
    }
    #endregion

    #region Inherited Methods
    public override void Activate() { }

    /// <summary>
    /// Triggers the transition to the Pause Menu for proceeding to the game
    /// </summary>
    public override void Close()
    {
        StartCoroutine(ReturnToStartScreen());
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
