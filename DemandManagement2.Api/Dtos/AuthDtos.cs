using System.ComponentModel.DataAnnotations;

namespace DemandManagement2.Api.Dtos;

public class RegisterDto
{
    [Required, MinLength(2), MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Requester";
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponseDto(
    string Token,
    string FullName,
    string Email,
    string Role,
    DateTime Expiration
);
