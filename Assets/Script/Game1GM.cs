using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using UnityEngine.UI;
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
    public BluetoothReceiver_new bluetoothReceiver;
    public TMP_Text scoreText;

    private int score = 0;
    public Game1State game1State;

    public GameObject tomato;
    public GameObject PingPong;
    public GameObject Pan;
    public GameObject Fork;
    public GameObject Spatula;
    public GameObject TableTennisBat;
    public GameObject Pan_left;
    public GameObject Fork_left;
    public GameObject Spatula_left;
    public GameObject TableTennisBat_left;
    public GameObject PracticeLight;
    public GameObject State1Light;
    // public GameObject State2Light;
    public GameObject smallRoom;
    public GameObject midRoom;
    public GameObject statium;

    public AudioSource sfxSource;
    public AudioClip levelUpClip;

    public Game1Fade game1Fade;

    public GameObject slime;
    
    public TMP_Text screenText;

    private float startTime;   // 開場時間

    public int energy = 0;
    public int maxEnergy = 1000;

    public Animator robotAnimator;        // 機器人 Animator
    public Animator robotFootAnimator;        // 機器人 Animator
    public ParticleSystem smokeEffect;    // 冒煙粒子效果
    public float minSmokeDuration = 0.5f; // 最短持續時間
    public float maxSmokeDuration = 5f;   // 最長持續時間

    private bool showingHint = false; // 是否正在顯示提示文字

    public BossHealthSystem bossHealthSystem;


    [Header("壞掉音效")]
    public AudioClip brokenSoundClip;    // 壞掉音效片段

    void Start()
    {
        game1State = Game1State.Practice;
        startTime = Time.time; // 記錄遊戲開始的時間
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;

        // 啟動定時提示 Coroutine
        StartCoroutine(ShowTriggerHintRoutine());
    }

    // Coroutine: 每3秒顯示一次提示文字，持續1秒後回到分數
    // Coroutine: 每3秒顯示一次提示文字，持續1秒後回到分數
    IEnumerator ShowTriggerHintRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            showingHint = true;
            scoreText.text = "Press Trigger to Use Skill!";

            yield return new WaitForSeconds(1f);

            showingHint = false;
            // 下一幀 Update() 會立刻把分數顯示回來
        }
    }

    public void NextState()
    {
        if (game1State == Game1State.Practice)
            ChangeState(Game1State.State1);
        else if (game1State == Game1State.State1)
            ChangeState(Game1State.State2);
        else if (game1State == Game1State.State2)
            ChangeState(Game1State.End);
    }

    float timer = 0f;
    public float interval = 1f / 60f; // 60 FPS

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer -= interval;

            if (bluetoothReceiver.speed > 0)
            {
                energy += (int)(bluetoothReceiver.speed);
            }
        }
        // if(Input.GetKey(KeyCode.DownArrow))
        // {
        //     Debug.Log("trigger！");
        //     energy = 0;
        // }
        if (energy > maxEnergy) energy = maxEnergy;
        if (!showingHint) scoreText.text = "Energy:\n" + energy;

        // TODO
        //如果按下VR手把的板機按鈕，能量歸零，機器人動畫暫停，冒煙效果(粒子效果)播放
        //根據原本持有的能量決定持續長短 (例如能量越多，效果持續越久，0.5~5秒不等)
         // ★ TODO: 板機觸發特效
         // ★ 兩手板機都可以觸發
        bool rightTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
        bool leftTrigger  = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

        if ((rightTrigger || leftTrigger) && energy > 0)
        {
            bossHealthSystem.TakeDamage(energy); 
            TriggerSmokeEffect();
        }
        screenText.text = bluetoothReceiver.outputText;

        // 狀態控制
        if (game1State == Game1State.Practice)
        {
            PingPong.SetActive(true);
            TableTennisBat.SetActive(true);
            TableTennisBat_left.SetActive(true);
            Pan.SetActive(false);
            Pan_left.SetActive(false);
            tomato.SetActive(false);
            PracticeLight.SetActive(true);
            State1Light.SetActive(false);
            // State2Light.SetActive(false);
            Fork.SetActive(false);
            Fork_left.SetActive(false);
            Spatula.SetActive(false);
            Spatula_left.SetActive(false);
            
            smallRoom.SetActive(true);
            midRoom.SetActive(false);
            statium.SetActive(false);
        }
        else if (game1State == Game1State.State1)
        {
            TableTennisBat.SetActive(false);
            TableTennisBat_left.SetActive(false);
            Pan.SetActive(true);
            Pan_left.SetActive(true);
            PracticeLight.SetActive(false);
            State1Light.SetActive(true);
            // State2Light.SetActive(false);
            Fork.SetActive(false);
            Fork_left.SetActive(false);
            Spatula.SetActive(false);
            Spatula_left.SetActive(false);
            // pingpongitem狀態交給本身去判斷
            smallRoom.SetActive(false);
            midRoom.SetActive(true);
            statium.SetActive(false);
        }
        else if (game1State == Game1State.State2)
        {
            TableTennisBat.SetActive(false);
            TableTennisBat_left.SetActive(false);
            PracticeLight.SetActive(false);
            State1Light.SetActive(false);
            // State2Light.SetActive(true);

            smallRoom.SetActive(false);
            midRoom.SetActive(false);
            statium.SetActive(true);
            
            // Pan邏輯交給NPCThrowTomato
            // pingpongitem狀態交給本身去判斷
        }
        else if (game1State == Game1State.End)
        {
            TableTennisBat.SetActive(false);
            TableTennisBat_left.SetActive(false);
            PracticeLight.SetActive(false);
            State1Light.SetActive(false);
            // State2Light.SetActive(true);
            Fork.SetActive(false);
            Fork_left.SetActive(false);
            Spatula.SetActive(false);
            Spatula_left.SetActive(false);
            Pan.SetActive(false);
            Pan_left.SetActive(false);
            slime.SetActive(false);


            smallRoom.SetActive(false);
            midRoom.SetActive(false);
            statium.SetActive(true);
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

    // 新增方法
    void TriggerSmokeEffect()
    {
        // 計算持續時間：能量越多，時間越長
        float t = (float)energy / maxEnergy; // 0~1
        float duration = Mathf.Lerp(minSmokeDuration, maxSmokeDuration, t);

        // 停止機器人動畫
        if (robotAnimator != null)
            robotAnimator.enabled = false;
        if (robotFootAnimator != null)
            robotFootAnimator.enabled = false;

        // 播放壞掉音效
        if (sfxSource != null && brokenSoundClip != null)
            sfxSource.PlayOneShot(brokenSoundClip);

        // 播放冒煙粒子
        if (smokeEffect != null)
        {
            smokeEffect.Play();
            StartCoroutine(StopSmokeAfterDuration(duration));
        }

        // 歸零能量
        energy = 0;
    }

    IEnumerator StopSmokeAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 停止冒煙
        if (smokeEffect != null)
            smokeEffect.Stop();

        // 恢復機器人動畫
        if (robotAnimator != null)
            robotAnimator.enabled = true;
        if (robotFootAnimator != null)
            robotFootAnimator.enabled = true;

    }

}
