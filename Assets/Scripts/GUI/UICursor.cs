using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICursor : MonoBehaviour
{
    public float moveSpeed = 8f;
    private bool _isMoving;
    public  bool IsMoving { get { return _isMoving; } }
    private Vector2 _destination;
    private RectTransform _rectTransform;


    void Start()
    {
        _isMoving = false;
        _destination = Vector2.zero;
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_destination != Vector2.zero)
            Move();
    }
    public void MoveTo(Vector2 localPosition) {
        _destination = localPosition;
    }

    void Move() {
        if (Vector2.Distance(_rectTransform.localPosition, _destination) < 5) {
            _rectTransform.localPosition = _destination;
            
            _isMoving = false;
            _destination = Vector2.zero;
        } else {
            _rectTransform.localPosition = Vector2.Lerp(_rectTransform.localPosition, _destination, moveSpeed * Time.smoothDeltaTime);
            
            _isMoving = true;
        }
    }
}
