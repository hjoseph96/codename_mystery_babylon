using System.Collections;
using UnityEngine;

using DarkTonic.MasterAudio;

public class NormalHealEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _greenPillar;
    [SerializeField] private DetectAnimationEnd _healAnimation;
    [SoundGroupAttribute, SerializeField] private string _healingSound;
    [SerializeField] private int _maxOpacity;
    [SerializeField] private float _fadeInDuration;
    [SerializeField] private float _fadeOutDuration;

    private float _startTime;
    private bool _isFadingIn = false;
    private bool _isFadingOut = false;


    void Awake()
    {

        StartCoroutine(StartFadingIn());

        _healAnimation.OnHaltSecondaryEffect += delegate ()
        {
            _greenPillar.Stop();
        };

        _healAnimation.OnAnimationEnd += delegate ()
        {

            _isFadingIn = false;
            
            // Play once then go away
            _healAnimation.gameObject.SetActive(false);

            StartFadingOut();
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (_isFadingIn)
            FadeIn();

        if (_isFadingOut)
            StartCoroutine(FadeOut());
    }

    private IEnumerator StartFadingIn()
    {
        _isFadingIn = true;

        _greenPillar.Play();
        _startTime = Time.time;

        // Let the animation get warmed up
        yield return new WaitForSecondsRealtime(1f);

        // Then play the circle heal
        _healAnimation.gameObject.SetActive(true);
        _healAnimation.Play();

        yield return new WaitForSecondsRealtime(.2f);

        MasterAudio.PlaySound3DFollowTransform(_healingSound, CampaignManager.AudioListenerTransform);
    }

    private void FadeIn()
    {
        var particleSettings = _greenPillar.main;
        var color = particleSettings.startColor.color;
        var startingOpacity = color.a;

        float t = (Time.time - _startTime) / _fadeInDuration;
        float opacity = Mathf.SmoothStep(startingOpacity, _maxOpacity, t);

        Debug.Log($"Fade In: {opacity}");

        color.a = opacity;
        particleSettings.startColor = color;
    }

    private void StartFadingOut()
    {
        _isFadingOut = true;
        _startTime = Time.time;
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSecondsRealtime(_fadeOutDuration);

        Destroy(this.gameObject);
    }
}
