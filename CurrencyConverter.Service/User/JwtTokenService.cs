using CurrencyConverter.Contract.User;
using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Dto.User.Response;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Service.User;

public class JwtTokenService : ITokenService
{
    private readonly ConfigDto _config;

    public JwtTokenService(ConfigDto config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a JWT token for the given user name.
    /// This is a fake implementation and it just include the user name and role as a claim for demonstration purposes.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public ApiResponseDto<string> GenerateToken(UserDto user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.JwtConfig.Issuer,
            audience: _config.JwtConfig.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_config.JwtConfig.ExpirationInMinutes),
            signingCredentials: creds);

        return new ApiResponseDto<string>
        {
            Success = true,
            Message = "Token generated successfully",
            Data = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
