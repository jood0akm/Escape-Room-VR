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
            Place(cam, 1f);
            var canvas = screenRoot.GetComponentInChildren<Canvas>(true);
            if (canvas != null) canvas.worldCamera = cam;
        }
        screenRoot.SetActive(true);

        // reaching the door: success sting, stop ticking/ambience, close any open hint
        if (GameAudio.Instance != null) GameAudio.Instance.PlayEscaped();
        var hint = Object.FindAnyObjectByType<HintPopup>();
        if (hint != null) hint.Close();
    }

    [Tooltip("How quickly the screen catches up with where the player looks (higher = snappier).")]
    public float followSpeed = 6f;

    // Keep the screen in front of the player's gaze (up/down and sideways), smoothed so it doesn't jitter.
    void LateUpdate()
    {
        if (!shown) return;
        var cam = Camera.main;
        if (cam == null) return;
        Place(cam, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
    }

    void Place(Camera cam, float t)
    {
        var target = cam.transform.position + cam.transform.forward * distance;
        var tr = screenRoot.transform;
        tr.position = Vector3.Lerp(tr.position, target, t);
        var look = tr.position - cam.transform.position;
        if (look.sqrMagnitude > 0.0001f)
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(look, Vector3.up), t);
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
