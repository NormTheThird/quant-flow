namespace QuantFlow.UI.WPF.Services;

public class AuthenticationApiService : IAuthenticationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationApiService> _logger;

    public AuthenticationApiService(HttpClient httpClient, ILogger<AuthenticationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthenticateResponse?> AuthenticateAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/v1.0/pub/authentication/authenticate", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Authentication failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthenticateResponse>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return apiResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return null;
        }
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}