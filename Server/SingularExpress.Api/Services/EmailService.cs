using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using SingularExpress.Interfaces;
using System;
using System.Threading.Tasks;

namespace SingularExpress.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private const string FromEmail = "nokubongangema8@gmail.com";
        private const string FromName = "Singular Express";

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string otp, string username)
        {
            try
            {
                var apiKey = _config["SendGrid:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("SendGrid API key is not configured");
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(FromEmail, FromName);
                var to = new EmailAddress(toEmail, username);
                var subject = "Your Password Reset Code";
                
                var plainTextContent = $"Hello {username},\n\n" +
                                      $"Your password reset code is: {otp}\n\n" +
                                      "This code will expire in 30 minutes.";

                var htmlContent = $@"
                    <html>
                        <body>
                            <h2>Hello {username},</h2>
                            <p>Your password reset code is: <strong>{otp}</strong></p>
                            <p>This code will expire in 30 minutes.</p>
                        </body>
                    </html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Body.ReadAsStringAsync();
                    throw new Exception($"Failed to send email. Status: {response.StatusCode}, Response: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email");
                throw; // Re-throw to let the controller handle it
            }
        }
    }
}