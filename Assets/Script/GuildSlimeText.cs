using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class TextAudioLine
{
    public string text;
    public AudioClip audio;
}

public class GuildSlimeText : MonoBehaviour
{
    public TMP_Text tMP_Text;
    public GameObject buttons;

    [Header("Slime 表情")]
    public Image slimeImage;
    public Sprite slimeOpenEye;
    public Sprite slimeCloseEye;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Guild 台詞")]
    public TextAudioLine[] linesGuild;

    [Header("Run 台詞")]
    public TextAudioLine[] linesRun;

    [Header("問題")]
    public TextAudioLine question;

    enum TalkStage
    {
        Guild,
        Run
    }

    TalkStage stage = TalkStage.Guild;
    int index = 0;

    float talkTimer = 0f;
    float interval = 3f;

    float runTimer = 0f;
    float askInterval = 30f;

    bool isAsking = false;

    void Start()
    {
        buttons.SetActive(false);
        ShowCurrentLine();
    }

    void Update()
    {
        if (stage == TalkStage.Run && !isAsking)
        {
            runTimer += Time.deltaTime;

            if (runTimer >= askInterval)
            {
                AskQuestion();
                return;
            }
        }

        if (isAsking) return;

        talkTimer += Time.deltaTime;

        if (talkTimer >= interval)
        {
            talkTimer = 0f;
            ShowNextLine();
        }
    }

    void ShowCurrentLine()
    {
        TextAudioLine line = GetCurrentLines()[index];

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

        if (stage == TalkStage.Guild && index >= linesGuild.Length)
        {
            stage = TalkStage.Run;
            index = 0;
            runTimer = 0f;
        }
        else if (stage == TalkStage.Run && index >= linesRun.Length)
        {
            index = 0;
        }

        ShowCurrentLine();
    }

    TextAudioLine[] GetCurrentLines()
    {
        return (stage == TalkStage.Guild) ? linesGuild : linesRun;
    }

    // --------------------------
    // 問問題
    // --------------------------
    void AskQuestion()
    {
        isAsking = true;

        tMP_Text.text = question.text;

        if (question.audio != null)
        {
            audioSource.clip = question.audio;
            audioSource.Play();
        }

        slimeImage.sprite = slimeOpenEye;

        buttons.SetActive(true);
    }

    // --------------------------
    // Yes
    // --------------------------
    public void ButtonYesClicked()
    {
        SceneManager.LoadScene("Scene2_new2");
    }

    // --------------------------
    // No
    // --------------------------
    public void ButtonNoClicked()
    {
        buttons.SetActive(false);
        isAsking = false;
        runTimer = 0f;
    }
}