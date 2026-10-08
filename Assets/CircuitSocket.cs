using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Base for the three table sockets. Tracks which circuit part is resting in the socket and reports it to the
// CircuitManager. A part only counts once the player has let go of it (it is not held by a hand), so carrying
// a part across the other sockets never triggers a "wrong" message.
public abstract class CircuitSocket : MonoBehaviour
{
    public CircuitManager circuitManager;
    public abstract CircuitManager.Slot Slot { get; }

    readonly Dictionary<Collider, CircuitManager.Part> inside = new Dictionary<Collider, CircuitManager.Part>();
    readonly List<Collider> stale = new List<Collider>();
    CircuitManager.Part reported = CircuitManager.Part.None;
    float nextCheck;

    static CircuitManager.Part PartOf(Collider c)
    {
        switch (c.tag)
        {
            case "Battery": return CircuitManager.Part.Battery;
            case "Resistor": return CircuitManager.Part.Resistor;
            case "LED": return CircuitManager.Part.LED;
            default: return CircuitManager.Part.None;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<CircuitSocket>() != null) return; // ignore the sockets themselves (LEDSocket is tagged "LED")
        var part = PartOf(other);
        if (part != CircuitManager.Part.None) inside[other] = part;
    }

    void OnTriggerExit(Collider other)
    {
        inside.Remove(other);
    }

    void OnDisable()
    {
        inside.Clear();
        Report(CircuitManager.Part.None);
    }

    void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + 0.1f;

        var now = CircuitManager.Part.None;
        stale.Clear();
        foreach (var kv in inside)
        {
            if (kv.Key == null || !kv.Key.enabled) { stale.Add(kv.Key); continue; }
            if (HeldByHand(kv.Key)) continue;
            now = kv.Value;
            break;
        }
        foreach (var s in stale) inside.Remove(s);
        Report(now);
    }

    void Report(CircuitManager.Part now)
    {
        if (now == reported) return;
        reported = now;
        if (circuitManager != null) circuitManager.SetSlot(Slot, now);
    }

    // True while a controller/hand is holding the part (a socket holding it doesn't count).
    static bool HeldByHand(Collider c)
    {
        var grab = c.GetComponentInParent<XRGrabInteractable>();
        if (grab == null || !grab.isSelected) return false;
        foreach (var i in grab.interactorsSelecting)
            if (!(i is XRSocketInteractor)) return true;
        return false;
    }
}
