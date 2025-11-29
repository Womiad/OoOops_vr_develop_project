using UnityEngine;

public class TomatoHit : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 檢查是不是平底鍋
        PanVelocityTracker pan = collision.collider.GetComponent<PanVelocityTracker>();
        if (pan == null) return;

        // 玩家揮動方向
        Vector3 panVelocity = pan.Velocity;

        // 避免站著不動也算打到
        if (panVelocity.magnitude < 0.2f) return;

        // 將番茄往平底鍋移動方向反射出去
        rb.linearVelocity = panVelocity * 1.2f;   // 1.2 可自由調整反射力量
    }
}

