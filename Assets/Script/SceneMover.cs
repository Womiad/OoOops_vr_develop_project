using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SceneMover : MonoBehaviour
{
    [Header("藍牙接收器 (抓 speed 用)")]
    public BluetoothReceiver bluetoothReceiver;

    [Header("GM (看state)")]
    public Scene1GM scene1GM;

    [Header("玩家/攝影機")]
    public Transform player; // 玩家或攝影機

    [Header("場景方塊設定")]
    public GameObject segmentPrefab;
    public int initialSegments = 5;  // 玩家前方生成數量
    public int backSegments = 2;     // 玩家後方生成數量
    public float offsetX = 10.4f;    // 每段 X 偏移
    public float offsetY = 0f;    // 每段 y 偏移
    public float offsetZ = 89.5f;    // 每段 Z 偏移

    [Header("起始位置設定")]
    public Vector3 startPosition = new Vector3(-21.4f, 0f, -54.2f); // ✅ 起始點

    private Queue<GameObject> segments = new Queue<GameObject>();
    private Vector3 nextSpawnPos;
    private Vector3 movementDir; // 玩家每幀前進方向

    void Start()
    {
        if (player == null)
            player = Camera.main.transform;

        // 計算玩家每幀前進方向（單位向量）
        movementDir = new Vector3(offsetX, offsetY, offsetZ).normalized;

        // 設定初始位置
        nextSpawnPos = startPosition;

        // 取得反方向（用來往後生成）
        Vector3 backwardDir = -movementDir;

        // ✅ 先往後生成幾段（造景用）
        Vector3 backSpawnPos = startPosition;
        for (int i = 0; i < backSegments; i++)
        {
            backSpawnPos += backwardDir * new Vector3(offsetX, offsetY, offsetZ).magnitude;
            GameObject backSeg = Instantiate(segmentPrefab, backSpawnPos, Quaternion.identity);
            segments.Enqueue(backSeg);
        }

        // ✅ 再往前生成主要行進方向的場景
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
        if (scene1GM.getScene1State() != Scene1State.Run) return;

        float speed = bluetoothReceiver.speed;

        // ✅ 玩家沿著 movementDir 前進
        player.position += movementDir * speed * Time.deltaTime;

        // 取得最後一段
        GameObject last = null;
        foreach (var seg in segments)
            last = seg;

        // 當玩家接近最後一段時 → 生成新段
        float distanceToLast = Vector3.Distance(player.position, last.transform.position);
        if (distanceToLast < offsetZ)
        {
            GameObject newSeg = Instantiate(segmentPrefab, nextSpawnPos, Quaternion.identity);
            segments.Enqueue(newSeg);
            nextSpawnPos += new Vector3(offsetX, offsetY, offsetZ);

            // 保持記憶體乾淨（只清掉最前面的，保留背後造景）
            while (segments.Count > initialSegments + backSegments)
            {
                GameObject old = segments.Dequeue();
                Destroy(old);
            }
        }
    }
    
}
