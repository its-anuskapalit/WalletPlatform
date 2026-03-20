using Notification.API.Services.Interfaces;

namespace Notification.API.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _config;

    public EmailService(ILogger<EmailService> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<bool> SendAsync(string to, string subject, string body)
    {
        // ── Simulated email send ──────────────────────────────
        // Replace this block with real provider (SendGrid, SMTP, AWS SES)
        // when going to production.
        await Task.Delay(50); // simulate network latency

        _logger.LogInformation(
            "[EMAIL SIMULATED] To: {To} | Subject: {Subject} | Body: {Body}",
            to, subject, body);

        // Simulate occasional failure for testing (5% failure rate)
        if (Random.Shared.Next(100) < 5)
        {
            _logger.LogWarning("[EMAIL SIMULATED] Simulated send failure for {To}", to);
            return false;
        }

        return true;

        /*
        ── Real SendGrid implementation (uncomment when ready) ──
        var client   = new SendGridClient(_config["SendGrid:ApiKey"]);
        var from     = new EmailAddress(_config["SendGrid:FromEmail"], "WalletPlatform");
        var toAddr   = new EmailAddress(to);
        var msg      = MailHelper.CreateSingleEmail(from, toAddr, subject, body, body);
        var response = await client.SendEmailAsync(msg);
        return response.IsSuccessStatusCode;
        */
    }
}