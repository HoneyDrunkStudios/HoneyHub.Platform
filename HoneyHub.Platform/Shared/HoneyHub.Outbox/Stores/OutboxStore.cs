using System.Text.Json;
using HoneyHub.Outbox.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Outbox.Stores;

public sealed class OutboxStore<TDbContext> : Abstractions.IOutboxStore where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public OutboxStore(TDbContext db) => _db = db;

    public Task EnqueueAsync(
        string eventType,
        string aggregateType,
        Guid aggregatePublicId,
        object payload,
        Guid? correlationId = null,
        Guid? causationId = null,
        DateTimeOffset? availableAt = null,
        CancellationToken ct = default)
    {
        var msg = new OutboxMessage
        {
            PublicId = Guid.NewGuid(),
            AggregateType = aggregateType,
            AggregatePublicId = aggregatePublicId,
            EventType = eventType,
            CorrelationId = correlationId,
            CausationId = causationId,
            Payload = JsonSerializer.Serialize(payload, JsonOpts),
            Status = 0,
            Attempts = 0,
            OccurredAt = DateTime.UtcNow,
            AvailableAt = (availableAt ?? DateTime.UtcNow).UtcDateTime,
            CreatedBy = "System",
            IsDeleted = false
        };

        _db.Set<OutboxMessage>().Add(msg);
        return Task.CompletedTask;
    }
}
