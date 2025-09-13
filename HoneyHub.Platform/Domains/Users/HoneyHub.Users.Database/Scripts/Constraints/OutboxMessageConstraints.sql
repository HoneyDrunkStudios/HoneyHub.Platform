-- Unique
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [UK_OutboxMessage_PublicId] UNIQUE NONCLUSTERED ([PublicId]);
GO

-- Status domain: 0=Pending, 1=InFlight, 2=Succeeded, 3=DeadLettered
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Status] CHECK ([Status] IN (0,1,2,3));
GO

-- JSON checks
ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Payload_IsJson] CHECK (ISJSON([Payload]) = 1);
GO

ALTER TABLE [dbo].[OutboxMessage]
ADD CONSTRAINT [CHK_OutboxMessage_Headers_IsJson] CHECK ([Headers] IS NULL OR ISJSON([Headers]) = 1);
GO