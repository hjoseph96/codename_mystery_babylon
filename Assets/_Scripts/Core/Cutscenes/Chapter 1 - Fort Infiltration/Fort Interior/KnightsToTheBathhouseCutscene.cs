using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class KnightsToTheBathhouseCutscene : Cutscene
{
    public static KnightsToTheBathhouseCutscene Instance;
    [SerializeField] private List<SpriteCharacterControllerExt> _KnightsbyID;

    public override void Init()
    {
        base.Init();

        Instance = this;
    }

    public IEnumerator MoveOutToBathHouse()
    {
        var exitPoint = new Vector2Int(251, 66);

        var firstKnight     = _KnightsbyID[0];
        var secondKnight    = _KnightsbyID[1];
        var thirdKnight     = _KnightsbyID[2];

        var camera = Camera.main.GetComponent<ProCamera2D>();

        bool knightsHaveLeft = false;

        var artur = GameObject.FindGameObjectWithTag("Main Player").GetComponent<SpriteCharacterControllerExt>();
        var arturUnit = artur.GetComponent<Unit>();

        var arturGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(artur.transform.position);
        arturUnit.SetGridPosition(arturGridPosition);

        artur.FreezeInput();

        firstKnight.StopSitting();
        secondKnight.StopSitting();
        thirdKnight.StopSitting();


        var entityManager = EntityManager.Instance;

        firstKnight.OnAutoMoveComplete += delegate ()
        {
            var entityRef = firstKnight.GetComponent<EntityReference>();
            
            entityManager.RemoveEntityReference(entityRef);
            
            Destroy(firstKnight.gameObject);
        };
        
        secondKnight.OnAutoMoveComplete += delegate ()
        {
            var entityRef = secondKnight.GetComponent<EntityReference>();
            
            entityManager.RemoveEntityReference(entityRef);
            
            Destroy(secondKnight.gameObject);
        };

        thirdKnight.OnAutoMoveComplete += delegate ()
        {
            var entityRef = thirdKnight.GetComponent<EntityReference>();
            
            entityManager.RemoveEntityReference(entityRef);
            Destroy(thirdKnight.gameObject);
            
            camera.SetSingleTarget(artur.transform);

            artur.AllowInput();

            knightsHaveLeft = true;
        };

        StartCoroutine(firstKnight.WalkToCoroutine(exitPoint));
        camera.SetSingleTarget(firstKnight.transform);

        yield return new WaitForSeconds(0.8f);

        StartCoroutine(secondKnight.WalkToCoroutine(exitPoint));
        camera.SetSingleTarget(secondKnight.transform);

        yield return new WaitForSeconds(0.8f);

        StartCoroutine(thirdKnight.WalkToCoroutine(exitPoint));
        camera.SetSingleTarget(thirdKnight.transform);

        yield return new WaitUntil(() => knightsHaveLeft);
    }
}
