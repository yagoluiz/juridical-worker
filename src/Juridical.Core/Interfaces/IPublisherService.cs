using Juridical.Core.Models.Publishers;

namespace Juridical.Core.Interfaces;

public interface IPublisherService
{
    Task PublishAsync(Message message);
}
