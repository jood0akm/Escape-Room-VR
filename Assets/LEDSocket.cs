// Right socket of the table (as the player sees it). Battery or LED may sit here; the resistor may not.
public class LEDSocket : CircuitSocket
{
    public override CircuitManager.Slot Slot { get { return CircuitManager.Slot.Right; } }
}
