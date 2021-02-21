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

        PopulateForecasts();
        
        _worldGrid = WorldGrid.Instance;
        _gridCursor = GridCursor.Instance;

        _gridCursor.SetAttackMode(_attackingUnit);
        _gridCursor.transform.position = _worldGrid.Grid.GetCellCenterWorld((Vector3Int) attackableCell);
        _gridCursor.AttackTargetChanged.AddListener(delegate(Unit targetUnit) {
            _defendingUnit = targetUnit;
            PopulateForecasts();
        });

        this.SetActive(true);
    }

    public override void ProcessInput(InputData input)
    {
        if (input.MovementVector.x != 0)
        {
            
        }
            

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

    private void PopulateForecasts()
    {
        _playerForecast.Populate(_attackingUnit, _defendingUnit);
        _enemyForecast.Populate(_defendingUnit, _attackingUnit);
    }
}
