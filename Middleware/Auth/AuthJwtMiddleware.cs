using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
    public class AuthJwtMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, IHashService hashService, IConfiguration configuration, DataContext dataContext)
        {
            String authMessage = "";
            String header = "", payload = "", signature = "";
            String authHeader = context.Request.Headers.Authorization.ToString();
            if (String.IsNullOrEmpty(authHeader))
            {
                authMessage = "Missing 'Authorization' header";
            }
            else
            {
                String authScheme = "Bearer ";
                if (!authHeader.StartsWith(authScheme))
                {
                    authMessage = $"Authorization scheme error: '{authScheme}' required";
                }
                else
                {
                    String jwt = authHeader[authScheme.Length..];
                    String[] parts = jwt.Split('.');
                    if(parts.Length != 3)
                    {
                        authMessage = "Invalid JWT format (splitting error)";
                    }
                    else
                    {
                        header = parts[0];
                        payload = parts[1];
                        signature = parts[2];
                    }
                }
            }

            if(authMessage == "")
            {
                // Перевіряємо підпис
                String secret = configuration.GetSection("Jwt").GetSection("Secret").Value!;
                String signatureRight = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(
                        hashService.Digest(secret + $"{header}.{payload}")));
                if(signature != signatureRight)
                {
                    authMessage = "JWT Signature error";
                }
                else
                {
                    JwtToken jwtToken = JsonSerializer.Deserialize<JwtToken>(
                        System.Text.Encoding.UTF8.GetString(
                            Convert.FromBase64String(payload) ) )! ;

                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim( ClaimTypes.Sid, jwtToken.Sub.ToString()! ),
                                new Claim( ClaimTypes.Name, jwtToken.Name ),
                                new Claim( ClaimTypes.Email, jwtToken.Email ),
                                new Claim( ClaimTypes.NameIdentifier, jwtToken.Slug ),
                            ],
                            nameof(AuthJwtMiddleware)
                        )
                    );
                    Console.WriteLine(jwtToken.Name);
                }
            }
            Console.WriteLine(authMessage);
            context.Items.Add(nameof(AuthJwtMiddleware), authMessage);
            await _next(context);
        }
    }
    public static class AuthJwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtToken(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthJwtMiddleware>();
        }
    }
}
