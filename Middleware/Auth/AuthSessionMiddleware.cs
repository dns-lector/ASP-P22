﻿using ASP_P22.Data.Entities;
using System.Globalization;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
    public class AuthSessionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            /* В UserController.Authenticate() зберігається сесія:
             * HttpContext.Session.SetString(
             *     "authUser",
             *     JsonSerializer.Serialize(ua.User)
             * );
             * Тут задача перевірити її наявність та відтворити користувача
             */

            // Перевіряємо чи є параметр ?logout
            if(context.Request.Query.Keys.Contains("logout"))
            {
                // Вихід з авторизованого режиму
                context.Session.Remove("authUser");
                context.Items.Remove("authUser");
                context.Response.Redirect(context.Request.Path);
            }
            else if(context.Session.Keys.Contains("authUser"))
            {
                Data.Entities.User user = JsonSerializer
                    .Deserialize<Data.Entities.User>(
                        context.Session.GetString("authUser")!)!;

                context.Items["authUser"] = user;
            }
            await _next(context);
        }
    }

    public static class AuthSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthSession(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthSessionMiddleware>();
        }
    }
}
/* Утримання авторизованого стану через збереження
 * даних у сесії
 */
