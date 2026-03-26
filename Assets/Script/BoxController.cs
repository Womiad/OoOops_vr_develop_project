using UnityEngine;
using System.Collections;

public class BoxController : MonoBehaviour
{
    [Header("動畫控制")]
    public Animator boxAnimator;
    public Animator targetAnimator;
    public string triggerName = "click";
    public string triggerNameO = "oooops";

    [Header("Oculus 手部/指向偵測")]
    public OVRInput.Button clickButton = OVRInput.Button.PrimaryIndexTrigger;

    private Camera mainCamera;

    [Header("點擊文字")]
    public GameObject clickText;

    private bool isTriggered = false;

    void Start()
    {
        if (boxAnimator == null)
        {
            boxAnimator = GetComponent<Animator>();
        }

        mainCamera = Camera.main;
    }

    void Update()
    {
        // ✅ 左右手都可觸發
        if ((OVRInput.GetDown(clickButton, OVRInput.Controller.RTouch) ||
             OVRInput.GetDown(clickButton, OVRInput.Controller.LTouch)) 
             && !isTriggered)
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    PlayClickAnimation();
                }
            }
        }
    }

    private void PlayClickAnimation()
    {
        if (boxAnimator != null)
        {
            boxAnimator.SetTrigger(triggerName);
            clickText.SetActive(false);
            Debug.Log("Box 被點擊，動畫觸發!");
            StartCoroutine(PlayAnimationWithDelay());
        }
    }

    IEnumerator PlayAnimationWithDelay()
    {
        isTriggered = true;

        Debug.Log("點擊 Box，2秒後播放『另一個物件』動畫");

        yield return new WaitForSeconds(2f);

        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger(triggerNameO);
            Debug.Log("目標物件動畫播放！");
        }
        else
        {
            Debug.LogWarning("沒有指定 targetAnimator！");
        }
    }
}