using HA.TFG.AppFinanzas.Core.Features.Authentication;
using System.Net.Http.Headers;

namespace HA.TFG.AppFinanzas.Infrastructure.Http;

public sealed partial class AuthHeaderHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
