using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIManager : MonoBehaviour, IInitializable
{
    public static UIManager Instance;

    [SerializeField] private GridBattleCanvas _battleCanvas;
    public GridBattleCanvas GridBattleCanvas { get => _battleCanvas; }

    [SerializeField] private BattleSceneCanvas _battleSceneCanvas;
    public BattleSceneCanvas BattleSceneCanvas { get => _battleSceneCanvas; }

    public void Init()
    {
        Instance = this;

        _battleSceneCanvas.Disable();
    }
}
