using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A controller that handles where speech bubbles spawn based on current animation 
/// <br>The Prefab with this script should be a child object onto any Entity that can speak and implements ISpeakableEntity</br>
/// </summary>
public class BubblePositionController : MonoBehaviour
{
    [Header("Left, then Right")]
    public List<Transform> sitting;
    public List<Transform> mounted;
    public List<Transform> standing;

    public Vector3 GetSpeechBubblePos(EntityInfo entityInfo, Direction direction)
    {
        if (entityInfo.sitting)
            return GetPos(sitting, GetDirection(direction, entityInfo.facingDirection));
        else if (entityInfo.mounted)
            return GetPos(mounted, GetDirection(direction, entityInfo.facingDirection));
        else
            return GetPos(standing, GetDirection(direction, entityInfo.facingDirection));
    }

    public Vector3 GetPos(List<Transform> transforms, Direction facingDirection)
    {
        if (transforms.Count < 2)
        {
            Debug.Log("Transform List is missing items");
            return Vector3.zero;
        }
        if (facingDirection == Direction.Left)
            return transforms[0].position;
        else if (facingDirection == Direction.Right)
            return transforms[1].position;
        else
        {
            Debug.Log("Facing Direction is neither left, nor right, returning left");
            return transforms[0].position;
        }
    }

    public Direction GetDirection(Direction direction, Direction entityDirection) => direction == Direction.None ? entityDirection : direction;

    public struct EntityInfo
    {
        public Direction facingDirection;
        public bool mounted;
        public bool sitting;
        // else assume standing
    }
}
