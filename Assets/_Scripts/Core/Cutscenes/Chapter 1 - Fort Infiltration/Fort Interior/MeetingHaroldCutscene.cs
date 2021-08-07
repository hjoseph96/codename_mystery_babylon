using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeetingHaroldCutscene : Cutscene
{
    public static MeetingHaroldCutscene Instance;

    [SerializeField] private SpriteCharacterControllerExt _Harold;
    [SerializeField] private SpriteCharacterControllerExt _Artur;

    public override void Init()
    {
        base.Init();

        Instance = this;
    }
}
