using ASP_P22.Data;
using ASP_P22.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
    public class AuthTokenMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration, DataContext dataContext)
        {
            String authMessage = "";
            Guid tokenId = default;
            String authHeader = context.Request.Headers.Authorization.ToString();
            if(String.IsNullOrEmpty(authHeader))
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
                    String credentials = authHeader[authScheme.Length..];
                    try { tokenId = Guid.Parse(credentials); }
                    catch { authMessage = "Invalid credentials format. UUID expected"; }
                }                    
            }
            if(authMessage == "")
            {
                AuthToken? authToken = dataContext
                    .AuthTokens
                    .Include(t => t.UserAccess)
                    .ThenInclude(ua => ua.User)
                    .FirstOrDefault(t => t.Jti == tokenId);
                if(authToken == null)
                {
                    authMessage = "Access token rejected";                
                }
                else if(authToken.Exp <= DateTime.Now)
                {
                    authMessage = "Access token expired";
                }  // Nbf
                else
                {
                    int lifetime = configuration
                        .GetSection("AuthToken")
                        .GetSection("Lifetime")
                        .Get<int>();
                    authToken.Exp.AddSeconds(lifetime);
                    Task saveChangesTask = dataContext.SaveChangesAsync();
                    User user = authToken.UserAccess.User;
                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim( ClaimTypes.Sid, user.Id.ToString() ),
                                new Claim( ClaimTypes.Name, user.Name ),
                                new Claim( ClaimTypes.Email, user.Email ),
                                new Claim( ClaimTypes.NameIdentifier, user.Slug ),
                            ],
                            nameof(AuthTokenMiddleware)
                        )
                    );
                    await saveChangesTask;
                }
            }
            context.Items.Add(nameof(AuthTokenMiddleware), authMessage);
            await _next(context);
        }
    }

    public static class AuthTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthToken(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthTokenMiddleware>();
        }
    }
}
/* Д.З. Призначити різним користувачам, зареєстрованим у системі, різні ролі.
 * Обмежити доступ до форми створення нових продуктів та категорій
 *  тільки тим користувачам, які мають відповідні права (canCreate).
 * ** Створити засоби адміністрування ролей користувачів - 
 *      вивести користувачів та їх ролі, додати перелік вибору ролі для
 *      кожного з них.
 */
