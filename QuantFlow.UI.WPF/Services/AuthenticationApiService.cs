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

    public async Task<bool> ValidateUserPreferencesAsync(Guid userId, string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync($"/api/v1.0/pub/user/preferences/{userId}", null);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to ensure user preferences with status code: {StatusCode}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring user preferences");
            return false;
        }
    }
}