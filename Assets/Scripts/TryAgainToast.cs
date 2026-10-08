using UnityEngine;

// "WRONG ARRANGEMENT - TRY AGAIN" banner. Appears low in the player's view, follows the gaze, hides itself.
public class TryAgainToast : MonoBehaviour
{
    public GameObject root;           // world-space canvas root, inactive until shown
    public float distance = 1.1f;
    public float dropBelowEyeLevel = 0.3f;
    public float seconds = 2.8f;
    public float followSpeed = 8f;

    float hideAt;
    bool visible;

    public void Show()
    {
        var cam = Camera.main;
        if (cam == null || root == null) return;
        visible = true;
        hideAt = Time.unscaledTime + seconds;
        Place(cam, 1f);
        var canvas = root.GetComponentInChildren<Canvas>(true);
        if (canvas != null) canvas.worldCamera = cam;
        root.SetActive(true);
    }

    public void Hide()
    {
        visible = false;
        if (root != null) root.SetActive(false);
    }

    void LateUpdate()
    {
        if (!visible) return;
        if (Time.unscaledTime >= hideAt) { Hide(); return; }
        var cam = Camera.main;
        if (cam != null) Place(cam, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
    }

    void Place(Camera cam, float t)
    {
        var target = cam.transform.position + cam.transform.forward * distance - cam.transform.up * dropBelowEyeLevel;
        var tr = root.transform;
        tr.position = Vector3.Lerp(tr.position, target, t);
        var look = tr.position - cam.transform.position;
        if (look.sqrMagnitude > 0.0001f)
            tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(look, Vector3.up), t);
    }
}
