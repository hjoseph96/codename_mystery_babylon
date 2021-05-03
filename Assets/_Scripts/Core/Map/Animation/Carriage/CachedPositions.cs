using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedPositions : MonoBehaviour
{
    private string groupName;
    public string GroupName     { get => groupName; }
    [SerializeField] private Direction _forDirection;
    public Direction Direction  { get => _forDirection; }

    private List<Transform> _positions = new List<Transform>();
    public List<Transform> Positions { get => _positions; }

    // Start is called before the first frame update
    void Start()
    {
        groupName = name;

        foreach (var obj in GetComponentsInChildren<Transform>())
            _positions.Add(obj);

        _positions.Remove(this.transform);
    }
}
