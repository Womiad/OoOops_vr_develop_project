using UnityEngine;
using System.Collections.Generic;

public class SceneMover : MonoBehaviour
{
    [Header("藍牙接收器 (抓 speed 用)")]
    public BluetoothReceiver_new bluetoothReceiver;

    [Header("GM (看state)")]
    public Scene1GM scene1GM;

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

    private Queue<GameObject> allSegments = new Queue<GameObject>();
    private Vector3 nextFrontPos;   // 下一個前方生成位置
    private Vector3 nextBackPos;    // 下一個後方生成位置（往更後面延伸用）
    private Vector3 movementDir;
    private float segmentLength;

    void Start()
    {
        if (player == null)
            player = Camera.main.transform;

        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;

        movementDir = new Vector3(offsetX, offsetY, offsetZ).normalized;
        segmentLength = new Vector3(offsetX, offsetY, offsetZ).magnitude;

        // 先生成後方塊（enqueue 順序：最遠後方 → 起點 → 前方）
        Vector3 backSpawnPos = startPosition;
        Vector3[] backPositions = new Vector3[backSegments];
        for (int i = 0; i < backSegments; i++)
        {
            backSpawnPos -= movementDir * segmentLength;
            backPositions[backSegments - 1 - i] = backSpawnPos; // 反轉順序讓最遠的先入列
        }
        nextBackPos = backSpawnPos - movementDir * segmentLength; // 再往後一格備用

        foreach (var pos in backPositions)
        {
            GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity);
            allSegments.Enqueue(seg);
        }

        // 再生成前方塊
        nextFrontPos = startPosition;
        for (int i = 0; i < initialSegments; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, nextFrontPos, Quaternion.identity);
            allSegments.Enqueue(seg);
            nextFrontPos += movementDir * segmentLength;
        }
    }

    void Update()
    {
        if (bluetoothReceiver == null || player == null) return;
        if (scene1GM.getScene1State() != Scene1State.Run) return;

        float speed = bluetoothReceiver.speed;
        player.position += movementDir * speed * Time.deltaTime;

        // ✅ 前方：接近最後一塊時往前補
        GameObject last = null;
        foreach (var seg in allSegments) last = seg;

        if (Vector3.Distance(player.position, last.transform.position) < offsetZ)
        {
            // 前方補一塊
            GameObject newFront = Instantiate(segmentPrefab, nextFrontPos, Quaternion.identity);
            allSegments.Enqueue(newFront);
            nextFrontPos += movementDir * segmentLength;
        }

        // ✅ 後方：超過 backSegments 塊就刪（與前方補塊脫鉤）
        while (allSegments.Count > initialSegments + backSegments)
        {
            GameObject old = allSegments.Dequeue();
            Destroy(old);
        }
    }
}