using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("音效")]
    public AudioClip coinSound;

    [Header("旋轉動畫")]
    public float rotateSpeed = 90f; // 每秒旋轉幾度（Y軸）

    private bool collected = false;

    void Update()
    {
        // 繞本地 Z 軸自轉（配合 -90, 0, 135 的初始朝向）
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime, Space.Self);
    }

    // Trigger 模式（需要金幣 Collider 勾選 Is Trigger）
    void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
        {
            Collect(other.transform.position);
        }
    }

    // 如果用 Collider 非 Trigger 也可以改用這個
    void OnCollisionEnter(Collision collision)
    {
        if (collected) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("MainCamera"))
        {
            Collect(collision.transform.position);
        }
    }

    void Collect(Vector3 pos)
    {
        collected = true;

        // ✅ 累加 PlayerPrefs 金幣數
        int current = PlayerPrefs.GetInt("CoinCount", 0);
        PlayerPrefs.SetInt("CoinCount", current + 1);
        PlayerPrefs.Save();

        Debug.Log($"吃到金幣！目前共 {current + 1} 個");

        // ✅ 播放音效（在原位置播完再消失，所以不能直接 Destroy）
        if (coinSound != null)
            AudioSource.PlayClipAtPoint(coinSound, pos);

        // ✅ 隱藏金幣本體，音效播完再真正銷毀
        gameObject.SetActive(false);
        Destroy(gameObject, coinSound != null ? coinSound.length : 0f);
    }
}