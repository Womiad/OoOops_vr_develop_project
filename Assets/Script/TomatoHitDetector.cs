using UnityEngine;

public class TomatoHitDetector : MonoBehaviour
{
    private bool hasScored = false;
    private bool lastHasScored = false;

    [Header("Hit Sound")]
    public AudioClip SpatulaSound;
    public AudioClip PanSound;
    public AudioClip PanSound2;
    public AudioClip ForkSound;
    public float volume = 1f;

    private AudioSource audioSource;
    public Game1GM game1GM;

    public GameObject tomato;
    public GameObject PingPong;
    public GameObject potato;
    public GameObject egg;
    public GameObject onigiri;

    public GameObject Pan;
    public GameObject Fork;
    public GameObject Spatula;

    GameObject currentTool;
    GameObject currentFood;

    private GameObject lastTool = null;

    public BossHealthSystem bossHealthSystem;
    

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        
        // // 有空的話想把換工具放到正確的地方
        // if(Pan.activeSelf) lastTool = Pan;
        // else if(Fork.activeSelf) lastTool = Fork;
        // else if(Spatula.activeSelf) lastTool = Spatula;
        // // 初始先選一個工具
        // if (game1GM.game1State == Game1State.State2) ChooseNewTool(null);
    }

    void Update()
    {
        if (game1GM.game1State == Game1State.State1)
        {
            PingPong.SetActive(!hasScored);
            egg.SetActive(hasScored);
        }
        else if (game1GM.game1State == Game1State.State2)
        {
            // 只在 hasScored 改變時處理
            if (hasScored != lastHasScored)
            {

                // 換食物
                ChooseFood();

                lastHasScored = hasScored;
            }
        }
    }

    // void ChooseNewTool(GameObject exclude)
    // {
    //     GameObject[] tools = { Pan, Fork, Spatula };

    //     foreach (var t in tools)
    //         t.SetActive(false);

    //     GameObject chosen;
    //     do
    //     {
    //         chosen = tools[Random.Range(0, tools.Length)];
    //     }
    //     while (chosen == exclude);

    //     if (game1GM.game1State == Game1State.State2) chosen.SetActive(true);
    //     currentTool = chosen;
    // }

    void ChooseFood()
    {
        // 關掉全部
        PingPong.SetActive(false);
        tomato.SetActive(false);
        potato.SetActive(false);
        egg.SetActive(false);
        onigiri.SetActive(false);

        if (!hasScored)
        {
            PingPong.SetActive(true);
            currentFood = PingPong;
        }
        else
        {
            GameObject[] foods = { tomato, potato, egg, onigiri };
            currentFood = foods[Random.Range(0, foods.Length)];
            currentFood.SetActive(true);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Pan") || collision.collider.CompareTag("Fork") || collision.collider.CompareTag("Spatula"))
        {
            if (hasScored) return;
            bossHealthSystem.TakeDamage(300f); // 每次碰撞造成 300 點傷害

            hasScored = true;

            if (collision.collider.CompareTag("Spatula") && SpatulaSound != null)
            {
                audioSource.PlayOneShot(SpatulaSound, volume);
            }
            else if (collision.collider.CompareTag("Fork") && ForkSound != null)
            {
                audioSource.PlayOneShot(ForkSound, volume);
            }
            else if (collision.collider.CompareTag("Pan"))
            {
                // ⭐ Pan 音效隨機二選一
                AudioClip panClip = Random.value < 0.5f ? PanSound : PanSound2;

                if (panClip != null)
                    audioSource.PlayOneShot(panClip, volume);
            }


            game1GM.addOneScorePoint();
        }
    }
}
