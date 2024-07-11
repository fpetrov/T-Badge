using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T_Badge.Common.Extensions;
using T_Badge.Common.Interfaces.Authentication;
using T_Badge.Contracts.Authentication.Requests;
using T_Badge.Contracts.Authentication.Responses;
using T_Badge.Infrastructure.QrGeneration;
using T_Badge.Models;
using T_Badge.Persistence;

namespace T_Badge.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetUsers).RequireAuthorization();
        group.MapGet("/{id:int}", GetUser).RequireAuthorization();
        group.MapPost("/login", SignIn);
        group.MapGet("/me", GetMe).RequireAuthorization();
        group.MapGet("/visit/{eventId:int}", VisitEvent).RequireAuthorization();
        group.MapGet("/visit/qr/{qrMetadata}", VisitEventWithQr).RequireAuthorization();
        group.MapPost("/register", SignUp);

        return group;
    }

    private static async Task<IResult> GetUsers(
        HttpContext context,
        [FromServices] ApplicationContext db)
    {
        return Results.Ok(await db.Users.ToListAsync());
    }

    private static async Task<IResult> GetUser(
        [FromRoute] int id,
        [FromServices] ApplicationContext db)
    {
        var user = await db.Users
            .Include(t => t.VisitedEvents)
            .FirstOrDefaultAsync(t => t.Id == id);

        user.Achievements.Clear();
        
        // Recalculate user achievements
        GetUserAchievements(user);

        await db.SaveChangesAsync();
        
        return user is not null
            ? Results.Ok(user)
            : Results.NotFound();
    }

    private static async Task<IResult> SignIn(
        [FromBody] SignInRequest request,
        [FromServices] ApplicationContext db,
        [FromServices] IPasswordVerifier passwordVerifier,
        [FromServices] IJwtTokenGenerator tokenGenerator)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null)
            return Results.NotFound();

        if (!passwordVerifier.Verify(request.Password, user.Password))
            return Results.Unauthorized();

        var jwtToken = tokenGenerator.GenerateToken(user);

        return Results.Ok(new AuthenticationResponse(
            user.Id,
            user.Username,
            jwtToken));
    }

    private static async Task<IResult> SignUp(
        [FromBody] SignUpRequest request,
        [FromServices] ApplicationContext db,
        [FromServices] IPasswordVerifier passwordVerifier,
        [FromServices] IJwtTokenGenerator tokenGenerator)
    {
        if (await db.Users.AnyAsync(x => x.Username == request.Username))
            return Results.BadRequest();

        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            Password = passwordVerifier.Hash(request.Password)
        };

        var createdUser = db.Users.Add(user).Entity;
        await db.SaveChangesAsync();

        var jwtToken = tokenGenerator.GenerateToken(createdUser);

        return Results.Ok(new AuthenticationResponse(
            createdUser.Id,
            createdUser.Username,
            jwtToken));
    }
    
    private static async Task<IResult> GetMe(
        HttpContext context,
        ApplicationContext db)
    {
        var identity = context.GetIdentity();

        return await GetUser(identity.Id, db);
    }
    
    private static async Task<IResult> VisitEvent(
        [FromRoute] int eventId,
        ApplicationContext db,
        HttpContext context)
    {
        var identity = context.GetIdentity();

        var user = await db.Users.FindAsync(identity.Id);
        var visitedEvent = await db.Events.FindAsync(eventId);
        
        if (visitedEvent is null)
            return Results.NotFound();

        user.VisitedEvents.Add(visitedEvent);

        await db.SaveChangesAsync();

        return Results.Ok();
    }
    
    private static async Task<IResult> VisitEventWithQr(
        [FromRoute] string qrMetadata,
        ApplicationContext db,
        HttpContext context,
        [FromServices] StringEncryptor encryptor)
    {
        var decryptedBytes = Convert.FromBase64String(qrMetadata);
        var decrypted = encryptor.Decrypt(decryptedBytes);
        var eventId = int.Parse(decrypted.Split(':')[1]);

        return await VisitEvent(eventId, db, context);
    }

    private static readonly List<Achievement> Achievements =
    [
        new()
        {
            Title = "Новичок",
            Description = "Посетить свое первое событие"
        },
        new()
        {
            Title = "Дьявольский рейтинг",
            Description = "Ваш рейтинг равен 6.66"
        },
        new()
        {
            Title = "Бывалый",
            Description = "Посетить 3 события"
        },
        new()
        {
            Title = "Tyler, The Creator",
            Description = "Организуйте свое первое мероприятие"
        },
        new()
        {
            Title = "Диванный эксперт",
            Description = "Посетить 5 событий"
        }
    ];

    private static void GetUserAchievements(User user)
    {
        if (user.VisitedEvents.Count > 0)
            user.Achievements.Add(Achievements[0]);

        if (user.VisitedEvents.Count >= 3)
            user.Achievements.Add(Achievements[2]);

        if (user.VisitedEvents.Count >= 5)
            user.Achievements.Add(Achievements[4]);

        if (user.CreatedEvents.Count > 0)
            user.Achievements.Add(Achievements[3]);
    }
}