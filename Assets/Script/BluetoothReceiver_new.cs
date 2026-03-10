using System.IO.Ports;
using UnityEngine;
using System.Threading;
using System.Globalization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public static class SceneNames
{
    public const string Scene1 = "Scene1";
    public const string Scene2 = "Scene2";
}

public class BluetoothReceiver_new : MonoBehaviour
{
    // retry bug
    #region Public Variables (保持不變)
    [Header("debug string")]
    public string outputText;

    [Header("藍牙設定(COM6無線，COM5有線)")]
    public string[] availablePorts = new string[] { "COM6", "COM5", "COM4" };
    public int currentPortIndex = 0;
    public string portName = "COM6";
    public int baudRate = 115200;

    [Header("速度參數")]
    public float speed = 0f;
    public float speedIncreaseRate = 0.1f;
    public float speedDecayRate = 0.05f;
    public float speedFastDecayRate = 0.5f;
    public float maxSpeed = 10f;
    public float minSpeed = 0f;

    [Header("接收狀態監控")]
    public bool isReceiving = false;
    public float receiveTimeout = 2f;

    [Header("Debug 資訊")]
    public int receivedCount = 0;
    public int parseErrorCount = 0;
    public string lastRawMessage = "";
    public float dataRate = 0f;

    [Header("自動重試設定")]
    public float autoRetryTimeout = 7f;
    public bool enableAutoRetry = true;
    public int maxRetryAttempts = 3;

    [Header("測試模式")]
    public bool enableTestMode = false;
    public float testSpeedDelta = 0f; // 測試用：手動調整速度變化量
    #endregion

    #region Private Variables
    SerialPort serialPort;
    Thread readThread;
    Thread connectThread;
    bool isRunning = false;
    bool isConnecting = false;
    string latestMessage = "";

    DataFrame lastData = null;

    bool isConnected = false;
    
    private DateTime lastReceiveTime = DateTime.MinValue;
    private readonly object timeLock = new object();
    
    private bool connectFailedFlag = false;
    private string connectFailedMessage = "";

    private bool connectionFailed = false;

    private int lastReceivedCount = 0;
    private float rateUpdateTimer = 0f;

    private int currentRetryAttempt = 0;
    private DateTime connectionStartTime;
    private bool isWaitingForData = false;

    public static BluetoothReceiver_new Instance;

    public TMP_Text connectionStatusText; // 用於顯示連線狀態的 UI Text

    string currentScene;

    #endregion

    #region State Machine
    private enum BTState
    {
        Idle,           // 初始狀態，未連線
        Connecting,     // 正在嘗試連線
        WaitingData,    // 已連線，等待資料
        Connected,      // 已連線且正在接收資料
        Disconnected,   // 斷線狀態
        TestMode        // 測試模式（不需連線）
    }

    private BTState currentState = BTState.Idle;
    private BTState previousState = BTState.Idle;

    private void TransitionToState(BTState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        OnStateExit(currentState);

        previousState = currentState;
        currentState = newState;

        // Enter new state
        OnStateEnter(currentState);

        Debug.Log($"🔄 State: {previousState} → {currentState}");
    }

    private void OnStateEnter(BTState state)
    {
        switch (state)
        {
            case BTState.Idle:
                OnEnterIdle();
                break;
            case BTState.Connecting:
                OnEnterConnecting();
                break;
            case BTState.WaitingData:
                OnEnterWaitingData();
                break;
            case BTState.Connected:
                OnEnterConnected();
                break;
            case BTState.Disconnected:
                OnEnterDisconnected();
                break;
            case BTState.TestMode:
                OnEnterTestMode();
                break;
        }
    }

