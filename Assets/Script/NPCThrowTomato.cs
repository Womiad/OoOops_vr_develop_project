using UnityEngine;

public class NPCThrowTomato : MonoBehaviour
{
    [Header("Tomato Settings")]
    public GameObject tomatoPrefab;
    public Transform throwPoint; // 預設丟出位置
    public float throwForce = 10f;

    [Header("Target")]
    public Transform playerHead;

    [Header("Throw Randomness")]
    public float horizontalRandom = 0.2f;
    public float randomSpinForce = 8f;
    public float verticalOffset = 6f;

    public Game1GM game1GM;

    public GameObject Pan;
    public GameObject Pan_left;
    public GameObject Fork;
    public GameObject Fork_left;
    public GameObject Spatula;
    public GameObject Spatula_left;

    private GameObject currentTool = null;
    private GameObject currentTool_left = null;

    // ⭐ 動畫事件會呼叫這個（可傳入自訂的丟點）// 其實換工具的邏輯可以改到這邊來
    public void ThrowTomatoByAnimationEvent(Transform customThrowPoint)
    {
        ThrowTomato(customThrowPoint);
    }

    // ⭐ 若動畫事件不想傳參數，可以呼叫這個
    public void ThrowTomatoByAnimationEvent_NoParam()
    {
        ThrowTomato(null); // 使用預設 throwPoint
    }

    // -----------------------------------------
    //     真正丟番茄的程式
    // -----------------------------------------
    void ThrowTomato(Transform customPoint)
    {
        Transform point = customPoint != null ? customPoint : throwPoint;

        if (tomatoPrefab == null || point == null || playerHead == null)
        {
            Debug.LogWarning("NPCThrowTomato：缺少設定！");
            return;
        }

        // 生成番茄
        GameObject tomato = Instantiate(tomatoPrefab, point.position, Quaternion.identity);

        Rigidbody rb = tomato.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("番茄 prefab 缺少 Rigidbody！");
            return;
        }

        // 計算方向
        Vector3 direction = (playerHead.position - point.position);

        // 計算水平右向量 (XZ 平面)
        Vector3 horizontalRight = Vector3.Cross(Vector3.up, direction).normalized;

        // 左右偏移
        float randomOffset = Random.Range(-horizontalRandom, horizontalRandom);
        direction += horizontalRight * randomOffset;

        // 補高 0.6
        direction.y += verticalOffset;

        // 正規化
        direction = direction.normalized;

        // 施加速度
        rb.linearVelocity = direction * throwForce;

        // 加上旋轉
        rb.angularVelocity = Random.insideUnitSphere * randomSpinForce;
    }

    void AniCallChooseNewTool()
    {
        ChooseNewTool(currentTool, currentTool_left);
    }


    void ChooseNewTool(GameObject exclude = null, GameObject exclude_left = null)
    {
        if(game1GM.game1State != Game1State.State2) return;
        GameObject[] tools = { Pan, Fork, Spatula };
        GameObject[] tools_left = { Pan_left, Fork_left, Spatula_left };

        foreach (var t in tools)
            t.SetActive(false);
        foreach (var tl in tools_left)
            tl.SetActive(false);

        GameObject chosen;
        GameObject chosen_left;
        do
        {
            chosen = tools[Random.Range(0, tools.Length)];
        }
        while (chosen == exclude);
        do
        {
            chosen_left = tools_left[Random.Range(0, tools_left.Length)];
        }
        while (chosen_left == exclude_left);

        if (game1GM.game1State == Game1State.State2) chosen.SetActive(true);
        if (game1GM.game1State == Game1State.State2) chosen_left.SetActive(true);
        currentTool = chosen;
        currentTool_left = chosen_left;
    }
}
