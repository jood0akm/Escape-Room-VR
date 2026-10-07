using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Lives on "doorbg". Once the circuit is solved and the door is open, grabbing doorbg
// shows the "YOU ESCAPED THIS ROOM" screen in front of the player (REPLAY / EXIT).
[RequireComponent(typeof(XRSimpleInteractable))]
public class EscapeScreen : MonoBehaviour
{
    public CircuitManager circuit;
    public GameObject screenRoot;     // MainMenu-style world-space canvas, inactive until shown
    public float distance = 1.0f;

    XRSimpleInteractable interactable;
    bool shown;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrabbed);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrabbed);
    }

    // Walking up to the open doorway also counts (handy with the XR Device Simulator).
    [Tooltip("Show the screen when the player's head gets this close to the open doorway.")]
    public float walkUpDistance = 0.9f;

    void Update()
    {
        if (shown || circuit == null || !circuit.doorOpened) return;
        var cam = Camera.main;
        var col = GetComponent<Collider>();
        if (cam == null || col == null) return;
        if (col.bounds.SqrDistance(cam.transform.position) <= walkUpDistance * walkUpDistance) Show();
    }

    void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (shown || circuit == null || !circuit.doorOpened) return;
        Show();
    }

    public void Show()
    {
        shown = true;
        var cam = Camera.main;
        if (cam != null)
        {
            var fwd = cam.transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
            fwd.Normalize();
            screenRoot.transform.position = cam.transform.position + fwd * distance;
            screenRoot.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            var canvas = screenRoot.GetComponentInChildren<Canvas>(true);
            if (canvas != null) canvas.worldCamera = cam;
        }
        screenRoot.SetActive(true);
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
