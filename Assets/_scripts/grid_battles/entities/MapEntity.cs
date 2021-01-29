using System.Collections.Generic;
using UnityEngine;

using Animancer;
using Sirenix.OdinInspector;

public class MapEntity : SerializedMonoBehaviour
{
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private DirectionalAnimationSet _Idles;
    [SerializeField] private DirectionalAnimationSet _Walks;

    [StringInList("UP", "DOWN", "LEFT", "RIGHT")] public string facingDirection;


    public bool hasMoved;

    [SerializeField]
    protected bool _canFly;
    public bool CanFly {
        get { return _canFly; }
    }
    [SerializeField]
    protected bool _canLead;
    public bool CanLead {
        get { return _canLead; }
    }


    public static List<string> BASE_STAT_NAMES = new List<string>{
        "HEALTH",
        "STRENGTH",
        "SPEED",
        "SKILL",
        "MAGIC",
        "LUCK",
        "RESISTANCE",
        "DEFENSE",
        "CONSITITUTION",
        "MOVE",
        "BIORHYTHM"
    };
    public Dictionary<string, int> BASE_STAT_VALUES;
    public Dictionary<string, int> DERIVED_STAT_VALUES;
    

    protected string _entityType;
    public string EntityType {
        get { return _entityType; }
    }

    protected Dictionary<string, Stat> _BASE_STATS = new Dictionary<string, Stat>();
    public Dictionary<string, Stat> BASE_STATS {
        get { return _BASE_STATS; }
    }
    
    protected Dictionary<string, DependentStat> _DERIVED_STATS = new Dictionary<string, DependentStat>();
    public Dictionary<string, DependentStat> DERIVED_STATS {
        get { return _DERIVED_STATS; }
    }

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

    protected int cellIndex = 0;
    private bool _isMoving = false;

    
    protected void Awake() {
        Play(_Idles);

        _renderer = GetComponent<SpriteRenderer>();

        if (BASE_STAT_VALUES.Count == 0)
            BASE_STAT_VALUES = new Dictionary<string, int>{
                {"HEALTH", 18},
                {"STRENGTH", 9},
                {"SPEED", 7},
                {"SKILL", 7},
                {"MAGIC", 4},
                {"LUCK", 6},
                {"RESISTANCE", 4},
                {"DEFENSE", 8},
                {"CONSITITUTION", 7},
                {"MOVE", 6 },
                {"BIORHYTHM", 10}
            };
        
        ValidateBaseStatValues();

        foreach (string statName in BASE_STAT_NAMES)
            _BASE_STATS[statName] = new Stat(BASE_STAT_VALUES[statName]);
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

    protected void TraverseCells() {
        _isMoving = true;
        
        Vector3 cellCenter = _movePath[cellIndex];
        // if (!rotationSet) {
        //     Quaternion newRotation = Quaternion.Euler(transform.eulerAngles.x, directionToLook(cellCenter), transform.eulerAngles.z);
        //     transform.rotation = newRotation;
        //     rotationSet = true;
        // }

        // Vector3 currentPosition = this.transform.position;

        // if(Vector3.Distance(currentPosition, cellCenter) > .1f) {
        //     string direction = currentlyLooking();
        //     float step =  moveSpeed * Time.deltaTime;

        //     transform.position = Vector3.MoveTowards(transform.position, cellCenter, step);
        // } else {
        //     rotationSet = false;
            
        //     if (cellIndex < movePath.Count - 1) {
        //         cellIndex += 1;
        //     } else {
        //         animator.Play("Idle");
                
        //         cellIndex = 0;
        //         movePath = new List<Vector3>();
        //         transform.position = Vector3.Lerp (transform.position, cellCenter, 0.5f);

        //         Cell occupyingCell = TGSInterface.CellAtPosition(cellCenter);
        //         currentCellIndex = occupyingCell.index;
                
        //         int mask = TGSInterface.CELL_MASKS[EntityType.ToUpper()];
        //         tgs.CellSetGroup(occupyingCell.index, mask);

        //         isMoving = false;
        //         OnReachedDestination.Invoke();
        //     }
        // }

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

    private void ValidateBaseStatValues() {
        List<string> missingStats = new List<string>();

        foreach(string statName in BASE_STAT_NAMES) {
            if (!BASE_STAT_VALUES.ContainsKey(statName)) missingStats.Add(statName);
        }

        if (missingStats.Count == 0) return;

        throw new System.Exception($"Missing the follow Base Stats on Entity: {this.gameObject.name} \n\n\n {string.Join(", ", missingStats)}");
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
