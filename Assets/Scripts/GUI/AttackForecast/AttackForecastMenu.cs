using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    List<Vector2Int> _allAttackableCells = new List<Vector2Int>();
    

    public void Show(Unit unit)
    {
        _attackingUnit = unit;
        _allAttackableCells = _attackingUnit.AllAttackableCells();
        _attackableWeaponsByPosition = _attackingUnit.AttackableWeapons();
        _allAttackableWeapons = new List<Weapon>(_attackableWeaponsByPosition.Keys);

        var attackableCell = _attackableWeaponsByPosition.Values.First<List<Vector2Int>>()[0];
        _defendingUnit = WorldGrid.Instance[attackableCell].Unit;

        _gridCursor = GridCursor.Instance;
        _gridCursor.SetAttackMode(_attackingUnit);
        _gridCursor.MoveInstant(attackableCell);
        // When Selected Attack Target Changes, execute:
        _gridCursor.AttackTargetChanged.AddListener(delegate(Unit targetUnit) {
            _defendingUnit = targetUnit;
            
            var availableWeapons = WeaponsThatCanReachTarget();
            if (!availableWeapons.Contains(_selectedWeapon))
                _selectedWeapon = availableWeapons[0];
            
            PopulateForecasts();
        });

        _selectedWeapon = _attackableWeaponsByPosition.Keys.First<Weapon>();
        if (WeaponsThatCanReachTarget(attackableCell).Contains(unit.EquippedWeapon))
            _selectedWeapon = unit.EquippedWeapon;
        
        PopulateForecasts();
        _weaponSelect.Populate(_selectedWeapon);

        Activate();
    }

    // Weapon Cycling Selection
    private void Update()
    {
        _weaponSelect.Populate(_selectedWeapon);
        PopulateForecasts();

        if (CanSwitchWeapon()) {
            _weaponSelect.ShowArrows();

            if (Input.GetKeyDown(KeyCode.Q))
                StartCoroutine(SwitchWeapon("Left"));
            
            if (Input.GetKeyUp(KeyCode.Q))
                _weaponSelect.DeactivateLeftArrow();
            
            if (Input.GetKeyDown(KeyCode.E))
                StartCoroutine(SwitchWeapon("Right"));

            if (Input.GetKeyUp(KeyCode.E))
                _weaponSelect.DeactivateRightArrow();
        }
        else
            _weaponSelect.HideArrows();
    }

     public override void ProcessInput(InputData input)
    {
        switch (input.KeyCode)
        {
            case KeyCode.Z:
                // Initiate Attack with Selected Weapon, change selected weapon to equipped
                break;

            case KeyCode.X:
                Close();
                break;
        }
    }



    public override void ResetState()
    {
        // _cursor.transform.parent = transform;
        // _selectedSlotIndex = 0;
    }

    public override void OnClose()
    {
        GridCursor.Instance.SetFreeMode();
    }

    private void PopulateForecasts()
    {
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

    private IEnumerator SwitchWeapon(string direction) {
        var selectableWeapons = WeaponsThatCanReachTarget();

        int nextWeaponIndex = selectableWeapons.IndexOf(_selectedWeapon);


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
        
        _selectedWeapon = selectableWeapons[nextWeaponIndex];
        _weaponSelect.Populate(_selectedWeapon);
    }
}
