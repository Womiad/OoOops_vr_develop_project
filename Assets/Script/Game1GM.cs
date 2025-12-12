using UnityEngine;
using TMPro;

public enum Game1State
{
    Practice,
    State1,
    State2,
    End
}

public class Game1GM : MonoBehaviour
{
    public TMP_Text scoreText;

    private int score = 0;
    public Game1State game1State;

    public GameObject tomato;
    public GameObject PingPong;
    public GameObject Pan;
    public GameObject TableTennisBat;

    private float startTime;   // 開場時間

    void Start()
    {
        game1State = Game1State.Practice;
        startTime = Time.time; // 記錄遊戲開始的時間
    }

    void Update()
    {
        scoreText.text = score + "";

        // ★ 檢查是否該切換到 State1
        if (game1State == Game1State.Practice)
        {
            float elapsed = Time.time - startTime;

            if (elapsed >= 40f || score >= 16)
            {
                game1State = Game1State.State1;
            }
        }

        // 狀態控制
        if (game1State == Game1State.Practice)
        {
            PingPong.SetActive(true);
            TableTennisBat.SetActive(true);
            Pan.SetActive(false);
            tomato.SetActive(false);
        }
        else if (game1State == Game1State.State1)
        {
            TableTennisBat.SetActive(false);
            Pan.SetActive(true);
            // pingpongitem狀態交給本身去判斷
        }
    }

    public void addOneScorePoint()
    {
        score++;
    }
}
