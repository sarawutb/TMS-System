using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Fleet;
using TmsSystem.BlazorWasm.Models;

namespace TmsSystem.BlazorWasm.Services;

public sealed class FleetService(HttpClient httpClient)
{
    public async Task<OperationResult<FleetSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<FleetSummaryDto>>("api/fleet/summary", cancellationToken);
            return result?.Success == true && result.Data is not null
                ? OperationResult<FleetSummaryDto>.Success(result.Data, result.Message ?? "Success.")
                : OperationResult<FleetSummaryDto>.Failure(result?.Message ?? "Failed to retrieve fleet summary.");
        }
        catch (Exception ex)
        {
            return OperationResult<FleetSummaryDto>.Failure($"Connection error: {ex.Message}");
        }
    }
}
