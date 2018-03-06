using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorFadeEffect : MonoBehaviour {
    [Range(1, 10)]
    public float fadeSpeed = 1.0f;

    private Color srcColor;
    private Color dstColor;
    private Color currentColor;
    private Color currentDstColor;
    private bool shouldLoop = true;
    private float epsilon = 0.05f;

    public void SetSourceColor(Color color)
    {
        srcColor = color;
        dstColor = color - new Color(0.6f, 0.6f, 0.6f, 0f);
        currentColor = srcColor;
        currentDstColor = dstColor;
    }
	
    public Color Blink(float deltaTime)
    {
        currentColor = Color.Lerp(currentColor, currentDstColor, fadeSpeed * deltaTime);
        if (Mathf.Abs(currentColor.r - currentDstColor.r) <= epsilon &&
            Mathf.Abs(currentColor.g - currentDstColor.g) <= epsilon &&
            Mathf.Abs(currentColor.b - currentDstColor.b) <= epsilon)
        {
            currentDstColor = shouldLoop ? srcColor : dstColor;
            shouldLoop = !shouldLoop;
        }

        return currentColor;
    }
}
