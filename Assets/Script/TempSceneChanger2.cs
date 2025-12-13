using UnityEngine;
using UnityEngine.SceneManagement;
public class TempSceneChanger2 : MonoBehaviour
{


    public PlayerJump pj;

    void Update()
    {

        // 按下 S 時開始
        if (Input.GetKeyDown(KeyCode.S))
        {
            pj.TriggerJumpUp();
        }
    }
}
