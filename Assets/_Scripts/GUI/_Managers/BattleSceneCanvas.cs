using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneCanvas : CanvasManager
{
    [SerializeField] private BattleHUD _playerHUD;
    public BattleHUD PlayerHUD { get => _playerHUD; }

    [SerializeField] private BattleHUD _enemyHUD;
    public BattleHUD EnemyHUD { get => _enemyHUD; }

    [SerializeField] private ExperienceBar _expBar;
    public ExperienceBar ExpBar { get => _expBar; }

}
