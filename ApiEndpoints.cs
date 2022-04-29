using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi;

public static class ApiEndpoints
{
    public static void RegisterItemApis(this WebApplication app)
    {
        app.MapGet("/items", [Authorize] async (ApiDbContext db) =>
        {
            return await db.Items.ToListAsync();
        });

        app.MapPost("/items", [Authorize] async (ApiDbContext db, Item item) =>
        {
            if (await db.Items.FirstOrDefaultAsync(x => x.Id == item.Id) != null)
            {
                return Results.BadRequest();
            }

            db.Items.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/Items/{item.Id}", item);
        });

        app.MapGet("/items/{id}", [Authorize] async (ApiDbContext db, int id) =>
        {
            var item = await db.Items.FirstOrDefaultAsync(x => x.Id == id);

            return item == null ? Results.NotFound() : Results.Ok(item);
        });

        app.MapPut("/items/{id}", [Authorize] async (ApiDbContext db, int id, Item item) =>
        {
            var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null)
            {
                return Results.BadRequest();
            }

            existItem.Title = item.Title;
            existItem.IsCompleted = item.IsCompleted;

            await db.SaveChangesAsync();
            return Results.Ok(item);
        });

        app.MapDelete("/items/{id}", [Authorize] async (ApiDbContext db, int id) =>
        {
            var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (existItem == null)
            {
                return Results.BadRequest();
            }

            db.Items.Remove(existItem);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    public static void RegisterUserApis(this WebApplication app, IConfiguration configuration)
    {
        app.MapPost("/accounts/login", [AllowAnonymous] (UserDto user) =>
        {
            if (user.Username == "admin" && user.Password == "123")
            {
                var secureKey = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);

                var issuer = configuration["Jwt:Issuer"];
                var audience = configuration["Jwt:Audience"];
                var securityKey = new SymmetricSecurityKey(secureKey);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

                var jwtTokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] {
                        new Claim("Id", "1"),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                        new Claim(JwtRegisteredClaimNames.Email, user.Username),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.Now.AddMinutes(5),
                    Audience = audience,
                    Issuer = issuer,
                    SigningCredentials = credentials
                };

                var token = jwtTokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = jwtTokenHandler.WriteToken(token);
                return Results.Ok(jwtToken);
            }
            return Results.Unauthorized();
        });
    }
}