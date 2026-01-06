using UnityEngine;

public class TempSpeedBooster : MonoBehaviour
{
    public BluetoothReceiver_new bluetoothReceiver;

    
    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;

    }
    void Update()
    {
        // 按「往上鍵」速度 +5
        if (Input.GetKeyDown(KeyCode.P))
        {
            bluetoothReceiver.testSpeedDelta += 20f;
            Debug.Log("Speed Up: " + bluetoothReceiver.speed);
        }
        else
        {
            if(bluetoothReceiver.testSpeedDelta > 0)
            {
                bluetoothReceiver.testSpeedDelta -= .5f;
            }else if(bluetoothReceiver.testSpeedDelta < 0)
            {
                bluetoothReceiver.testSpeedDelta += .5f;
            }
        }
    }
}
