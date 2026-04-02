using UnityEngine;
using UnityEngine.SceneManagement;
public class TempSceneChangerGame1 : MonoBehaviour
{

    [Header("目標場景")]
    [SerializeField]
    string sceneName;

    public void changeScene()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        // 按下 R 時開始
        if (Input.GetKeyDown(KeyCode.S))
        {
            changeScene();
        }
    }
}
