using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quill : MonoBehaviour
{
    public Transform follow;
    private bool _instant;

    private IEnumerator _followState;

    private Vector3 startPoint;
    
    [SerializeField]
    [Range(0.5f, 25)]
    private float _maxDistance;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float _speedMultiplier = 1f;

    [SerializeField]
    [Range(0.01f, 3f)]
    private float _idleSpeedMultiplier = 1f;

    private bool _idle;
    private bool _out = true;

    private void Awake()
    {
        follow.GetComponent<UICursor>().UponMove += UpdatePosition;
    }

    private void OnEnable()
    {
        startPoint = transform.position;
        UpdatePosition(true);
    }

    private void OnDestroy()
    {
        follow.GetComponent<UICursor>().UponMove -= UpdatePosition;
    }

    private void Update()
    {
        if (_idle)
        {
            if (_out)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(startPoint.x - 50, startPoint.y + 50, startPoint.z), 0.2f * _idleSpeedMultiplier);
                if (Vector3.Distance(transform.position, startPoint) > _maxDistance)
                    _out = !_out;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPoint, 0.2f * _idleSpeedMultiplier);
                if (transform.position == startPoint)
                    _out = !_out;
            }
        }
    }

    private IEnumerator Follow()
    {
        while(Vector3.Distance(transform.position, follow.transform.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, follow.transform.position, .25f * Vector3.Distance(transform.position, follow.transform.position) * _speedMultiplier);
            if (_instant)
            {
                _instant = false;
                transform.position = follow.transform.position;
            }
            yield return null;
        }

        _followState = null;
        startPoint = transform.position;
        _idle = true;
        _out = true;
        yield return null;
    }

    private void UpdatePosition(bool instant)
    {
        _instant = instant;
        _idle = false;
        if (_followState != null)
            StopCoroutine(_followState);
        else
            StopAllCoroutines();

        _followState = Follow();
        StartCoroutine(_followState);
    }
}


