using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Grids2D;

public class GridInterface : MonoBehaviour
{
    public Material clearMaterial;

    private int _columnCount;
    public int ColumnCount {
        get { return _columnCount;}
    }
    private int _rowCount;
    public int RowCount {
        get { return _rowCount;}
    }

    public int barrierCost = 10000;

    public static Dictionary<string, int> CELL_MASKS = new Dictionary<string, int>();
    protected Grid2D _grid;

    protected UnityEvent OnFirstScan = new UnityEvent();

    protected static List<string> PHASES = new List<string>{"PLAYER", "ENEMY", "OTHER_ENEMY", "ALLY"};
    protected Dictionary<int, Bounds> _cellPositions = new Dictionary<int, Bounds>();
    protected List<PlayerEntity> _players = new List<PlayerEntity>();
    protected List<EnemyEntity> _enemies  = new List<EnemyEntity>();
    protected List<AllyEntity> _allies    = new List<AllyEntity>();
    protected List<OtherEnemyEntity> _otherEnemies    = new List<OtherEnemyEntity>();
    protected Dictionary<PlayerEntity, Cell> _playerCells = new Dictionary<PlayerEntity, Cell>();
    protected Dictionary<EnemyEntity, Cell> _enemyCells = new Dictionary<EnemyEntity, Cell>();
    protected Dictionary<OtherEnemyEntity, Cell> _otherEnemyCells = new Dictionary<OtherEnemyEntity, Cell>();
    protected Dictionary<AllyEntity, Cell> _allyCells = new Dictionary<AllyEntity, Cell>();

