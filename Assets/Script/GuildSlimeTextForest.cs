using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GuildSlimeTextForest : MonoBehaviour
{
    public TMP_Text tMP_Text;

    [Header("Slime 表情")]
    public Image slimeImage;
    public Sprite slimeOpenEye;   // 睜眼
    public Sprite slimeCloseEye;  // 閉眼

    [Header("整個指引 UI（播完要隱藏）")]
    public GameObject guideRoot;   // 包含文字 + slime 的 panel

    string[] linesGuild =
    {
        "歡迎來到螢火蟲之森！",
        "這裡有很多神奇的東西呢",
        "聽說有一種能讓人上天堂的發光蘑菇",
        "如果看到請務必靠近看看！",
        "我要先去忙啦，下次見！",
    };

    enum TalkStage
    {
        Guild,
        Run
    }

    TalkStage stage = TalkStage.Guild;
    int index = 0;

    float talkTimer = 0f;     // 每 3 秒切換台詞
    float interval = 3f;

    void Start()
    {
        // ⬆️ 一開始就顯示第一句，不要等 3 秒
        ShowNextLine();
    }

    void Update()
    {
        // Run 階段完全不說話
        if (stage == TalkStage.Run)
            return;

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
        // 防止越界
        if (index >= linesGuild.Length)
        {
            // Guild 播完 → 隱藏 UI
            stage = TalkStage.Run;

            if (guideRoot != null)
                guideRoot.SetActive(false);

            return;
        }

        // 顯示台詞
        tMP_Text.text = linesGuild[index];

        // Slime 表情切換
        slimeImage.sprite = (index % 2 == 0) ? slimeOpenEye : slimeCloseEye;

        index++;
    }
}
