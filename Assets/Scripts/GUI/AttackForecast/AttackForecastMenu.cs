using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AttackForecastMenu : Menu
{
    [SerializeField] PlayerForecast _playerForecast;
    [SerializeField] EnemyForecast _enemyForecast;

    Unit _attackingUnit;
    Unit _defendingUnit;
    WorldGrid _worldGrid;
    GridCursor _gridCursor;
    Weapon _selectedWeapon;
    Dictionary<Weapon, List<Vector2Int>> _attackableWeaponsByPosition = new Dictionary<Weapon, List<Vector2Int>>();
    List<Vector2Int> _allAttackableCells = new List<Vector2Int>();
    

    public void Show(Unit unit)
    {
        _attackingUnit = unit;
        _allAttackableCells = _attackingUnit.AllAttackableCells();
        _attackableWeaponsByPosition = _attackingUnit.AttackableWeapons();

        _selectedWeapon = _attackableWeaponsByPosition.Keys.First<Weapon>();

        
        var attackableCell = _attackableWeaponsByPosition.Values.First<List<Vector2Int>>()[0];
        _defendingUnit = WorldGrid.Instance[attackableCell].Unit;


        _playerForecast.Populate(unit, _defendingUnit);
        _enemyForecast.Populate(_defendingUnit, unit);
        
        _worldGrid = WorldGrid.Instance;
        _gridCursor = GridCursor.Instance;

        _gridCursor.SetRestrictedMode(_attackingUnit, true);
        _gridCursor.transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) attackableCell);

        this.SetActive(true);
    }

    public override void ProcessInput(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                SelectedOption.Execute();
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
}
