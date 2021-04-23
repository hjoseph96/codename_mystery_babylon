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
    private ArticyReference _articyRef;

    private Dictionary<string, string> _ChapterTechnicalNames = new Dictionary<string, string>
    {
        { "Chapter 1 - Fort Infiltration", "Dlg_8EC4C13A" }
    };

    // Start is called before the first frame update
    public void Init()
    {
        _flowPlayer = GetComponent<ArticyFlowPlayer>();
        _articyRef  = GetComponent<ArticyReference>();

        var chapterDialogueObj = _articyRef.GetObject<ArticyObject>();

        if (chapterDialogueObj is Dialogue == false)
            throw new System.Exception("[DialogueManager] Assigned ArticyRef is not a Dialogue...");

        var chapterDialogue = chapterDialogueObj as Dialogue;

        _chapter = new ArticyChapter(chapterDialogue);

        _flowPlayer.startOn = (ArticyRef)chapterDialogue;

        _flowPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnFlowPlayerPaused(IFlowObject aObject)
    {
        if (aObject is IDialogue)
        {
            _flowPlayer.Play();
            return;
        }

        var objWithText = aObject as IObjectWithText;
        if (objWithText != null)
        {
            Debug.Log("Catch me");
        }
    }

    public void OnBranchesUpdated(IList<Branch> aBranches)
    {
    }
}
