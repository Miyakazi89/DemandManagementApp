namespace DemandManagement2.Api.Services;

public class EmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "Demand Management";
    public bool EnableSsl { get; set; } = true;
    public bool Enabled { get; set; } = false;
}
