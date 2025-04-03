using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_P22.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ApiUserController(DataAccessor dataAccessor, IConfiguration configuration, IHashService hashService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IHashService _hashService = hashService;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public RestResponseModel Authenticate()
        {
            RestResponseModel restResponseModel = new()
            {
                CacheLifetime = 86400,
                Description = "User API: Authenticate",
                Meta = new() {
                    { "locale", "uk" },
                    { "dataType", "object" }
                },
            };
            UserAccess? userAccess = null;
            try
            {
                userAccess = _dataAccessor.BasicAuthenticate();
            }
            catch (Exception e)
            {
                restResponseModel.Status.Code = 500;
                restResponseModel.Status.Phrase = "Internal Server Error";
                restResponseModel.Status.IsSuccess = false;
                restResponseModel.Description = e.Message;
                return restResponseModel;
            }
            if(userAccess == null)
            {
                restResponseModel.Status.Code = 401;
                restResponseModel.Status.Phrase = "Unauthorized";
                restResponseModel.Status.IsSuccess = false;
                restResponseModel.Description = "Authentication failed";
                return restResponseModel;
            }
            AuthToken authToken = _dataAccessor.CreateTokenForUserAccess(userAccess);
            // restResponseModel.Data = authToken.Jti;
            restResponseModel.Data = AuthTokenToJwt(authToken);
            return restResponseModel;
        }

        private String AuthTokenToJwt(AuthToken authToken)
        {
            String header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
            String header64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(header));
            String payload = JsonSerializer.Serialize(new JwtToken
            {
                Jti = authToken.Jti,
                Sub = authToken.Sub,
                Iat = authToken.Iat,
                Exp = authToken.Exp,
                Name = authToken.UserAccess.User.Name,
                Email = authToken.UserAccess.User.Email,
                Slug = authToken.UserAccess.User.Slug
            });
            String payload64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            String jwtData = $"{header64}.{payload64}";
            String secret = _configuration.GetSection("Jwt").GetSection("Secret").Value!;
            String signature = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    _hashService.Digest(secret + jwtData)));

            return $"{jwtData}.{signature}";
        }
    }
}
/* Токени авторизації.
 * Автентифікація - одержання токену (передача логіну/паролю, відповідь - токен)
 * Авторизація - передача токену у складі запитів до сервера як підтвердження
 *  попередньої автентифікації.
 *  
 * Стуруктура токену визначається проєктом.
 * Але є і стандарти на кшталт JWT (JSON Web Token).
 * 
 */

/* Д.З. Реалізувати перевірку форми автентифікації перед її надсиланням,
 * за наявності очевидних зауважень, повідомити користувача та не надсилати дані.
 * Продовжити впровадження АРІ акційних пропозицій та знижок.
 */
