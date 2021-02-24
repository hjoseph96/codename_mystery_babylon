using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Animancer;
using UnityEngine;


public class Battler : SerializedMonoBehaviour
{
    [ReadOnly] protected string Name;
    [SerializeField] protected BattleHUD HUD;

    public Unit Unit { get; private set; }
    protected Dictionary<string, bool> BattleResults = new Dictionary<string, bool>();

    protected AnimancerComponent Animancer;

    private PostEffectMaskRenderer _pixelateShaderRenderer;
    
    public void Setup(Unit unit, BattleHUD hud, Dictionary<string, bool> battleResults, PostEffectMask pixelShaderMask)
    {
        Unit = unit;
        HUD = hud;
        BattleResults = battleResults;

        _pixelateShaderRenderer = GetComponent<PostEffectMaskRenderer>();
        _pixelateShaderRenderer.mask = pixelShaderMask;

        HUD.Populate(Unit);
    }

    // Start is called before the first frame update
    void Awake()
    {
        Animancer = GetComponent<AnimancerComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
