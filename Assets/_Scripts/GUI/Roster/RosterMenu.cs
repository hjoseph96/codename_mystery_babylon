using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RosterMenu : Menu
{
    public GameObject CardPrefab;
    public Transform CardsParent;
    public AdvancedDisplayMenu displayMenu;
    private List<CharacterCard> _cards = new List<CharacterCard>();
    private List<Canvas> CardsCanvasComponent = new List<Canvas>();

    private float lerpingIndex;
    private bool _isLerping;
    [FoldoutGroup("UI Parameters")]
    [SerializeField] private Vector2 origin;
    [SerializeField] private float rotationAngle;
    private int _previousSelectedIndex;
    [SerializeField] private int _selectedOptionIndex;

    public int SelectedIndex
    {
        get { return _selectedOptionIndex; }
        set
        {
            _previousSelectedIndex = _selectedOptionIndex;
            _selectedOptionIndex = Mathf.Clamp(value, 0, _cards.Count - 1);

            if (_previousSelectedIndex != _selectedOptionIndex)
                StartCoroutine(UpdateCardsPosition());
        }
    }

    private void Init()
    {
        gameObject.SetActive(true);

        _previousSelectedIndex = _selectedOptionIndex;

        origin = Vector2.zero;

        // TODO: Change to permanent storage of player units instead
        var playerUnits = EntityManager.Instance.PlayerUnits();

        for (int i = 0; i < playerUnits.Count; i++)
            InstantiateCard(playerUnits[i]);

        StartCoroutine(UpdateCardsPosition());
        UserInput.Instance.InputTarget = this;
    }

    private void Clear()
    {
        foreach (var card in _cards)
        {
            Destroy(card.gameObject);
        }

        _cards.Clear();
        CardsCanvasComponent.Clear();
    }
    private void InstantiateCard(Unit u)
    {
        // get the card component
        var card = Instantiate(CardPrefab, origin, Quaternion.identity, CardsParent).GetComponent<CharacterCard>();

        // Instance the card with data from the unit
        card.AssignData(u);
        _cards.Add(card);
        CardsCanvasComponent.Add(card.GetComponent<Canvas>());
        // Assign reference for menu
        card.Menu = this;
    }
    private IEnumerator UpdateCardsPosition()
    {
        UpdateCardsSortingLayer();
        _isLerping = true;
        float interpolation = 0f;
        while (interpolation < 1)
        {
            interpolation += .1f;
            lerpingIndex = Mathf.Lerp(_previousSelectedIndex, SelectedIndex, interpolation);
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                var indexOffset = i - lerpingIndex;
                UpdateCardPosition(card, indexOffset);
                if (i == SelectedIndex)
                    PopInOut(i, interpolation);
            }

            yield return new WaitForEndOfFrame();
        }
        _isLerping = false;
    }

    private void UpdateCardPosition(CharacterCard card, float indexOffset)
    {
        var cardRect = (RectTransform)card.transform;
        var deltaX = cardRect.rect.width / 5;
        var deltaY = -cardRect.rect.height / 2;
        cardRect.anchoredPosition = origin + new Vector2(indexOffset * deltaX * Mathf.Sign(indexOffset), indexOffset * deltaY);
        card.transform.eulerAngles = new Vector3(card.transform.eulerAngles.x, card.transform.eulerAngles.y, indexOffset * rotationAngle);
    }

    private void PopInOut(int cardIndex, float lerpValue)
    {
        RectTransform card = (RectTransform)_cards[cardIndex].transform;
        float XPos = Mathf.Lerp(card.rect.width / 2, 0, lerpValue);
        card.anchoredPosition += new Vector2(card.anchoredPosition.x + XPos, card.anchoredPosition.y);
    }

    private void UpdateCardsSortingLayer()
    {
        for (int i = 0; i < CardsCanvasComponent.Count; i++)
        {
            var card = _cards[i];
            var cardCanvas = CardsCanvasComponent[i];
            if (i < SelectedIndex)
            {
                cardCanvas.sortingOrder = i + 100;
                card.Fade();
            }
            else if (i == SelectedIndex)
            {
                cardCanvas.sortingOrder = 200;
                card.Highlight();
            }
            else
            {
                cardCanvas.sortingOrder = -i + 100;
                card.Fade();
            }
        }
    }

    public void SelectCard(CharacterCard card)
    {
        var unit = EntityManager.Instance.PlayerUnits()[card.transform.GetSiblingIndex()];
        Deactivate();
        displayMenu.PreviousMenu = this;
        displayMenu.Show(unit);

    }

    private void Show()
    {
        Clear();
        Init();
    }

    public override void Activate()
    {
        base.Activate();
        Show();
    }

    [Button("Previous")]
    private void Previous()
    {
        if (_isLerping)
            return;

        SelectedIndex--;
    }

    [Button("Next")]
    private void Next()
    {
        if (_isLerping)
            return;

        SelectedIndex++;
    }

    #region Menu Functions
    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                    break;

                    PressOption(SelectedOption);
                    SelectedOption.Execute();
                break;

            case KeyCode.X:
                if (input.KeyState == KeyState.Down)
                    break;

                Close();
                gameObject.SetActive(false);
                PreviousMenu.Activate();
                break;
        }
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_isLerping)
            SelectedIndex -= input.y;

        return _cards[SelectedIndex];
    }
    #endregion

}
