using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BossHealthSystem : MonoBehaviour
{
    [Header("Hearts / Phase")]
    public GameObject[] hearts;

    [Header("HP Bar")]
    public RawImage hpBar;           // 原本的 RawImage
    private RectTransform hpBarRect; // 控制寬度
    private float originalWidth;

    [Header("HP Settings")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Effects")]
    public float damageSmoothSpeed = 5f;
    public float invincibleTime = 1f;

    [Header("Events")]
    public UnityEvent onPhaseChanged;
    public UnityEvent onDeath;

    private int currentPhase;
    private float displayHP;
    private bool isInvincible = false;

    void Start()
    {
        hpBarRect = hpBar.GetComponent<RectTransform>();
        originalWidth = hpBarRect.sizeDelta.x;

        ResetHealth();
    }

    void Update()
    {
        UpdateBarSmooth();
    }

    public void TakeDamage(float dmg)
    {
        if (isInvincible || currentPhase <= 0) return;

        currentHP -= dmg;

        if (currentHP <= 0)
            StartCoroutine(HandlePhaseChange());
    }

    IEnumerator HandlePhaseChange()
    {
        isInvincible = true;

        currentPhase--;
        UpdateHearts();
        onPhaseChanged?.Invoke();

        if (currentPhase <= 0)
        {
            currentHP = 0;
            UpdateBarInstant();
            onDeath?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        currentHP = maxHP;
        displayHP = maxHP;

        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    public void ResetHealth()
    {
        currentPhase = hearts.Length;
        currentHP = maxHP;
        displayHP = maxHP;
        isInvincible = false;

        UpdateHearts();
        UpdateBarInstant();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].SetActive(i < currentPhase);
    }

    void UpdateBarSmooth()
    {
        displayHP = Mathf.Lerp(displayHP, currentHP, Time.deltaTime * damageSmoothSpeed);
        float normalized = displayHP / maxHP;

        // 控制 RawImage 寬度
        hpBarRect.sizeDelta = new Vector2(originalWidth * normalized, hpBarRect.sizeDelta.y);

        // 變色
        hpBar.color = GetHPColor(normalized);
    }

    void UpdateBarInstant()
    {
        float normalized = currentHP / maxHP;
        hpBarRect.sizeDelta = new Vector2(originalWidth * normalized, hpBarRect.sizeDelta.y);
        hpBar.color = GetHPColor(normalized);
    }

    Color GetHPColor(float t)
    {
        if (t > 0.5f)
            return Color.Lerp(Color.yellow, Color.green, (t - 0.5f) * 2f);
        else
            return Color.Lerp(Color.red, Color.yellow, t * 2f);
    }
}