    private void OnStateExit(BTState state)
    {
        switch (state)
        {
            case BTState.Connecting:
                OnExitConnecting();
                break;
            case BTState.TestMode:
                OnExitTestMode();
                break;
        }
    }

    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case BTState.Idle:
                UpdateIdle();
                break;
            case BTState.Connecting:
                UpdateConnecting();
                break;
            case BTState.WaitingData:
                UpdateWaitingData();
                break;
            case BTState.Connected:
                UpdateConnected();
                break;
            case BTState.Disconnected:
                UpdateDisconnected();
                break;
            case BTState.TestMode:
                UpdateTestMode();
                break;
        }
    }
    #endregion

    #region State: Idle
    private void OnEnterIdle()
    {
        isConnected = false;
        isConnecting = false;
        isReceiving = false;
        connectionFailed = false;
        
        if (outputText != null)
            outputText = "press start to connect to ESP32\n";
    }

    private void UpdateIdle()
    {
        // Idle 狀態等待 Connect() 被呼叫
        if (enableTestMode && previousState != BTState.TestMode)
        {
            TransitionToState(BTState.TestMode);
        }
    }
    #endregion

    #region State: Connecting
    private void OnEnterConnecting()
    {
        isConnecting = true;
        connectionStartTime = DateTime.Now;
        isWaitingForData = false;
        
        Log($"🔍 正在連線到 {portName}... (嘗試 {currentRetryAttempt + 1}/{maxRetryAttempts})");

        if(currentScene == SceneNames.Scene1) connectionStatusText.text = $"Connecting to {portName}... \n(attempt {currentRetryAttempt + 1}/{maxRetryAttempts})";

        connectThread = new Thread(ConnectInBackground);
        connectThread.Start();
    }

    private void OnExitConnecting()
    {
        isConnecting = false;
    }

    private void UpdateConnecting()
    {
        // 檢查連線失敗旗標
        if (connectFailedFlag)
        {
            lock (this)
            {
                if (connectFailedFlag)
                {
                    HandleConnectError(connectFailedMessage);
                    connectFailedFlag = false;
                    connectFailedMessage = "";
                    TransitionToState(BTState.Disconnected);
                }
            }
        }
    }
    #endregion

    #region State: WaitingData
    private void OnEnterWaitingData()
    {
        isConnected = true;
        isWaitingForData = true;
        Debug.Log($"✅ 已連線到 ESP32 ({portName})！等待資料中...");
    }

    private void UpdateWaitingData()
    {
        // 檢查是否收到資料
        bool hasReceivedData = false;
        lock (this)
        {
            hasReceivedData = !isWaitingForData;
        }

        if (hasReceivedData)
        {
            TransitionToState(BTState.Connected);
            return;
        }

        // 檢查超時
        if (enableAutoRetry)
        {
            double timeSinceConnectionStart = (DateTime.Now - connectionStartTime).TotalSeconds;
            
            if (timeSinceConnectionStart > autoRetryTimeout)
            {
                Log($"⏱️ {autoRetryTimeout} 秒內未收到資料，斷線並重試...");
                CloseConnection();
                TransitionToState(BTState.Disconnected);
                return;
            }
        }

        // 更新顯示
        if (outputText != null)
        {
            double waitTime = (DateTime.Now - connectionStartTime).TotalSeconds;
            outputText = $"🔗 Connected to {portName}\n" +
                         $"⏳ Waiting for data... ({waitTime:F1}s / {autoRetryTimeout}s)\n" +
                         $"📦 Received: {receivedCount}\n" +
                         $"❌ Parse Errors: {parseErrorCount}\n" +
                         $"📊 Rate: {dataRate:F1} Hz\n" +
                         $"📝 Last raw: {lastRawMessage}";
        }
    }
    #endregion

    #region State: Connected
    private void OnEnterConnected()
    {
        isConnected = true;
        isReceiving = true;
    }

    private void UpdateConnected()
    {
        // 計算資料接收速率
        rateUpdateTimer += Time.deltaTime;
        if (rateUpdateTimer >= 1f)
        {
            dataRate = receivedCount - lastReceivedCount;
            lastReceivedCount = receivedCount;
            rateUpdateTimer = 0f;
        }

        // 用系統時間檢查接收狀態
        double timeSinceLastReceive;
        lock (timeLock)
        {
            timeSinceLastReceive = (DateTime.Now - lastReceiveTime).TotalSeconds;
        }
        
        isReceiving = timeSinceLastReceive < receiveTimeout;

        // 如果超過接收超時，返回 WaitingData 狀態
        if (!isReceiving)
        {
            Log("⚠️ 資料接收中斷");
            TransitionToState(BTState.WaitingData);
            return;
        }

        // 處理訊息
        ProcessReceivedData();
    }
    #endregion

    #region State: Disconnected
    private void OnEnterDisconnected()
    {
        isConnected = false;
        isReceiving = false;
        
        // 觸發重試邏輯
        if (enableAutoRetry)
        {
            TryNextConnection();
        }
    }

    private void UpdateDisconnected()
    {
        // 等待重試或手動連線
    }
    #endregion

    #region State: TestMode
    private void OnEnterTestMode()
    {
        isConnected = true;  // 測試模式視為已連線
        isConnecting = false;
        isReceiving = true;  // 測試模式視為正在接收資料
        
        Log("🧪 進入測試模式 - 可手動調整 testSpeedDelta 來測試速度變化");
    }

    private void OnExitTestMode()
    {
        // 不會退出測試模式
    }

    private void UpdateTestMode()
    {
        // 模擬 Weight 變化來更新速度
        UpdateSpeed(testSpeedDelta);

        // 更新顯示
        if (outputText != null)
        {
            outputText = $"🧪 TEST MODE\n\n" +
                         $"⚡ Speed: {speed:F2}\n" +
                         $"📊 Test Delta: {testSpeedDelta:F2}\n\n" +
                         $"💡 調整 testSpeedDelta 來測試速度變化";
        }
    }
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
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
        TransitionToState(BTState.Idle);
        CheckCurrentScene(SceneManager.GetActiveScene());
    }

    void Update()
    {
        UpdateStateMachine();
    }

    void OnApplicationQuit()
    {
        CleanupThreadsAndPort();
    }

    void OnDestroy()
    {
        CleanupThreadsAndPort();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene(scene);
    }

    void CheckCurrentScene(Scene scene)
    {
        Debug.Log("現在場景是: " + scene.name);

        if (scene.name == SceneNames.Scene1)
        {
            currentScene = SceneNames.Scene1;
        }
        else if (scene.name == SceneNames.Scene2)
        {
            currentScene = SceneNames.Scene2;
        }
    }
    #endregion

    #region Public API (保持不變)
    public void Connect()
    {
        // 如果在測試模式，不允許連線
        if (currentState == BTState.TestMode)
        {
            Log("⚠️ 目前在測試模式，無法連線。");
            return;
        }

        if (currentState == BTState.Connecting || currentState == BTState.Connected || currentState == BTState.WaitingData)
        {
            Log("⚠️ 已經連線中或正在連線，請勿重複連線。");
            return;
        }

        currentPortIndex = 0;
        currentRetryAttempt = 0;
        portName = availablePorts[currentPortIndex];
        
        TransitionToState(BTState.Connecting);
    }

    public bool isBTconnectionFailed()
    {
        return connectionFailed;
    }

    public bool isBTConnected()
    {
        return isConnected;
    }

    public bool isDataReceiving()
    {
        return isReceiving;
    }

    public bool isConnectingNow()
    {
        return isConnecting;
    }
    #endregion

    #region Connection Logic
    void ConnectInBackground()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.ReadTimeout = 500;
            serialPort.NewLine = "\n";
            serialPort.DtrEnable = true;
            serialPort.RtsEnable = true;
            serialPort.Open();

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            isRunning = true;
            readThread = new Thread(ReadSerial);
            readThread.Start();

            lock (this)
            {
                connectionFailed = false;
            }

            // 在主執行緒中切換到 WaitingData 狀態
            // 這裡不直接切換，而是設定旗標讓 Update 處理
        }
        catch (System.UnauthorizedAccessException)
        {
            HandleConnectErrorInThread($"❌ 無法開啟埠 {portName}：可能被其他程式佔用。");
        }
        catch (System.IO.IOException)
        {
            HandleConnectErrorInThread($"❌ 找不到埠 {portName}，請確認藍牙序列埠已配對。");
        }
        catch (System.Exception e)
        {
            HandleConnectErrorInThread($"❌ 開啟序列埠失敗：{e.Message}");
        }
    }

    void HandleConnectErrorInThread(string msg)
    {
        Debug.LogWarning(msg);
        lock (this)
        {
            connectFailedFlag = true;
            connectFailedMessage = msg;
        }
    }

    void HandleConnectError(string msg)
    {
        Log(msg);
    }

    void TryNextConnection()
    {
        currentRetryAttempt++;
        
        if (currentRetryAttempt < maxRetryAttempts)
        {
            Log($"🔄 {autoRetryTimeout} 秒後重試 {portName}...");
            Invoke(nameof(RetryConnection), 1f);
        }
        else
        {
            currentPortIndex++;
            currentRetryAttempt = 0;
            
            if (currentPortIndex < availablePorts.Length)
            {
                portName = availablePorts[currentPortIndex];
                Log($"🔄 切換到 {portName}...");
                Invoke(nameof(RetryConnection), 1f);
            }
            else
            {
                Log("❌ 所有 COM 埠都無法連線，請檢查藍芽配對。");
                connectionFailed = true;
                TransitionToState(BTState.Idle);
            }
        }
    }

    void RetryConnection()
    {
        TransitionToState(BTState.Connecting);
    }

    void ReadSerial()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string message = serialPort.ReadLine();
                
                lock (this)
                {
                    latestMessage = message.Trim();
                    lastRawMessage = latestMessage;
                    receivedCount++;
                    isWaitingForData = false;
                }

                lock (timeLock)
                {
                    lastReceiveTime = DateTime.Now;
                }

                // 第一次收到資料，切換到 WaitingData -> Connected
                if (currentState == BTState.Connecting && latestMessage.Contains("ConnectionSuccess"))
                {
                    TransitionToState(BTState.WaitingData);
                }

                //TODO: 太久收不到資料要換port或是宣布失敗
            }
            catch (System.TimeoutException)
            {
                // 讀不到資料時靜默即可
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ 讀取序列埠錯誤：{e.Message}");
                break;
            }
        }
        
        Debug.Log("🔌 ReadSerial thread ended.");
    }

    void CloseConnection()
    {
        isRunning = false;

        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join(1000);
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log("🔌 已關閉序列埠。");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"關閉序列埠時發生錯誤：{e.Message}");
            }
        }
    }

    void CleanupThreadsAndPort()
    {
        isRunning = false;

        if (connectThread != null && connectThread.IsAlive)
            connectThread.Join(1000);

        if (readThread != null && readThread.IsAlive)
            readThread.Join(1000);

        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("🔌 已關閉序列埠。");
        }
    }
    #endregion

    #region Data Processing
    void ProcessReceivedData()
    {
        string msgCopy = "";
        lock (this)
        {
            if (!string.IsNullOrEmpty(latestMessage))
            {
                msgCopy = latestMessage;
                latestMessage = "";
            }
        }

        if (!string.IsNullOrEmpty(msgCopy))
        {
            DataFrame currentData = ParseData(msgCopy);
            if (currentData != null)
            {
                string displayText = "";

                if (lastData != null)
                {
                    DataFrame delta = new DataFrame
                    {
                        AngleX = currentData.AngleX - lastData.AngleX,
                        AngleY = currentData.AngleY - lastData.AngleY,
                        AngleZ = currentData.AngleZ - lastData.AngleZ,
                        Weight = currentData.Weight - lastData.Weight
                    };

                    UpdateSpeed(delta.Weight);

                    displayText =
                        $"🔗 Connected ({portName}) | 📦 Received: {receivedCount} | ❌ Errors: {parseErrorCount} | 📊 Rate: {dataRate:F1} Hz\n\n" +
                        $"AX: {currentData.AX:F2}, AY: {currentData.AY:F2}, AZ: {currentData.AZ:F2}\n" +
                        $"AngleX: {currentData.AngleX:F2} ({delta.AngleX:+0.00;-0.00})\n" +
                        $"AngleY: {currentData.AngleY:F2} ({delta.AngleY:+0.00;-0.00})\n" +
                        $"AngleZ: {currentData.AngleZ:F2} ({delta.AngleZ:+0.00;-0.00})\n" +
                        $"Weight: {currentData.Weight:F2} ({delta.Weight:+0.00;-0.00})\n\n" +
                        $"⚡ Speed: {speed:F2}\n" +
                        $"📶 Receiving: {isReceiving}";
                }
                else
                {
                    displayText =
                        $"🔗 Connected ({portName}) | 📦 Received: {receivedCount} | ❌ Errors: {parseErrorCount} | 📊 Rate: {dataRate:F1} Hz\n\n" +
                        $"📦 init data：\n" +
                        $"AX: {currentData.AX:F2}, AY: {currentData.AY:F2}, AZ: {currentData.AZ:F2}\n" +
                        $"AngleX: {currentData.AngleX:F2}\n" +
                        $"AngleY: {currentData.AngleY:F2}\n" +
                        $"AngleZ: {currentData.AngleZ:F2}\n" +
                        $"Weight: {currentData.Weight:F2}\n\n" +
                        $"⚡ Speed: {speed:F2}\n" +
                        $"📶 Receiving: {isReceiving}";
                }

                Debug.Log(displayText);
                if (outputText != null)
                    outputText = displayText;

                lastData = currentData;
            }
        }
    }

    void UpdateSpeed(float deltaWeight)
    {
        if (deltaWeight > 1f)
            speed += Mathf.Abs(deltaWeight) * speedIncreaseRate;
        else if (Mathf.Abs(deltaWeight) <= 0.4f)
            speed -= speedFastDecayRate * Time.deltaTime * 60f;
        else
            speed -= speedDecayRate * Time.deltaTime * 60f;

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    DataFrame ParseData(string line)
    {
        try
        {
            line = line.Trim();

            if (line.StartsWith("ConnectionSuccess, Weight:"))
            {
                string value = line.Split(':')[1];
                float weight = float.Parse(value, CultureInfo.InvariantCulture);

                return new DataFrame
                {
                    AX = 0,
                    AY = 0,
                    AZ = 0,
                    AngleX = 0,
                    AngleY = 0,
                    AngleZ = 0,
                    Weight = weight
                };
            }

            if (!line.Contains("|"))
            {
                Debug.LogWarning($"⚠️ 資料格式錯誤：{line}");
                parseErrorCount++;
                return null;
            }

            string[] parts = line.Split('|');
            string[] accelParts = parts[0].Trim().Split(' ');
            string[] angleParts = parts[1].Trim().Split(' ');
            string[] weightPart = parts[2].Trim().Split(' ');

            return new DataFrame
            {
                AX = float.Parse(accelParts[0].Split(':')[1], CultureInfo.InvariantCulture),
                AY = float.Parse(accelParts[1].Split(':')[1], CultureInfo.InvariantCulture),
                AZ = float.Parse(accelParts[2].Split(':')[1], CultureInfo.InvariantCulture),
                AngleX = float.Parse(angleParts[0].Split(':')[1], CultureInfo.InvariantCulture),
                AngleY = float.Parse(angleParts[1].Split(':')[1], CultureInfo.InvariantCulture),
                AngleZ = float.Parse(angleParts[2].Split(':')[1], CultureInfo.InvariantCulture),
                Weight = float.Parse(weightPart[0].Split(':')[1], CultureInfo.InvariantCulture)
            };
        }
        catch (Exception e)
        {
            Debug.LogWarning($"⚠️ 解析資料失敗：{line} 錯誤：{e.Message}");
            parseErrorCount++;
            return null;
        }
    }
    #endregion

    #region Utility
    void Log(string message)
    {
        Debug.Log(message);
        if (outputText != null)
            outputText = message;
    }
    #endregion

    #region Data Class
    class DataFrame
    {
        public float AX, AY, AZ;
        public float AngleX, AngleY, AngleZ;
        public float Weight;
    }
    #endregion
}