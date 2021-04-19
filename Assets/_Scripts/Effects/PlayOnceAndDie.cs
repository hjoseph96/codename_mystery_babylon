using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class PlayOnceAndDie : MonoBehaviour
{
    [SerializeField] private AnimationClip _clip;
    private AnimancerComponent _animancer;

    // Start is called before the first frame update
    void Start()
    {
        _animancer = GetComponent<AnimancerComponent>();
        var state = _animancer.Play(_clip);

        state.Events.OnEnd += delegate ()
        {
            Destroy(this);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
