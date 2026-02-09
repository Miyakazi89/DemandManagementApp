namespace DemandManagement2.Ui.Services;

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Role { get; set; } = "Requester";
}

public record AuthResponse(string Token, string FullName, string Email, string Role, DateTime Expiration);
