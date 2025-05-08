using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using StudyJet.API.Services.Interface;

namespace StudyJet.API.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IdentityResult> SendConfirmationEmailAsync(string recipientEmail, string confirmationLink)
        {
            // Check for null or empty values
            if (string.IsNullOrEmpty(recipientEmail) || string.IsNullOrEmpty(confirmationLink))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email or confirmation link is missing." });
            }

            // Ensure the configuration values are not null
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];
            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "From email or name is missing in configuration." });
            }

            try
            {
                var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
                {
                    Port = int.Parse(_configuration["Smtp:Port"]),  
                    Credentials = new NetworkCredential(
                        _configuration["Smtp:Username"],
                        _configuration["Smtp:Password"]),
                    EnableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]),
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Email Confirmation",
                    Body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error sending email: " + smtpEx.Message });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "An unexpected error occurred: " + ex.Message });
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SendEmailAsync(string recipientEmail, string subject, string body)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(recipientEmail) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email, subject, or body is missing." });
            }

            // Check configuration values
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];
            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "From email or name is missing in configuration." });
            }

            try
            {
                var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
                {
                    Port = int.Parse(_configuration["Smtp:Port"]),
                    Credentials = new NetworkCredential(
                        _configuration["Smtp:Username"],
                        _configuration["Smtp:Password"]),
                    EnableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]),
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error sending email: " + smtpEx.Message });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "An unexpected error occurred: " + ex.Message });
            }

            return IdentityResult.Success;




        }


    }
}
