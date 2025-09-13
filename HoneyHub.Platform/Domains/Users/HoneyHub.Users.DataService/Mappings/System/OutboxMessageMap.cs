using HoneyHub.Core.DataService.Mappings;
using HoneyHub.Outbox.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyHub.Users.DataService.Mappings.System;

public sealed class OutboxMessageMap : IEntityMap
{
    public void Configure(ModelBuilder modelBuilder)
    {
        var b = modelBuilder.Entity<OutboxMessage>();

        b.ToTable("OutboxMessage", "dbo");
        b.HasKey(x => x.Id);

        b.Property(x => x.PublicId).IsRequired();
        b.Property(x => x.AggregateType).IsRequired().HasMaxLength(100);
        b.Property(x => x.EventType).IsRequired().HasMaxLength(200);
        b.Property(x => x.Payload).IsRequired();
        b.Property(x => x.CreatedBy).HasMaxLength(100);
        b.Property(x => x.UpdatedBy).HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<byte>();

        b.HasIndex(x => x.PublicId).IsUnique().HasDatabaseName("UK_OutboxMessage_PublicId");
        b.HasIndex(x => new { x.AvailableAt, x.Id })
            .HasDatabaseName("IX_OutboxMessage_PendingDue")
            .HasFilter("[Status] = 0");
        b.HasIndex(x => new { x.AggregateType, x.AggregatePublicId, x.OccurredAt })
            .HasDatabaseName("IX_OutboxMessage_Aggregate");
        b.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("IX_OutboxMessage_OccurredAt");
    }
}
