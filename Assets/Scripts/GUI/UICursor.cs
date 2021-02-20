using UnityEngine;

public class UICursor : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float _totalMovementTime = 0.1f;
    [SerializeField, Range(0, 1)]
    private float _normalizedDistanceThreshold = 0.95f;

    public bool IsMoving { get; private set; }

    private RectTransform _rectTransform;
    private Vector3 _destination;
    private float _movementStartTime;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (IsMoving)
            Move();
    }

    public void MoveTo(Vector2 destination, bool instant = false)
    {
        if (instant)
        {
            _rectTransform.localPosition = destination;
            return;
        }

        IsMoving = true;
        _destination = destination;
        _movementStartTime = Time.time;
    }

    private void Move() {
        var t = (Time.time - _movementStartTime) / _totalMovementTime;

        // If t is greater than threshold value
        if (t >= _normalizedDistanceThreshold || (_rectTransform.localPosition - _destination).magnitude <= 0.01f)
        {
            _rectTransform.localPosition = _destination;
            IsMoving = false;
        }
        // If we didn't reach threshold value yet, simply call Slerp
        else
        {
            _rectTransform.localPosition = Vector3.Slerp(_rectTransform.localPosition, _destination, t);
        }
    }
}
