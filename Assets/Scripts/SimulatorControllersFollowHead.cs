using System.Collections;
using UnityEngine;

// In the XR Device Simulator the controllers keep a fixed pose in rig space, so they
// stay behind when the simulated head turns. This rotates them around the head by the
// head's yaw so they turn with the player. Does nothing on a real headset.
public class SimulatorControllersFollowHead : MonoBehaviour
{
    [SerializeField] Transform head;
    [SerializeField] Transform leftController;
    [SerializeField] Transform rightController;

    bool m_Active;
    Vector3 m_LastLeft = new Vector3(float.NaN, 0f, 0f);
    Vector3 m_LastRight = new Vector3(float.NaN, 0f, 0f);

    IEnumerator Start()
    {
#pragma warning disable CS0618
        if (FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator>() == null)
            yield break;
#pragma warning restore CS0618

        // Wait a frame so this callback runs after the TrackedPoseDrivers' onBeforeRender ones.
        yield return null;
        m_Active = true;
        Application.onBeforeRender += Apply;
    }

    void OnDestroy()
    {
        if (m_Active)
            Application.onBeforeRender -= Apply;
    }

    void LateUpdate()
    {
        if (m_Active)
            Apply();
    }

    void Apply()
    {
        if (head == null)
            return;

        var yaw = Quaternion.Euler(0f, head.localEulerAngles.y, 0f);
        Rotate(leftController, yaw, ref m_LastLeft);
        Rotate(rightController, yaw, ref m_LastRight);
    }

    void Rotate(Transform controller, Quaternion yaw, ref Vector3 lastApplied)
    {
        if (controller == null)
            return;

        // Skip if the pose driver hasn't refreshed the pose since we last rotated it (avoids compounding).
        if (controller.localPosition == lastApplied)
            return;

        controller.localPosition = head.localPosition + yaw * (controller.localPosition - head.localPosition);
        controller.localRotation = yaw * controller.localRotation;
        lastApplied = controller.localPosition;
    }
}
