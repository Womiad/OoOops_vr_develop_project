using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalBackToStart : MonoBehaviour
{

    public string sceneName = "Scene1_new"; // 預設場景名稱，可以在 Inspector 中修改
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    public void BackTpStart()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        // 按下 S 時開始
        if (Input.GetKeyDown(KeyCode.S))
        {
            BackTpStart();
        }
    }
}
