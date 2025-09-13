-- Hot-path dequeue: only Pending & due messages
CREATE INDEX [IX_OutboxMessage_PendingDue]
    ON [dbo].[OutboxMessage] ([AvailableAt], [Id])
    INCLUDE ([Attempts], [EventType], [AggregateType])
    WHERE [Status] = 0;
GO

-- Aggregate lookup (audit/troubleshooting)
CREATE NONCLUSTERED INDEX [IX_OutboxMessage_Aggregate]
    ON [dbo].[OutboxMessage] ([AggregateType], [AggregatePublicId], [OccurredAt]);
GO

-- Time-based queries
CREATE NONCLUSTERED INDEX [IX_OutboxMessage_OccurredAt]
    ON [dbo].[OutboxMessage] ([OccurredAt]);
GO