namespace DemandManagement2.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendDemandNotificationAsync(string eventType, Guid demandId, string demandTitle, string? recipientEmail);
}
