using System.Net.Http.Json;
using Haulory.Mobile.Contracts.Auth;

namespace Haulory.Mobile.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var url = new Uri(_httpClient.BaseAddress!, "api/auth/register");
            System.Diagnostics.Debug.WriteLine($"REGISTER URL: {url}");

            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"REGISTER STATUS: {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"REGISTER BODY: {body}");

            if (response.IsSuccessStatusCode)
                return (true, null);

            return (false, body);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("REGISTER EXCEPTION:");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return (false, ex.ToString());
        }
    }

    public async Task<(LoginResponse? Response, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var body = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"LOGIN STATUS: {(int)response.StatusCode} {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"LOGIN BODY: {body}");

            if (!response.IsSuccessStatusCode)
                return (null, body);

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return (result, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LOGIN EXCEPTION:");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return (null, ex.ToString());
        }
    }
}