using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StudyJet.API.Data.Entities;
using StudyJet.API.DTOs.User;
using StudyJet.API.Tests.Utilities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using NewtonsoftJson = Newtonsoft.Json.JsonSerializer;
using SystemTextJson = System.Text.Json.JsonSerializer;


namespace StudyJet.API.Tests.IntegrationTests
{
    public class RegistrationIntegrationTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public RegistrationIntegrationTest(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _userManager = factory.Services.GetRequiredService<UserManager<User>>();
            _roleManager = factory.Services.GetRequiredService<RoleManager<IdentityRole>>();

        }


        [Fact]
        public async Task Register_ReturnsOk_WhenValidDataIsSent()
        {
            // Simulate a form with file upload (multipart/form-data)
            var form = new MultipartFormDataContent();
            form.Add(new StringContent("newuser123"), "UserName");
            form.Add(new StringContent("newuser@example.com"), "Email");
            form.Add(new StringContent("Test123!"), "Password");
            form.Add(new StringContent("Test123!"), "ConfirmPassword");
            form.Add(new StringContent("New User"), "FullName");

            var response = await _client.PostAsync("/api/auth/register", form);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User registered successfully", content);
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameAlreadyExists()
        {
            // First registration
            var form1 = new MultipartFormDataContent();
            form1.Add(new StringContent("existinguser"), "UserName");
            form1.Add(new StringContent("existing@example.com"), "Email");
            form1.Add(new StringContent("Test123!"), "Password");
            form1.Add(new StringContent("Test123!"), "ConfirmPassword");
            form1.Add(new StringContent("Test User"), "FullName");
            await _client.PostAsync("/api/auth/register", form1);

            // Registering again with same username
            var form2 = new MultipartFormDataContent();
            form2.Add(new StringContent("existinguser"), "UserName");
            form2.Add(new StringContent("newemail@example.com"), "Email");
            form2.Add(new StringContent("Test123!"), "Password");
            form2.Add(new StringContent("Test123!"), "ConfirmPassword");
            form2.Add(new StringContent("Another User"), "FullName");

            var response = await _client.PostAsync("/api/auth/register", form2);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("usernameExists", content);
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            // First registration
            var form1 = new MultipartFormDataContent();
            form1.Add(new StringContent("userA"), "UserName");
            form1.Add(new StringContent("existingemail@example.com"), "Email");
            form1.Add(new StringContent("Test123!"), "Password");
            form1.Add(new StringContent("Test123!"), "ConfirmPassword");
            form1.Add(new StringContent("User A"), "FullName");
            await _client.PostAsync("/api/auth/register", form1);

            // Registering again with same email
            var form2 = new MultipartFormDataContent();
            form2.Add(new StringContent("userB"), "UserName");
            form2.Add(new StringContent("existingemail@example.com"), "Email");
            form2.Add(new StringContent("Test123!"), "Password");
            form2.Add(new StringContent("Test123!"), "ConfirmPassword");
            form2.Add(new StringContent("User B"), "FullName");

            var response = await _client.PostAsync("/api/auth/register", form2);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("emailExists", content); // depends on your actual error
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent("mismatchuser"), "UserName");
            form.Add(new StringContent("mismatch@example.com"), "Email");
            form.Add(new StringContent("Test123!"), "Password");
            form.Add(new StringContent("WrongConfirm!"), "ConfirmPassword");
            form.Add(new StringContent("Mismatch User"), "FullName");

            var response = await _client.PostAsync("/api/auth/register", form);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();

            // Deserialize the response to check for errors
            var jsonResponse = SystemTextJson.Deserialize<JsonElement>(content);

            // Checking if the "errors" object exists and contains "ConfirmPassword"
            if (!(jsonResponse.TryGetProperty("errors", out var errors) &&
                  errors.TryGetProperty("ConfirmPassword", out var confirmPasswordErrors)))
            {
                Assert.Fail("Error message for ConfirmPassword mismatch not found.");
            }
        }


    }
}

