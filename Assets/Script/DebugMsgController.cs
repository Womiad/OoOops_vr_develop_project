using UnityEngine;

public class DebugMsgController : MonoBehaviour
{
    [Header("Debug Canvas")]
    [SerializeField] private Canvas debugCanvas;

    private bool _isVisible = false;

    void Start()
    {
        // 預設不渲染 Canvas
        if (debugCanvas != null)
            debugCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        // 按 M 鍵切換顯示/隱藏
        if (Input.GetKeyDown(KeyCode.M))
        {
            _isVisible = !_isVisible;

            if (debugCanvas != null)
                debugCanvas.gameObject.SetActive(_isVisible);
        }
    }
}