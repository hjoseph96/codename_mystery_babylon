using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Articy.Codename_Mysterybabylon;


[RequireComponent(typeof(ArticyFlowPlayer))]
public class DialogueManager : SerializedMonoBehaviour, IInitializable, IArticyFlowPlayerCallbacks
{
    [SerializeField, ValueDropdown("ChapterList")]
    private string _currentChapter;
    public string CurrentChapter { get => _currentChapter; }
    private List<string> ChapterList()
    {
        // TODO: See if we can get Chapter Names directly from ArticyDatabase
        return new List<string>
        {
            "Chapter 1 - Fort Infiltration"
        };
    }

    
    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes with Portrait")]
    [SerializeField] private GameObject _leftPortraitDialogBoxPrefab;
    
    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private GameObject _rightPortraitDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [Header("Dialog Boxes")]
    [SerializeField] private GameObject _leftDialogBoxPrefab;

    [FoldoutGroup("Dialog Box Prefabs")]
    [SerializeField] private GameObject _rightDialogBoxPrefab;


    private ArticyChapter _chapter;
    public ArticyChapter Chapter { get => _chapter; }

    private ArticyFlowPlayer _flowPlayer;

    private Dictionary<string, string> _ChapterTechnicalNames = new Dictionary<string, string>
    {
        { "Chapter 1 - Fort Infiltration", "Dlg_8EC4C13A" }
    };

    // Start is called before the first frame update
    public void Init()
    {
        _flowPlayer = GetComponent<ArticyFlowPlayer>();

        var chapterTechnicalName = _ChapterTechnicalNames[CurrentChapter];
        var chapterDialogue = ArticyDatabase.GetObject(chapterTechnicalName) as Dialogue;

        var mainCharacters = ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>();
        var firstEntity = mainCharacters[0];
        Debug.Log("Catch me.");

        _chapter = new ArticyChapter(chapterDialogue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        var objWithText = aObject as IObjectWithText;
        if (objWithText != null)
        {
        }
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {

    }
}
