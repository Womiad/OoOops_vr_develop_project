using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game1Fade : MonoBehaviour
{
    public FadeController fc;

    [Header("Fade Black Image")]
    public RawImage blackImage;

    [Header("BGM")]
    public AudioSource bgmSource;   // ⭐ 新增

    public string nextSceneName = "final"; 

    void Start()
    {
        if (blackImage != null)
        {
            Color bc = blackImage.color;
            bc.a = 0f;
            blackImage.color = bc;
        }

        fc.FadeFromBlack(1f);
    }

    public void FadeAndToBeContinued()
    {
        StartCoroutine(FadeOutAndShowText());
    }

    IEnumerator FadeOutAndShowText()
    {
        float duration = 1f;

        float t = 0f;
        Color blackColor = blackImage.color;

        float startVolume = bgmSource != null ? bgmSource.volume : 0f;

        // ⭐ 畫面 + BGM 同時淡出
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            // 畫面變黑
            blackColor.a = Mathf.Lerp(0f, 1f, lerp);
            blackImage.color = blackColor;

            // 音量淡出
            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, lerp);

            yield return null;
        }

        blackColor.a = 1f;
        blackImage.color = blackColor;

        if (bgmSource != null)
            bgmSource.volume = 0f;

        yield return new WaitForSeconds(2f);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("沒有設定 nextSceneName！");
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

        float startVolume = bgmSource != null ? bgmSource.volume : 0f;

        // 🔴 變黑 + 音量下降
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            c.a = Mathf.Lerp(0f, 1f, lerp);
            blackImage.color = c;

            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, lerp);

            yield return null;
        }

        c.a = 1f;
        blackImage.color = c;

        onMidPoint?.Invoke();

        // 🔵 變回透明 + 音量回來
        t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;

            c.a = Mathf.Lerp(1f, 0f, lerp);
            blackImage.color = c;

            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(0f, startVolume, lerp);

            yield return null;
        }

        c.a = 0f;
        blackImage.color = c;

        if (bgmSource != null)
            bgmSource.volume = startVolume;
    }
}