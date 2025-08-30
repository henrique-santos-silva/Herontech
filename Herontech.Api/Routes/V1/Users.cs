using Herontech.Application.Security;
using Herontech.Domain;
using Herontech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Herontech.Api.Routes.V1;

public static class Users
{
    public record RegisterRequest(string Email, string Password);
    public record CreateUserRequest(string Email, string Password, bool IsActive = true);
    public record UserDto(Guid Id, string Email, bool IsActive, bool IsEmailConfirmed);

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateUserHandler) /*.RequireAuthorization("Admin")*/;
        app.MapPost("/register", RegisterHandler);
        app.MapPost("/register-first-sysadmin", RegisterFirstSysAdminHandler);
        
        return app;
    }

    private static async Task<IResult> RegisterHandler(
        [FromBody] RegisterRequest req,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest("Email e Password são obrigatórios.");

        string email = req.Email.Trim().ToLowerInvariant();

        bool exists = await db.Set<User>().AnyAsync(u => u.Email == email, ct);
        if (exists) return Results.Conflict("Usuário já existe.");

        (byte[] salt, byte[] hash) = PasswordHasher.HashPassword(req.Password);

        User u = new User
        {
            Email = email,
            IsEmailConfirmed = false,
            PassWordSalt = salt,
            PasswordHash = hash,
            IsActive = true
        };

        db.Set<User>().Add(u);
        await db.SaveChangesAsync(ct);

        UserDto dto = new UserDto(u.Id, u.Email, u.IsActive, u.IsEmailConfirmed);
        return Results.Created($"/users/{u.Id}", dto);
    }
    
    private static async Task<IResult> RegisterFirstSysAdminHandler(
        [FromBody] RegisterRequest req,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest("Email e Password são obrigatórios.");

        string email = req.Email.Trim().ToLowerInvariant();

        bool exists = await db.Set<User>().AnyAsync(cancellationToken: ct);
        if (exists) return Results.Conflict("Já Existem Usuários na Base");

        (byte[] salt, byte[] hash) = PasswordHasher.HashPassword(req.Password);

        User u = new User
        {
            Role = RoleEnum.SysAdmin,
            Email = email,
            IsEmailConfirmed = true,
            PassWordSalt = salt,
            PasswordHash = hash,
            IsActive = true
        };

        db.Set<User>().Add(u);
        await db.SaveChangesAsync(ct);

        UserDto dto = new UserDto(u.Id, u.Email, u.IsActive, u.IsEmailConfirmed);
        return Results.Created($"/users/{u.Id}", dto);
    }
    
    private static async Task<IResult> CreateUserHandler(
        [FromBody] CreateUserRequest req,
        [FromServices] AppDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest("UserName, Email e Password são obrigatórios.");

        string email = req.Email.Trim().ToLowerInvariant();

        bool exists = await db.Set<User>().AnyAsync(u => u.Email == email, ct);
        if (exists) return Results.Conflict("Usuário já existe.");

        (byte[] salt, byte[] hash) = PasswordHasher.HashPassword(req.Password);

        User u = new()
        {
            Email = email,
            IsEmailConfirmed = false,
            PassWordSalt = salt,
            PasswordHash = hash,
            IsActive = req.IsActive
        };

        db.Set<User>().Add(u);
        await db.SaveChangesAsync(ct);

        UserDto dto = new UserDto(u.Id,  u.Email, u.IsActive, u.IsEmailConfirmed);
        return Results.Created($"/users/{u.Id}", dto);
    }
}
