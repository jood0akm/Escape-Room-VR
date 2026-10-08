// Left socket of the table (as the player sees it). Battery or LED may sit here; the resistor may not.
public class BatterySocket : CircuitSocket
{
    public override CircuitManager.Slot Slot { get { return CircuitManager.Slot.Left; } }
}
