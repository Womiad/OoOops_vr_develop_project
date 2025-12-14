using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // 如果你用 SceneManager

public class PlayerJump : MonoBehaviour
{
    public float jumpHeight = 300f;     // 上升高度
    public float jumpDuration = 2f;     // 上升所需時間

    public FadeController fc;

    private bool isJumping = false;

    void Start()
    {
        fc.FadeFromBlack(.5f);
    }

    public void TriggerJumpUp()
    {
        if (!isJumping)
        {
            StartCoroutine(JumpUpRoutine());
        }
    }

    private IEnumerator JumpUpRoutine()
    {
        isJumping = true;

        // 🔵 同時啟動：1.8 秒後轉場
        StartCoroutine(ChangeSceneAfterDelay(1.8f));

        float timer = 0f;
        Vector3 startPos = transform.position;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;           
            float height = Mathf.Lerp(0, jumpHeight, t);

            transform.position = new Vector3(
                transform.position.x,
                startPos.y + height,
                transform.position.z
            );

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(
            transform.position.x,
            startPos.y + jumpHeight,
            transform.position.z
        );

        isJumping = false;
    }

    // 🟣 1.8 秒後切換場景
    private IEnumerator ChangeSceneAfterDelay(float delay)
    {
        fc.FadeToBlack(1.8f);
        yield return new WaitForSeconds(delay);

        // 你可以改成你自己的場景名稱
        SceneManager.LoadScene("Cloud");

        // 如果你是用自己的 GM，改成：
        // SceneChanger.Instance.ChangeScene(3);
    }
}
