using UnityEngine;
using System.Collections.Generic;

public class Scene2Mover : MonoBehaviour
{
    [Header("藍牙接收器 (抓 speed 用)")]
    public BluetoothReceiver_new bluetoothReceiver;

    [Header("玩家/攝影機")]
    public Transform player;

    [Header("場景方塊設定")]
    public GameObject segmentPrefab;
    public int initialSegments = 5;
    public int backSegments = 2;
    public float offsetX = 10.4f;
    public float offsetY = 0f;
    public float offsetZ = 89.5f;

    [Header("起始位置設定")]
    public Vector3 startPosition = new Vector3(-21.4f, 0f, -54.2f);

    private Queue<GameObject> segments = new Queue<GameObject>();
    private Vector3 nextSpawnPos;
    private Vector3 movementDir;

    [Header("蘑菇生成設定")]
    public GameObject mushroomPrefab;
    public float spawnMushroomDistance = 300f;
    public float mushroomAheadOffset = 50f;
    private bool mushroomSpawned = false;

    [Header("玩家動畫設定")]
    public PlayerJump playerJump;
    private bool mushroomTriggered = false;
    private Vector3 mushroomSpawnPos;

    // ===============================
    // 🪙 金幣生成設定
    // ===============================
    [Header("金幣生成設定")]
    public GameObject coinPrefab;           // 金幣 Prefab（需掛 Coin.cs）
    public AudioClip coinSound;             // 吃金幣音效
    public float coinBaseInterval = 80f;    // 固定間隔距離
    public float coinRandomInterval = 40f;  // 額外隨機距離 (0 ~ 此值)
    public int coinMinCount = 3;            // 最少金幣數
    public int coinMaxCount = 5;            // 最多金幣數
    public float coinSpacing = 3f;          // 同組金幣之間的間距
    public float coinHeightOffset = 1.5f;   // 金幣高度偏移（相對玩家）

    private float nextCoinSpawnDistance;    // 下一次生成金幣的累計距離

    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;

        if (player == null)
            player = Camera.main.transform;

        movementDir = new Vector3(offsetX, offsetY, offsetZ).normalized;
        nextSpawnPos = startPosition;

        // ✅ 重置金幣計數
        PlayerPrefs.SetInt("CoinCount", 0);
        PlayerPrefs.Save();

        // ✅ 設定第一次生成金幣的距離
        nextCoinSpawnDistance = coinBaseInterval + Random.Range(0f, coinRandomInterval);

        // 往後生成場景
        Vector3 backwardDir = -movementDir;
        Vector3 backSpawnPos = startPosition;
        for (int i = 0; i < backSegments; i++)
        {
            backSpawnPos += backwardDir * new Vector3(offsetX, offsetY, offsetZ).magnitude;
            GameObject backSeg = Instantiate(segmentPrefab, backSpawnPos, Quaternion.identity);
            segments.Enqueue(backSeg);
        }

        // 往前生成場景
        for (int i = 0; i < initialSegments; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, nextSpawnPos, Quaternion.identity);
            segments.Enqueue(seg);
            nextSpawnPos += new Vector3(offsetX, offsetY, offsetZ);
        }
    }

    void Update()
    {
        if (bluetoothReceiver == null || player == null) return;

        float speed = bluetoothReceiver.speed;
        player.position += movementDir * speed * Time.deltaTime;

        // 場景接力生成
        GameObject last = null;
        foreach (var seg in segments) last = seg;

        float distanceToLast = Vector3.Distance(player.position, last.transform.position);
        if (distanceToLast < offsetZ)
        {
            GameObject newSeg = Instantiate(segmentPrefab, nextSpawnPos, Quaternion.identity);
            segments.Enqueue(newSeg);
            nextSpawnPos += new Vector3(offsetX, offsetY, offsetZ);

            while (segments.Count > initialSegments + backSegments)
            {
                GameObject old = segments.Dequeue();
                Destroy(old);
            }
        }

        // 蘑菇生成
        if (!mushroomSpawned)
        {
            float traveled = Vector3.Distance(player.position, startPosition);
            if (traveled >= spawnMushroomDistance)
            {
                mushroomSpawnPos = player.position + movementDir * mushroomAheadOffset;
                Instantiate(mushroomPrefab, mushroomSpawnPos, Quaternion.identity);
                mushroomSpawned = true;
                Debug.Log("蘑菇已生成！");
            }
        }

        if (mushroomSpawned && !mushroomTriggered)
        {
            float dist = Vector3.Distance(player.position, mushroomSpawnPos);
            if (dist < 5f)
            {
                playerJump.TriggerJumpUp();
                mushroomTriggered = true;
                Debug.Log("玩家吃到蘑菇動畫觸發！");
            }
        }

        // ===============================
        // 🪙 金幣生成邏輯
        // ===============================
        float traveledTotal = Vector3.Distance(player.position, startPosition);
        if (coinPrefab != null && traveledTotal >= nextCoinSpawnDistance)
        {
            SpawnCoinGroup();
            nextCoinSpawnDistance = traveledTotal + coinBaseInterval + Random.Range(0f, coinRandomInterval);
        }
    }

    void SpawnCoinGroup()
    {
        int count = Random.Range(coinMinCount, coinMaxCount + 1);
        Vector3 basePos = player.position
                        + movementDir * 40f
                        + Vector3.up * coinHeightOffset;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = basePos + movementDir * (i * coinSpacing);

            // ✅ 讓金幣面朝玩家前進方向直立
            Quaternion coinRotation = Quaternion.Euler(-90f, 0f, 135f);
            GameObject coin = Instantiate(coinPrefab, spawnPos, coinRotation);

            Coin coinScript = coin.GetComponent<Coin>();
            if (coinScript != null)
                coinScript.coinSound = coinSound;
        }

        Debug.Log($"生成了 {count} 個金幣！");
    }
}