using UnityEngine;

public class TomatoHitDetector : MonoBehaviour
{
    private bool hasScored = false;   // 每顆番茄只加分一次

    [Header("Hit Sound")]
    public AudioClip hitSound;        // 播放的音效
    public float volume = 1f;

    private AudioSource audioSource;

    public Game1GM game1GM;

    void Start()
    {
        // 你可以把 AudioSource 放在番茄 prefab 上
        audioSource = GetComponent<AudioSource>();

        // 如果 prefab 上沒有 AudioSource，自動加一個
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 檢查是否打到平底鍋（平底鍋須設 Tag "Pan"）
        if (collision.collider.CompareTag("Pan"))
        {
            if (hasScored) return;

            hasScored = true;

            // ✔ 播音效
            if (hitSound != null)
                audioSource.PlayOneShot(hitSound, volume);

            // ✔ 加分
            game1GM.addOneScorePoint();
        }
    }
}
