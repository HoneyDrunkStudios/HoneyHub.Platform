namespace HoneyHub.Outbox.Abstractions;

public interface IOutboxStore
{
    Task EnqueueAsync(
        string eventType,
        string aggregateType,
        Guid aggregatePublicId,
        object payload,
        Guid? correlationId = null,
        Guid? causationId = null,
        DateTimeOffset? availableAt = null,
        CancellationToken ct = default);
}
