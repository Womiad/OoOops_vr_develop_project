using System.Collections;
using UnityEngine;
using TMPro;

public enum Scene2State
{
    Original,
    MushroomEvent
}

public class Scene2GM : MonoBehaviour
{
    public BluetoothReceiver bluetoothReceiver;
    public TMP_Text debugText;

    private bool explainTimerRunning = false;

    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver.Instance;

    }

    void Update()
    {
        debugText.text = bluetoothReceiver.outputText;
    }

}