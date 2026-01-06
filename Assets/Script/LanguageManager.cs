using UnityEngine;
using TMPro;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;
    // 有bug 還得改

    public bool english = true;
    public bool setEnglish = false;
    public TMP_Text LetsGoBtn;
    public TMP_Text NotyetBtn;

    private const string LANGUAGE_KEY = "English"; // 1 = English, 0 = Chinese

    void Awake()
    {
        // Singleton（避免重複）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if(setEnglish) SetEnglish(true);
        LoadLanguage();
        if (IsEnglish())
        {
            LetsGoBtn.text = "Let's go!";
            NotyetBtn.text = "I want to \npractice more...";
        }
    }

    public void SetEnglish(bool isEnglish)
    {
        english = isEnglish;
        PlayerPrefs.SetInt(LANGUAGE_KEY, isEnglish ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadLanguage()
    {
        english = PlayerPrefs.GetInt(LANGUAGE_KEY, 0) == 1;
    }

    public bool IsEnglish()
    {
        return english;
    }
}
