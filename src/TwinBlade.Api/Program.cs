using TwinBlade.Api.Extensions;
using TwinBlade.Api.Hubs;
using TwinBlade.Application.Commands.Auth;
using TwinBlade.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGameOpenApi();

builder.Services.AddCognitoAuth(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(SignInCommand).Assembly));

builder.Services.AddInfrastructure(builder.Configuration);

// SignalR
builder.Services.AddSignalR();

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
