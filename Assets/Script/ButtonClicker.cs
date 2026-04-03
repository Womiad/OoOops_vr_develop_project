using UnityEngine;
using UnityEngine.Events;

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

    private bool isHovering = false;
    private float lastPressTime = -999f;
    [Header("Oculus 手部/指向偵測")]
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;

    void Update()
    {
        Debug.Log($"left: {leftHandRay}, right: {rightHandRay}");
        isHovering = IsRayHitting(leftHandRay) || IsRayHitting(rightHandRay);

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

    bool IsRayHitting(Transform rayOrigin)
    {
        if (rayOrigin == null) return false;

        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit, rayDistance))
        {
            // 打到東西：畫綠線到碰撞點
            Debug.DrawRay(rayOrigin.position, rayOrigin.forward * hit.distance, Color.green);
            return hit.collider.gameObject == gameObject;
        }

        // 沒打到：畫紅線到最遠距離
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