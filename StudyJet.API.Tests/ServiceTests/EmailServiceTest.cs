using Microsoft.Extensions.Configuration;
using Moq;
using StudyJet.API.Repositories.Interface;
using StudyJet.API.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudyJet.API.Tests.ServiceTests
{
    public class EmailServiceTest
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<SmtpClient> _mockSmtpClient;
        private readonly EmailService _emailService;



        public EmailServiceTest()
        {
            _mockConfig = new Mock<IConfiguration>();
            _emailService = new EmailService(_mockConfig.Object);
        }

       
        [Theory]
        [InlineData(null, "http://example.com")]
        [InlineData("user@example.com", null)]
        [InlineData("", "http://example.com")]
        [InlineData("user@example.com", "")]
        public async Task SendConfirmationEmailAsync_ReturnsFailure_WhenInputIsInvalid(string email, string link)
        {
            // Act
            var result = await _emailService.SendConfirmationEmailAsync(email, link);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("missing"));
        }


       
        [Fact]
        public async Task SendConfirmationEmailAsync_ReturnsFailure_WhenConfigValuesMissing()
        {
            // Arrange
            _mockConfig.Setup(c => c["EmailSettings:FromEmail"]).Returns("");
            _mockConfig.Setup(c => c["EmailSettings:FromName"]).Returns("Test Service");

            // Act
            var result = await _emailService.SendConfirmationEmailAsync("user@example.com", "http://example.com");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("missing"));
        }

       
       
        [Fact]
        public async Task SendConfirmationEmailAsync_ReturnsFailure_WhenInvalidEmailIsProvided()
        {
            // Arrange: Set up mock configuration values
            _mockConfig.Setup(c => c["EmailSettings:FromEmail"]).Returns("noreply@example.com");
            _mockConfig.Setup(c => c["EmailSettings:FromName"]).Returns("Test Service");
            _mockConfig.Setup(c => c["Smtp:Host"]).Returns("smtp.example.com");
            _mockConfig.Setup(c => c["Smtp:Port"]).Returns("25");
            _mockConfig.Setup(c => c["Smtp:Username"]).Returns("username");
            _mockConfig.Setup(c => c["Smtp:Password"]).Returns("password");
            _mockConfig.Setup(c => c["Smtp:EnableSsl"]).Returns("true");

            // Act: Pass null email to simulate invalid input
            var result = await _emailService.SendConfirmationEmailAsync(null, "http://example.com");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("Email or confirmation link is missing."));
        }

        

        [Fact]
        public async Task SendConfirmationEmailAsync_ReturnsFailure_WhenUnexpectedExceptionOccurs()
        {
            // Arrange
            _mockConfig.Setup(c => c["EmailSettings:FromEmail"]).Returns("noreply@example.com");
            _mockConfig.Setup(c => c["EmailSettings:FromName"]).Returns("Test Service");
            _mockConfig.Setup(c => c["Smtp:Host"]).Returns("smtp.example.com");
            _mockConfig.Setup(c => c["Smtp:Port"]).Returns("25");
            _mockConfig.Setup(c => c["Smtp:Username"]).Returns("username");
            _mockConfig.Setup(c => c["Smtp:Password"]).Returns("password");
            _mockConfig.Setup(c => c["Smtp:EnableSsl"]).Returns("true");

            var exception = new Exception("Unexpected error");
            _mockConfig.Setup(x => x["Smtp:Host"]).Throws(exception);

            // Act
            var result = await _emailService.SendConfirmationEmailAsync("user@example.com", "http://example.com");

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("An unexpected error occurred"));
        }

       

    }

}

