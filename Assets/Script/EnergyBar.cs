using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    public Image fillImage;

    [Range(0,1)]
    public float value = 1f;

    public Game1GM game1GM;

    // 音效
    public AudioSource energyIncreaseSound;

    private float lastValue;

    [Header("Pitch 設定")]
    public float minPitch = 1f;
    public float maxPitch = 2f;

    // ⭐ 新增：音效延遲停止時間
    private float soundTimer = 0f;
    public float soundHoldTime = 0.1f; // 0.1秒內沒增加才停止

    void Start()
    {
        lastValue = (float)game1GM.energy / game1GM.maxEnergy;

        if (energyIncreaseSound != null)
            energyIncreaseSound.loop = true;
    }

    void Update()
    {
        value = (float)game1GM.energy / game1GM.maxEnergy;
        value = Mathf.Clamp01(value);

        fillImage.fillAmount = value;

        // ⭐ 如果有增加 → 重置計時器
        if (value > lastValue)
        {
            soundTimer = soundHoldTime;

            if (!energyIncreaseSound.isPlaying)
            {
                energyIncreaseSound.Play();
            }
        }

        // ⭐ 計時器遞減
        soundTimer -= Time.deltaTime;

        // ⭐ 更新 pitch（只要在播放就更新）
        if (energyIncreaseSound.isPlaying)
        {
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, value);
            energyIncreaseSound.pitch = targetPitch;
        }

        // ⭐ 超過時間才停止（避免抖動）
        if (soundTimer <= 0f && energyIncreaseSound.isPlaying)
        {
            energyIncreaseSound.Stop();
        }

        lastValue = value;
    }
}