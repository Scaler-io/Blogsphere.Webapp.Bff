namespace Blogsphere.Webapp.Bff.Infrastructure.Http
{
    /// <summary>
    /// Overrides the outgoing HTTP Host header (and forwarded host) when set.
    /// This is useful in local Docker setups where connectivity uses host.docker.internal
    /// but IdentityServer issuer/host-based validation must remain localhost.
    /// </summary>
    public sealed class HostHeaderOverrideHandler(string? hostHeaderOverride) : DelegatingHandler
    {
        private readonly string _hostHeaderOverride = string.IsNullOrWhiteSpace(hostHeaderOverride) ? null : hostHeaderOverride;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_hostHeaderOverride is not null)
            {
                request.Headers.Host = _hostHeaderOverride;
                request.Headers.TryAddWithoutValidation("X-Forwarded-Host", _hostHeaderOverride);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

