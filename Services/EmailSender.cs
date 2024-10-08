using System.Net;
using System.Net.Mail;
using Sciencetopia.Services;

public class EmailSender : IEmailSender
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromAddress;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public EmailSender(IConfiguration configuration)
    {
        _smtpServer = configuration["Smtp:Server"] ?? throw new ArgumentNullException(nameof(_smtpServer), "SMTP server is not configured.");
        if (!int.TryParse(configuration["Smtp:Port"], out _smtpPort))
        {
            throw new ArgumentException("Invalid SMTP port number.");
        }
        _fromAddress = configuration["Smtp:FromAddress"] ?? throw new ArgumentNullException(nameof(_fromAddress), "From address is not configured.");
        _smtpUsername = configuration["Smtp:Username"] ?? throw new ArgumentNullException(nameof(_smtpUsername), "SMTP username is not configured.");
        _smtpPassword = configuration["Smtp:Password"] ?? throw new ArgumentNullException(nameof(_smtpPassword), "SMTP password is not configured.");
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        using var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromAddress),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        await client.SendMailAsync(mailMessage);
    }
}

