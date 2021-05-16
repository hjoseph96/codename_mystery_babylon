using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Animancer;
using Sirenix.OdinInspector;
using System;

public class SeatedKnight : MonoBehaviour, ISpeakableEntity
{
    [SerializeField] private bool _wearingHelmet;

    // With Helmet
    [FoldoutGroup("Animations")]
    [Header("With Helmet")]
    [SerializeField] private DirectionalAnimationSet _Sitting;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Turn_Head_Down;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Turn_Head_Left;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Turn_Head_Right;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Turn_Head_Up;

    // Helmetless
    [FoldoutGroup("Animations")]
    [Header("Helmetless")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Helmetless;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Helmetless_Turn_Head_Down;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Helmetless_Turn_Head_Left;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Helmetless_Turn_Head_Right;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _Sitting_Helmetless_Turn_Head_Up;


    [InfoBox("Only Left, Right, Up and Down are supported"), ShowIf("InvalidDirection")]
    [SerializeField]
    public Direction _startingDirection;

    /// <summary>
    /// Note: player must have SpriteCharacterControllerExt as of current iteration
    /// </summary>
    private GameObject player;

    private bool InvalidDirection()
    {
        var supportedDirections = new List<Direction> { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
        return !supportedDirections.Contains(_startingDirection);
    }

    private AnimancerComponent _Animancer;
    

    private Direction _facingDirection;
    private Vector2 _Facing;
    private MapDialogue _attachedDialogue;

    // Start is called before the first frame update
    void Start()
    {
        _Animancer = GetComponent<AnimancerComponent>();
        _Facing = DirectionUtility.DirectionToFacing[_startingDirection];

        _facingDirection = _startingDirection;

        _attachedDialogue = GetComponent<MapDialogue>();
        _attachedDialogue.OnDialogueBegin += delegate (GameObject speakingTo)
       {
           LookAt(speakingTo.transform.position);
           if (speakingTo.CompareTag("Player"))
           {
               player = speakingTo;
               player.GetComponent<SpriteCharacterControllerExt>().OnPositionChanged += UpdateFacing;
           }
       };
        _attachedDialogue.OnDialogueEnd += ResetFacing;

        if (_wearingHelmet)
            Play(_Sitting);
        else
            Play(_Sitting_Helmetless);
    }

    private void OnDestroy()
    {
        if (_attachedDialogue != null)
            _attachedDialogue.OnDialogueEnd -= ResetFacing;
    }


    [Button("Test Facing Animations")]
    public void LookAt(Vector3 position)
    {
       _Facing = DirectionUtility.GetFacing(transform.position, position);

        Play(TurningHeadAnimations());
    }

    private void Play(DirectionalAnimationSet animations)
    {
        var directionalClip = animations.GetClip(_Facing);
        if (!_Animancer.IsPlayingClip(directionalClip))
        {
            var state = _Animancer.Play(directionalClip);
        }
    }

    private DirectionalAnimationSet TurningHeadAnimations()
    {
        DirectionalAnimationSet animations;
        switch(_facingDirection)
        {
            case Direction.Left:
                animations = _Sitting_Helmetless_Turn_Head_Left;
                if (_wearingHelmet)
                    animations = _Sitting_Turn_Head_Left;


                return animations;
            case Direction.Right:
                animations = _Sitting_Helmetless_Turn_Head_Right;
                if (_wearingHelmet)
                    animations = _Sitting_Turn_Head_Right;

                return animations;
            case Direction.Up:
                animations = _Sitting_Helmetless_Turn_Head_Up;
                if (_wearingHelmet)
                    animations = _Sitting_Turn_Head_Up;

                return animations;
            case Direction.Down:
                animations = _Sitting_Helmetless_Turn_Head_Down;
                if (_wearingHelmet)
                    animations = _Sitting_Turn_Head_Down;

                return animations;
        }

        throw new System.Exception($"[SeatedKnight] Cannot find Turning Head Animations for Direction: {_facingDirection}");
    }

    /// <summary>
    /// Resets the head facing of this object to the default position prior to talking.
    /// </summary>
    private void ResetFacing()
    {
        if (player != null)
        {
            player.GetComponent<SpriteCharacterControllerExt>().OnPositionChanged -= UpdateFacing;
            player = null;
        }
        _Facing = DirectionUtility.DirectionToFacing[_startingDirection];
        Play(TurningHeadAnimations());
    }


    /// <summary>
    /// Updates the facing to face the player when moving, could be updated to face NPC characters too by removing the tag check.
    /// </summary>
    /// <param name="pos"></param>
    private void UpdateFacing(Vector3 pos)
    {
        var newFacing = DirectionUtility.GetFacing(transform.position, new Vector3(pos.x, pos.y, 0));

        if (newFacing == _Facing) 
            return;

        _Facing = newFacing;
        Play(TurningHeadAnimations());
    }

    public BubblePositionController.EntityInfo GetEntityInfo()
    {
        return new BubblePositionController.EntityInfo { facingDirection = DirectionUtility.FacingToDirection[_Facing], mounted = false, sitting = true };
    }
}
