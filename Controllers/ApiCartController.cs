using ASP_P22.Data;
using ASP_P22.Middleware.Auth;
using ASP_P22.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_P22.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class ApiCartController(DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        RestResponseModel restResponseModel => new()
            {
                CacheLifetime = 0,
                Description = "User's cart API: Active cart",
                Manipulations = new()
                {
                    Read = "/api/cart",
                },
                Meta = new() {
                    { "locale", "uk" },
                    { "dataType", "object" }
                },
            };

        [HttpGet]
        public RestResponseModel DoGet()
        {
            var res = restResponseModel;

            String? userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if(userId == null)
            {
                res.Status = new() { Code=401, IsSuccess=false, Phrase="Unauthorized" };
                res.Data = HttpContext.Items[nameof(AuthTokenMiddleware)];
                return res;
            }
            res.Data = userId;
            return res;
        }

        [HttpPost]
        public RestResponseModel DoPost([FromQuery]String productId)
        {
            var res = restResponseModel;
            String? userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId == null)
            {
                res.Status = new() { Code = 401, IsSuccess = false, Phrase = "Unauthorized" };
                res.Data = HttpContext.Items[nameof(AuthTokenMiddleware)];
                return res;
            }
            try
            {
                _dataAccessor.AddToCart(userId, productId);
                res.Data = "Created";
            }
            catch (Exception ex) 
            {
                res.Status = new() { Code = 400, IsSuccess = false, Phrase = "Bad request" };
                res.Data = ex.Message;
            }
            return res;
        }
    }
}
