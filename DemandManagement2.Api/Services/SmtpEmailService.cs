using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DemandManagement2.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email disabled. Would send to {To}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl
                ? MailKit.Security.SecureSocketOptions.StartTls
                : MailKit.Security.SecureSocketOptions.Auto);

            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {To}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", toEmail, subject);
        }
    }

    public async Task SendDemandNotificationAsync(string eventType, Guid demandId, string demandTitle, string? recipientEmail)
    {
        if (string.IsNullOrEmpty(recipientEmail)) return;

        var (subject, body) = eventType switch
        {
            "Created" => (
                $"New Demand Created: {demandTitle}",
                BuildHtml(demandTitle, "A new demand request has been created and is awaiting review.", "#2563eb")),
            "Assessed" => (
                $"Demand Assessed: {demandTitle}",
                BuildHtml(demandTitle, "Your demand request has been assessed. Please check the assessment details.", "#06b6d4")),
            "Approved" => (
                $"Demand Approved: {demandTitle}",
                BuildHtml(demandTitle, "Your demand request has been approved.", "#22c55e")),
            "Rejected" => (
                $"Demand Rejected: {demandTitle}",
                BuildHtml(demandTitle, "Your demand request has been rejected. Please review the comments.", "#ef4444")),
            "OnHold" => (
                $"Demand On Hold: {demandTitle}",
                BuildHtml(demandTitle, "Your demand request has been placed on hold.", "#f59e0b")),
            "InfoRequested" => (
                $"Additional Information Requested: {demandTitle}",
                BuildHtml(demandTitle, "Additional information has been requested for your demand. Please update and resubmit.", "#f59e0b")),
            _ => (
                $"Demand Update: {demandTitle}",
                BuildHtml(demandTitle, $"Your demand request has been updated (event: {eventType}).", "#2563eb"))
        };

        await SendEmailAsync(recipientEmail, subject, body);
    }

    private static string BuildHtml(string title, string message, string accentColor) => $"""
        <div style="font-family:Segoe UI,Arial,sans-serif;max-width:600px;margin:0 auto;background:#0f172a;color:#e2e8f0;border-radius:12px;overflow:hidden;">
            <div style="background:{accentColor};padding:20px 30px;">
                <h1 style="margin:0;font-size:20px;color:#fff;">Demand Management</h1>
            </div>
            <div style="padding:30px;">
                <h2 style="margin:0 0 12px;font-size:18px;color:#f8fafc;">{title}</h2>
                <p style="margin:0 0 20px;font-size:15px;line-height:1.6;color:#cbd5e1;">{message}</p>
                <p style="margin:0;font-size:13px;color:#64748b;">This is an automated notification from the Demand Management system.</p>
            </div>
        </div>
        """;
}
