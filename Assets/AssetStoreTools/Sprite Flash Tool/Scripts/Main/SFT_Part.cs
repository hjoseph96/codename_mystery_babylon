using UnityEngine;
using SpriteFlashTools.PropertyAttributes;

[System.Serializable]
public class SFT_Part
{
    // Main
    public string partName = "";
    [SpriteRendererNoneSelectedColorIndicator(true, 0.8f)] public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    [Range(0f, 1f)] public float flashAmount = 1f;
    [DoubleMinAndMaxValue(0)] public double duration = 1.0f;
    public bool decreaseAmountOverTime = true;
    [StringListPopup(new string[] { "Default", "Diffuse" })] public string selectedMaterial = "Default";


    // Editor stats
    [HideInInspector] public Color previewModeCapturedFlashColor;
    [HideInInspector] public float previewModeCapturedFlashAmount;
    [HideInInspector] public double timeUntilDeactivation = 0;
    [HideInInspector] public double timeOfDeactivation = 0;

    [HideInInspector] public bool isActivted = false;
    [HideInInspector] public float flashedAmountCaptured = 0f;
    [HideInInspector] public double timeSinceActivated = 0f;
}
