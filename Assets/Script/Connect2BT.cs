using UnityEngine;

public class Connect2BT : MonoBehaviour
{
    
    public BluetoothReceiver_new bluetoothReceiver;

    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && (bluetoothReceiver.currentState == 0))
        {
            bluetoothReceiver.Connect();
        }
    }
}
