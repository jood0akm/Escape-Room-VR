// Middle socket: ONLY the resistor belongs here. Anything else in this socket is a wrong arrangement.
public class ResistorSocket : CircuitSocket
{
    public override CircuitManager.Slot Slot { get { return CircuitManager.Slot.Middle; } }
}
