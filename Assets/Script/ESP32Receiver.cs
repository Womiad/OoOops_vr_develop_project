using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ESP32Receiver : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    byte[] buffer = new byte[1024];

    void Start()
    {
        // ⚠️ Unity 所在電腦要先連上 ESP32_AP Wi-Fi
        try
        {
            client = new TcpClient("192.168.4.1", 8080);
            stream = client.GetStream();
            Debug.Log("Connected to ESP32!");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
        }
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Debug.Log("ESP32 says: " + message.Trim());
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}
