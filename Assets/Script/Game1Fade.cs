using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;   // 記得加


public class Game1Fade : MonoBehaviour
{
    public FadeController fc;

    [Header("To Be Continued Text")]
    public TMP_Text toBeContinuedText;

    [Header("Fade Black Image")]
    public RawImage blackImage;


    void Start()
{
    // 黑幕一開始透明
    if (blackImage != null)
    {
        Color bc = blackImage.color;
        bc.a = 0f;
        blackImage.color = bc;
    }

    // To Be Continued 文字一開始透明
    if (toBeContinuedText != null)
    {
        Color c = toBeContinuedText.color;
        c.a = 0f;
        toBeContinuedText.color = c;
    }

    // 開場淡入
    fc.FadeFromBlack(1f);
}


    public void FadeAndToBeContinued()
    {
        StartCoroutine(FadeOutAndShowText());
    }

    IEnumerator FadeOutAndShowText()
    {
        float duration = 1f;

        // 1️⃣ RawImage 淡黑
        float t = 0f;
        Color blackColor = blackImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            blackColor.a = Mathf.Lerp(0f, 1f, t / duration);
            blackImage.color = blackColor;
            yield return null;
        }

        blackColor.a = 1f;
        blackImage.color = blackColor;

        // 2️⃣ TMP alpha 0 → 1
        t = 0f;
        Color c = toBeContinuedText.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            toBeContinuedText.color = c;
            yield return null;
        }

        c.a = 1f;
        toBeContinuedText.color = c;
    }

}
