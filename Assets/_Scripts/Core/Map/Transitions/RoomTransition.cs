using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using DarkTonic.MasterAudio;
using Com.LuisPedroFonseca.ProCamera2D;

public class RoomTransition : MonoBehaviour
{
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



    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !SceneLoader.Instance.IsInTransition)
        {
            var playerController = collision.GetComponent<SpriteCharacterControllerExt>();

            playerController.FreezeInput();

            // Block other transitions from firing upon destination
            SceneLoader.Instance.SetIsTransition();

            StartCoroutine(TransitionToDestination(playerController));
        }
    }

    private IEnumerator TransitionToDestination(SpriteCharacterControllerExt playerController)
    {
        var camera = ProCamera2D.Instance;
        var cameraFade = camera.GetComponent<ProCamera2DTransitionsFX>();

        UIManager.Instance.GridBattleCanvas.Disable();
        cameraFade.TransitionExit();

        MasterAudio.PauseEverything();
        yield return MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(_transitionSound, CampaignManager.AudioListenerTransform);

        var emergeFromCell = CellToLandIn + _emergeOffsets[EmergeFromDirection];

        var worldGrid = WorldGrid.Instance;
        worldGrid.PlaceGameObject(playerController.gameObject, emergeFromCell);

        // Handle Door Animation
        AnimatedDoor door = null;
        if (_isDoor)
        {
            door = worldGrid.FindDoor(emergeFromCell);
            door.SetOpenImmediate();
        }


        var targetWorldPosition = worldGrid.Grid.GetCellCenterWorld((Vector3Int)CellToLandIn);
        SceneLoader.Instance.SetDebugMoveTarget(targetWorldPosition);

        camera.UpdateType = UpdateType.ManualUpdate;
        camera.Move(999999f);

        cameraFade.OnTransitionEnterEnded += delegate ()
        {

            playerController.OnAutoMoveComplete += delegate ()
            {
                if (_isDoor && door != null)
                    door.Close();

                SceneLoader.Instance.ClearDebugMoveTarget();
                SceneLoader.Instance.SetTransitionComplete();

                cameraFade.OnTransitionEnterStarted = null;
                cameraFade.OnTransitionEnterEnded = null;

                camera.UpdateType = UpdateType.LateUpdate;

                playerController.AllowInput();
            };

            playerController.WalkTo(targetWorldPosition);
        };

        cameraFade.OnTransitionEnterStarted += delegate ()
        {
            MasterAudio.UnpauseEverything();
            UIManager.Instance.GridBattleCanvas.Enable();
        };
        cameraFade.TransitionEnter();
    }
}
