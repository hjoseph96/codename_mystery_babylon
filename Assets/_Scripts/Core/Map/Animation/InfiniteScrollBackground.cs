using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollBackground : MonoBehaviour
{
    [SerializeField] private float _StartX;
    [SerializeField] private float _EndX;

    [SerializeField] private float scrollSpeed;


    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Start()
    {
        startPosition   = transform.position;
        startPosition.x = _StartX;

        transform.position = startPosition;

        endPosition     = transform.position;
        endPosition.x   = _EndX;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, endPosition) < 0.1f)
            transform.position = startPosition;
    }
}
