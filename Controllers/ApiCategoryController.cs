using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Models.Shop;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class ApiCategoryController(DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IStorageService _storageService = storageService;

        [HttpGet]
        public RestResponseModel CategoriesList() => new()
        {
            CacheLifetime = 86400,
            Description = "Product Category API: Categories List",
            Manipulations = new()
            {
                Read = "/api/category/{id}",
            },
            Meta = new() {
                { "locale", "uk" },
                { "dataType", "object" }
            },
            Data = _dataAccessor.CategoriesList(),
        };

        [HttpGet("{id}")]
        public RestResponseModel CategoryById(string id)
        {
            return new()
            {
                CacheLifetime = 86400,
                Description = "Product Category API: Category By Id",
                Meta = new() {
                    { "locale", "uk" },
                    { "dataType", "object" }
                },
                Data = _dataAccessor.CategoryById(id)
            };
        }
    }
}
/*
API Controller  vs  MVC Controller

MVC Controller
Один контролер обсуговує кілька адрес, зазвичай, одним методом запиту (GET)
GET /Shop/Category  | 
GET /Shop/Product   | ShopController
GET /Shop/Cart      | 
Повернення методів -- IActionResult, частіше за все - View


API Controller 
Має одну адресу, а відмінності задаються методами запиту
GET  /api/category  | 
POST /api/category  | ApiCategoryController
PUT  /api/category  | 
Повернення - об'єкти (мови) з подальшою (автоматичною) конверсією у JSON / text

REST - архітектурні обмеження
Uniform Interface - вимога дотримуватись єдиної форми (інтерфейсу) як у запитах,
так і у відповідях до сервера. Наприклад, дані про локалізацію (обрану мову)
завжди передаємо заголовком Accept-Language / або у складі самого запиту /en/service...
Але скрізь однаково.
Відповідь:
 з описом (self descriptive)
 з метаданими, у т.ч. про маніпуляцію ресурсом
 авторизація у кожному запиті / відповідь показує результат
 дані про можливість кешування
 + статус виконання


Практичні поради:
Сервери часто працюють у режимі "proxy" - один сервер ретранслює відповіді іншого.

   ASP     asp/api/data  Proxy  site/api/data
[firm server]   <--->  [Hosting]  <----> Users
   DB                    site
                404

За такої схеми краще вживати внутрішні статуси і коди, що передаються у тілі
відповідей, замість стандартних НТТР-засобів

НЕ гарно
           GET /x              GET /x
[firm] <------------> [site] <-------->
       400 x = null     ??     500 - не зміг завершити запит


Краще
           GET /x              GET /x
[firm] <------------> [site] <-------->
          200 OK        ??     200 OK
     {400 x = null}          {400 x = null}


CORS / CORP -- Cross Origin Resource  Sharing / Policy
Обмеження, пов'язані з запитами, що надсилаються з інших джерел, ніж сам сайт-оригінал.
Якщо відповідь сервера не містить певних заголовків, то згідно з CORP такі відповіді
мають блокуватись і не передаватись клієнту. Браузер виконує ці вимоги. 
Postman - ні

Заголовки:
Access-Control-Allow-Origin: дозволені джерела (або *)
Access-Control-Allow-Methods: дозволені методи 
        (GET автоматично дозволений, інші треба зазначати)
Access-Control-Allow-Headers: дозволені заголовки 
        (за замовчанням не дозволено нічого, у т.ч. Authorization)
 */
