using Juridical.Core.Models.Requests;
using Juridical.Core.Models.Responses;

namespace Juridical.Core.Interfaces;

public interface IMessageService
{
    Task<ServiceResponse> SendAsync(MessageRequest request);
}
