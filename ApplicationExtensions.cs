using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace TodoApi;

public static class ApplicationExtensions
{
    public static void AddMyData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:DefaultConnection"];
        services.AddDbContext<ApiDbContext>(options => options.UseSqlite(connectionString));
    }

    public static void AddMySwagger(this IServiceCollection services)
    {
        var securityScheme = new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JSON Web Token based security",
        };

        var securityReq = new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        var contactInfo = new OpenApiContact()
        {
            Name = "Cinnamon",
            Email = "info@CinnamonSoftware.com",
            Url = new Uri("https://CinnamonSoftware.com")
        };

        var license = new OpenApiLicense()
        {
            Name = "Free License",
        };

        var info = new OpenApiInfo()
        {
            Version = "V1",
            Title = "Todo List Api",
            Description = "Todo List Api",
            Contact = contactInfo,
            License = license
        };

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", info);
            options.AddSecurityDefinition("Bearer", securityScheme);
            options.AddSecurityRequirement(securityReq);
        });
    }

    public static void UseMySwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    public static void AddMyAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidAlgorithms = new string[] { SecurityAlgorithms.HmacSha512 },
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });

        services.AddAuthentication();
        services.AddAuthorization();
    }

    public static void UseMyAuthentication(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}