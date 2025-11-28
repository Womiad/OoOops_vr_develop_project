using System.IO.Ports;
using UnityEngine;
using System.Threading;
using System.Globalization;
using TMPro;
using System;

public class BluetoothReceiver : MonoBehaviour
{
    [Header("debug string")]
    public string outputText;

    [Header("藍牙設定(COM6無線，COM5有線)")]
    public string portName = "COM6";
    public int baudRate = 115200;

    SerialPort serialPort;
    Thread readThread;
    Thread connectThread;
    bool isRunning = false;
    bool isConnecting = false;
    string latestMessage = "";

    DataFrame lastData = null;

    [Header("速度參數")]
    public float speed = 0f;
    public float speedIncreaseRate = 0.1f;
    public float speedDecayRate = 0.05f;
    public float speedFastDecayRate = 0.5f;
    public float maxSpeed = 10f;
    public float minSpeed = 0f;

    bool isConnected = false;

    [Header("接收狀態監控")]
    public bool isReceiving = false;
    public float receiveTimeout = 2f;
    
    private DateTime lastReceiveTime = DateTime.MinValue;
    private readonly object timeLock = new object();
    
    private bool connectFailedFlag = false;
    private string connectFailedMessage = "";

    private bool connectionFailed = false;

    [Header("Debug 資訊")]
    public int receivedCount = 0;
    public int parseErrorCount = 0;
    public string lastRawMessage = "";
    public float dataRate = 0f;
    
    private int lastReceivedCount = 0;
    private float rateUpdateTimer = 0f;

    public static BluetoothReceiver Instance;

    void Awake()
    {
        // --- Singleton + 保持跨場景 ---
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
        if (outputText != null)
            outputText = "press start to connect to ESP32\n";
    }

    public void Connect()
    {
        if (isConnected || isConnecting)
        {
            Log("⚠️ 已經連線中或正在連線，請勿重複連線。");
            return;
        }

        isConnecting = true;
        Log($"🔍 正在連線到 {portName}...");

        connectThread = new Thread(ConnectInBackground);
        connectThread.Start();
    }

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
                isConnected = true;
                isConnecting = false;
            }
            
        connectionFailed = false;

            Debug.Log($"✅ 已連線到 ESP32 ({portName})！");
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
            isConnected = false;
            isConnecting = false;
            connectFailedFlag = true;
            connectFailedMessage = msg;
        }
    }

    void HandleConnectError(string msg)
    {
        Log(msg);
        isConnected = false;
        isConnecting = false;
        connectionFailed = true;
        // if (scene1GM != null)
        //     scene1GM.setConnectFailed();
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
                }

                lock (timeLock)
                {
                    lastReceiveTime = DateTime.Now;
                }
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

    void Update()
    {
        // ✅ 在主執行緒處理連線失敗
        if (connectFailedFlag)
        {
            lock (this)
            {
                if (connectFailedFlag)
                {
                    HandleConnectError(connectFailedMessage);
                    connectFailedFlag = false;
                    connectFailedMessage = "";
                }
            }
        }

        if (!isConnected)
        {
            isReceiving = false;
            return;
        }

        // ✅ 計算資料接收速率
        rateUpdateTimer += Time.deltaTime;
        if (rateUpdateTimer >= 1f)
        {
            dataRate = receivedCount - lastReceivedCount;
            lastReceivedCount = receivedCount;
            rateUpdateTimer = 0f;
        }

        // ✅ 用系統時間檢查接收狀態
        double timeSinceLastReceive;
        lock (timeLock)
        {
            timeSinceLastReceive = (DateTime.Now - lastReceiveTime).TotalSeconds;
        }
        
        isReceiving = timeSinceLastReceive < receiveTimeout;

        // ✅ 處理訊息
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
                        $"🔗 Connected | 📦 Received: {receivedCount} | ❌ Errors: {parseErrorCount} | 📊 Rate: {dataRate:F1} Hz\n\n" +
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
                        $"🔗 Connected | 📦 Received: {receivedCount} | ❌ Errors: {parseErrorCount} | 📊 Rate: {dataRate:F1} Hz\n\n" +
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

        // ✅ 顯示連線但沒收到資料的狀態
        if (isConnected && !isReceiving && lastData == null)
        {
            if (outputText != null)
            {
                outputText = $"🔗 Connected to {portName}\n" +
                                  $"⏳ Waiting for data...\n" +
                                  $"📦 Received: {receivedCount}\n" +
                                  $"❌ Parse Errors: {parseErrorCount}\n" +
                                  $"📊 Rate: {dataRate:F1} Hz\n" +
                                  $"📝 Last raw: {lastRawMessage}";
            }
        }
    }

    void UpdateSpeed(float deltaWeight)
    {
        if (Mathf.Abs(deltaWeight) > 1f)
            speed += Mathf.Abs(deltaWeight) * speedIncreaseRate;
        else if (Mathf.Abs(deltaWeight) <= 0.8f)
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

            // ---------------------------
            // 📌 如果格式是：Weight:12345
            // ---------------------------
            if (line.StartsWith("Weight:"))
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

            // ---------------------------
            // 📌 原本的格式（有 AX/AY/AZ + Angle + Weight）
            // ---------------------------
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


    void Log(string message)
    {
        Debug.Log(message);
        if (outputText != null)
            outputText = message;
    }

    void OnApplicationQuit()
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

    void OnDestroy() // just for testing
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

    class DataFrame
    {
        public float AX, AY, AZ;
        public float AngleX, AngleY, AngleZ;
        public float Weight;
    }
}