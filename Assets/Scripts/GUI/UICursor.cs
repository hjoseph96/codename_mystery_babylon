using UnityEngine;

public class UICursor : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float _totalMovementTime = 0.1f;
    [SerializeField, Range(0, 1)]
    private float _normalizedDistanceThreshold = 0.95f;

    public bool IsMoving { get; private set; }

    private RectTransform RectTransform => transform as RectTransform;
    private Vector3 _destination;
    private float _movementStartTime;

    private void OnEnable()
    {
        IsMoving = false;
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
            RectTransform.localPosition = destination;
            return;
        }

        IsMoving = true;
        _destination = destination;
        _movementStartTime = Time.time;
    }

    private void Move()
    {
        var t = (Time.time - _movementStartTime) / _totalMovementTime;

        // If t is greater than threshold value
        if (t >= _normalizedDistanceThreshold || (RectTransform.localPosition - _destination).magnitude <= 0.01f)
        {
            RectTransform.localPosition = _destination;
            IsMoving = false;
        }
        // If we didn't reach threshold value yet, simply call Slerp
        else
        {
            RectTransform.localPosition = Vector3.Slerp(RectTransform.localPosition, _destination, t);
        }
    }
}
