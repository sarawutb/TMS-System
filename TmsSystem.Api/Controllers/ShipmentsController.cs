using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Application.Dtos.Shipments;
using TmsSystem.Domain.Entities;
using TmsSystem.Domain.Workflow;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/shipment")]
[Route("api/shipments")]
// ponytail: use C# 12 primary constructor and leverage the base controller's Context property to avoid redundant fields
public sealed class ShipmentsController(TmsDbContext dbContext) : CrudController<Shipment>(dbContext)
{
    protected override IQueryable<Shipment> ApplyPagedFilters(IQueryable<Shipment> query, string? search)
    {
        query = ApplySearchFilter(query, search);
        var status = Request.Query["status"].ToString();
        return string.IsNullOrWhiteSpace(status) ? query : query.Where(s => s.ShipmentStatus == status);
    }

    [HttpPost("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, UpdateShipmentStatusRequestDto request, CancellationToken cancellationToken)
    {
        var reqStatus = request.Status.Trim();
        if (!ShipmentStatusTransitionPolicy.IsKnownStatus(reqStatus))
        {
            return ApiFailure<ShipmentStatusChangeResponseDto>(StatusCodes.Status400BadRequest, "Unknown shipment status.", [$"Status must be one of: {string.Join(", ", ShipmentStatusTransitionPolicy.AllStatuses)}."]);
        }

        var shipment = await Context.Shipments.FirstOrDefaultAsync(x => x.ShipmentId == id, cancellationToken);
        if (shipment is null) return ApiFailure<ShipmentStatusChangeResponseDto>(StatusCodes.Status404NotFound, "Shipment not found.");

        var prevStatus = shipment.ShipmentStatus;
        if (!ShipmentStatusTransitionPolicy.CanTransition(prevStatus, reqStatus))
        {
            var allowed = ShipmentStatusTransitionPolicy.GetAllowedNextStatuses(prevStatus);
            return ApiFailure<ShipmentStatusChangeResponseDto>(StatusCodes.Status409Conflict, $"Invalid shipment status transition from '{prevStatus}' to '{reqStatus}'.", [allowed.Count == 0 ? "No further transitions are allowed." : $"Allowed next statuses: {string.Join(", ", allowed)}."]);
        }

        if (string.Equals(prevStatus, reqStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ApiSuccess(new ShipmentStatusChangeResponseDto { ShipmentId = shipment.ShipmentId, ShipmentNo = shipment.ShipmentNo, PreviousStatus = prevStatus, CurrentStatus = shipment.ShipmentStatus }, message: "Shipment status is already current.");
        }

        shipment.ShipmentStatus = reqStatus;
        shipment.ReviseDate = DateTime.UtcNow;
        shipment.ReviseBy = User.Identity?.Name ?? "api";

        var trackingEvent = new TrackingEvent
        {
            ShipmentId = shipment.ShipmentId,
            DriverId = shipment.DriverId,
            VehicleId = shipment.VehicleId,
            EventCode = ToEventCode(reqStatus),
            EventName = $"Shipment status changed to {reqStatus}",
            EventDate = request.EventDate ?? DateTime.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? "API" : request.SourceType.Trim(),
            Remark = request.Remark,
            IsActive = true,
            CreateDate = DateTime.UtcNow,
            CreateBy = User.Identity?.Name ?? "api"
        };

        Context.TrackingEvents.Add(trackingEvent);
        await Context.SaveChangesAsync(cancellationToken);

        return ApiSuccess(new ShipmentStatusChangeResponseDto { ShipmentId = shipment.ShipmentId, ShipmentNo = shipment.ShipmentNo, PreviousStatus = prevStatus, CurrentStatus = shipment.ShipmentStatus, TrackingEventId = trackingEvent.TrackingEventId }, message: "Shipment status updated.");
    }

    private static string ToEventCode(string status)
        => status.Replace(" ", "_", StringComparison.Ordinal).Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();
}
