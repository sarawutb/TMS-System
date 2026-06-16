using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;


public sealed class LoginViewModel(
    AuthSessionService authService,
    NavigationManager navigationManager,
    LoadingService loadingService)
    : BaseViewModel(loadingService)
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    public bool IsLoggingIn { get => IsLoading; set => IsLoading = value; }

    public async Task HandleLoginAsync()
    {
        IsLoggingIn = true;
        ErrorMessage = null;

        var result = await authService.LoginAsync(UserName, Password);
        if (result.IsSuccess)
        {
            var returnUrl = GetReturnUrl();
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                navigationManager.NavigateTo(returnUrl);
            }
            else
            {
                navigationManager.NavigateTo("");
            }
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsLoggingIn = false;
    }

    private string? GetReturnUrl()
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = uri.Query.TrimStart('?');

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValue = part.Split('=', 2);
            var key = DecodeQueryValue(keyValue[0]);

            if (keyValue.Length == 2 && string.Equals(key, "returnUrl", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = DecodeQueryValue(keyValue[1]);
                return IsLocalReturnUrl(returnUrl) ? returnUrl : null;
            }
        }

        return null;
    }

    private static string DecodeQueryValue(string value)
    {
        return Uri.UnescapeDataString(value.Replace("+", " "));
    }

    private static bool IsLocalReturnUrl(string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        return !returnUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.StartsWith("//", StringComparison.Ordinal)
            && !returnUrl.StartsWith('\\')
            && !returnUrl.StartsWith("login", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.StartsWith("/login", StringComparison.OrdinalIgnoreCase);
    }
}
