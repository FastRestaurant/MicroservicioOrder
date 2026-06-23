using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace OrderService.Presentation.Http;

internal static class HttpResiliencePolicies
{
    private static PolicyBuilder<HttpResponseMessage> Transient =>
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>(ex => ex.InnerException is TimeoutException)
            .OrResult(msg => msg.StatusCode >= HttpStatusCode.InternalServerError
                          || msg.StatusCode == HttpStatusCode.RequestTimeout);

    public static IAsyncPolicy<HttpResponseMessage> Retry =>
        Transient.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreaker =>
        Transient.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));
}
