using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;


public class MapTransition : MonoBehaviour
{
    [ValueDropdown("GetSceneNamesInBuild")]
    public string SceneName;

    private string[] GetSceneNamesInBuild()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[sceneCount];
        
        for (int i = 0; i < sceneCount; i++)
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
        
        return scenes;
    }

    public Vector2Int CellToLandIn;

    [SoundGroup, SerializeField] private string _transitionSound;

    public Direction EmergeFromDirection;

    private Dictionary<Direction, Vector2Int> _emergeOffsets = new Dictionary<Direction, Vector2Int> {
        { Direction.Up, new Vector2Int(0, 1) },
        { Direction.Down, new Vector2Int(0, -1) },
        { Direction.Left, new Vector2Int(-1, 0) },
        { Direction.Right, new Vector2Int(1, 0) },
    };

    [SerializeField] private bool _isDoor;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !SceneLoader.Instance.IsInTransition)
        {
            var playerController = collision.GetComponent<SpriteCharacterControllerExt>();
            playerController.FreezeInput();

            Action onTransitionExit = delegate ()
            {
                var worldGrid = WorldGrid.Instance;

                var emergeFromCell = CellToLandIn + _emergeOffsets[EmergeFromDirection];
                
                AnimatedDoor door = null;
                if (_isDoor)
                {
                    door = worldGrid.FindDoor(emergeFromCell);
                    door.SetOpenImmediate();
                }

                worldGrid.PlaceGameObject(playerController.gameObject, emergeFromCell);
                
                var camera = ProCamera2D.Instance;

                camera.UpdateType = UpdateType.ManualUpdate;

                // Manually Move ProCamera2D
                camera.RemoveAllCameraTargets();
                camera.AddCameraTarget(playerController.transform);
                camera.Move(999999f);

                // Destroy any pre-existing Players in the scene
                foreach (var controller in FindObjectsOfType<SpriteCharacterControllerExt>())
                    if (controller != playerController)
                        Destroy(controller.gameObject);

                var cameraFade = camera.GetComponent<ProCamera2DTransitionsFX>();

                var uiCamera = camera.GetComponentsInChildren<Camera>().Where((camera) => camera.gameObject.layer == LayerMask.NameToLayer("UI")).First();
                cameraFade.OnTransitionEnterEnded += delegate ()
                {
                    var targetWorldPosition = worldGrid.Grid.GetCellCenterWorld((Vector3Int)CellToLandIn);
                    SceneLoader.Instance.SetDebugMoveTarget(targetWorldPosition);


                    if (uiCamera != null)
                    {
                        UIManager.Instance.GridBattleCanvas.SetCamera(uiCamera);
                        uiCamera.enabled = false;
                        uiCamera.enabled = true;
                    }

                    playerController.OnAutoMoveComplete += delegate ()
                    {
                        if (_isDoor && door != null)
                            door.Close();

                        SceneLoader.Instance.ClearDebugMoveTarget();
                        SceneLoader.Instance.SetTransitionComplete();

                        camera.UpdateType = UpdateType.LateUpdate;

                        playerController.AllowInput();
                    };

                    playerController.WalkTo(targetWorldPosition);
                };

                cameraFade.TransitionEnter();
            };

            SceneLoader.Instance.BeginMapTransition(SceneName, onTransitionExit, _transitionSound);
        }
    }



}
