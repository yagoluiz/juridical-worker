using Juridical.Worker.Models.Requests;
using Juridical.Worker.Models.Responses;

namespace Juridical.Worker.Interfaces;

public interface IMessageService
{
    Task<ServiceResponse> SendAsync(MessageRequest request);
}
