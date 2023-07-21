using Collectify.Data.IRepositories;
using Collectify.Data.Repositories;
using Collectify.Domain.Entities.Users;
using Collectify.Domain.Enums;
using Collectify.Service.IServices.IUsers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Collectify.Service.Services.Users;

public class AuthorizationService : IAuthorizationService
{
    private readonly IConfiguration configuration;
    private readonly IRepository<User> userRepository;
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthorizationService(IHttpContextAccessor httpContextAccessor, IRepository<User> userRepository, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.userRepository = userRepository;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<User> GetUserAsync()
    {
        string jwtToken = httpContextAccessor.HttpContext.Request.Cookies["token"];
        string secretKey = configuration["JWT:Key"];

        var jwtHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Set to true if you want to validate the issuer
            ValidateAudience = false, // Set to true if you want to validate the audience
            ValidateLifetime = true, // Set to true if you want to validate the token's expiration
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };

        if (jwtToken is null)
            return null;

        ClaimsPrincipal claimsPrincipal = jwtHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);

        var claims = claimsPrincipal.Claims;

        var userId = long.Parse(claims.FirstOrDefault(c => c.Type == "Id")?.Value);
        var userPassword = claims.FirstOrDefault(c => c.Type == "Password")?.Value;

        var user = await userRepository.SelectAsync(u => u.Id == userId && u.Password == userPassword);

        return user;
    }

    public async Task<User> AuthorizeAsync(UserRole[] roles = null)
    {
        var user = await GetUserAsync();

        if (user is null || !user.Verified || user.IsBlocked)
            return null;

        if (roles is not null)
            foreach (var role in roles)
                if (user.Role == role)
                    return user;

        return roles is null ? user : null;
    }
}