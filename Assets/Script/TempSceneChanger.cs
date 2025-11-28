using UnityEngine;
using UnityEngine.SceneManagement;
public class TempSceneChanger : MonoBehaviour
{

    [Header("目標場景")]
    [SerializeField]
    string sceneName;

    public Scene1GM scene1GM;

    public void changeScene()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        // 按下 R 時開始
        if (Input.GetKeyDown(KeyCode.R))
        {
            scene1GM.startButtonClicked();
        }

        // 按下 R 時開始
        if (Input.GetKeyDown(KeyCode.S))
        {
            changeScene();
        }
    }
}
