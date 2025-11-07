using System.IO.Ports;
using UnityEngine;
using System.Threading;
using System.Globalization;
using TMPro;

public class BluetoothReceiver : MonoBehaviour
{
    [Header("TextMeshPro 顯示物件")]
    public TMP_Text outputText;

    SerialPort serialPort;
    Thread readThread;
    bool isRunning = false;
    string latestMessage = "";

    DataFrame lastData = null;

    public float speed = 0f;
    public float speedIncreaseRate = 0.1f;   // 當 weight 上升時速度增加比例
    public float speedDecayRate = 0.05f;     // 一般減速速率
    public float speedFastDecayRate = 0.5f;  // ✅ 微小變化時超快減速速率
    public float maxSpeed = 10f;
    public float minSpeed = 0f;

    void Start()
    {
        serialPort = new SerialPort("COM6", 115200);
        serialPort.ReadTimeout = 100;

        try
        {
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerial);
            readThread.Start();
            Debug.Log("✅ 已連線到 ESP32 藍牙序列埠！");
            if (outputText != null)
                outputText.text = "✅ 已連線到 ESP32 藍牙序列埠！\n";
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ 開啟序列埠失敗：" + e.Message);
            if (outputText != null)
                outputText.text = "❌ 開啟序列埠失敗：" + e.Message;
        }
    }

    void ReadSerial()
    {
        while (isRunning && serialPort.IsOpen)
        {
            try
            {
                string message = serialPort.ReadLine();
                lock (this)
                {
                    latestMessage = message.Trim();
                }
            }
            catch (System.TimeoutException) { }
        }
    }

    void Update()
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

                    // ✅ 更新 Speed
                    UpdateSpeed(delta.Weight);

                    displayText =
                        $"AX: {currentData.AX:F2}, AY: {currentData.AY:F2}, AZ: {currentData.AZ:F2}\n" +
                        $"AngleX: {currentData.AngleX:F2} ({delta.AngleX:+0.00;-0.00})\n" +
                        $"AngleY: {currentData.AngleY:F2} ({delta.AngleY:+0.00;-0.00})\n" +
                        $"AngleZ: {currentData.AngleZ:F2} ({delta.AngleZ:+0.00;-0.00})\n" +
                        $"Weight: {currentData.Weight:F2} ({delta.Weight:+0.00;-0.00})\n\n" +
                        $"⚡ Speed: {speed:F2}";
                }
                else
                {
                    displayText =
                        $"📦 初始資料：\n" +
                        $"AX: {currentData.AX:F2}, AY: {currentData.AY:F2}, AZ: {currentData.AZ:F2}\n" +
                        $"AngleX: {currentData.AngleX:F2}\n" +
                        $"AngleY: {currentData.AngleY:F2}\n" +
                        $"AngleZ: {currentData.AngleZ:F2}\n" +
                        $"Weight: {currentData.Weight:F2}\n\n" +
                        $"⚡ Speed: {speed:F2}";
                }

                Debug.Log(displayText);
                if (outputText != null)
                    outputText.text = displayText;

                lastData = currentData;
            }
        }
    }

    void UpdateSpeed(float deltaWeight)
    {
        if (deltaWeight > 1.5f)
        {
            // ✅ 加速：Weight 上升明顯
            speed += deltaWeight * speedIncreaseRate;
        }
        else if (deltaWeight <= 0.1f)
        {
            // ✅ 幾乎沒變 → 超快減速
            speed -= speedFastDecayRate * Time.deltaTime * 60f;
        }
        else
        {
            // ✅ 一般情況（微小下降或變化不大）→ 慢慢減速
            speed -= speedDecayRate * Time.deltaTime * 60f;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
    }

    DataFrame ParseData(string line)
    {
        try
        {
            string[] parts = line.Split('|');
            string[] accelParts = parts[0].Trim().Split(' ');
            string[] angleParts = parts[1].Trim().Split(' ');
            string[] weightPart = parts[2].Trim().Split(' ');

            DataFrame data = new DataFrame
            {
                AX = float.Parse(accelParts[0].Split(':')[1], CultureInfo.InvariantCulture),
                AY = float.Parse(accelParts[1].Split(':')[1], CultureInfo.InvariantCulture),
                AZ = float.Parse(accelParts[2].Split(':')[1], CultureInfo.InvariantCulture),
                AngleX = float.Parse(angleParts[0].Split(':')[1], CultureInfo.InvariantCulture),
                AngleY = float.Parse(angleParts[1].Split(':')[1], CultureInfo.InvariantCulture),
                AngleZ = float.Parse(angleParts[2].Split(':')[1], CultureInfo.InvariantCulture),
                Weight = float.Parse(weightPart[0].Split(':')[1], CultureInfo.InvariantCulture)
            };
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("⚠️ 解析資料失敗：" + line + " | 錯誤：" + e.Message);
            return null;
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
            readThread.Join();

        if (serialPort != null && serialPort.IsOpen)
            serialPort.Close();
    }

    class DataFrame
    {
        public float AX, AY, AZ;
        public float AngleX, AngleY, AngleZ;
        public float Weight;
    }
}
