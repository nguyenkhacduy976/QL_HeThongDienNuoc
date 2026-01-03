using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QL_HethongDiennuoc.Services.ApiClients;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private void AddAuthHeader()
    {
        // Forward cookies authentication
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var cookieContainer = new System.Net.CookieContainer();
            foreach (var cookie in context.Request.Cookies)
            {
                // Chỉ forward cookie xác thực của ASP.NET Core
                if (cookie.Key.StartsWith(".AspNetCore.Cookies"))
                {
                    _httpClient.DefaultRequestHeaders.Add("Cookie", $"{cookie.Key}={cookie.Value}");
                }
            }
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
                return default;

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResponse?> PostAsync<TResponse>(string endpoint, object data)
    {
        try
        {
            AddAuthHeader();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new Exception($"API Error: {ex.Message}");
        }
    }

    public async Task<TResponse?> PutAsync<TResponse>(string endpoint, object data)
    {
        try
        {
            AddAuthHeader();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new Exception($"API Error: {ex.Message}");
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = errorContent;
                try
                {
                    // Try to parse as JSON error object
                    using (var doc = JsonDocument.Parse(errorContent))
                    {
                        if (doc.RootElement.TryGetProperty("message", out var msgElement))
                        {
                            errorMessage = msgElement.GetString();
                        }
                        else if (doc.RootElement.TryGetProperty("title", out var titleElement))
                        {
                             errorMessage = titleElement.GetString();
                        }
                    }
                }
                catch
                {
                    // If parsing fails, use original content or default message
                    if (string.IsNullOrWhiteSpace(errorMessage)) errorMessage = "Lỗi khi gọi API (không có chi tiết).";
                }

                throw new Exception(errorMessage);
            }

            return true;
        }
        catch (Exception ex)
        {
             throw ex; 
        }
    }

    public async Task<byte[]?> GetBytesAsync(string endpoint)
    {
        try
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync(endpoint);
            
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }
}
