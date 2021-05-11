using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Sirenix.OdinInspector;

public class MapDialogue : SerializedMonoBehaviour
{
    [SerializeField]
    private List<EntityReference> _Participants;
    public List<EntityReference> Participants { get => _Participants; }

    [HideInInspector]
    public System.Action<GameObject> OnDialogueBegin;


    private ArticyDataContainer _articyData;
    private bool _hasStarted        = false;
    private bool _isWithinTrigger   = false;
    private GameObject _player;

    private int _currentDialogIndex = 0;
    private ArticyObject _currentDialogue;
    // Start is called before the first frame update
    void Start()
    {
        _articyData = GetComponent<ArticyDataContainer>();
        _currentDialogue = _articyData.References[_currentDialogIndex];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !_hasStarted && _isWithinTrigger)
        {
            StartDialogue();
            ActionNoticeManager.Instance.HideNotice();

            if (OnDialogueBegin != null)
                OnDialogueBegin.Invoke(_player);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
           ActionNoticeManager.Instance.ShowNotice("To Talk");
            _isWithinTrigger = true;

            _player = other.gameObject;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            ActionNoticeManager.Instance.HideNotice();
            _isWithinTrigger = false;
            
            _player = null;
        }
    }


    public void StartDialogue()
    {
        DialogueManager.Instance.OnDialogueComplete += delegate ()
        {
            if (_articyData.References.Count - 1 > _currentDialogIndex)
            {
                _currentDialogIndex++;
                _currentDialogue = _articyData.References[_currentDialogIndex];
                
                DialogueManager.Instance.OnDialogueComplete = null;
            }
        };
        DialogueManager.Instance.SetDialogueToPlay(_currentDialogue, DialogType.Map, this);
        DialogueManager.Instance.Play();

        _hasStarted = true;
    }

    public void Reset() => _hasStarted = false;
}
