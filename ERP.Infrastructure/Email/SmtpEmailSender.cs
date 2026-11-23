using System.Net;
using System.Net.Mail;
using ERP.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace ERP.Infrastructure.Email;

public class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException("Email:Host and Email:FromAddress must be configured.");

        using var message = new MailMessage(_options.FromAddress, to, subject, body);
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

        await client.SendMailAsync(message, cancellationToken);
    }
}
