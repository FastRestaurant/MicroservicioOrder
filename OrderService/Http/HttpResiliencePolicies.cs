using Polly;
using Polly.Extensions.Http;

namespace OrderService.Presentation.Http;

internal static class HttpResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> Retry =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));
}
