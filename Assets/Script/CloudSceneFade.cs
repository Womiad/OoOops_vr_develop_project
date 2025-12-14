using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // 如果你用 SceneManager

public class CloudSceneFade : MonoBehaviour
{
    public FadeController fc;

    public AudioClip Sound;
    public float volume = 1f;

    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fc.FadeFromBlack(.5f);
        audioSource.playOnAwake = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeAndChangeScene()
    {
        fc.FadeToBlack(.5f);
        StartCoroutine(ChangeSceneAfterDelay(.5f));
    }

    private IEnumerator ChangeSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 你可以改成你自己的場景名稱
        SceneManager.LoadScene("game1");

    }

    public void playSound()
    {
        audioSource.PlayOneShot(Sound, volume);
    }
}
