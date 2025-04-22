using System.IdentityModel.Tokens.Jwt;

namespace StudyJet.API.Utilities
{
    public class CustomClaimTypes
    {
        public const string FullName = "fullName";
        public const string UserName = JwtRegisteredClaimNames.Sub;
        public const string UserId = "userId";
        public const string Email = JwtRegisteredClaimNames.Email;
        public const string Jti = JwtRegisteredClaimNames.Jti;
        public const string Iat = JwtRegisteredClaimNames.Iat;
        public const string Role = "role";


    }
}
