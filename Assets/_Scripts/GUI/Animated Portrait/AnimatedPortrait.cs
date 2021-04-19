using System.Collections.Generic;
using UnityEngine;

public class AnimatedPortrait : MonoBehaviour
{
    public string Name;

    public static Dictionary<string, int> AnimationsAsHashes = new Dictionary<string, int>
    {
        { "neutral", -1541085712 },
        { "talking", -1717018742 }
    };

    private Animator _animator;
    private EyeController _eyes;

    // Start is called before the first frame update
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _eyes = GetComponentInChildren<EyeController>();
    }

    public void SetNeutral() => _animator.Play("neutral");
    
    public void Talk() => _animator.Play("talking");
}
