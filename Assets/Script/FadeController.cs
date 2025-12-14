using UnityEngine;

public class FadeController : MonoBehaviour
{
    public OVRScreenFade screenFade;

    void Start()
    {
       
    }

    public void FadeToBlack(float duration = 1f)
    {
        screenFade.fadeTime = duration;
        screenFade.FadeOut();
    }

    public void FadeFromBlack(float duration = 1f)
    {
        screenFade.fadeTime = duration;
        screenFade.FadeIn();
    }
}
