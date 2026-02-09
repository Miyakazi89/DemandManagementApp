namespace DemandManagement2.Domain.Entities;

public enum UserRole
{
    Requester = 0,
    Assessor = 1,
    Admin = 2
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Requester;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
