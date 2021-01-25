using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class MapEntity : MonoBehaviour
{
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private DirectionalAnimationSet _Idles;
    [SerializeField] private DirectionalAnimationSet _Walks;

    [StringInList("UP", "DOWN", "LEFT", "RIGHT")] public string facingDirection;

    public string entityType;
    public bool hasMoved;
    public bool canFly;
    private SpriteRenderer _renderer;
    public SpriteRenderer Renderer {
        get { return _renderer; }
    }

    private List<Vector2> _movePath;
    public List<Vector2> MovePath {
        get { return _movePath;}
        set {
            if (!_isMoving) 
                _movePath = value;
        }
    }


    protected static string[] FACING_DIRECTIONS = {"UP", "DOWN", "LEFT", "RIGHT"};

    private bool _isMoving = false;

    
    protected void Awake() {
        Play(_Idles);
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    protected void Update()
    {
        if (_isMoving) {
            Play(_Walks);
        } else {
            Play(_Idles);
        }
        
    }

    private void Play(DirectionalAnimationSet animations)
    {
        Vector2 _direction = FacingVector();
        AnimationClip clip = animations.GetClip(_direction);
        _Animancer.Play(clip);
    }

    private Vector2 FacingVector() {
        if (facingDirection == "UP") return new Vector2(0, 1);
        if (facingDirection == "DOWN") return new Vector2(0, -1);
        if (facingDirection == "LEFT") return new Vector2(1, 0);
        if (facingDirection == "RIGHT") return new Vector2(-1, 0);

        Debug.Log("Invalid facingDirection");
        return new Vector2(0,0);
    }


    /************************************************************************************************************************/
    #if UNITY_EDITOR
    /************************************************************************************************************************/

    /// <summary>[Editor-Only]
    /// Sets the character's starting sprite in Edit Mode so you can see it while working in the scene.
    /// </summary>
    /// <remarks>
    /// Called by the Unity Editor in Edit Mode whenever an instance of this script is loaded or a value is changed
    /// in the Inspector.
    /// </remarks>
    private void OnValidate()
    {
        if (_Idles == null)
            return;

        AnimancerUtilities.EditModePlay(_Animancer, _Idles.GetClip(FacingVector()), true);
    }

    /************************************************************************************************************************/
    #endif
    /************************************************************************************************************************/

}
