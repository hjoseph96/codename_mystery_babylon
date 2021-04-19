using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    // Start is called before the first frame update
    void Start() => _particleSystem = GetComponent<ParticleSystem>();

    // Update is called once per frame
    void Update()
    {
        if (_particleSystem.time > 0.985f)
            Destroy(this.gameObject);
    }
}
