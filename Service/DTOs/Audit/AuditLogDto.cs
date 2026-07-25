namespace Service.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = null!;
    public UserAuditDTO? User { get; set; }
}
