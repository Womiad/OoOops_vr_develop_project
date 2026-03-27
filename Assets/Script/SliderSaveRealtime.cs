using UnityEngine;
using UnityEngine.UI;

public class SliderSaveRealtime : MonoBehaviour
{
    public Slider speedSlider;
    public float step = 0.5f;

    void Start()
    {
        // 讀取舊值
        float saved = PlayerPrefs.GetFloat("speed_scale", 1f);
        speedSlider.value = saved;

        // 監聽滑桿變化
        speedSlider.onValueChanged.AddListener(OnValueChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeValue(step);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeValue(-step);
        }
    }

    void ChangeValue(float amount)
    {
        float newValue = speedSlider.value + amount;

        // 限制在 Slider 範圍內
        newValue = Mathf.Clamp(newValue, speedSlider.minValue, speedSlider.maxValue);

        speedSlider.value = newValue; // 這行會自動觸發 OnValueChanged
    }

    void OnValueChanged(float value)
    {
        PlayerPrefs.SetFloat("speed_scale", value);
        PlayerPrefs.Save();

        Debug.Log("Updated speed_scale: " + value);
    }
}