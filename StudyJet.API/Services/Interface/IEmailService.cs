using Microsoft.AspNetCore.Identity;

namespace StudyJet.API.Services.Interface
{
    public interface IEmailService
    {
        Task<IdentityResult> SendEmailAsync(string recipientEmail, string subject, string body);

        Task<IdentityResult> SendConfirmationEmailAsync(string recipientEmail, string confirmationLink);


    }
}
