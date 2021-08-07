using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZenoviaBattler : Magician
{
    [SerializeField] private float _AscensionHeight = 5f;
    [SerializeField] private float _AscensionSpeed = 6f;
    

    private IEnumerator AscensionMovement()
    {
        var apex = startingPoint;
        apex.y += _AscensionHeight;

        while((Vector2)transform.position != apex)
            transform.position = Vector2.Lerp(transform.position, apex, Time.deltaTime * _AscensionSpeed);

        yield return new WaitUntil(() => (Vector2)transform.position == apex);
    }

    private IEnumerator DescensionMovement()
    {
        while((Vector2)transform.position != startingPoint)
            transform.position = Vector2.Lerp(transform.position, startingPoint, Time.deltaTime * (_AscensionSpeed * 1.5f));

        yield return new WaitUntil(() => (Vector2)transform.position == startingPoint);
    }

    // Critical Atk Animation Events
    private void Ascend() => StartCoroutine(AscensionMovement());

    private void Descend() => StartCoroutine(DescensionMovement());
}
