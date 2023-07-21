using Collectify.Service.Responses;
using Collectify.Data.IRepositories;
using Collectify.Domain.Entities.Users;
using Collectify.Service.IServices.IUsers;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Collectify.Service.DTOs.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Collectify.Service.Services.Users;

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration configuration;
    private readonly IConfiguration configurations;
    private readonly IRepository<User> userRepository;
    private readonly IAuthorizationService authorizationService;
    private readonly IRepository<UserToken> userVerificationTokenRepository;

    public AuthenticationService(IRepository<UserToken> userVerificationTokenRepository, IAuthorizationService authorizationService, IRepository<User> userRepository, IConfiguration configurations, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.configurations = configurations;
        this.userRepository = userRepository;
        this.authorizationService = authorizationService;
        this.userVerificationTokenRepository = userVerificationTokenRepository;
    }
    public async Task<Response<string>> AuthenticateAsync(UserLoginDto dto)
    {
        var user = await this.userRepository
            .SelectAsync(u => u.Email == dto.Email && u.Password == dto.Password);

        if (user is null)
            return new Response<string>
            {
                Code = 400,
                Message = "Password or email is incorrect."
            };

        if (user.IsBlocked)
            return new Response<string>
            {
                Code = 400,
                Message = "Your account has been blocked."
            };

        var token = GenerateToken(user);

        return new Response<string>
        {
            Result = token
        };
    }

    private string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("Id", user.Id.ToString()),
                new Claim("Password", user.Password)
            }),
            Audience = configuration["JWT:Audience"],
            Issuer = configuration["JWT:Issuer"],
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JWT:Expire"])),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<Response<bool>> CheckSentVerificationMailAsync()
    {
        var user = await this.authorizationService.GetUserAsync();

        if (user is null || user.Verified)
            return new Response<bool>
            {
                Code = 405,
                Message = "Authorization error"
            };

        var token = await this.userVerificationTokenRepository
            .SelectAsync(t => t.UserId == user.Id);

        if (token is null)
            return new Response<bool>
            {
                Code = 404,
                Message = "Not found"
            };

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<bool>> SendVerificationMail(long userId)
    {
        var user = await this.authorizationService.GetUserAsync();

        if (user is null || user.Verified)
            return new Response<bool>
            {
                Code = 405,
                Message = "Authorization error"
            };

        await this.userVerificationTokenRepository.DeleteAsync(t => t.UserId == userId);

        var token = Guid.NewGuid().ToString();

        var userVerificationToken = new UserToken()
        {
            Token = token,
            UserId = userId
        };

        await this.userVerificationTokenRepository.InsertAsync(userVerificationToken);

        await this.userVerificationTokenRepository.SaveAsync();

        // sending mail
        var myEmail = configurations["Email:Address"];
        var myPassword = configurations["Email:Password"];
        var verificationLink = "collectify.somee.com/account/verificate?token=" + token;
        var subject = "Verify Your Email - Collectify";
        var body = $"Dear {user.Name},\r\n\r\nThank you for signing up with Collectify. Please use the following verification link to complete your registration:\r\n\r\nVerification Link: {verificationLink}\r\n\r\nIf you did not sign up for an account on Collectify, please disregard this email.\r\n\r\nBest regards,\r\nCollectify Team";

        var smtpClient = new SmtpClient("smtp.gmail.com", 587);
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(myEmail, myPassword);
        smtpClient.EnableSsl = true;

        var message = new MailMessage();
        message.From = new MailAddress(myEmail);
        message.To.Add(user.Email);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        smtpClient.Send(message);

        return new Response<bool>
        {
            Result = true
        };
    }

    public async Task<Response<bool>> VerificateAsync(string token)
    {
        var user = await this.authorizationService.GetUserAsync();

        if (user is null || user.Verified)
            return new Response<bool>
            {
                Code = 405,
                Message = "Authorization error"
            };

        var tokenEntity = await this.userVerificationTokenRepository
            .SelectAsync(t => t.Token == token);

        if (tokenEntity is null 
            || tokenEntity.UserId != user.Id)
            return new Response<bool>
            {
                Code = 400,
                Message = "Try to recieve new verification code from us"
            };

        var userEntity = await this.userRepository.SelectAsync(u => u.Id == user.Id);

        userEntity.Verified = true;
        await this.userRepository.SaveAsync();

        return new Response<bool>
        {
            Result = true
        };
    }
}