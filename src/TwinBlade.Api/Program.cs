using TwinBlade.Api.Extensions;
using TwinBlade.Api.Hubs;
using TwinBlade.Api.Services;
using TwinBlade.Application.Abstractions.Realtime;
using TwinBlade.Application.Commands.Auth;
using TwinBlade.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load(Path.Combine(builder.Environment.ContentRootPath, "../../.env"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGameOpenApi();

builder.Services.AddCognitoAuth(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SignInCommand).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

// SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<IGameHubService, GameHubService>();

builder.Services.Configure<RouteOptions>(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseGameOpenApi();
}

app.UseMiddleware<TwinBlade.Api.Middleware.ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

app.Run();
