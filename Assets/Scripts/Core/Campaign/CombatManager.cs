using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using Crosstales.TrueRandom;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _friendlyBattler;
    [FoldoutGroup("Battle Scene")]
    [ReadOnly] private Battler _hostileBattler;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private BattleHUD _enemyHUD;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private BattleHUD _playerHUD;
    
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private GameObject _playerForeground;
    
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _enemyBattlerSpawnPoint;
    [FoldoutGroup("Battle Scene")]
    [SerializeField] private Transform _playerBattlerSpawnPoint;


    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _battleCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _battleUICamera;
     [FoldoutGroup("Cameras")]
    [SerializeField] private Camera _pixelationCamera;
    [FoldoutGroup("Cameras")]
    [SerializeField] private float _transitionSpeed = 0.5f;
    private ProCamera2DTransitionsFX _battleTransitionFX;

    private bool _startTransition = false;
    private PostEffectMask _pixelateEffectMask;

    private Vector3 _platformOriginalPosition;


    public async void Load(Unit attacker, Unit defender)
    {
        _battleUICamera.enabled = false;
        _battleUICamera.enabled = true;

        _pixelationCamera.enabled = false;
        _pixelationCamera.enabled = true;
        _pixelateEffectMask = _pixelationCamera.GetComponent<PostEffectMask>();


        await SetupBattlers(attacker, defender);

        _battleTransitionFX = _battleCamera.GetComponentInChildren<ProCamera2DTransitionsFX>();
        _battleTransitionFX.OnTransitionEnterStarted += delegate () {
            _platformOriginalPosition = _playerForeground.transform.position;

            SetToTransitionPosition(_friendlyBattler.transform);
            SetToTransitionPosition(_playerForeground.transform);
            SetToTransitionPosition(_hostileBattler.transform, 4f);

            _startTransition = true; 
        };
        
        _battleTransitionFX.TransitionEnter();

    }

    // Update is called once per frame
    void Update()
    {
        if (_startTransition)
        {
            bool friendlyReached = ReachedPosition(_friendlyBattler.transform, _playerBattlerSpawnPoint.position);
            bool enemyReached = ReachedPosition(_hostileBattler.transform, _enemyBattlerSpawnPoint.position);
            bool platformReached = ReachedPosition(_playerForeground.transform, _platformOriginalPosition);  
            
            if (friendlyReached && enemyReached && platformReached)
                _startTransition = false;
        }
    }
    
    async Task<bool> SetupBattlers(Unit attacker, Unit defender)
    {
        await AssignBattlerByTeam(attacker, defender);
        await AssignBattlerByTeam(defender, attacker);

        return true;
    }


    async Task<bool> AssignBattlerByTeam(Unit unit, Unit opposingUnit)
    {
        Battler newBattler;

        Transform parent = _playerBattlerSpawnPoint;
        bool isFriendly = false;

        switch(unit.TeamId) {
            case Team.LocalPlayerTeamId: case Team.AllyTeamId: case Team.NeutralTeamId:
                isFriendly = true;
                break;
            case Team.EnemyTeamId: case Team.OtherEnemyTeamId:
                isFriendly = false;
                parent = _enemyBattlerSpawnPoint;
                break;
        }

        var battlerObject = Instantiate( unit.BattlerPrefab, parent.position, unit.BattlerPrefab.transform.rotation, parent );
        newBattler = battlerObject.GetComponent<Battler>();

        var battleResults = await BattleResults(unit, opposingUnit);
        if (isFriendly)
        {
            newBattler.Setup(unit, _playerHUD, battleResults, _pixelateEffectMask);
            _friendlyBattler = newBattler;
        }
        else {
            newBattler.Setup(unit, _enemyHUD, battleResults, _pixelateEffectMask);
            _hostileBattler = newBattler;
        }

        return true;
    }

    private async Task<Dictionary<string, bool>> BattleResults(Unit attacker, Unit defender)
    {
        Dictionary<string, bool> battleResults = new Dictionary<string, bool>();
        
        // If the attacking cannot defend himself, return empty
        if (!attacker.CanDefend(defender.GridPosition))
            return battleResults;


        var hitResults = await HitResults(attacker, defender);
        var critResults = await CriticalHitResults(attacker, defender);

        // Merge the dictionaries
        return hitResults.Concat(critResults)
            .ToLookup(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, g => g.First());        
    }

    private async Task<Dictionary<string, bool>> HitResults(Unit attacker, Unit defender)
    {
        Dictionary<string, bool> hitResults = new Dictionary<string, bool>();
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        int chance = await RollDice();
        var hitChance = attackPreview["ACCURACY"];

        if ( WithinRange(chance, hitChance) )
            hitResults["HIT"] = true;
        else
            hitResults["HIT"] = false;
        
        if( attacker.CanDoubleAttack(defender, attacker.EquippedWeapon) )
        {
            hitResults["DOUBLE_ATTACK"] = true;
            chance = await RollDice();

            if (WithinRange(chance, hitChance))
                hitResults["SECOND_HIT"] = true;
            else
                hitResults["SECOND_HIT"] = false;
        } else {
            hitResults["DOUBLE_ATTACK"] = false;
        }

        return hitResults;
    }

    private async Task<Dictionary<string, bool>> CriticalHitResults(Unit attacker, Unit defender)
    {
        Dictionary<string, bool> critResults = new Dictionary<string, bool>();
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        int chance = await RollDice();
        var critChance = attackPreview["CRIT_RATE"];

        if ( WithinRange(chance, critChance) )
            critResults["CRITICAL"] = true;
        else
            critResults["CRITICAL"] = false;

        if( attacker.CanDoubleAttack(defender, attacker.EquippedWeapon) )
        {
            chance = await RollDice();

            if (WithinRange(chance, critChance))
                critResults["CRIT_SECOND_HIT"] = true;
            else
                critResults["CRIT_SECOND_HIT"] = false;
        }

        return critResults;
    }


    // This makes an API call, hence the asyc other thread to receive a truly random integer
    async Task<int> RollDice()
    {
        int rollResult = -1;

        bool diceRolled = false;

        TRManager.Instance.GenerateInteger(0, 100, 1);
        TRManager.Instance.OnGenerateIntegerFinished += delegate(List<int> results, string key) {
            rollResult = results[0];
            diceRolled = true;
        };


        await new WaitUntil(() => diceRolled == true);

        if (rollResult == -1)
            throw new System.Exception("Invalid Dice Roll!");

        return rollResult;
    }

    private bool WithinRange(int chance, int range) => chance >= 1 && chance <= range;

    private void SetToTransitionPosition(Transform objTransform, float amount = -4f)
    {
        var transitionStartPos =  new Vector3(
            objTransform.position.x + amount, objTransform.position.y, objTransform.transform.position.z
        );
        
        Debug.Log($"{objTransform.name} Transition Start: {transitionStartPos}");
        Debug.Log($"{objTransform.name} ORIGINAL Start: {objTransform.position}");
        
        objTransform.position = transitionStartPos;

    }

    private void MoveToOriginalPosition(Transform objTransform)
    {
    }

    private bool ReachedPosition(Transform objTransform, Vector3 originalPosition)
    {
        objTransform.position = Vector3.Slerp(objTransform.transform.position, originalPosition, (_transitionSpeed * Time.smoothDeltaTime) / 2.2f);

        if (Vector3.Distance(objTransform.position, originalPosition) < .03f)
        {
            objTransform.position = originalPosition;
            return true;
        }

        return false;
    }
}
