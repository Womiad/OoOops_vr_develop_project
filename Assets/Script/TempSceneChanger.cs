using UnityEngine;
using UnityEngine.SceneManagement;
public class TempSceneChanger : MonoBehaviour
{

    [Header("目標場景")]
    [SerializeField]
    string sceneName;

    public void changeScene()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
    }
}
