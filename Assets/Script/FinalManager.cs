using UnityEngine;
using System.Collections;

public enum FinalSceneState
{
    Intro_Ani,
    Walk,
    Final_Ani
}

public class FinalManager : MonoBehaviour
{
    private FinalSceneState currentState;

    public FinalSceneState GetState() => currentState;

    public void SetState(FinalSceneState newState)
    {
        currentState = newState;
        Debug.Log($"[FinalManager] State → {newState}");

        switch (newState)
        {
            case FinalSceneState.Intro_Ani:
                StartCoroutine(FinalSequence());
                break;

            case FinalSceneState.Walk:
                break;

            case FinalSceneState.Final_Ani:
                StartCoroutine(FinalAniSequence());
                break;
        }
    }

    // ==================
    // Walk 設定
    // ==================
    [Header("Walk 設定")]
    public BluetoothReceiver_new bluetoothReceiver;
    public Transform player;
    public float walkStopZ = 2.54f;

    // ==================
    // 金幣設定
    // ==================
    [Header("金幣設定")]
    public GameObject coinPrefab;
    public Transform coinDropPoint;
    public Transform coinTrigger;
    public float coinTriggerRadius = 1f;
    public AudioClip coinSound;
    public int coinCount = 20;
    public float coinSpawnInterval = 0.1f;
    public float coinFallDuration = 1f;
    public float coinSpreadRadius = 1f;

    // ==================
    // 寶箱設定
    // ==================
    [Header("寶箱設定")]
    public Animator smallChestAnimator;
    public Animator bigChestAnimator;
    public string smallChestShrinkTrigger = "Shrink";
    public string bigChestGrowTrigger = "Grow";
    public float smallChestAnimDuration = 1f;
    public float bigChestAnimDuration = 1f;
    public float chestOverlapTime = 0.3f;

    // ==================
    // Final_Ani 設定
    // ==================
    [Header("Final_Ani 設定")]
    public string bigChestOpenTrigger = "Open";
    public float bigChestOpenDuration = 1f;
    public FinalFade finalFade;

    // ==================
    // 提示文字 / 流程
    // ==================
    [Header("提示文字 (3D)")]
    public GameObject hintText;

    public GameObject OoOoPsLogo;

    [Header("流程時間設定")]
    public float delayBetweenSteps = 0.5f;

    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;

        if (player == null)
            player = Camera.main.transform;

        if (hintText != null)
            hintText.SetActive(false);

        OoOoPsLogo.SetActive(false);

        SetState(FinalSceneState.Intro_Ani);
    }

    void Update()
    {
        if (currentState != FinalSceneState.Walk) return;
        if (bluetoothReceiver == null || player == null) return;

        float speed = bluetoothReceiver.speed / 10f;

        Vector3 pos = player.position;
        pos.z = Mathf.Min(pos.z + speed * Time.deltaTime, walkStopZ);
        player.position = pos;

        if (player.position.z >= walkStopZ)
            SetState(FinalSceneState.Final_Ani);
    }

    // ==================
    // Intro_Ani 流程
    // ==================
    IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(1.5f);

        StartCoroutine(SpawnCoins());
        float waitForCoins = coinCount * coinSpawnInterval + coinFallDuration + delayBetweenSteps;
        yield return new WaitForSeconds(waitForCoins);

        if (smallChestAnimator != null)
            smallChestAnimator.SetTrigger(smallChestShrinkTrigger);

        yield return new WaitForSeconds(smallChestAnimDuration - chestOverlapTime);

        if (bigChestAnimator != null)
            bigChestAnimator.SetTrigger(bigChestGrowTrigger);

        yield return new WaitForSeconds(bigChestAnimDuration + delayBetweenSteps);
        

        if (hintText != null)
            hintText.SetActive(true);

        SetState(FinalSceneState.Walk);
    }

    // ==================
    // Final_Ani 流程
    // ==================
    IEnumerator FinalAniSequence()
    {
        
        // 大寶箱開啟動畫
        if (bigChestAnimator != null)
            bigChestAnimator.SetTrigger(bigChestOpenTrigger);

        yield return new WaitForSeconds(bigChestOpenDuration + delayBetweenSteps);

        OoOoPsLogo.SetActive(true);
        yield return new WaitForSeconds(2f);

        // FadeOut → 換場景
        if (finalFade != null)
            finalFade.FadeAndToBeContinued();
    }

    // ==================
    // 金幣生成
    // ==================
    IEnumerator SpawnCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = coinDropPoint.position + new Vector3(
                Random.Range(-coinSpreadRadius, coinSpreadRadius),
                0f,
                Random.Range(-coinSpreadRadius, coinSpreadRadius)
            );

            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.Euler(-90f, 0f, 135f));
            StartCoroutine(CoinFallAndCollect(coin));

            yield return new WaitForSeconds(coinSpawnInterval);
        }
    }

    IEnumerator CoinFallAndCollect(GameObject coin)
    {
        Vector3 startPos = coin.transform.position;
        Vector3 endPos = coinTrigger.position + new Vector3(
            Random.Range(-coinTriggerRadius, coinTriggerRadius),
            0f,
            Random.Range(-coinTriggerRadius, coinTriggerRadius)
        );

        float elapsed = 0f;
        while (elapsed < coinFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / coinFallDuration;
            float easedT = t * t;
            coin.transform.position = Vector3.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        if (coinSound != null)
            AudioSource.PlayClipAtPoint(coinSound, endPos);

        Destroy(coin);
    }
}