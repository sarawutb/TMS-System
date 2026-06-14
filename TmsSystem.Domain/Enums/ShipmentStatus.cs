namespace TmsSystem.Domain.Enums;

public static class ShipmentStatuses
{
    public const string Draft = "Draft";
    public const string Planned = "Planned";
    public const string Optimized = "Optimized";
    public const string Tendered = "Tendered";
    public const string Accepted = "Accepted";
    public const string Dispatched = "Dispatched";
    public const string AtPickup = "At Pickup";
    public const string Loaded = "Loaded";
    public const string InTransit = "In Transit";
    public const string Exception = "Exception";
    public const string AtDelivery = "At Delivery";
    public const string Delivered = "Delivered";
    public const string PodSubmitted = "POD Submitted";
    public const string FreightAudited = "Freight Audited";
    public const string Invoiced = "Invoiced";
    public const string Closed = "Closed";
    public const string Cancelled = "Cancelled";
}
