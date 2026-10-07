using UnityEngine;

public class BatterySocket : MonoBehaviour
{
    public CircuitManager circuitManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Battery"))
        {
            Debug.Log("Battery Connected");
            circuitManager.batteryConnected = true;
        }
    }
}