using UnityEngine;

public class NPCThrowTomatoOld : MonoBehaviour
{
    [Header("Tomato Settings")]
    public GameObject tomatoPrefab;
    public Transform throwPoint;
    public float throwForce = 10f;

    [Header("Target")]
    public Transform playerHead;

    [Header("Timing")]
    public float minThrowInterval = 2f;
    public float maxThrowInterval = 4f;

    [Header("Throw Randomness")]
    public float horizontalRandom = 0.2f;
    public float randomSpinForce = 8f;

    private float nextThrowTime = 0f;
    private bool firstThrowDone = false;


    void Start()
    {
        // ⭐ 第一次固定在 1.25秒後丟出
        nextThrowTime = Time.time + 1.25f;
    }

    void Update()
    {
        // if (Time.time >= nextThrowTime)
        // {
        //     ThrowTomato();

        //     if (!firstThrowDone)
        //     {
        //         // 第一次完成，之後恢復原本隨機機制
        //         firstThrowDone = true;
        //         ScheduleNextThrow();
        //     }
        //     else
        //     {
        //         ScheduleNextThrow();
        //     }
        // }
    }

    void ScheduleNextThrow()
    {
        nextThrowTime = Time.time + Random.Range(minThrowInterval, maxThrowInterval);
    }

    void ThrowTomato(Transform customPoint)
    {
        Transform point = customPoint != null ? customPoint : throwPoint;

        if (tomatoPrefab == null || point == null || playerHead == null)
        {
            Debug.LogWarning("NPCThrowTomato：缺少設定！");
            return;
        }

        // 生成番茄
        GameObject tomato = Instantiate(tomatoPrefab, point.position, Quaternion.identity);

        Rigidbody rb = tomato.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("番茄 prefab 缺少 Rigidbody！");
            return;
        }

        // 計算方向
        Vector3 direction = playerHead.position - point.position;

        // 計算水平右向量（XZ 平面）
        Vector3 horizontalRight = Vector3.Cross(Vector3.up, direction).normalized;

        // 左右偏移
        float randomOffset = Random.Range(-horizontalRandom, horizontalRandom);
        direction += horizontalRight * randomOffset;

        // 補高
        direction.y += 0.6f;

        // 施加速度（不再 normalize，讓 Y 分量有效）
        rb.linearVelocity = direction.normalized * throwForce;

        // 加上旋轉
        rb.angularVelocity = Random.insideUnitSphere * randomSpinForce;
    }
}
