using System.Net.Http.Json;
using System.Text.Json;
using Juridical.Core.Interfaces;
using Juridical.Core.Models.Requests;
using Juridical.Core.Models.Responses;
using Microsoft.Extensions.Configuration;

namespace Juridical.Core.Services;

public class MessageService : IMessageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public MessageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ServiceResponse> SendAsync(MessageRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            _configuration.GetValue<string>("MESSAGE_SERVICE_SEND_URI"), request);

        var contentResponse = await response.Content.ReadAsStringAsync();

        return !response.IsSuccessStatusCode
            ? new ServiceResponse(false, contentResponse)
            : new ServiceResponse(true, JsonSerializer.Deserialize<MessageResponse>(contentResponse));
    }
}
