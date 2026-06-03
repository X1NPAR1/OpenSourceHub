using Microsoft.Extensions.Logging;

namespace OpenSourceHub.Infrastructure.GitHub;

public sealed class HttpRetryHandler : DelegatingHandler
{
    private readonly ILogger<HttpRetryHandler> _logger;
    private const int MaxRetries = 3;

    public HttpRetryHandler(ILogger<HttpRetryHandler> logger)
    {
        _logger = logger;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        HttpResponseMessage? response = null;
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                response = await base.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                    return response;

                if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
                {
                    if (attempt < MaxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        _logger.LogWarning("HTTP {Status} on attempt {Attempt}. Retrying in {Delay}s",
                            (int)response.StatusCode, attempt, delay.TotalSeconds);
                        await Task.Delay(delay, ct);
                        continue;
                    }
                }

                return response;
            }
            catch (HttpRequestException ex) when (attempt < MaxRetries)
            {
                _logger.LogWarning(ex, "Request failed on attempt {Attempt}. Retrying...", attempt);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
            }
        }
        return response ?? new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
    }
}
