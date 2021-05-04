using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Codename_Mysterybabylon;

public enum EntityType
{
    None,
    PlayableCharacter,
    NonPlayableCharacter
}

public class Entity : SerializedMonoBehaviour
{
    [FoldoutGroup("Game Data"), ShowInInspector]
    public AnimatedPortrait Portrait { get; protected set; }

    // Articy Template Properties

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Name { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public int Age { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Appearance { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string BornIn { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Occupation { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Personality { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public Sex Gender { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public Species Species { get; protected set; }
}
