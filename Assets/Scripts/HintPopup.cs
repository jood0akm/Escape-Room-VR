using UnityEngine;

// HINT button in the room -> popup with the circuit diagram that follows the player's gaze.
public class HintPopup : MonoBehaviour
{
    public GameObject popupRoot;      // world-space canvas root, inactive until opened
    public float distance = 1.2f;
    public float followSpeed = 6f;

    bool open;

    public void Open()
    {
        var cam = Camera.main;
        if (cam == null) return;
        open = true;
        Place(cam, 1f);
        var canvas = popupRoot.GetComponentInChildren<Canvas>(true);
        if (canvas != null) canvas.worldCamera = cam;
        popupRoot.SetActive(true);
        if (GameAudio.Instance != null) GameAudio.Instance.PlayHint();
    }

    public void Close()
    {
        open = false;
        popupRoot.SetActive(false);
    }

    void LateUpdate()
    {
        if (!open) return;
        var cam = Camera.main;
        if (cam != null) Place(cam, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
    }

    void Place(Camera cam, float t)
    {
        var target = cam.transform.position + cam.transform.forward * distance;
        var tr = popupRoot.transform;
        tr.position = Vector3.Lerp(tr.position, target, t);
        var look = tr.position - cam.transform.position;
        if (look.sqrMagnitude > 0.0001f)
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(look, Vector3.up), t);
    }
}
