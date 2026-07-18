namespace POS.Domain.Modules.Orders.Enums;

/// <summary>Distinguishes a staff-assisted in-store sale from a customer self-checkout online.</summary>
public enum OrderChannel
{
    InStore,
    Online
}
