using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using T_Badge.Common.Extensions;
using T_Badge.Common.Policies;
using T_Badge.Contracts.Events.Requests;
using T_Badge.Infrastructure.QrGeneration;
using T_Badge.Models;
using T_Badge.Persistence;

namespace T_Badge.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this RouteGroupBuilder group)
    {
        // group.MapGet("/", GetEvents).RequireAuthorization(AdminPolicy.Key);
        group.MapGet("/", GetEvents).RequireAuthorization();
        group.MapGet("/{code:int}", GetEvent).RequireAuthorization();
        group.MapPost("/", CreateEvent).RequireAuthorization();
        group.MapGet("/qr/{code:int}", GenerateQrCode).RequireAuthorization();
        
        return group;
    }
    
    private static async Task<IResult> GetEvents(
        ApplicationContext db)
    {
        return Results.Ok(await db.Events.ToListAsync());
    }
    
    private static async Task<IResult> GetEvent(
        int code,
        ApplicationContext db)
    {
        return await db.Events.FirstOrDefaultAsync(t => t.Code == code)
            is { } result
                ? Results.Ok(result)
                : Results.NotFound();
    }
    
    private static async Task<IResult> CreateEvent(
        [FromBody] CreateEventRequest request,
        HttpContext context,
        ApplicationContext db)
    {
        var identity = context.GetIdentity();
        var user = await db.Users.FindAsync(identity.Id);
        
        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            Start = request.Start,
            End = request.End,
            Rating = 0,
            Author = user
        };
        
        db.Events.Add(eventEntity);
        await db.SaveChangesAsync();
        
        return Results.Ok();
    }
    
    private static IResult GenerateQrCode(
        int code,
        [FromServices] StringEncryptor encryptor)
    {
        var encryptedBytes = encryptor.Encrypt($"EventCode:{code}");
        var encrypted = Convert.ToBase64String(encryptedBytes);
        
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(encrypted, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(data);

        var codeBytes = qrCode.GetGraphic(25);

        return Results.File(codeBytes, "image/png");
    }
}