using TmsSystem.Application.Dtos.Fleet;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class FleetDashboardViewModel(FleetService fleetService)
{
    public FleetSummaryDto Summary { get; private set; } = new();
    public bool IsLoading { get; private set; } = true;
    public string ErrorMessage { get; private set; } = string.Empty;

    public decimal AverageUtilization => Summary.Vehicles.Count == 0 ? 0m : decimal.Round(Summary.Vehicles.Average(x => x.UtilizationPercent), 2);
    public int DueMaintenanceCount => Summary.Vehicles.Count(x => x.NextMaintenanceDate?.Date <= DateTime.UtcNow.Date.AddDays(7));
    public int SafetyEventCount => Summary.Vehicles.Sum(x => x.SafetyEventCount30Days);

    public async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await fleetService.GetSummaryAsync();
        if (result.IsSuccess && result.Data is not null)
        {
            Summary = result.Data;
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsLoading = false;
    }
}
