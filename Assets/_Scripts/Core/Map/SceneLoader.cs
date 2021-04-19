using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;

public class SceneLoader : MonoBehaviour, IInitializable
{
    [ValueDropdown("GetSceneNamesInBuild")]
    public string StartingScene;

    private string[] GetSceneNamesInBuild()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

        return scenes;
    }

    public static SceneLoader Instance;

    private KeyValuePair<string, SceneContainer> _currentScene = new KeyValuePair<string, SceneContainer>();

    [HideInInspector] public Action BeforeNextSceneLoad;
    [HideInInspector] public Action OnSceneLoaded;

    private bool _dontShowCamera;
    public bool DontShowCamera { get => _dontShowCamera; }


    private bool _isInTransition = false;
    public bool IsInTransition { get => _isInTransition; }

    private bool _activeSceneSet = false;

    private Vector3 _targetWorldPosition = Vector3.negativeInfinity;

    public void Init()
    {
        Instance = this;

        if (SceneManager.sceneCount == 1)
            StartCoroutine(LoadScene(StartingScene));
        else
        {
            foreach ( var scene in FindObjectsOfType<SceneContainer>())
            {
                var entryPoint = scene.GetComponentInChildren<EntryPoint>();

                if (entryPoint != null)
                {
                    entryPoint.Init();

                    // Set the active scene to the first scene additively loaded!
                    // Meaning this is where gameobjects will be instantiated, GetComponent will look, etc.

                    if (!_activeSceneSet)
                    {
                        var activeScene = entryPoint.gameObject.scene;
                        SceneManager.SetActiveScene(activeScene);

                        _currentScene = new KeyValuePair<string, SceneContainer>(activeScene.name, scene);

                        _activeSceneSet = true;
                    }
                }
            }

        }
    }

    public IEnumerator LoadScene(string sceneName, bool transitionMode = false)
    {
        AsyncOperation asyncLoad;
        if (!_currentScene.Equals(default(KeyValuePair<string, SceneContainer>)))
        {
            asyncLoad = SceneManager.UnloadSceneAsync(_currentScene.Key);

            while (!asyncLoad.isDone)
                yield return null;
        }


        asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Loading Scene Progress: {asyncLoad.progress}");

            yield return null;
        }

        if (BeforeNextSceneLoad != null)
            BeforeNextSceneLoad.Invoke();

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        asyncLoad.completed += delegate (AsyncOperation load)
        {
            var sceneContainer = FindObjectsOfType<SceneContainer>().Last();
            _currentScene = new KeyValuePair<string, SceneContainer>(sceneName, sceneContainer);

            var entryPoint = sceneContainer.GetComponentInChildren<EntryPoint>();

            // Prevent TransitionEnter in CampaignManager.Init
            _dontShowCamera = true;

            if (!entryPoint.InitOnAwake)
                entryPoint.Init();

            if (OnSceneLoaded != null)
                OnSceneLoaded.Invoke();


            BeforeNextSceneLoad = null;
            OnSceneLoaded = null;
        };

        asyncLoad.allowSceneActivation = true;
    }


    public void BeginMapTransition(string sceneName, Action onTransitionEnterFinished, string transitionSound)
    {
        _isInTransition = true;
        StartCoroutine(MapTransition(sceneName, onTransitionEnterFinished, transitionSound));
    }


    public void SetTransitionComplete() => _isInTransition = false;
    
    
    public void SetIsTransition() => _isInTransition = true;

    
    private IEnumerator MapTransition(string sceneName, Action onTransitionExitFinished, string transitionSound)
    {
        if (!GetSceneNamesInBuild().Contains(sceneName))
            throw new Exception($"Scene: #{sceneName} is not in File --> Build Settings or is not checked...");

        var camera = ProCamera2D.Instance;
        var cameraFade = camera.GetComponent<ProCamera2DTransitionsFX>();


        // Keep black screen alive during transition
        camera.transform.parent = null;
        DontDestroyOnLoad(camera);

        // Turn off UI
        var uiCamera = camera.GetComponentsInChildren<Camera>().Where((camera) => camera.gameObject.layer == LayerMask.NameToLayer("UI")).First();
        if (uiCamera != null)
            uiCamera.gameObject.SetActive(false);

        // Fade Out
        cameraFade.StartSceneOnEnterState = true;
        cameraFade.TransitionExit();

        
        yield return MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(transitionSound, CampaignManager.AudioListenerTransform);

        OnSceneLoaded += delegate ()
        {
            // Destroy Camera from Past Scene
            Destroy(camera.gameObject);

            if (onTransitionExitFinished != null)
                onTransitionExitFinished.Invoke();
        };

        yield return LoadScene(sceneName);
    }

    
    public void SetDebugMoveTarget(Vector3 targetPos) => _targetWorldPosition = targetPos;
    
    
    public void ClearDebugMoveTarget() => _targetWorldPosition = Vector3.negativeInfinity;


    private void OnDrawGizmos()
    {
        if (_targetWorldPosition != Vector3.negativeInfinity)
            Gizmos.DrawCube(_targetWorldPosition, Vector3.one);
    }
}
