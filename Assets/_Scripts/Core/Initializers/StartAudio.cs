using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class StartAudio : MonoBehaviour, IInitializable
{
    [SerializeField] private EventSounds _sound;

    // Start is called before the first frame update
    public void Init()
    {
        StartCoroutine(EnableSound());
    }

    private IEnumerator EnableSound()
    {
        yield return new WaitForSeconds(0.3f);
        _sound.enabled = true;
    }
}
