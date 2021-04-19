using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Animancer.Examples.DirectionalSprites;
using DarkTonic.MasterAudio;

[ExecuteAlways]
public class JumpTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _startTrigger;
    [SerializeField] private BoxCollider2D _endTrigger;
    
    // TODO: Move to SpriteCharacterCotnrollerExt
    [SerializeField] private float _jumpHeight = 2.42f;
    public PlayOnceAndDie LandingEffect;

    // TODO: Move to SpriteCharacterCotnrollerExt
    [SoundGroup] public string JumpSound;
    [SoundGroup] public string LandingSound;


    private Vector2 _startPosition;
    private Vector2 _endPosition;
    public Vector2 Destination => _endPosition;




    private List<Vector2> _jumpPath = new List<Vector2>();

    private bool _canJump;
    private SpriteCharacterControllerExt _playerController;

    // Start is called before the first frame update
    private void Awake() 
    {
        SetPositions();
        PopulateJumpPath();

        if (!_startTrigger.TryGetComponent<JumpTriggerStartingPoint>(out _))   
            _startTrigger.gameObject.AddComponent<JumpTriggerStartingPoint>();
    }

    private void Update()
    {
        if (_canJump)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ActionNoticeManager.Instance.HideNotice();
                _playerController.transform.position = _startPosition;
                _playerController.Jump(this);
                MasterAudio.PlaySound3DFollowTransform(JumpSound, CampaignManager.AudioListenerTransform);
            }
        }
    }

    public Vector2 GetPositionAtTime(float time) => MathParabola.Parabola(_startPosition, _endPosition, _jumpHeight, time);

    public Vector2 GetHighestPoint() => _jumpPath.OrderByDescending((jumpPathPoint) => jumpPathPoint.y).First();


    public void AllowJumping(SpriteCharacterControllerExt playerController)
    {
        _playerController = playerController;
        ActionNoticeManager.Instance.ShowNotice("To Jump");
        _canJump = true;
    }
    public void DisableJumping()
    {
        _playerController = null;
        ActionNoticeManager.Instance.HideNotice();
        _canJump = false;
    }


    private void SetPositions()
    {
        if (_startTrigger == null || _endTrigger == null)
            throw new System.Exception("JumpTrigger has unset start or end points...");

        _startPosition = _startTrigger.bounds.center;
        _endPosition = _endTrigger.bounds.center;
    }

    private void PopulateJumpPath()
    {
        _jumpPath = new List<Vector2>();
        float time = 0;

        while (time <= 1.1)
        {
            Vector2 jumpPathPoint = MathParabola.Parabola(_startPosition, _endPosition, _jumpHeight, time);
            _jumpPath.Add(jumpPathPoint);

            time += 0.1f;
        }
    }

    private void OnDrawGizmos()
    {
        // TODO: Move this to an editor class, only show parabolas' when the gameobject is selected
        if (_startTrigger == null || _endTrigger == null)
            return;

        SetPositions();
        PopulateJumpPath();

        Vector2 prevPos = _jumpPath[0];
        for (int i = 1; i <= _jumpPath.Count - 1; i++)
        {
            Vector3 currPos = _jumpPath[i];
            Gizmos.DrawLine(prevPos, currPos);
            Gizmos.DrawSphere(currPos, 0.01f);
            prevPos = currPos;
        }

    }
}