using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Com.LuisPedroFonseca.ProCamera2D;

public class IntermittentShaking : MonoBehaviour
{
    [Sirenix.OdinInspector.MinMaxSlider(1, 30)]
    public Vector2Int ShakeIntervalRange = new Vector2Int(1, 30);

    private ProCamera2DShake _shaker;

    private bool _isShaking = false;

    private void Start()
    {
        _shaker = GetComponent<ProCamera2DShake>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isShaking)
            StartCoroutine(InvokeShake());
    }
    
    private IEnumerator InvokeShake()
    {
        _isShaking = true;

        var waitTime = Random.Range(ShakeIntervalRange.x, ShakeIntervalRange.y);

        yield return new WaitForSeconds(waitTime);

        _shaker.Shake(0);

        _isShaking = false;
    }
}
