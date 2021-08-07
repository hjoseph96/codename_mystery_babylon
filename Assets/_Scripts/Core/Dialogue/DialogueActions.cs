using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActions
{
    #region Movement Related Actions
    /// <summary>
    /// Used for a unique EntityReference to WALK to a given Vector2Int grid cell.
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public IEnumerator WalkTo(EntityReference walker, int x, int y)
    {
        var targetGridPosition = new Vector2Int(x, y);

        if (!WorldGrid.Instance.PointInGrid(targetGridPosition))
            throw new Exception($"[DialogueActions] Given point '[{x}, {y}]' is not within the WorldGrid...");

        yield return walker.GetComponent<SpriteCharacterControllerExt>().WalkToCoroutine(targetGridPosition);
    }

    /// <summary>
    /// Used for a non-unique instance of an Entity to WALK to a given grid cell.
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="entityID"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public IEnumerator InstanceWalkTo(EntityReference walker, int entityID, int x, int y)
    {
        var targetEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");


        var controller = targetEntity.GetComponent<SpriteCharacterControllerExt>();

        var targetGridPosition = new Vector2Int(x, y);
        if (!WorldGrid.Instance.PointInGrid(targetGridPosition))
            throw new Exception($"[DialogueActions] Given point '[{x}, {y}]' is not within the WorldGrid...");


        yield return controller.WalkToCoroutine(targetGridPosition);
    }


    /// <summary>
    /// Used for two unique EntityReferences to WALK to a neighboring grid cell
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator WalkNextTo(EntityReference walker, EntityReference target)
    {
        var controller = walker.GetComponent<SpriteCharacterControllerExt>();
        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(walker.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(target.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path);
    }

    // For walking to an Instance of an entity by entity_id
    // walker must not be an Instance, himself...
    /// <summary>
    /// Used for a unique Entity to WALK to a non-unique instance's neighboring cell.
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="target"></param>
    /// <param name="entityID"></param>
    /// <returns></returns>
    public IEnumerator WalkNextToInstance(EntityReference walker, EntityReference target, int entityID)
    {
        var controller = walker.GetComponent<SpriteCharacterControllerExt>();
        var targetEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");


        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(walker.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(targetEntity.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path);
    }

    /// <summary>
    /// Used for one non-unique instace of an Entity to WALK to another non-unique instance of an Entity
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="firstEntityID"></param>
    /// <param name="target"></param>
    /// <param name="secondEntityID"></param>
    /// <returns></returns>
    public IEnumerator InstanceWalkNextToInstance(EntityReference walker, int firstEntityID, EntityReference target, int secondEntityID)
    {
        var walkingEntity = DialogueManager.Instance.FetchEntityById(firstEntityID);
        var targetEntity = DialogueManager.Instance.FetchEntityById(secondEntityID);

        if (walkingEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={firstEntityID}...");

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={secondEntityID}...");


        var controller = walkingEntity.GetComponent<SpriteCharacterControllerExt>();

        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(walkingEntity.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(targetEntity.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path);
    }


    /// <summary>
    /// Used for a unique Entity to RUN to a given cell in the WorldGrid
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public IEnumerator RunTo(EntityReference runner, int x, int y)
    {
        var targetGridPosition = new Vector2Int(x, y);

        if (!WorldGrid.Instance.PointInGrid(targetGridPosition))
            throw new Exception($"[DialogueActions] Given point '[{x}, {y}]' is not within the WorldGrid...");

        yield return runner.GetComponent<SpriteCharacterControllerExt>().WalkToCoroutine(targetGridPosition, true);
    }

    /// <summary>
    /// Used for a non-unique instance of an Entity to RUN to a given cell in the WorldGrid.
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="entityID"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public IEnumerator InstanceRunTo(EntityReference runner, int entityID, int x, int y)
    {
        var runningEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (runningEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");

        var targetGridPosition = new Vector2Int(x, y);

        if (!WorldGrid.Instance.PointInGrid(targetGridPosition))
            throw new Exception($"[DialogueActions] Given point '[{x}, {y}]' is not within the WorldGrid...");

        var controller = runningEntity.GetComponent<SpriteCharacterControllerExt>();
        yield return controller.WalkToCoroutine(targetGridPosition, true);
    }

    /// <summary>
    /// Used for one unique Entity to RUN to a neighboring cell of another unique Entity
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator RunNextTo(EntityReference runner, EntityReference target)
    {
        var controller = runner.GetComponent<SpriteCharacterControllerExt>();
        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(runner.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(target.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path, true);
    }

    /// <summary>
    /// Used for a unique Entity to RUN to a non-unique instance's neighboring cell.
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="target"></param>
    /// <param name="entityID"></param>
    /// <returns></returns>
    public IEnumerator RunNextToInstance(EntityReference runner, EntityReference target, int entityID)
    {
        var controller = runner.GetComponent<SpriteCharacterControllerExt>();
        var targetEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");


        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(runner.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(targetEntity.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path, true);
    }

    /// <summary>
    /// Used for one non-unique instace of an Entity to RUN to another non-unique instance of an Entity
    /// </summary>
    /// <param name="walker"></param>
    /// <param name="firstEntityID"></param>
    /// <param name="target"></param>
    /// <param name="secondEntityID"></param>
    /// <returns></returns>
    public IEnumerator InstanceRunNextToInstance(EntityReference runner, int firstEntityID, EntityReference target, int secondEntityID)
    {
        var runningEntity = DialogueManager.Instance.FetchEntityById(firstEntityID);
        var targetEntity = DialogueManager.Instance.FetchEntityById(secondEntityID);

        if (runningEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={firstEntityID}...");

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={secondEntityID}...");


        var controller = runningEntity.GetComponent<SpriteCharacterControllerExt>();

        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(runningEntity.transform.position);
        var targetGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(targetEntity.transform.position);
        var sortingLayerID = controller.Renderer.sortingLayerID;

        var closestNeighbor = GridUtility.FindClosestNeighbor(sortingLayerID, currentGridPosition, targetGridPosition);

        var path = GridUtility.FindPath(sortingLayerID, currentGridPosition, closestNeighbor);

        yield return controller.WalkToCoroutine(path, true);
    }
    #endregion

    #region Rotation-based Actions
    /// <summary>
    /// Used for one unique Entity to rotate the direction of another unique Entity
    /// </summary>
    /// <param name="looker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator LookAt(EntityReference looker, EntityReference target)
    {
        var spriteCharacterController = looker.GetComponent<SpriteCharacterControllerExt>();
        var directionToLook = DirectionUtility.GetDirection(looker.transform.position, target.transform.position);

        spriteCharacterController.Rotate(directionToLook);
        spriteCharacterController.SetIdle();

        yield return new WaitUntil(() => spriteCharacterController.Facing == directionToLook.ToVector());

    }

    /// <summary>
    /// Used for one unique Entity to look the direction of a non-unique Entity
    /// </summary>
    /// <param name="looker"></param>
    /// <param name="target"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public IEnumerator LookAtInstance(EntityReference looker, EntityReference target, int entityID)
    {
        var spriteCharacterController = looker.GetComponent<SpriteCharacterControllerExt>();

        var targetEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");

        var directionToLook = DirectionUtility.GetDirection(looker.transform.position, targetEntity.transform.position);

        spriteCharacterController.Rotate(directionToLook);
        spriteCharacterController.SetIdle();

        yield return new WaitUntil(() => spriteCharacterController.Facing == directionToLook.ToVector());
    }

    /// <summary>
    /// Used for a unique Entity to rotate to a given direction
    /// </summary>
    /// <param name="looker"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public IEnumerator LookAtDirection(EntityReference looker, string direction)
    {
        var spriteCharacterController = looker.GetComponent<SpriteCharacterControllerExt>();

        var validDirections = new List<string> { "Up", "Down", "Left", "Right" };
        if (!validDirections.Contains(direction))
            throw new Exception($"[DialogueActions] Invalid direction: {direction}");

        var directionToLook = (Direction)Enum.Parse(typeof(Direction), direction);

        spriteCharacterController.Rotate(directionToLook);
        spriteCharacterController.SetIdle();

        yield return new WaitUntil(() => spriteCharacterController.Facing == directionToLook.ToVector());
    }

    /// <summary>
    /// Used for a non-unique Entity to rotate to a given direction
    /// </summary>
    /// <param name="looker"></param>
    /// <param name="entityID"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public IEnumerator InstanceLookAtDirection(EntityReference looker, int entityID, string direction)
    {
        var targetEntity = DialogueManager.Instance.FetchEntityById(entityID);

        if (targetEntity == null)
            throw new Exception($"[DialogueActions] There's no EntityReference with entity_id={entityID}...");

        var spriteCharacterController = targetEntity.GetComponent<SpriteCharacterControllerExt>();

        var validDirections = new List<string> { "Up", "Down", "Left", "Right" };
        if (!validDirections.Contains(direction))
            throw new Exception($"[DialogueActions] Invalid direction: {direction}");

        var directionToLook = (Direction)Enum.Parse(typeof(Direction), direction);

        spriteCharacterController.Rotate(directionToLook);
        spriteCharacterController.SetIdle();

        yield return new WaitUntil(() => spriteCharacterController.Facing == directionToLook.ToVector());
    }

    #endregion

    #region Character Controller Actions

    public IEnumerator SetIdle(EntityReference entity)
    {
        var entityController = entity.GetComponent<SpriteCharacterControllerExt>();
        entityController.RemoveIncapacitation();
        entityController.SetIdle();

        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator AllowInput(EntityReference playerToMove)
    {
        var spriteCharacterController = playerToMove.GetComponent<SpriteCharacterControllerExt>();

        spriteCharacterController.AllowInput();

        yield return new WaitUntil(() => spriteCharacterController.State == ControllerState.Ground);
    }

    /*public IEnumerator BeginFollowing(EntityReference follower, EntityReference playerToFollow)
    {
        var buddyController = follower.GetComponent<BuddyController>();
        buddyController.StartFollowing(playerToFollow.gameObject);

        yield return new WaitUntil(() => buddyController.IsFollowing);
    }*/
    #endregion

    public IEnumerator ChangeUnitName(EntityReference entity, string newName)
    {
        var unit = entity.GetComponent<Unit>();
        unit.ChangeName(newName);

        yield return new WaitUntil(() => unit.Name == newName);
    }

    #region Cutscene Custom Actions

    public IEnumerator ThreeKnightsEmerge()
    {
        yield return KnightPatrolCutscene.Instance.ThreeKnightsEmerge();
    }

    public IEnumerator ChristianChangeOutfit()
    {
        yield return PostTutorialCutscene.Instance.ChristianChangeOutfit();
    }

    public IEnumerator GoFindRope()
    {
        yield return PostTutorialCutscene.Instance.GoFindRope();
    }


    public IEnumerator GoFetchRope()
    {
        yield return PostTutorialCutscene.Instance.GoFetchRope();
    }

    public IEnumerator GoBackToWhistling()
    {
        yield return PostTutorialCutscene.Instance.GoBackToWhistling();
    }

    public IEnumerator GoRestrainChristian()
    {
        yield return PostTutorialCutscene.Instance.GoRestrainChristian();
    }

    public IEnumerator ArturLeavesForTheFort()
    {
        yield return PostTutorialCutscene.Instance.ArturLeavesForTheFort();
    }
    public IEnumerator KillChristian()
    {
        yield return PostTutorialCutscene.Instance.KillChristian();
    }

    public IEnumerator ArturEquipsKnightArmor()
    {
        yield return PostTutorialCutscene.Instance.ArturEquipsKnightArmor();
    }

    public IEnumerator ArturChangeIntoKnight()
    {
        yield return ArturChangeIntoKnightCutscene.Instance.ArturChangeIntoKnight();
    }

    public IEnumerator OpenThePortcullis()
    {
        yield return TalkToGatekeepersCutscene.Instance.OpenThePortcullis();
    }

    public IEnumerator MoveOutToBathHouse()
    {
        yield return KnightsToTheBathhouseCutscene.Instance.MoveOutToBathHouse();
    }

    public IEnumerator StartFortTour()
    {
        yield return FortTourCutscene.Instance.StartFortTour();
    }

    public IEnumerator WalkToCommandersRoom()
    {
        yield return FortTourCutscene.Instance.WalkToCommandersRoom();
    }

    public IEnumerator HeadToTheDungeonEntrance()
    {
        yield return FortTourCutscene.Instance.HeadToTheDungeonEntrance();
    }

    public IEnumerator BackToTheMessHall()
    {
        yield return FortTourCutscene.Instance.BackToTheMessHall();
    }

    public IEnumerator GoUpToBarracks()
    {
        yield return FortTourCutscene.Instance.GoUpToBarracks();
    }

    public IEnumerator HeadToTheAromory()
    {
        yield return FortTourCutscene.Instance.HeadToTheArmory();
    }

    #endregion

    public IEnumerator ClearMapDialogBoxes()
    {
        yield return DialogueManager.Instance.ClearMapDialogBoxes();
    }

    public IEnumerator Yield(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
    }
}
