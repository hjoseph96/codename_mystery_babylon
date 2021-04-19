using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class AttackForecastMenu : Menu
{
    [SerializeField] WeaponSelectView _weaponSelect;
    [SerializeField] PlayerForecast _playerForecast;
    [SerializeField] EnemyForecast _enemyForecast;

    Unit _attackingUnit;
    Unit _defendingUnit;
    GridCursor _gridCursor;
    Weapon _selectedWeapon;
    List<Weapon> _allAttackableWeapons = new List<Weapon>();
    Dictionary<Weapon, List<Vector2Int>> _attackableWeaponsByPosition = new Dictionary<Weapon, List<Vector2Int>>();
    PlaySoundResult _attackSound = new PlaySoundResult();


    public void Show(Unit unit)
    {
        _attackingUnit = unit;
        _attackableWeaponsByPosition = _attackingUnit.AttackableWeapons();
        _allAttackableWeapons = new List<Weapon>(_attackableWeaponsByPosition.Keys);

        var attackableCell = _attackableWeaponsByPosition.Values.First()[0];
        _defendingUnit = WorldGrid.Instance[attackableCell].Unit;

        _gridCursor = GridCursor.Instance;

        MakeAttackingUnitFaceTarget();

        var attackableCells = unit.AllAttackableCells();
        _gridCursor.SetRestrictedToListMode(attackableCells, UpdateForecast, BeginBattle, OnExitCallback);
        _gridCursor.MoveInstant(attackableCell);

        _selectedWeapon = _attackableWeaponsByPosition.Keys.First();
        if (WeaponsThatCanReachTarget(attackableCell).Contains(unit.EquippedWeapon))
            _selectedWeapon = unit.EquippedWeapon;

        Activate();
        PopulateForecasts();

        UserInput.Instance.AddInputTarget(_gridCursor);
    }

    public void UpdateForecast(Vector2Int targetPosition)
    {
        _defendingUnit = WorldGrid.Instance[targetPosition].Unit;
        Debug.Assert(_defendingUnit != null);
        Debug.Assert(_defendingUnit.IsEnemy(_attackingUnit));

        var availableWeapons = WeaponsThatCanReachTarget();
        if (!availableWeapons.Contains(_selectedWeapon))
            _selectedWeapon = availableWeapons[0];

        MakeAttackingUnitFaceTarget();
        PopulateForecasts();
    }

    public override void ProcessInput(InputData input)
    {
        if (CanSwitchWeapon())
        {
            _weaponSelect.ShowArrows();

            switch (input.KeyCode)
            {
                case KeyCode.Q:
                    if (input.KeyState == KeyState.Down)
                        StartCoroutine(SwitchWeapon("Left"));
                    else
                        _weaponSelect.DeactivateLeftArrow();
                    break;

                case KeyCode.E:
                    if (input.KeyState == KeyState.Down)
                        StartCoroutine(SwitchWeapon("Right"));
                    else
                        _weaponSelect.DeactivateRightArrow();
                    break;
            }
        }
        else
            _weaponSelect.HideArrows();
    }

    private void BeginBattle(Vector2Int targetPosition)
    {
        _attackingUnit.EquipWeapon(_selectedWeapon);
                
        // Defender turns to face attacker
        var attackerPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int) _attackingUnit.GridPosition);
        _defendingUnit.LookAt(attackerPosition);
        _defendingUnit.SetIdle();
        
        var campaignManager = CampaignManager.Instance;
        campaignManager.OnCombatReturn = _attackingUnit.TookAction;
        campaignManager.StartCombat(_attackingUnit, _defendingUnit, ConfirmSound);
        
        Close();
        PreviousMenu.ResetAndHide();
    }

    public void OnExitCallback()
    {
        MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
        Close();
    }

    public override void OnClose()
    {
        UserInput.Instance.RemoveInputTarget(_gridCursor);

        _gridCursor.ExitRestrictedMode();
        _gridCursor.ClearAll();
        _gridCursor.SetFreeMode();
    }

    private void MakeAttackingUnitFaceTarget()
    {
        Vector2 defenderPosition = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int) _defendingUnit.GridPosition);
            
        _attackingUnit.LookAt(defenderPosition);
        _attackingUnit.SetIdle();
    }

    private void PopulateForecasts()
    {
        _weaponSelect.Populate(_selectedWeapon);
        _playerForecast.Populate(_attackingUnit, _defendingUnit, _selectedWeapon);
        _enemyForecast.Populate(_defendingUnit, _attackingUnit);
    }

    private bool CanSwitchWeapon() {
        if (_gridCursor.IsMoving)
            return false;

        return WeaponsThatCanReachTarget().Count > 1;
    }

    private List<Weapon> WeaponsThatCanReachTarget() {
        var weaponsThatCanReachTarget = new List<Weapon>();

        foreach(KeyValuePair<Weapon, List<Vector2Int>> entry in _attackableWeaponsByPosition)
            if (entry.Value.Contains(_gridCursor.GridPosition))
                weaponsThatCanReachTarget.Add(entry.Key);
        
        return weaponsThatCanReachTarget;
    }

    
    private List<Weapon> WeaponsThatCanReachTarget(Vector2Int cellPosition) {
        var weaponsThatCanReachTarget = new List<Weapon>();

        foreach(KeyValuePair<Weapon, List<Vector2Int>> entry in _attackableWeaponsByPosition)
            if (entry.Value.Contains(cellPosition))
                weaponsThatCanReachTarget.Add(entry.Key);
        
        return weaponsThatCanReachTarget;
    }

    private IEnumerator SwitchWeapon(string direction)
    {
        var selectableWeapons = WeaponsThatCanReachTarget();
        var nextWeaponIndex = selectableWeapons.IndexOf(_selectedWeapon);

        yield return new WaitForSeconds(0.3f); // 1 second between weapon changes

        switch (direction) {
            case "Left":
                _weaponSelect.ActivateLeftArrow();
                
                nextWeaponIndex -= 1;
                if (nextWeaponIndex < 0) nextWeaponIndex = selectableWeapons.Count - 1;    
                
                break;
            case "Right":
                _weaponSelect.ActivateRightArrow();
                
                nextWeaponIndex += 1;
                if (nextWeaponIndex > selectableWeapons.Count - 1) nextWeaponIndex = 0;    
                
                
                break;
        }

        MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
        _selectedWeapon = selectableWeapons[nextWeaponIndex];
        PopulateForecasts();
    }
}
