using UnityEngine;
using System.Collections;

public class CircuitManager : MonoBehaviour
{
    // What sits in each of the three sockets (left -> right as the player sees the table).
    public enum Part { None, Battery, Resistor, LED }
    public enum Slot { Left, Middle, Right }

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

    [Header("Wrong arrangement feedback")]
    public TryAgainToast tryAgainToast;
    public float wrongCooldown = 1.5f;

    private bool activated = false;
    private Part left, middle, right;
    private bool wasWrong;
    private float lastWrongTime = -99f;

    void Start()
    {
        ledRenderer.material = ledOffMaterial;
    }

    // Called by the sockets whenever the part resting in them changes.
    public void SetSlot(Slot slot, Part part)
    {
        if (activated) return;
        switch (slot)
        {
            case Slot.Left: left = part; break;
            case Slot.Middle: middle = part; break;
            case Slot.Right: right = part; break;
        }
        Evaluate();
    }

    // Rule: the RESISTOR must be in the middle. Battery and LED go on the two sides (either side).
    void Evaluate()
    {
        resistorConnected = middle == Part.Resistor;
        batteryConnected = left == Part.Battery || right == Part.Battery;
        ledConnected = left == Part.LED || right == Part.LED;

        // "Try again" only once all three sockets are filled and the arrangement is not the right one.
        bool allFilled = left != Part.None && middle != Part.None && right != Part.None;
        bool wrong = allFilled && !(resistorConnected && batteryConnected && ledConnected);

        if (wrong && !wasWrong && Time.time - lastWrongTime >= wrongCooldown)
        {
            lastWrongTime = Time.time;
            if (GameAudio.Instance != null) GameAudio.Instance.PlayWrong();
            if (tryAgainToast != null) tryAgainToast.Show();
        }
        wasWrong = wrong;
    }

    void Update()
    {
        if (batteryConnected &&
            resistorConnected &&
            ledConnected &&
            !activated)
        {
            activated = true;
            if (tryAgainToast != null) tryAgainToast.Hide();
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
