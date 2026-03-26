using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;   // 記得加
using UnityEngine.SceneManagement;


public class FinalFade : MonoBehaviour
{

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

    while (t < duration)
    {
        t += Time.deltaTime;
        yield return null;
    }

}
public void FadeFlash(System.Action onMidPoint)
{
    StartCoroutine(FadeFlashRoutine(onMidPoint));
}

IEnumerator FadeFlashRoutine(System.Action onMidPoint)
{
    float duration = 0.3f;

    float t = 0f;
    Color c = blackImage.color;

    // 🔴 變黑
    while (t < duration)
    {
        t += Time.deltaTime;
        c.a = Mathf.Lerp(0f, 1f, t / duration);
        blackImage.color = c;
        yield return null;
    }

    c.a = 1f;
    blackImage.color = c;
}
}
