using Microsoft.AspNetCore.Mvc;
using TmsSystem.Api.Responses;
using TmsSystem.Application.Common;

namespace TmsSystem.Api.Controllers;

public abstract class TmsControllerBase : ControllerBase
{
    protected IActionResult ApiSuccess<T>(T? data, int code = StatusCodes.Status200OK, string? message = null)
        => StatusCode(code, ApiResponse<T>.SuccessResponse(data, code, message));

    protected IActionResult ApiFailure<T>(int code, string? message, IEnumerable<string>? errors = null, T? data = default)
        => StatusCode(code, ApiResponse<T>.FailureResponse(code, message, errors, data));

    protected IActionResult ApiFromOperation<T>(OperationResult<T> result, int successCode = StatusCodes.Status200OK, int failureCode = StatusCodes.Status400BadRequest)
        => result.IsSuccess
            ? ApiSuccess(result.Data, successCode, result.Message)
            : ApiFailure<T>(failureCode, result.Message, result.Errors);
}
