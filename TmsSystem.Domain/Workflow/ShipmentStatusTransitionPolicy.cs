using TmsSystem.Domain.Enums;

namespace TmsSystem.Domain.Workflow;

public static class ShipmentStatusTransitionPolicy
{
    private static readonly HashSet<string> KnownStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        ShipmentStatuses.Draft,
        ShipmentStatuses.Planned,
        ShipmentStatuses.Optimized,
        ShipmentStatuses.Tendered,
        ShipmentStatuses.Accepted,
        ShipmentStatuses.Dispatched,
        ShipmentStatuses.AtPickup,
        ShipmentStatuses.Loaded,
        ShipmentStatuses.InTransit,
        ShipmentStatuses.Exception,
        ShipmentStatuses.AtDelivery,
        ShipmentStatuses.Delivered,
        ShipmentStatuses.PodSubmitted,
        ShipmentStatuses.FreightAudited,
        ShipmentStatuses.Invoiced,
        ShipmentStatuses.Closed,
        ShipmentStatuses.Cancelled
    };

    private static readonly Dictionary<string, string[]> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        [ShipmentStatuses.Draft] = [ShipmentStatuses.Planned, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.Planned] = [ShipmentStatuses.Optimized, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.Optimized] = [ShipmentStatuses.Tendered, ShipmentStatuses.Accepted, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.Tendered] = [ShipmentStatuses.Accepted, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.Accepted] = [ShipmentStatuses.Dispatched, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.Dispatched] = [ShipmentStatuses.AtPickup, ShipmentStatuses.Exception, ShipmentStatuses.Cancelled],
        [ShipmentStatuses.AtPickup] = [ShipmentStatuses.Loaded, ShipmentStatuses.Exception],
        [ShipmentStatuses.Loaded] = [ShipmentStatuses.InTransit, ShipmentStatuses.Exception],
        [ShipmentStatuses.InTransit] = [ShipmentStatuses.AtDelivery, ShipmentStatuses.Exception],
        [ShipmentStatuses.Exception] =
        [
            ShipmentStatuses.Dispatched,
            ShipmentStatuses.AtPickup,
            ShipmentStatuses.Loaded,
            ShipmentStatuses.InTransit,
            ShipmentStatuses.AtDelivery,
            ShipmentStatuses.Cancelled
        ],
        [ShipmentStatuses.AtDelivery] = [ShipmentStatuses.Delivered, ShipmentStatuses.Exception],
        [ShipmentStatuses.Delivered] = [ShipmentStatuses.PodSubmitted],
        [ShipmentStatuses.PodSubmitted] = [ShipmentStatuses.FreightAudited],
        [ShipmentStatuses.FreightAudited] = [ShipmentStatuses.Invoiced],
        [ShipmentStatuses.Invoiced] = [ShipmentStatuses.Closed],
        [ShipmentStatuses.Closed] = [],
        [ShipmentStatuses.Cancelled] = []
    };

    public static IReadOnlyCollection<string> AllStatuses => KnownStatuses;

    public static bool IsKnownStatus(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) && KnownStatuses.Contains(status);
    }

    public static bool CanTransition(string currentStatus, string nextStatus)
    {
        if (string.Equals(currentStatus, nextStatus, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowed)
            && allowed.Contains(nextStatus, StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyCollection<string> GetAllowedNextStatuses(string currentStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var allowed)
            ? allowed
            : Array.Empty<string>();
    }
}
