using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P22.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class ApiUserController(DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

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
            restResponseModel.Data = authToken.Jti;
            return restResponseModel;
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
