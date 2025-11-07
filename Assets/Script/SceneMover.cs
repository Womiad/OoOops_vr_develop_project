using UnityEngine;
using System.Collections.Generic;

public class SceneMover : MonoBehaviour
{
    [Header("藍牙接收器 (抓 speed 用)")]
    public BluetoothReceiver bluetoothReceiver;

    [Header("玩家/攝影機")]
    public Transform player; // 玩家或攝影機

    [Header("場景方塊設定")]
    public GameObject segmentPrefab;
    public int initialSegments = 5;
    public float offsetX = 10.4f;  // 每段 X 偏移
    public float offsetZ = 89.5f;  // 每段 Z 偏移

    private Queue<GameObject> segments = new Queue<GameObject>();
    private Vector3 nextSpawnPos = Vector3.zero;
    private Vector3 movementDir; // 玩家每幀前進方向

    void Start()
    {
        if (player == null)
            player = Camera.main.transform;

        // 計算玩家每幀前進方向（單位向量）
        movementDir = new Vector3(offsetX, 0, offsetZ).normalized;

        // 預生成初始場景
        for (int i = 0; i < initialSegments; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, nextSpawnPos, Quaternion.identity);
            segments.Enqueue(seg);
            nextSpawnPos += new Vector3(offsetX, 0, offsetZ);
        }
    }

    void Update()
    {
        if (bluetoothReceiver == null || player == null) return;

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
            nextSpawnPos += new Vector3(offsetX, 0, offsetZ);

            // 保持記憶體乾淨
            if (segments.Count > initialSegments)
            {
                GameObject old = segments.Dequeue();
                Destroy(old);
            }
        }
    }
}
