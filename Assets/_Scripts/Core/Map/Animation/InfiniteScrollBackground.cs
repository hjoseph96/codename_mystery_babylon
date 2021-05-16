using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollBackground : MonoBehaviour, IInitializable
{
    public static InfiniteScrollBackground Instance;

    [SerializeField] private float _StartX;
    [SerializeField] private float _EndX;

    [SerializeField] private float scrollSpeed;

    private bool _isStopped = false;


    private Vector3 startPosition;
    private Vector3 endPosition;

    public void Init()
    {
        Instance = this;

        startPosition   = transform.position;
        startPosition.x = _StartX;

        transform.position = startPosition;

        endPosition     = transform.position;
        endPosition.x   = _EndX;
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isStopped)
        {
            transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, endPosition) < 0.1f)
                transform.position = startPosition;
        }
    }

    public void Stop() => _isStopped = true;

    public void StartMoving() => _isStopped = false;

}
