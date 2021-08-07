using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class MapDialogue : SerializedMonoBehaviour
{
    [SerializeField]
    private bool _RotateToFacePlayer;

    [SerializeField]
    private EntityToIdMapper _entityToIDMapper;

    [HideInInspector]
    public System.Action<GameObject> OnDialogueBegin;

    [HideInInspector]
    public System.Action OnDialogueEnd;
    private List<EntityReference> _Participants = new List<EntityReference>();
    public List<EntityReference> Participants { get => _Participants; }

    private ArticyDataContainer _articyData;
    private bool _hasStarted = false;
    private bool _isWithinTrigger = false;
    private GameObject _player;

    private int _currentDialogIndex = 0;
    private ArticyObject _currentDialogue;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    public void Init()
    {
        _Participants = new List<EntityReference>();

        _articyData = GetComponent<ArticyDataContainer>();

        if (_articyData != null && _articyData.References.Count > 0)
            _currentDialogue = _articyData.References[_currentDialogIndex];
    }

    public void Clear()
    {
        _hasStarted = false;
        _currentDialogIndex = 0;
        _currentDialogue = null;
        _Participants = new List<EntityReference>();
    }


    public void AddParticipant(EntityReference entityToAdd)
    {
        if (!_Participants.Contains(entityToAdd))
        {
            _Participants.Add(entityToAdd);
            var refs = entityToAdd.GetComponents<EntityReference>();

            foreach (var entity in refs)
                if (!Participants.Any((e) => e.EntityName == entity.EntityName))
                    _Participants.Add(entity);
        }
    }

    public EntityReference GetEntityByID(int id) => _entityToIDMapper.GetEntity(id);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!_hasStarted && _isWithinTrigger && !DialogueManager.Instance.IsRunningAnAction)
            {
                if (_RotateToFacePlayer)
                    RotateToFacePlayer(_player);

                StartDialogue();
            
                ActionNoticeManager.Instance.HideNotice();
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        var campaignManager = CampaignManager.Instance;

        var notInCombat = campaignManager == null || !campaignManager.IsInCombat;
        if (other.CompareTag("Main Player") && notInCombat && !DialogueManager.Instance.IsPlaying)
        {
            if (_currentDialogue == null)
            {
                Init();

                if (_currentDialogue == null)
                    return;
            }
            ActionNoticeManager.Instance.ShowNotice("To Talk");
            _isWithinTrigger = true;

            _player = other.gameObject;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Main Player"))
        {
            ActionNoticeManager.Instance.HideNotice();
            _isWithinTrigger = false;

            _player = null;
        }
    }

    private void RotateToFacePlayer(GameObject player)
    {
        var direction = DirectionUtility.GetDirection(transform.position, player.transform.position);
        var controller = GetComponentInParent<SpriteCharacterControllerExt>();
        
        if (controller != null)
        {
            controller.Rotate(direction);
            controller.SetIdle();
        }
        else
        {
            var unit = GetComponentInParent<Unit>();
            
            unit.Rotate(direction);
            unit.SetIdle();
        }

    }


    public void StartDialogue()
    {
        DialogueManager.Instance.OnDialogueComplete += delegate ()
        {
            if (_articyData != null && _articyData.References.Count - 1 > _currentDialogIndex)
            {
                _currentDialogIndex++;
                _currentDialogue = _articyData.References[_currentDialogIndex];
                DialogueManager.Instance.OnDialogueComplete = null;
            }

            if (_isWithinTrigger && !_hasStarted)
                ActionNoticeManager.Instance.ShowNotice("To Talk");
        };


        var uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        uiCamera.enabled = false;
        uiCamera.enabled = true;

        DialogueManager.Instance.SetDialogueToPlay(_currentDialogue, DialogType.Map, this);
        DialogueManager.Instance.Play();

        _hasStarted = true;

        OnDialogueBegin?.Invoke(_player);
    }

    public void Reset()
    {
        _hasStarted = false;
        OnDialogueEnd?.Invoke();
    }
}
