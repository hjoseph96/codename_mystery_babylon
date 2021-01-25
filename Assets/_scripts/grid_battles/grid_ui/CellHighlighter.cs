using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CellHighlighter : MonoBehaviour
{
    private SpriteFlashTool spriteFlashToolComponent;
    private float currentTime = 0f;

    [Header("Settings")]
    [SerializeField] private float flashInterval = 1f; // in seconds..
    public bool loopEnabled = true;

    private void Awake()
    {
        spriteFlashToolComponent = GetComponent<SpriteFlashTool>();
    }

    private void Update()
    {
        FlashLoopHandler();
    }

    private void FlashLoopHandler ()
    {
        if (!loopEnabled)
            return;
        currentTime += Time.deltaTime;
        if(currentTime >= flashInterval)
        {
            spriteFlashToolComponent.Flash();
            currentTime = 0f;
        }
    }
}
