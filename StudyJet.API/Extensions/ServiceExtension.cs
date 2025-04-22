using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


namespace StudyJet.API.Extensions
{
    public static class ServiceExtension
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    RoleClaimType = ClaimTypes.Role

                };


                // FOR COOKIES
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Extract JWT from the "AuthToken" cookie instead of the Authorization header
                        context.Token = context.Request.Cookies["AuthToken"];
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var subClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(subClaim))
                        {
                            var identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(ClaimTypes.Name, subClaim));
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };


            });
        }


    }
}
