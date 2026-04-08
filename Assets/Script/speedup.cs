using UnityEngine;




public class speedup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public BluetoothReceiver_new bluetoothReceiver;
    void Start()
    {
        if (bluetoothReceiver == null)
            bluetoothReceiver = BluetoothReceiver_new.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            bluetoothReceiver.OnStateEnter(BluetoothReceiver_new.BTState.Connected);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            if(bluetoothReceiver.speed <= 20 )bluetoothReceiver.speed += 10;
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if(bluetoothReceiver.speed >= 0 ){
                bluetoothReceiver.speed -= 10;
                if(bluetoothReceiver.speed < 0 ){
                    bluetoothReceiver.speed = 0;
                }
            }
        }
    }
}
