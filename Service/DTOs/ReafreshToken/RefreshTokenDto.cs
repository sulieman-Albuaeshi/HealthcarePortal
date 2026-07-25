namespace Service.DTOs;

public class RefreshTokenDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? TokenHash { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public UserDto? User { get; set; }
}
