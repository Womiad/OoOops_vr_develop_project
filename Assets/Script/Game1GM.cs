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
    public GameObject Fork;
    public GameObject Spatula;
    public GameObject TableTennisBat;
    public GameObject PracticeLight;
    public GameObject State1Light;
    public GameObject State2Light;

    public AudioSource sfxSource;
    public AudioClip levelUpClip;
    


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
                ChangeState(Game1State.State1);
            }
        }else if (game1State == Game1State.State1)
        {
            float elapsed = Time.time - startTime;

    
            if (elapsed >= 90f || score >= 32)
            {
                ChangeState(Game1State.State2);
            }
        }

        // 狀態控制
        if (game1State == Game1State.Practice)
        {
            PingPong.SetActive(true);
            TableTennisBat.SetActive(true);
            Pan.SetActive(false);
            tomato.SetActive(false);
            PracticeLight.SetActive(true);
            State1Light.SetActive(false);
            State2Light.SetActive(false);
            Fork.SetActive(false);
            Spatula.SetActive(false);
        }
        else if (game1State == Game1State.State1)
        {
            TableTennisBat.SetActive(false);
            Pan.SetActive(true);
            PracticeLight.SetActive(false);
            State1Light.SetActive(true);
            State2Light.SetActive(false);
            Fork.SetActive(false);
            Spatula.SetActive(false);
            // pingpongitem狀態交給本身去判斷
        }
        else if (game1State == Game1State.State2)
        {
            TableTennisBat.SetActive(false);
            PracticeLight.SetActive(false);
            State1Light.SetActive(false);
            State2Light.SetActive(true);
            
            // Pan邏輯交給pingpongitem　（之後要改到NPCThrowTomato那邊）
            // pingpongitem狀態交給本身去判斷
        }
    }

    public void addOneScorePoint()
    {
        score++;
    }


    void ChangeState(Game1State newState)
    {
        if (game1State == newState) return;

        game1State = newState;

        if (newState == Game1State.State1 || newState == Game1State.State2)
        {
            sfxSource.PlayOneShot(levelUpClip);
        }
    }

}
