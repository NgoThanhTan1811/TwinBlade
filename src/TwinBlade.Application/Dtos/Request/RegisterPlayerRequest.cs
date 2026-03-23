namespace TwinBlade.Application.Dtos.Request;

public sealed record RegisterPlayerRequest(string Email, string Password, string Username);
