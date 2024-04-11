using Polly;

namespace CounterService.Core.Helpers;

public class CircuitBreakerService
{
    private readonly HttpClient _httpClient;
    private readonly Policy _circuitBreakerPolicy;
    
    public CircuitBreakerService()
    {
        _httpClient = new HttpClient();
        _circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(
            exceptionsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (ex, t) =>
            {
                // This action is executed when the circuit is opened
                Console.WriteLine("Circuit breaker opened.");
            },
            onReset: () =>
            {
                // This action is executed when the circuit is reset
                Console.WriteLine("Circuit breaker reset.");
            }
        );
    }
    public async Task<T?> MakeRequestAsync<T>(string url)
    {
        // Wrap the HTTP request with the circuit breaker policy
        var response = await _circuitBreakerPolicy.Execute(async () =>
        {
            return await _httpClient.GetAsync(url);
        });

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        throw new Exception($"Request failed with status code {response.StatusCode}");
    }
}