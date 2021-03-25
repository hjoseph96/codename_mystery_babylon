using UnityEngine;
using System.Collections.Generic;

[HelpURL("https://pulsarxstudio.com/sprite-flash-tool-documentation/")]
[RequireComponent(typeof(SpriteRenderer))]
[AddComponentMenu("Tools/Sprite Flash Tool")]
public class SpriteFlashTool : MonoBehaviour
{
    // Ref:
    public SpriteRenderer myRenderer;

    // Shader
    public int _matAlpha;
    public int _matColor;
    public string currentShader = "Default";

    // Main:
    public Color flashColor = Color.white;
    public float flashAmount = 1f;
    public double duration = 1.0f;
    public bool decreaseAmountOverTime = true;

    // Local:
    private bool activated = false;
    private double timeSinceActivated = 0f;
    private float flashedAmountCaptured = 0f;

    // Multiple Sprites
    public bool useMultipleSprites = false;
    [Reorderable] public List<SFT_Part> myAdditionalPartsList = new List<SFT_Part>();


    void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        Material temp = new Material(Shader.Find("Sprites/Flash Tool/" + currentShader));
        myRenderer.material = temp;
        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].spriteRenderer)
                {
                    Material tempMatForPart = new Material(Shader.Find("Sprites/Flash Tool/" + myAdditionalPartsList[i].selectedMaterial));
                    myAdditionalPartsList[i].spriteRenderer.material = tempMatForPart;
                }
            }
        }
        _matAlpha = Shader.PropertyToID("_FlashAmount");
        _matColor = Shader.PropertyToID("_FlashColor");
        Deactivate(true, true);
    }


    void Update()
    {
        if (activated)
        {
            if (decreaseAmountOverTime)
            {
                flashedAmountCaptured -= flashAmount / (float)(duration) * Time.deltaTime;
                myRenderer.material.SetFloat(_matAlpha, flashedAmountCaptured);
            }
            timeSinceActivated += Time.deltaTime;
            if (timeSinceActivated >= duration)
                Deactivate();
        }

        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].isActivted && myAdditionalPartsList[i].spriteRenderer != null)
                {
                    if (myAdditionalPartsList[i].decreaseAmountOverTime)
                    {
                        myAdditionalPartsList[i].flashedAmountCaptured -= myAdditionalPartsList[i].flashAmount / (float)(myAdditionalPartsList[i].duration) * Time.deltaTime;
                        myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, myAdditionalPartsList[i].flashedAmountCaptured);
                    }
                    myAdditionalPartsList[i].timeSinceActivated += Time.deltaTime;
                    if (myAdditionalPartsList[i].timeSinceActivated >= myAdditionalPartsList[i].duration) // Deactivate
                    {
                        myAdditionalPartsList[i].isActivted = false;
                        myAdditionalPartsList[i].timeSinceActivated = 0f;
                        myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, 0f);
                    }
                }
            }

        }
    }

    public void FlashAll()
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        timeSinceActivated = 0f;
        flashedAmountCaptured = flashAmount;
        myRenderer.material.SetColor(_matColor, flashColor);
        myRenderer.material.SetFloat(_matAlpha, flashAmount);
        activated = true;

        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].spriteRenderer != null)
                {
                    myAdditionalPartsList[i].timeSinceActivated = 0f;
                    myAdditionalPartsList[i].flashedAmountCaptured = myAdditionalPartsList[i].flashAmount;
                    myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, myAdditionalPartsList[i].flashColor);
                    myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, myAdditionalPartsList[i].flashAmount);
                    myAdditionalPartsList[i].isActivted = true;
                }
            }
        }
    }

    public void FlashPart(string _partName)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        bool partExists = false;
        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].partName == _partName && myAdditionalPartsList[i].spriteRenderer != null)
                {
                    myAdditionalPartsList[i].timeSinceActivated = 0f;
                    myAdditionalPartsList[i].flashedAmountCaptured = myAdditionalPartsList[i].flashAmount;
                    myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, myAdditionalPartsList[i].flashColor);
                    myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, myAdditionalPartsList[i].flashAmount);
                    myAdditionalPartsList[i].isActivted = true;
                    partExists = true;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("'Use Multiple Sprites' must be set to True in order to flash part: '" + _partName + "'");
        }

        if (!partExists)
            Debug.Log("Part: '" + _partName + "' Doesn't exist");
    }

    public void FlashPart(string _partName, Color _flashColor, float _flashAmount, float _duration, bool _decreaseAmountOverTime)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        bool partExists = false;
        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].partName == _partName && myAdditionalPartsList[i].spriteRenderer != null)
                {
                    myAdditionalPartsList[i].flashColor = _flashColor;
                    myAdditionalPartsList[i].flashAmount = _flashAmount;
                    myAdditionalPartsList[i].duration = _duration;
                    myAdditionalPartsList[i].decreaseAmountOverTime = _decreaseAmountOverTime;

                    myAdditionalPartsList[i].timeSinceActivated = 0f;
                    myAdditionalPartsList[i].flashedAmountCaptured = myAdditionalPartsList[i].flashAmount;
                    myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, myAdditionalPartsList[i].flashColor);
                    myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, myAdditionalPartsList[i].flashAmount);
                    myAdditionalPartsList[i].isActivted = true;
                    partExists = true;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("'Use Multiple Sprites' must be set to True in order to flash part: '" + _partName + "'");
        }

        if (!partExists)
            Debug.Log("Part: '" + _partName + "' Doesn't exist");
    }

    public void FlashPart(string _partName, Color _flashColor, float _flashAmount, double _duration, bool _decreaseAmountOverTime)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        bool partExists = false;
        if (useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].partName == _partName && myAdditionalPartsList[i].spriteRenderer != null)
                {
                    myAdditionalPartsList[i].flashColor = _flashColor;
                    myAdditionalPartsList[i].flashAmount = _flashAmount;
                    myAdditionalPartsList[i].duration = _duration;
                    myAdditionalPartsList[i].decreaseAmountOverTime = _decreaseAmountOverTime;

                    myAdditionalPartsList[i].timeSinceActivated = 0f;
                    myAdditionalPartsList[i].flashedAmountCaptured = myAdditionalPartsList[i].flashAmount;
                    myAdditionalPartsList[i].spriteRenderer.material.SetColor(_matColor, myAdditionalPartsList[i].flashColor);
                    myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, myAdditionalPartsList[i].flashAmount);
                    myAdditionalPartsList[i].isActivted = true;
                    partExists = true;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("'Use Multiple Sprites' must be set to True in order to flash part: '" + _partName + "'");
        }

        if (!partExists)
            Debug.Log("Part: '" + _partName + "' Doesn't exist");
    }


    public void Flash()
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        timeSinceActivated = 0f;
        flashedAmountCaptured = flashAmount;
        myRenderer.material.SetColor(_matColor, flashColor);
        myRenderer.material.SetFloat(_matAlpha, flashAmount);
        activated = true;
    }

    public void Flash(Color _flashColor, float _flashAmount, float _duration, bool _decreaseAmountOverTime)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        flashColor = _flashColor;
        flashAmount = _flashAmount;
        duration = _duration;
        decreaseAmountOverTime = _decreaseAmountOverTime;

        timeSinceActivated = 0f;
        flashedAmountCaptured = flashAmount;
        myRenderer.material.SetColor(_matColor, flashColor);
        myRenderer.material.SetFloat(_matAlpha, flashAmount);
        activated = true;
    }

    public void Flash(Color _flashColor, float _flashAmount, double _duration, bool _decreaseAmountOverTime)
    {
        if (!this.gameObject.activeInHierarchy)
            return;
        flashColor = _flashColor;
        flashAmount = _flashAmount;
        duration = _duration;
        decreaseAmountOverTime = _decreaseAmountOverTime;

        timeSinceActivated = 0f;
        flashedAmountCaptured = flashAmount;
        myRenderer.material.SetColor(_matColor, flashColor);
        myRenderer.material.SetFloat(_matAlpha, flashAmount);
        activated = true;
    }


    public void Deactivate(bool _deactivateMain = true, bool _deactivateAllParts = false)
    {
        if (_deactivateMain)
        {
            activated = false;
            timeSinceActivated = 0f;
            myRenderer.material.SetFloat(_matAlpha, 0f);
        }
        if (_deactivateAllParts && useMultipleSprites)
        {
            for (int i = 0; i < myAdditionalPartsList.Count; i++)
            {
                if (myAdditionalPartsList[i].spriteRenderer != null)
                {
                    myAdditionalPartsList[i].spriteRenderer.material.SetFloat(_matAlpha, 0f);
                    myAdditionalPartsList[i].timeSinceActivated = 0f;
                    myAdditionalPartsList[i].isActivted = false;
                }
            }
        }
    }


}
