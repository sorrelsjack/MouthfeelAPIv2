using Microsoft.IdentityModel.Tokens;
using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Helpers
{
    public class TokenHelper
    {
        public const string Issuer = "https://mouthfeel-api.azurewebsites.net";
        public const string Audience = "https://mouthfeel-api.azurewebsites.net";

        public static string GenerateSecureSecret()
        {
            var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.Key);
        }

        public static string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(MouthfeelApiConfiguration.JwtSigningSecret);

            var claimsIdentity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            });

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = Issuer,
                Audience = Audience,
                Expires = DateTime.Now.AddDays(2),
                SigningCredentials = signingCredentials,

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
