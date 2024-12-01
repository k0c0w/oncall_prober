using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Options;

namespace OnCallProber.Services;

internal sealed class EventsService
{
    private readonly OnCallInfo _onCallInfo;
    private readonly ILogger<EventsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly SignatureEncoder _signatureEncoder;

    public EventsService(
        IOptions<OnCallInfo> config,
        IHttpClientFactory httpClientFactory, 
        ILogger<EventsService> logger,
        SignatureEncoder signatureEncoder)
    {
        _onCallInfo = config.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _signatureEncoder = signatureEncoder;
    }
    
    public async Task<bool> CreateNewEventAsync(CreateEvent createEvent)
    {
        const string endpoint = "/api/v0/events";
        var fullEndpoint = $"{_onCallInfo.Host}{endpoint}";

        var json = JsonSerializer.Serialize(createEvent);
        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, fullEndpoint)
        {
            Content = jsonContent,
        };
        request.Headers.Add(
            "Authorization", 
            GetAuthorizationHeader(endpoint, HttpMethod.Post, json));

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Remote returned invalid status code {code}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process event {Event} creation due to exception.", createEvent);
            return false;
        }

        return true;
    }

    private string GetAuthorizationHeader(string endpoint, HttpMethod httpMethod, string body)
    {
        var strMethod = httpMethod.Method.ToUpper();
        var window = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

        var value = $"{window} {strMethod} {endpoint} {body}";
        var signature = _signatureEncoder.ComputeSignature(value);
        
        return $"hmac {_onCallInfo.AppName}:{signature}";
    }
    
    public record CreateEvent
    {
        [JsonPropertyName("user")]
        public required string User { get; init; }
        
        [JsonPropertyName("team")]
        public required string Team { get; init; }
        
        [JsonPropertyName("role")]
        public required string Role { get; init; }
        
        [JsonPropertyName("start")]
        public required long StartTimestamp { get; init; }
        
        [JsonPropertyName("end")]
        public required long EndTimestamp { get; init; }
    }
}