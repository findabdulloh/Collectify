using Collectify.Data.IRepositories;
using Collectify.Data.Repositories;
using Collectify.Service.IServices;
using Collectify.Service.IServices.IItems;
using Collectify.Service.IServices.IUsers;
using Collectify.Service.Services;
using Collectify.Service.Services.Items;
using Collectify.Service.Services.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Collectify.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IItemCommentLikeService, ItemCommentLikeService>();
            services.AddScoped<IItemCommentService, ItemCommentService>();
            services.AddScoped<IItemLikeService, ItemLikeService>();
            services.AddScoped<IItemFieldService, ItemFieldService>();
            services.AddScoped<IItemService, ItemService>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICloudService, CloudService>();
            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<IPhotoService, PhotoService>();
        }

        public static void AddJwtService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}