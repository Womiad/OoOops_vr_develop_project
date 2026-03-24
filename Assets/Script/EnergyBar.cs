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

    private float lastValue; // 用來追蹤前一幀的能量值

    [Header("Pitch 設定")]
    public float minPitch = 1f; // 最低音調
    public float maxPitch = 2f; // 最高音調

    void Start()
    {
        lastValue = (float)game1GM.energy / game1GM.maxEnergy;
        if (energyIncreaseSound != null)
            energyIncreaseSound.loop = true; // 循環播放
    }

    void Update()
    {
        value = (float)game1GM.energy / game1GM.maxEnergy;
        if (value > 1f) value = 1f;
        fillImage.fillAmount = value;

        // 比較能量是否升高
        if (value > lastValue)
        {
            if (!energyIncreaseSound.isPlaying)
            {
                energyIncreaseSound.Play();
            }

            // 根據能量更新 pitch
            float targetPitch = Mathf.Lerp(minPitch, maxPitch, value);
            energyIncreaseSound.pitch = targetPitch;
        }
        else
        {
            if (energyIncreaseSound.isPlaying)
            {
                energyIncreaseSound.Stop();
            }
        }

        lastValue = value; // 更新前一幀的能量值
    }
}