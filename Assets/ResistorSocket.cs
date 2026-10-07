using UnityEngine;

public class ResistorSocket : MonoBehaviour
{
    public CircuitManager circuitManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Resistor"))
        {
            Debug.Log("Resistor Connected");
            circuitManager.resistorConnected = true;
        }
    }
}