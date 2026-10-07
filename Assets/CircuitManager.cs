using UnityEngine;
using System.Collections;

public class CircuitManager : MonoBehaviour
{
    public bool batteryConnected;
    public bool resistorConnected;
    public bool ledConnected;
    public bool doorOpened; // set when the door finished sliding open

    public Renderer ledRenderer;

    public Material ledOffMaterial;
    public Material ledOnMaterial;

    public AudioSource audioSource; // صوت الكهرباء
    public AudioSource doorAudio;   // صوت الباب

    public Transform door;

    private bool activated = false;

    void Start()
    {
        ledRenderer.material = ledOffMaterial;
    }

    void Update()
    {
        if (batteryConnected &&
            resistorConnected &&
            ledConnected &&
            !activated)
        {
            activated = true;
            StartCoroutine(ActivateCircuit());
        }
    }

    IEnumerator ActivateCircuit()
    {
        yield return new WaitForSeconds(2f);

        ledRenderer.material = ledOnMaterial;

        if (audioSource != null)
            audioSource.Play();

        if (doorAudio != null)
            doorAudio.Play();

        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        Vector3 startPos = door.position;
        Vector3 targetPos = startPos + new Vector3(1.5f, 0f, 0f);

        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            door.position = Vector3.Lerp(
                startPos,
                targetPos,
                elapsed / duration
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        door.position = targetPos;
        doorOpened = true;
    }
}