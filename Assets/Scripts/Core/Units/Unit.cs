using UnityEngine;

public class Unit : MonoBehaviour, IInitializable
{
    [DistinctUnitType]
    public UnitType UnitType;

    public int MovePoints = 7;
    public int MinAttackRange = 1, MaxAttackRange = 2;

    //public UnitStats Stats;

    [HideInInspector] public Vector2Int GridPosition;

    public void Init()
    {

        GridPosition = GridUtility.SnapToGrid(this);
        WorldGrid.Instance[GridPosition].Unit = this;

        //Debug.Log(GridPosition);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = WorldGrid.Instance.MouseToGrid();
            var path = GridUtility.FindPath(this, pos);
            if (path != null)
            {
                var last = transform.position;
                for (var i = 0; i < path.Length; i++)
                {
                    var next = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int) path[i]);
                    Debug.DrawLine(last, next, Color.blue, 5f);
                    last = next;
                }
            }
        }
    }

    // ==================================
    // EXAMPLE FIRE EMBLEM FORMULAS BELOW
    // ==================================

    // public int AttackDamage() {
    //     int weaponMight = equippedWeapon.STATS["MIGHT"].CalculateValue();
        
    //     if (equippedWeapon.WeaponType == "GRIMIORE") { // MAGIC USER
    //         int magicStat = BASE_STATS["MAGIC"].CalculateValue();
            
    //         return magicStat + weaponMight;
    //     } else {
    //         int strengthStat = BASE_STATS["STRENGTH"].CalculateValue();

    //         return strengthStat + weaponMight;
    //     }
    // }

    // public int AttackSpeed() {
    //     int weaponWeight = equippedWeapon.STATS["WEIGHT"].CalculateValue();
    //     int strengthStat = BASE_STATS["STRENGTH"].CalculateValue();
    //     int burden = weaponWeight - strengthStat;

    //     if (burden < 0)
    //         burden = 0;
        
    //     return BASE_STATS["SPEED"].CalculateValue() - burden;
    // }

    // public Dictionary<string, int> PreviewAttack(Entity defender) {
    //     Dictionary<string, int> battleStats = new Dictionary<string, int>();

    //     int atkDmg;
    //     if (equippedWeapon.WeaponType == "GRIMIORE") {
    //         atkDmg = AttackDamage() - defender.BASE_STATS["RESISTANCE"].CalculateValue();
    //     } else {
    //         atkDmg = AttackDamage() - defender.BASE_STATS["DEFENSE"].CalculateValue();
    //     }

    //     battleStats["ATK_DMG"] = atkDmg;
    //     battleStats["ACCURACY"] = Accuracy(defender);
    //     battleStats["CRIT_RATE"] = CriticalHitRate(defender);
    //     battleStats["CRIT_MULTIPLIER"] = equippedWeapon.STATS["CRIT_MULTIPLIER"].CalculateValue();
    //     return battleStats;
    // }

    // public bool CanDoubleAttack(Entity target) {
    //     int minDoubleAttackBuffer = 5;

    //     if ((this.AttackSpeed() - target.AttackSpeed()) > minDoubleAttackBuffer)
    //         return true;

    //     return false;
    // }

    // public int CriticalHitRate(Entity target) {
    //     int skill = BASE_STATS["SKILL"].CalculateValue();
    //     int weaponCritChance = equippedWeapon.STATS["CRIT_RATE"].CalculateValue();

    //     int critRate = ((skill / 2) + weaponCritChance) - target.BASE_STATS["LUCK"].CalculateValue();

    //     // if (WEAPON_RANKS[equippedWeapon.WeaponType] == Weapon.RANKS["S"])
    //     //     critRate += 5;

    //     return critRate;
    // }

    // public int DodgeChance() {
    //     int luckStat = BASE_STATS["LUCK"].CalculateValue();
    //     int biorhythmStat = BASE_STATS["BIORHYTHM"].CalculateValue();

    //     return (AttackSpeed() * 2) + luckStat + biorhythmStat;
    // }

    // public int HitRate(int cellIndex) {
    //     int boxDistance = tgs.CellGetBoxDistance(currentCellIndex, cellIndex);
    //     int luckStat = BASE_STATS["LUCK"].CalculateValue();
    //     int skillStat = BASE_STATS["SKILL"].CalculateValue();
    //     int biorhythmStat = BASE_STATS["BIORHYTHM"].CalculateValue();
    //     int weaponHitStat = equippedWeapon.STATS["HIT"].CalculateValue();

    //     int hitRate = (skillStat * 2) + luckStat + biorhythmStat + weaponHitStat;
    //     if (boxDistance >= 2)
    //         hitRate -= 15;
        
    //     return hitRate;
    // }

    // public int Accuracy(Entity target) {
    //     int boxDistance = tgs.CellGetBoxDistance(currentCellIndex, target.currentCellIndex);

    //     if (equippedWeapon.WeaponType != "STAFF") {
    //         // Add weapon W△ Weapon triangle effects
    //         return HitRate(target.currentCellIndex) - target.DodgeChance();
    //     } else {
    //         int magicStat = BASE_STATS["MAGIC"].CalculateValue();
    //         int resistanceStat = BASE_STATS["RESISTANCE"].CalculateValue();
    //         int skillStat = BASE_STATS["SKILL"].CalculateValue();
            
    //         return (magicStat - resistanceStat) + skillStat + 30 - (boxDistance * 2);
    //     }
    // }

}
