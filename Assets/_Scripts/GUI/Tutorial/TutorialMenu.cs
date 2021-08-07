using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TutorialMenu : Menu, IInitializable
{
    public static TutorialMenu Instance;
    [SerializeField] private List<GameObject> _Pages;
    [SerializeField] TextMeshProUGUI _backIndicator;
    [SerializeField] TextMeshProUGUI _nextIndicator;
    private int _currentPageIndex = 0;

    protected GameObject _CurrentPage { get => _Pages[_currentPageIndex]; }
    private bool _waiting = false;
    [SerializeField]
    private float _timeToWait = .25f;


    public void Init()
    {
        Instance = this;
    }

    public void Show()
    {
        Activate();

        _Pages[_currentPageIndex].SetActive(true);
    }

    public override void Activate()
    {
        base.Activate();
        RefreshPages();
    }

    public override void ProcessInput(InputData input)
    {
        if (_waiting)
            return;

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (_currentPageIndex == _Pages.Count - 1)
                    CloseAndSetGridCursor();
                else
                    NextPage();
                break;

            case KeyCode.X:
                if (_currentPageIndex == 0)
                    CloseAndSetGridCursor();
                else
                    PreviousPage();
                break;
        }
    }

    private void CloseAndSetGridCursor()
    {
        Close();
        GridCursor.Instance.SetFreeMode();
    }

    private void NextPage()
    {
        _CurrentPage.SetActive(false);
        
        _currentPageIndex++;
        RefreshPages();

        _CurrentPage.SetActive(true);
    }

    private void PreviousPage()
    {
        _CurrentPage.SetActive(false);

        _currentPageIndex--;
        RefreshPages();

        _CurrentPage.SetActive(true);
    }

    private void RefreshPages()
    {
        _waiting = true;
        _currentTime = _timeToWait;
        StartCoroutine(Counter());

        if (_currentPageIndex == 0)
        {
            _backIndicator.text = "Close";
            _nextIndicator.text = "Next";
        }

        if (_currentPageIndex == _Pages.Count - 1)
        {
            _nextIndicator.text = "Finish";
            _backIndicator.text = "Back";
        }
    }

    private IEnumerator Counter()
    {
        while(_currentTime > 0)
        {
            _currentTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        _waiting = false;
    }
}
