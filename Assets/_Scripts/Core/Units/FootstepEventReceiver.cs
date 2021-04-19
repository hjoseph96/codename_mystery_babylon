using UnityEngine;
using UnityEngine.Serialization;

using Animancer;
using Sirenix.OdinInspector;

public class FootstepEventReceiver : MonoBehaviour
{
    /************************************************************************************************************************/

    [SerializeField]
    private AnimationEventReceiver _OnPlayFootsteps;

    /// <summary>[<see cref="SerializeField"/>] A callback for Animation Events with the Function Name "Event".</summary>
    public ref AnimationEventReceiver OnPlayFootsteps => ref _OnPlayFootsteps;

    /************************************************************************************************************************/

    private void Awake() => _OnPlayFootsteps.SetFunctionName("PlayFootsteps");

    /// <summary>Called by Animation Events with the Function Name "PlayFootsteps".</summary>
    public void PlayFootsteps(AnimationEvent animationEvent)
    {
        _OnPlayFootsteps.SetFunctionName("PlayFootsteps");
        _OnPlayFootsteps.HandleEvent(animationEvent);
    }

    /************************************************************************************************************************/
}
