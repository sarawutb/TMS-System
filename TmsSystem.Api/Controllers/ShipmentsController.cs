using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Application.Dtos.Shipments;
using TmsSystem.Domain.Entities;
using TmsSystem.Domain.Workflow;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/shipment")]
[Route("api/shipments")]
public sealed class ShipmentsController : CrudController<Shipment>
{
    private readonly TmsDbContext dbContext;

    public ShipmentsController(TmsDbContext dbContext) : base(dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpPost("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(long id, UpdateShipmentStatusRequestDto request, CancellationToken cancellationToken)
    {
        var requestedStatus = request.Status.Trim();
        if (!ShipmentStatusTransitionPolicy.IsKnownStatus(requestedStatus))
        {
            return ApiFailure<ShipmentStatusChangeResponseDto>(
                StatusCodes.Status400BadRequest,
                "Unknown shipment status.",
                [$"Status must be one of: {string.Join(", ", ShipmentStatusTransitionPolicy.AllStatuses)}."]);
        }

        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.ShipmentId == id, cancellationToken);
        if (shipment is null)
        {
            return ApiFailure<ShipmentStatusChangeResponseDto>(StatusCodes.Status404NotFound, "Shipment not found.");
        }

        var previousStatus = shipment.ShipmentStatus;
        if (!ShipmentStatusTransitionPolicy.CanTransition(previousStatus, requestedStatus))
        {
            var allowed = ShipmentStatusTransitionPolicy.GetAllowedNextStatuses(previousStatus);
            var allowedText = allowed.Count == 0 ? "No further transitions are allowed." : $"Allowed next statuses: {string.Join(", ", allowed)}.";

            return ApiFailure<ShipmentStatusChangeResponseDto>(
                StatusCodes.Status409Conflict,
                $"Invalid shipment status transition from '{previousStatus}' to '{requestedStatus}'.",
                [allowedText]);
        }

        if (string.Equals(previousStatus, requestedStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ApiSuccess(new ShipmentStatusChangeResponseDto
            {
                ShipmentId = shipment.ShipmentId,
                ShipmentNo = shipment.ShipmentNo,
                PreviousStatus = previousStatus,
                CurrentStatus = shipment.ShipmentStatus
            }, message: "Shipment status is already current.");
        }

        shipment.ShipmentStatus = requestedStatus;
        shipment.ReviseDate = DateTime.UtcNow;
        shipment.ReviseBy = User.Identity?.Name ?? "api";

        var trackingEvent = new TrackingEvent
        {
            ShipmentId = shipment.ShipmentId,
            EventCode = ToEventCode(requestedStatus),
            EventName = $"Shipment status changed to {requestedStatus}",
            EventDate = request.EventDate ?? DateTime.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? "API" : request.SourceType.Trim(),
            Remark = request.Remark,
            IsActive = true,
            CreateDate = DateTime.UtcNow,
            CreateBy = User.Identity?.Name ?? "api"
        };

        dbContext.TrackingEvents.Add(trackingEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiSuccess(new ShipmentStatusChangeResponseDto
        {
            ShipmentId = shipment.ShipmentId,
            ShipmentNo = shipment.ShipmentNo,
            PreviousStatus = previousStatus,
            CurrentStatus = shipment.ShipmentStatus,
            TrackingEventId = trackingEvent.TrackingEventId
        }, message: "Shipment status updated.");
    }

    private static string ToEventCode(string status)
    {
        return status.Replace(" ", "_", StringComparison.Ordinal).Replace("-", "_", StringComparison.Ordinal).ToUpperInvariant();
    }
}
