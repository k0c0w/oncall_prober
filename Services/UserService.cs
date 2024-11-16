using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace OnCallProber.Services;

internal sealed class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly string _host;
    private readonly HttpClient _httpClient;

    public UserService(
        IHttpClientFactory httpClientFactory,
        ILogger<UserService> logger, 
        IOptions<OnCallInfo> options)
    {
        _logger = logger;
        _host = options.Value.Host;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<bool> CreateAsync(string user)
    {
        var endpoint = $"{_host}/api/v0/users";
        _logger.LogInformation("Sending POST request to {Endpoint}", endpoint);
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, new { name = user });
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Remote returned invalid status code {code}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process user {User} creation due to exception.", user);
            return false;
        }
        
        return true;
    }

    public async Task<bool> DeleteAsync(string user)
    {
        var endpoint = $"{_host}/api/v0/users/{user}";
        _logger.LogInformation("Sending DELETE request to {Endpoint}", endpoint);
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Remote returned invalid status code {code}", response.StatusCode);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogInformation(
                        "User {User} was not found in remote while delete operation.", 
                        user);
                }
                return false;
            }    
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to process user {User} deletion due to exception.", 
                user);
            return false;
        }
        
        return true;
    }
}