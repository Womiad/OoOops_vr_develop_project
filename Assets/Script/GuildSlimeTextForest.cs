using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
// public class TextAudioLine
// {
//     public string text;
//     public AudioClip audio;
// }

public class GuildSlimeTextForest : MonoBehaviour
{
    public TMP_Text tMP_Text;

    [Header("Slime 表情")]
    public Image slimeImage;
    public Sprite slimeOpenEye;
    public Sprite slimeCloseEye;

    [Header("整個指引 UI（播完要隱藏）")]
    public GameObject guideRoot;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("台詞（直接在 Inspector 設定）")]
    public TextAudioLine[] linesGuild;

    enum TalkStage
    {
        Guild,
        Run
    }

    TalkStage stage = TalkStage.Guild;
    int index = 0;

    float talkTimer = 0f;
    float interval = 3f;

    void Start()
    {
        // 一開始就顯示第一句
        ShowCurrentLine();
    }

    void Update()
    {
        if (stage == TalkStage.Run)
            return;

        talkTimer += Time.deltaTime;

        if (talkTimer >= interval)
        {
            talkTimer = 0f;
            ShowNextLine();
        }
    }

    void ShowCurrentLine()
    {
        if (index >= linesGuild.Length)
            return;

        TextAudioLine line = linesGuild[index];

        tMP_Text.text = line.text;

        // 播音效
        if (line.audio != null)
        {
            audioSource.clip = line.audio;
            audioSource.Play();
        }

        // 表情切換
        slimeImage.sprite = (index % 2 == 0) ? slimeOpenEye : slimeCloseEye;
    }

    void ShowNextLine()
    {
        index++;

        if (index >= linesGuild.Length)
        {
            stage = TalkStage.Run;

            if (guideRoot != null)
                guideRoot.SetActive(false);

            return;
        }

        ShowCurrentLine();
    }
}