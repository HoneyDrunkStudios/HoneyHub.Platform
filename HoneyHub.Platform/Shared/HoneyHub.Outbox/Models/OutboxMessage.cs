namespace HoneyHub.Outbox.Models;

public class OutboxMessage
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }

    public string AggregateType { get; set; } = null!;
    public Guid? AggregatePublicId { get; set; }
    public string EventType { get; set; } = null!;
    public Guid? CorrelationId { get; set; }
    public Guid? CausationId { get; set; }

    public string Payload { get; set; } = null!;
    public string? Headers { get; set; }

    public byte Status { get; set; }
    public int Attempts { get; set; }

    public DateTime OccurredAt { get; set; }
    public DateTime AvailableAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public string? LastError { get; set; }

    public string CreatedBy { get; set; } = "System";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
