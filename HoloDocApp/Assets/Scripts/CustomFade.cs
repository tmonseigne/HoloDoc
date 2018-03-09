using System.Collections;

using UnityEngine;

using TMPro;

public class CustomFade : MonoBehaviour {
    private Color dstColor;
    private Color currentColor;

    public void Fade(TextMeshPro target, float targetAlpha, float speed) {
        dstColor = target.faceColor;
        dstColor.a = targetAlpha;
        StartCoroutine(DoFade(target, targetAlpha, speed));
    }

    IEnumerator DoFade(TextMeshPro target, float targetAlpha, float speed)
    {
        while (Mathf.Abs(target.faceColor.a - targetAlpha) > 0.005)
        {
            target.faceColor = Color.Lerp(target.faceColor, dstColor, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
