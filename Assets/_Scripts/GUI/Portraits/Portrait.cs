using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Portrait : MonoBehaviour
{
    public Sprite Default;

    [InfoBox("ImageColor is used to shade/tint the Portrait when it's assigned in the GUI.")]
    public Color ImageColor;
}
