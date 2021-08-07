using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArkCameraTrigger : MonoBehaviour
{
    [SerializeField] private MailingListScreen _mailingListScreen;
    [SerializeField] private GameObject _uiCamera;

    private Animator _animator;
    private ArkCinematicCamera _camera;
    private bool _cinematicEnded = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();

        _camera = ArkCinematicCamera.Instance;
        _camera.OnCameraFadedIn = delegate ()
        {
            _animator.Play("Ark Loop");
        };
        _camera.FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        if (_cinematicEnded)
        {
            
            StartCoroutine(GoToNextScreen());

            _cinematicEnded = false;
        }
    }

    private IEnumerator GoToNextScreen()
    {

        yield return new WaitForSeconds(1.2f);

        _camera.OnCameraFadedOut = delegate ()
        {
            _camera.SetActive(false);
            _mailingListScreen.SetActive(true);
            _uiCamera.SetActive(true);
        };

        _camera.FadeOut();
    }

    // Animation Event
    private void CompleteCinematic()
    {
        _cinematicEnded = true;
    }
}
