namespace Juridical.Core.Interfaces;

public interface ISubscriberService : IDisposable
{
    Task SubscriberAsync(CancellationToken cancellationToken);
}
