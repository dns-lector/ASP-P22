using ASP_P22.Data.Entities;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
    public class AuthTokenMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // if () {
            //     context.User = new ClaimsPrincipal(
            //         new ClaimsIdentity(
            //             [
            //                 new Claim( ClaimTypes.Sid, user.Id.ToString() ),
            //                 new Claim( ClaimTypes.Name, user.Name ),
            //                 new Claim( ClaimTypes.Email, user.Email ),
            //                 new Claim( ClaimTypes.NameIdentifier, user.Slug ),
            //             ],
            //             nameof(AuthSessionMiddleware)
            //         )
            //     );
            // }
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
