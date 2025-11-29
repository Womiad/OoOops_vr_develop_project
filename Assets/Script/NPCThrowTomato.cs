using UnityEngine;

public class NPCThrowTomato : MonoBehaviour
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
    public float randomSpinForce = 8f;     // ⭐ 新增：旋轉力度

    private float nextThrowTime = 0f;

    void Start()
    {
        ScheduleNextThrow();
    }

    void Update()
    {
        if (Time.time >= nextThrowTime)
        {
            ThrowTomato();
            ScheduleNextThrow();
        }
    }

    void ScheduleNextThrow()
    {
        nextThrowTime = Time.time + Random.Range(minThrowInterval, maxThrowInterval);
    }

    void ThrowTomato()
    {
        if (tomatoPrefab == null || throwPoint == null || playerHead == null)
        {
            Debug.LogWarning("NPCThrowTomato：缺少設定！");
            return;
        }

        GameObject tomato = Instantiate(tomatoPrefab, throwPoint.position, Quaternion.identity);

        Rigidbody rb = tomato.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("番茄 prefab 缺少 Rigidbody！");
            return;
        }

        //-------------------------------
        //      投擲方向
        //-------------------------------
        Vector3 direction = (playerHead.position - throwPoint.position).normalized;

        float randomOffset = Random.Range(-horizontalRandom, horizontalRandom);
        direction += transform.right * randomOffset;

        direction += Vector3.up * 0.6f;
        direction = direction.normalized;

        rb.linearVelocity = direction * throwForce;

        //-------------------------------
        //      ⭐ 番茄旋轉 / 滾動
        //-------------------------------
        rb.angularVelocity = Random.insideUnitSphere * randomSpinForce;
    }
}
