using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Animancer;

public class BuddyController : MonoBehaviour
{
    [FoldoutGroup("Animations")]
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private DirectionalAnimationSet _Idle;
    [SerializeField] private DirectionalAnimationSet _Walk;
    [SerializeField] private DirectionalAnimationSet _Run;
    [SerializeField] private DirectionalAnimationSet _Push;
    [SerializeField] private DirectionalAnimationSet _Jump;
    [SerializeField] private DirectionalAnimationSet _InAir;
    [SerializeField] private DirectionalAnimationSet _Landing;
    [SerializeField] private Vector2 _Facing = Vector2.down;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
