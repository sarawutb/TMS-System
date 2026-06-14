using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TmsSystem.BlazorWasm.Services;

public sealed class AuthHeaderHandler(ApiAuthenticationStateProvider authenticationStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var session = await authenticationStateProvider.GetSessionAsync();
        if (session is not null && !string.IsNullOrWhiteSpace(session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
