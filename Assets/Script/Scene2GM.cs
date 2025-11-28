using System.Collections;
using UnityEngine;
using TMPro;

// public enum Scene2State
// {
//     Menu = 0,
//     SearchingESP32,
//     Explain,
//     Run,
//     Failed2Connect
// }

public class Scene2GM : MonoBehaviour
{
    public BluetoothReceiver bluetoothReceiver;
    // Scene2State scene2State;

    // [Header("UI顯示參考")]
    // public GameObject menuItem;
    // public GameObject searchingItem;
    // public GameObject explainItem;
    // public GameObject runItem;
    // public GameObject failed2ConnectItem;
    public TMP_Text debugText;

    private bool explainTimerRunning = false;

    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver.Instance;

        // scene2State = Scene2State.Menu;
        // setUI();
    }

    void Update()
    {
        // // ✅ 在 SearchingESP32 狀態時持續檢查連線狀態
        // if (scene2State == Scene2State.SearchingESP32)
        // {
        //     if (bluetoothReceiver.isDataReceiving())
        //     {
        //         // ✅ 收到資料了，進入 Explain
        //         Debug.Log("✅ 收到資料，進入 Explain 階段");
        //         nextScene2State();
        //     }
        //     else if (bluetoothReceiver.isConnectingNow())
        //     {
        //         // ✅ 正在連線中，繼續顯示 searchingItem
        //         // 可以在這裡更新 UI 顯示「連線中...」
        //     }
        //     else if (bluetoothReceiver.isBTConnected() && !bluetoothReceiver.isDataReceiving())
        //     {
        //         // ✅ 已連線但還沒收到資料，繼續等待
        //         Debug.Log("⏳ 已連線，等待接收資料...");
        //     }
        //     // ✅ 如果連線失敗，BluetoothReceiver 會呼叫 setConnectFailed()
        // }
        debugText.text = bluetoothReceiver.outputText;
    }

    // public void startButtonClicked()
    // {
    //     if (scene2State == Scene2State.Menu)
    //     {
    //         // ✅ 先切換到 Searching 狀態
    //         scene2State = Scene2State.SearchingESP32;
    //         setUI();
            
    //         // ✅ 然後才開始連線
    //         bluetoothReceiver.Connect();
    //     }
    // }

    // public void retryButtonClicked()
    // {
    //     if (scene2State == Scene2State.Failed2Connect)
    //     {
    //         // ✅ 先切換狀態
    //         scene2State = Scene2State.SearchingESP32;
    //         setUI();
            
    //         // ✅ 再連線
    //         bluetoothReceiver.Connect();
    //     }
    // }

    // public void setConnectFailed()
    // {
    //     // ✅ 只有在 Searching 狀態才能設為失敗
    //     if (scene2State == Scene2State.SearchingESP32)
    //     {
    //         scene2State = Scene2State.Failed2Connect;
    //         setUI();
    //         Debug.Log("❌ 連線失敗，切換到 Failed2Connect 狀態");
    //     }
    // }

    // public void nextScene2State()
    // {
    //     if (scene2State == Scene2State.Menu)
    //     {
    //         scene2State = Scene2State.SearchingESP32;
    //     }
    //     else if (scene2State == Scene2State.SearchingESP32)
    //     {
    //         scene2State = Scene2State.Explain;
    //     }
    //     else if (scene2State == Scene2State.Explain)
    //     {
    //         scene2State = Scene2State.Run;
    //     }

    //     Debug.Log("Now State: " + scene2State.ToString());
    //     setUI();

    //     // ✅ 如果剛進入 Explain 狀態，啟動 1.5 秒倒數
    //     if (scene2State == Scene2State.Explain && !explainTimerRunning)
    //     {
    //         StartCoroutine(ExplainWaitAndNext());
    //     }
    // }

    // IEnumerator ExplainWaitAndNext()
    // {
    //     explainTimerRunning = true;
    //     Debug.Log("🕒 Explain 階段開始，1.5 秒後進入 Run");
    //     yield return new WaitForSeconds(1.5f);
        
    //     // ✅ 檢查狀態是否還在 Explain（防止中途被改變）
    //     if (scene2State == Scene2State.Explain)
    //     {
    //         scene2State = Scene2State.Run;
    //         Debug.Log("🚀 自動切換到 Run 狀態！");
    //         setUI();
    //     }
        
    //     explainTimerRunning = false;
    // }

    // public Scene2State getScene2State()
    // {
    //     return scene2State;
    // }

    // void setUI()
    // {
    //     menuItem.SetActive(scene2State == Scene2State.Menu);
    //     searchingItem.SetActive(scene2State == Scene2State.SearchingESP32);
    //     explainItem.SetActive(scene2State == Scene2State.Explain);
    //     runItem.SetActive(scene2State == Scene2State.Run);
    //     failed2ConnectItem.SetActive(scene2State == Scene2State.Failed2Connect);
    // }
}