    bool firstScan = false;
    // Start is called before the first frame update
    protected void Start()
    {
        _grid = Grid2D.instance;
        _rowCount = _grid.rowCount;
        _columnCount = _grid.columnCount;

        _grid.gameObject.GetComponent<MeshRenderer>().material = clearMaterial;
        _grid.highlightMode = HIGHLIGHT_MODE.None;

        CELL_MASKS["TRAVERSABLE_ON_LAND"] = 0;
        CELL_MASKS["PLAYER"] = 1;
        CELL_MASKS["ENEMY"] = 2;
        CELL_MASKS["OTHER_ENEMY"] = 3;
        CELL_MASKS["ALLY"] = 4;
        CELL_MASKS["IMMOVABLE_ON_LAND"] = 5;
        CELL_MASKS["IMMOVABLE_IN_AIR"] = 6;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!firstScan) {
            firstScan = true;
            _cellPositions = CalculateCellBounds(_grid);
            
            OnFirstScan.Invoke();
        }
    }

    Dictionary<int, Bounds> CalculateCellBounds(Grid2D grid) {
        Dictionary<int, Bounds> cellPositions = new Dictionary<int, Bounds>();

        foreach(Cell cell in _grid.cells) {
            Vector3 position    = grid.CellGetPosition(cell.index);
            Vector3 boundsSize  = new Vector3(0.32f, 0.32f, 1);

            Bounds cellBounds = new Bounds(position, boundsSize);

            cellPositions[cell.index] = cellBounds;
        }

        return cellPositions;
    }

    public static Cell CellAtPosition(Vector3 position) {
        Grid2D grid = Grid2D.instance;
        Dictionary<int, Bounds> cellPositions = new Dictionary<int, Bounds>();

        foreach(Cell cell in grid.cells) {
            Vector3 cellPosition = grid.CellGetPosition(cell.index);
            Vector3 boundsSize   = new Vector3(0.32f, 0.32f, 20);

            Bounds cellBounds = new Bounds(cellPosition, boundsSize);

            cellPositions[cell.index] = cellBounds;
        }

        foreach (Cell cell in grid.cells) {
            Bounds cellBounds = cellPositions[cell.index];
            if (IsWithinCell(cellBounds, position))
                return cell;
        }

        throw new UnityException("No Cell at given postion: " + position);
    }
    protected Cell GetCellAtPosition(Vector3 position) {
        foreach (Cell cell in _grid.cells) {
            Bounds cellBounds = _cellPositions[cell.index];
            if (WithinCell(cellBounds, position))
                return cell;
        }

        throw new UnityException("No Cell at given postion: " + position);
    }

    protected bool WithinCell(Bounds cellBounds, Vector3 position) {
        Vector3 center = cellBounds.center;
        Vector3 extents = cellBounds.extents;

        float xMin = center.x - extents.y;
        float xMax = center.x + extents.y;
        float yMin = center.y - extents.y;
        float yMax = center.y + extents.y;

        bool withinY = position.y > yMin && position.y < yMax;
        bool withinX = position.x > xMin && position.x < xMax;

        return withinX && withinY;
    }

    
    public static bool IsWithinCell(Bounds cellBounds, Vector3 position) {
        Vector3 center = cellBounds.center;
        Vector3 extents = cellBounds.extents;

        float xMin = center.x - extents.y;
        float xMax = center.x + extents.y;
        float yMin = center.y - extents.y;
        float yMax = center.y + extents.y;

        bool withinY = position.y > yMin && position.y < yMax;
        bool withinX = position.x > xMin && position.x < xMax;

        return withinX && withinY;
    }

    protected bool WithinCellAsBounds(Bounds cellBounds, Bounds meshBounds) {
        return cellBounds.Intersects(meshBounds);
    }

    protected bool WithinCellAsColliders(Bounds cellBounds, List<Bounds> colliderBounds) {
        bool anyWithin = false;

        foreach(Bounds colliderBoundary in colliderBounds) {
            if (colliderBoundary.Intersects(cellBounds))
                anyWithin = true;
        }

        return anyWithin;
    }

    protected bool cellWithinList(int cellIndex, List<Cell> cellList) {
        bool within = false;

        foreach(Cell cell in cellList) {
            if (cell.index == cellIndex)
                within = true;
        }

        return within;
    }




    protected void AddPlayer(MapEntity entity) {
        PlayerEntity player = entity.GetComponent<PlayerEntity>();
        _players.Add(player);
    }

    protected void AddAlly(MapEntity entity) {
        AllyEntity ally = entity.GetComponent<AllyEntity>();
        _allies.Add(ally);
    }
    protected void AddEnemy(MapEntity entity) {
        EnemyEntity enemy = entity.GetComponent<EnemyEntity>();
        _enemies.Add(enemy);
    }
    protected void AddOtherEnemy(MapEntity entity) {
        OtherEnemyEntity otherEnemy = entity.GetComponent<OtherEnemyEntity>();
        _otherEnemies.Add(otherEnemy);
    }

    protected void SetCellMaskForEntity(int cellIndex, string EntityType) {
        _grid.CellSetGroup(cellIndex, CELL_MASKS[EntityType.ToUpper()]);
    }


    protected Dictionary<PlayerEntity, Cell> PlayerInhabitingCells() {
        Dictionary<PlayerEntity, Cell> cellsWherePlayersAre = new Dictionary<PlayerEntity, Cell>();

        if (_players.Count == 0) return cellsWherePlayersAre;

        foreach(PlayerEntity player in _players) {
            Vector3 currentPosition = player.transform.position;
            Cell cell = GetCellAtPosition(currentPosition);
            
            cellsWherePlayersAre.Add(player, cell);
            SetCellMaskForEntity(cell.index, player.EntityType);
        }

        return cellsWherePlayersAre;
    }
    
    protected Dictionary<EnemyEntity, Cell> EnemyInhabitingCells() {
        Dictionary<EnemyEntity, Cell> cellsWhereEnemiesAre = new Dictionary<EnemyEntity, Cell>();

        if (_enemies.Count == 0) return cellsWhereEnemiesAre;

        foreach(EnemyEntity enemy in _enemies) {
            Vector3 currentPosition = enemy.transform.position;
            Cell cell = GetCellAtPosition(currentPosition);
            
            cellsWhereEnemiesAre.Add(enemy, cell);
            SetCellMaskForEntity(cell.index, enemy.EntityType);
        }

        return cellsWhereEnemiesAre;
    }


    protected Dictionary<OtherEnemyEntity, Cell> OtherEnemyInhabitingCells() {
        Dictionary<OtherEnemyEntity, Cell> cellsWhereOtherEnemiesAre = new Dictionary<OtherEnemyEntity, Cell>();

        if (_otherEnemies.Count == 0) return cellsWhereOtherEnemiesAre;

        foreach(OtherEnemyEntity otherEnemy in _otherEnemies) {
            Vector3 currentPosition = otherEnemy.transform.position;
            Cell cell = GetCellAtPosition(currentPosition);
            
            cellsWhereOtherEnemiesAre.Add(otherEnemy, cell);
            SetCellMaskForEntity(cell.index, otherEnemy.EntityType);
        }

        return cellsWhereOtherEnemiesAre;
    }

    
    protected Dictionary<AllyEntity, Cell> AllyInhabitingCells() {
        Dictionary<AllyEntity, Cell> cellsWhereAlliesAre = new Dictionary<AllyEntity, Cell>();

        if (_otherEnemies.Count == 0) return cellsWhereAlliesAre;

        foreach(AllyEntity ally in _allies) {
            Vector3 currentPosition = ally.transform.position;
            Cell cell = GetCellAtPosition(currentPosition);
            
            cellsWhereAlliesAre.Add(ally, cell);
            SetCellMaskForEntity(cell.index, ally.EntityType);
        }

        return cellsWhereAlliesAre;
    }

}
