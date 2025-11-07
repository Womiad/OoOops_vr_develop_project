using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using UnityEngine.UI;
using TMPro;

public class ArduinoPort : MonoBehaviour
{
    string portName_1 = "COM4"; 
    int setBaudRate = 115200;
    Parity parity = Parity.None;
    int dataBits = 8;
    StopBits stopBits = StopBits.One;

    SerialPort serialPort = null;
    public int isClosePort = 1;

    public TextMeshProUGUI showStatusTxt;
    public TextMeshProUGUI valueText;


    // open the port 
    void Start() 
    {
        OpenPort();
    }

    void Update() 
    {
        ReadData();
    }


    public void OpenPort() 
    {
        serialPort = new SerialPort(portName_1, setBaudRate, parity, dataBits, stopBits);
        
        // check whether port is open 
        try{
            serialPort.Open();
            showStatusTxt.text = "Open Port Success...";
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void ClosePort() 
    {
        try{
            serialPort.Close();
            showStatusTxt.text = "Close Port Success...";
        }catch(Exception ex) 
        {
            Debug.Log(ex.Message);
        }
    }

    public void ReadData() 
    {
        if(serialPort.IsOpen)
        {
            string a = serialPort.ReadExisting();
            if(a != "" )valueText.text = a;
            Thread.Sleep(100);              // delay 0.5s to read the data
            
        }
    }


}