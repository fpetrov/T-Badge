using T_Badge;
using T_Badge.Common.Extensions;
using T_Badge.Endpoints;
using T_Badge.Infrastructure;
using T_Badge.Middlewares;
using T_Badge.Middlewares.Authentication;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.ApplyMigrations();
app.UseMiddleware<AuthenticationMiddleware>();

var api = app.MapGroup("/api");

api.MapGet("/", () => "T-Badge API is here!");

api
    .MapGroup("/users")
    .MapUserEndpoints();

api
    .MapGroup("/events")
    .MapEventEndpoints();

app.Run();