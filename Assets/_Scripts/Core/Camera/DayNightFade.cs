using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraFilterPack_Colors_Adjust_PreFilters))]
public class DayNightFade : MonoBehaviour
{
    [SerializeField]
    private float _changeRate = 0.01f;
    private CameraFilterPack_Colors_Adjust_PreFilters _photoFilter;
    void Start()
    {
        _photoFilter = GetComponent<CameraFilterPack_Colors_Adjust_PreFilters>();

    }

    void LateUpdate()
    {
        _photoFilter.FadeFX = Mathf.PingPong(Time.time * _changeRate, 0.382f);
    }
}
