using System.Collections;

using TMPro;

using UnityEngine;


public class CustomFade : MonoBehaviour {

    private Color dstColor;
	private float epsilon = 0.005f;

    public void Fade(TextMeshPro target, float targetAlpha, float speed) {
		this.dstColor = target.faceColor;
		this.dstColor.a = targetAlpha;
        StartCoroutine(DoFade(target, targetAlpha, speed));
    }

    IEnumerator DoFade(TextMeshPro target, float targetAlpha, float speed) {
        while (Mathf.Abs(target.faceColor.a - targetAlpha) > epsilon) {
            target.faceColor = Color.Lerp(target.faceColor, this.dstColor, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
