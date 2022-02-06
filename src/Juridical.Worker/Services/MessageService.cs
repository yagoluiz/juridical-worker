using System.Net.Http.Json;
using System.Text.Json;
using Juridical.Worker.Interfaces;
using Juridical.Worker.Models.Requests;
using Juridical.Worker.Models.Responses;

namespace Juridical.Worker.Services;

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
