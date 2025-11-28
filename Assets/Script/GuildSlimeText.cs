using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuildSlimeText : MonoBehaviour
{
    public TMP_Text tMP_Text;

    public GameObject buttons;

    [Header("Slime 表情")]
    public Image slimeImage;
    public Sprite slimeOpenEye;   // 睜眼
    public Sprite slimeCloseEye;  // 閉眼

    string[] linesGuild =
    {
        "Hi~你來啦~",
        "今天負責載你的那顆軟軟的是我的朋友",
        "坐上去之後，往下壓一壓......",
        "我們就會彈起來囉！",
        "其他的，邊跳邊看也可以！"
    };

    string[] linesRun =
    {
        "一二一二一二一二",
        "彈一彈，一起向前吧！",
        "風景不錯吧，我也很喜歡這裡~",
        "等你熟練彈跳之後，\n我們就能去更多地方啦！"
    };

    string question = "你好像漸漸熟練了，\n要一起去其他地方逛逛嗎？";

    enum TalkStage
    {
        Guild,
        Run
    }

    TalkStage stage = TalkStage.Guild;
    int index = 0;

    float talkTimer = 0f;     // 每 3 秒切換台詞
    float interval = 3f;

    float runTimer = 0f;      // Run 的 60 秒計時
    float askInterval = 30f;

    bool isAsking = false;    // 正在問問題（停止跑台詞）

    void Start()
    {
        buttons.SetActive(false);
    }

    void Update()
    {
        // Guild 階段不需要 60 秒計時
        if (stage == TalkStage.Run && !isAsking)
        {
            runTimer += Time.deltaTime;

            if (runTimer >= askInterval)
            {
                AskQuestion();
                return;
            }
        }

        // 正在問問題時暫停對話輪播
        if (isAsking) return;

        // 每 3 秒輪播台詞
        talkTimer += Time.deltaTime;

        if (talkTimer >= interval)
        {
            talkTimer = 0f;
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        string[] lines = (stage == TalkStage.Guild) ? linesGuild : linesRun;

        tMP_Text.text = lines[index];

        // Slime 表情切換
        slimeImage.sprite = (index % 2 == 0) ? slimeOpenEye : slimeCloseEye;

        index++;

        // Guild 播完 → 切到 Run
        if (stage == TalkStage.Guild && index >= linesGuild.Length)
        {
            stage = TalkStage.Run;
            index = 0;
            runTimer = 0f;
            return;
        }

        // Run 循環
        if (stage == TalkStage.Run && index >= linesRun.Length)
        {
            index = 0;
        }
    }

    // --------------------------
    // 問問題（停止輪播並顯示按鈕）
    // --------------------------
    void AskQuestion()
    {
        isAsking = true;

        tMP_Text.text = question;
        slimeImage.sprite = slimeOpenEye;

        buttons.SetActive(true);
    }

    // --------------------------
    // 按 Yes → 換場景
    // --------------------------
    public void ButtonYesClicked()
    {
        SceneManager.LoadScene("Scene2");
    }

    // --------------------------
    // 按 No → 回到 Run 循環
    // --------------------------
    public void ButtonNoClicked()
    {
        buttons.SetActive(false);

        isAsking = false;

        // 重設 60 秒計時
        runTimer = 0f;
    }
}
