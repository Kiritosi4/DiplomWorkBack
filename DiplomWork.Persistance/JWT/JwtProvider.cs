using DiplomWork.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DiplomWork.Persistance.JWT
{
    public class JwtProvider
    {
        readonly JwtOptions _options;

        public JwtProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string GetToken(User user)
        {
            var credits = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            Claim[] claims = [
                new ("UserId", user.Id.ToString()),
                new ("Id", Guid.NewGuid().ToString()
                )];

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                claims: claims,
                signingCredentials: credits,
                expires: DateTime.UtcNow.AddHours(_options.ExpiresHours));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

