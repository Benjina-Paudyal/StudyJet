using StudyJet.API.DTOs.User;
using StudyJet.API.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using Microsoft.AspNetCore.Identity;
using StudyJet.API.Data.Entities;

namespace StudyJet.API.Tests.IntegrationTests
{
    public class LoginIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {

        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public LoginIntegrationTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var loginDto = new UserLoginDTO
            {
                Email = "nonexistentuser@example.com",
                Password = "SomePassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User not found", content);
        }


        [Fact]
        public async Task Login_Successful_WhenValidCredentialsAreProvided()
        {
            // Arrange
            var uniqueEmail = $"validuser{Guid.NewGuid()}@example.com";
            var loginDto = new UserLoginDTO
            {
                Email = uniqueEmail,
                Password = "ValidPassword123!"
            };

            // Scope to resolve UserManager
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var testUser = new User
                {
                    UserName = uniqueEmail,
                    Email = uniqueEmail,
                    FullName = "Test User",
                    EmailConfirmed = true
                };

                // Adding user to the database 
                var result = await userManager.CreateAsync(testUser, loginDto.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Description}");
                    }
                }

                Assert.True(result.Succeeded, $"User creation failed! Check the error messages above. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("\"token\"", content);
            }
        }


        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenEmailNotConfirmed()
        {
            var loginDto = new UserLoginDTO
            {
                Email = "unconfirmeduser@example.com",
                Password = "ValidPassword123!"
            };

            // Use scope to resolve UserManager
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                // Create the test user in the database using UserManager
                var testUser = new User
                {
                    UserName = "unconfirmeduser@example.com",
                    Email = "unconfirmeduser@example.com",
                    FullName = "Unconfirmed User",
                    EmailConfirmed = false, // Email is not confirmed
                };

                // Add the user to the database (UserManager will handle password hashing)
                var result = await userManager.CreateAsync(testUser, loginDto.Password);

                // Ensure user creation was successful
                Assert.True(result.Succeeded);

                // Act: Perform the login request
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

                // Assert: Ensure the response is unauthorized
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

                // Read the response content
                var content = await response.Content.ReadAsStringAsync();

                // Assert that the response contains the "Email not confirmed" error
                Assert.Contains("Email not confirmed", content);
            }
        }


        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidPasswordIsProvided()
        {
            var loginDto = new UserLoginDTO
            {
                Email = "validuser@example.com",
                Password = "InvalidPassword123!" // Invalid password
            };

            // Scope to resolve UserManager
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                // Creating the test user in database 
                var testUser = new User
                {
                    UserName = "validuser@example.com",
                    Email = "validuser@example.com",
                    FullName = "Valid User",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(testUser, "ValidPassword123!");
                Assert.True(result.Succeeded);
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("Invalid email or password", content);
            }
        }


        [Fact]
        public async Task Login_ReturnsPasswordChangeRequired_WhenUserNeedsPasswordChange()
        {
            var loginDto = new UserLoginDTO
            {
                Email = "userneedschangepassword@example.com",
                Password = "ValidPassword123!"
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var testUser = new User
                {
                    UserName = "userneedschangepassword@example.com",
                    Email = "userneedschangepassword@example.com",
                    FullName = "User Needing Password Change",
                    EmailConfirmed = true,
                    NeedToChangePassword = true
                };

                var result = await userManager.CreateAsync(testUser, "ValidPassword123!");
                Assert.True(result.Succeeded);
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("You need to change your password", content);
            }
        }


        [Fact]
        public async Task Login_Returns2FARequired_When2FAIsEnabled()
        {
            var loginDto = new UserLoginDTO
            {
                Email = "userwith2fa@example.com",
                Password = "ValidPassword123!"
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var testUser = new User
                {
                    UserName = "userwith2fa@example.com",
                    Email = "userwith2fa@example.com",
                    FullName = "User With 2FA",
                    EmailConfirmed = true,
                    TwoFactorEnabled = true
                };

                var result = await userManager.CreateAsync(testUser, "ValidPassword123!");
                Assert.True(result.Succeeded);
                var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("\"requires2FA\":true", content);
            }




        }
    }
}





