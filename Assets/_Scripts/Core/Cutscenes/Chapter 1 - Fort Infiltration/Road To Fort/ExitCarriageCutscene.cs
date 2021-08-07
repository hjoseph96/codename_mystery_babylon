using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Com.LuisPedroFonseca.ProCamera2D;

public class ExitCarriageCutscene : Cutscene
{
    [FoldoutGroup("Sprite Controllers")]
    [SerializeField] private CarriageController _carriageController;
    [FoldoutGroup("Sprite Controllers")]
    public SpriteCharacterControllerExt _artur;
    [FoldoutGroup("Sprite Controllers")]
    public SpriteCharacterControllerExt _jacques;
    [FoldoutGroup("Sprite Controllers")]
    public SpriteCharacterControllerExt _zenovia;
    [FoldoutGroup("Sprite Controllers")]
    public SpriteCharacterControllerExt _penelope;

    [FoldoutGroup("GridPaths")]
    [SerializeField] private GridPathComponent _arturGridPath;
    [FoldoutGroup("GridPaths")]
    [SerializeField] private GridPathComponent _jacquesGridPath;
    [FoldoutGroup("GridPaths")]
    [SerializeField] private GridPathComponent _zenoviaGridPath;
    [FoldoutGroup("GridPaths")]
    [SerializeField] private GridPathComponent _penelopeGridPath;


    // Start is called before the first frame update
    public override void Init()
    {
        _carriageController.OnFinishedMoving += delegate ()
        {
            _carriageController.ExteriorDoor.OnDoorOpened += delegate ()
            {
                StartCoroutine("MovePlayers");
            };
            _carriageController.ExteriorDoor.Open();
        };
        _carriageController.RunTo(this.transform.position);
    }

   private IEnumerator MovePlayers()
    {
        var exteriorDoor = _carriageController.ExteriorDoor;
        exteriorDoor.Open();

        var camera2D = ProCamera2D.Instance;
        camera2D.RemoveAllCameraTargets();
        camera2D.OffsetX = 0;
        camera2D.HorizontalFollowSmoothness = 0.06f;


        yield return new WaitForSeconds(.5f);

        _artur.DisableCollider();
        _artur.transform.position = exteriorDoor.InsideDoorSpawnPoint.position;

        StartCoroutine(_artur.WalkToCoroutine(_arturGridPath.GridPath));

        camera2D.AddCameraTarget(_artur.transform);
        camera2D.UpdateType = UpdateType.ManualUpdate;
        camera2D.Move(999999f);
        yield return new WaitForSeconds(1f);

        var jacquesDialogues = _jacques.GetComponentInChildren<ArticyDataContainer>();
        jacquesDialogues.Empty();

        _jacques.StopSitting();
        _jacques.transform.position = exteriorDoor.InsideDoorSpawnPoint.position;
        
        StartCoroutine(_jacques.WalkToCoroutine(_jacquesGridPath.GridPath));

        yield return new WaitForSeconds(1f);
        camera2D.UpdateType = UpdateType.LateUpdate;

        var zenoviaDialogues = _zenovia.GetComponentInChildren<ArticyDataContainer>();
        zenoviaDialogues.Empty();

        _zenovia.StopSitting();
        _zenovia.transform.position = exteriorDoor.InsideDoorSpawnPoint.position;
        StartCoroutine(_zenovia.WalkToCoroutine(_zenoviaGridPath.GridPath));

        yield return new WaitForSeconds(1f);

        var penelopeDialogues = _penelope.GetComponentInChildren<ArticyDataContainer>();
        penelopeDialogues.Empty();

        _penelope.StopSitting();
        _penelope.transform.position = exteriorDoor.InsideDoorSpawnPoint.position;
        _penelope.OnAutoMoveComplete += delegate () { Play(); };
        
        StartCoroutine(_penelope.WalkToCoroutine(_penelopeGridPath.GridPath));

    }
}
