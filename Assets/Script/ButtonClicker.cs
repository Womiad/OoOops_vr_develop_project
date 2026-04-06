using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonClicker : MonoBehaviour
{
    [Header("觸發設定")]
    public UnityEvent onPressed;

    [Header("防連按")]
    public float cooldown = 1f;

    [Header("Ray 來源（左右手控制器）")]
    public Transform leftHandRay;
    public Transform rightHandRay;
    public float rayDistance = 10f;

    [Header("Oculus 手部/指向偵測")]
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;

    [Header("Hover 顏色")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.85f, 0.85f, 0.85f); // 淺灰

    private bool isHovering = false;
    private bool wasHovering = false;
    private float lastPressTime = -999f;
    private Image buttonImage;

    void Awake()
    {
        // 優先抓自己身上的 Button，再 fallback 到 Image
        Button btn = GetComponent<Button>();
        buttonImage = btn != null ? btn.targetGraphic as Image : GetComponent<Image>();
    }

    void Update()
    {
        Debug.Log($"left: {leftHandRay}, right: {rightHandRay}");
        isHovering = IsRayHitting(leftHandRay) || IsRayHitting(rightHandRay);

        // 只在狀態改變時才改色，避免每幀都 set
        if (isHovering != wasHovering)
        {
            SetHoverColor(isHovering);
            wasHovering = isHovering;
        }

        if (isHovering)
        {
            Debug.Log("isHovering");
            if (OVRInput.GetDown(clickButton, OVRInput.Controller.RTouch) ||
                OVRInput.GetDown(clickButton, OVRInput.Controller.LTouch))
            {
                TryPress();
            }
        }
    }

    void SetHoverColor(bool hovering)
    {
        if (buttonImage == null) return;
        buttonImage.color = hovering ? hoverColor : normalColor;
    }

    bool IsRayHitting(Transform rayOrigin)
    {
        if (rayOrigin == null) return false;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, rayDistance))
        {
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * hit.distance, Color.green);
            return hit.collider.gameObject == gameObject;
        }

        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * rayDistance, Color.red);
        return false;
    }

    void TryPress()
    {
        if (Time.time - lastPressTime < cooldown) return;
        lastPressTime = Time.time;

        Debug.Log("[ButtonClicker] 觸發！");
        onPressed?.Invoke();
    }
}