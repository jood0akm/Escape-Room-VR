using UnityEngine;

public class LEDSocket : MonoBehaviour
{
    public CircuitManager circuitManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LED"))
        {
            Debug.Log("LED Connected");
            circuitManager.ledConnected = true;
        }
    }
}