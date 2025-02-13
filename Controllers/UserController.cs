using ASP_P22.Data;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController(
        DataContext dataContext, 
        IKdfService kdfService,
        IRandomService randomService,
        ILogger<UserController> logger,
        IStorageService storageService,
        IConfiguration configuration) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IRandomService _randomService = randomService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<UserController> _logger = logger;
        private readonly IStorageService _storageService = storageService;

        public IActionResult Index()
        {
            UserSignUpPageModel pageModel = new();

            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var formModel = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                ); 
                pageModel.FormModel = formModel;               
                (pageModel.ValidationStatus, pageModel.Errors) = 
                    ValidateUserSignUpFormModel(formModel);


                // ViewData["formModel"] = formModel;
                // ViewData["validationStatus"] = validationStatus;
                // ViewData["errors"] = errors;
                if(pageModel.ValidationStatus ?? false)
                {
                    // Реєструємо у БД
                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel!.UserName,
                        Email = formModel.UserEmail,

                        Phone = formModel.UserPhone,
                        WorkPosition = formModel.UserPosition,
                        PhotoUrl = formModel.UserPhotoSavedName,

                        Slug = formModel.UserLogin,  // **ДЗ
                    };
                    String salt = _randomService.FileName();
                    var (iter, len) = KdfSettings();
                    Data.Entities.UserAccess ua = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Login = formModel.UserLogin,
                        Salt = salt,
                        Dk = _kdfService.Dk(formModel.Password1, salt, iter, len)
                    };
                    _dataContext.Users.Add(user);
                    _dataContext.UsersAccess.Add(ua);
                    _dataContext.SaveChanges();
                    pageModel.User = user;
                }
                HttpContext.Session.Remove("formModel");
            }
            
            return View(pageModel);
        }

        public ViewResult Cart()
        {
            return View();
        }

        public ViewResult Profile([FromRoute] String id)
        {
            UserProfilePageModel pageModel;
            var profileUser = _dataContext.Users.FirstOrDefault(u => u.Slug == id);
            if (profileUser == null)
            {
                pageModel = new() { IsFound = false };
            }
            else
            {
                pageModel = new()
                {
                    IsFound = true,
                    Name = profileUser.Name,
                    Email = profileUser.Email,
                    PhotoUrl = "/Storage/Item/" + profileUser.PhotoUrl,
                    Phone = profileUser.Phone ?? "--",
                    MostViewed = id,
                    Recent = "Razor",
                    Role = profileUser.WorkPosition ?? "--"
                };
                /* Name = HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Name)
                        ?.Value ?? String.Empty,*/
            }
            return View(pageModel);
        }

        [HttpGet]
        public JsonResult Authenticate()
        {
            String authHeader = Request.Headers.Authorization.ToString();  // "Basic dGVzdDoxMjM="
            if (String.IsNullOrEmpty(authHeader))
            {
                return AuthError("Authorization header required");
            }
            String authScheme = "Basic ";
            if( ! authHeader.StartsWith(authScheme))
            {
                return AuthError($"Authorization scheme error: '{authScheme}' required");
            }
            String credentials = authHeader[authScheme.Length..];  // "dGVzdDoxMjM="

            String authData = System.Text.Encoding.UTF8.GetString(
                Base64UrlTextEncoder.Decode(credentials));         // "test:123"

            String[] parts = authData.Split(':', 2);               // ["test", "123"]
            if(parts.Length != 2)
            {
                return AuthError("Authorization credentials malformed");
            }
            // login - parts[0], password - parts[1]
            var ua = _dataContext
                .UsersAccess
                .Include(ua => ua.User)
                .FirstOrDefault(ua => ua.Login == parts[0]);

            if(ua == null)
            {
                return AuthError("Authorization rejected");
            }
            var (iter, len) = KdfSettings();
            String dk1 = _kdfService.Dk(parts[1], ua.Salt, iter, len);
            if(dk1 != ua.Dk)
            {
                return AuthError("Authorization rejected.");
            }
            // return Json(ua.User);
            HttpContext.Session.SetString(
                "authUser",
                JsonSerializer.Serialize(ua.User)
            );
            return Json("Ok");
        }

        private JsonResult AuthError(String message)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Json(message);
        }

        public RedirectToActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            // return View("Index");  // Украй не рекомендується переходити на 
            // представлення після прийняття даних форми

            // Перевіряємо чи є у формі файл і зберігаємо його
            // Оскільки сам файл не серіалізується, у моделі зберігаємо
            //  ім'я (URL) з яким він збережений

            if(formModel.UserPhoto != null && formModel.UserPhoto.Length != 0)
            {
                _logger.LogInformation("File uploaded {name}", formModel.UserPhoto.FileName);

                formModel.UserPhotoSavedName = _storageService.Save(formModel.UserPhoto);
            }
            
            HttpContext.Session.SetString(
                "formModel",
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction("Index");
        }


        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }
        
        private (bool, Dictionary<String,String>) ValidateUserSignUpFormModel(UserSignUpFormModel? formModel)
        {
            bool status = true;
            Dictionary<String, String> errors = [];

            if(formModel == null)
            { 
                status = false;
                errors["ModelState"] = "Модель не передано";
                return (status, errors);
            }

            if(String.IsNullOrEmpty(formModel.UserName))
            {
                status = false;
                errors["UserName"] = "Ім'я не може бути порожнім";
            }
            else if( ! Regex.IsMatch(formModel.UserName, "^[A-ZА-Я].*"))
            {
                status = false;
                errors["UserName"] = "Ім'я має починатись з великої літери";
            }

            if (String.IsNullOrEmpty(formModel.UserEmail))
            {
                status = false;
                errors["UserEmail"] = "Email не може бути порожнім";
            }
            else if (!Regex.IsMatch(formModel.UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                status = false;
                errors["UserEmail"] = "Email не відповідає шаблону";
            }

            if (String.IsNullOrEmpty(formModel.UserLogin))
            {
                status = false;
                errors["UserLogin"] = "Логін не може бути порожнім";
            }
            else if (formModel.UserLogin.Contains(':'))
            {
                status = false;
                errors["UserLogin"] = "Логін не може містити символ ':'";
            }
            else if (_dataContext
                .UsersAccess
                .Count(ua => ua.Login == formModel.UserLogin) > 0)
            {
                status = false;
                errors["UserLogin"] = "Логін вже використовується";
            }

            /* Д.З. Завершити валідацію даних від форми реєстрації користувача
             * Пароль: повинен містити літеру, цифру, спец-символ (дозволяється доповнити)
             * Повтор паролю: має збігатись з паролем
             * !! при відображенні помилок паролі не прийнято відновлювати у полях
             */

                return (status, errors);
        }
    }
}
/* Задача: утримання авторизації
 * 
 * Middleware: архітектура побудови ПЗ за якої програмні запити проходять
 * ланцюг однотипних обробників, кожен з яких може або передати роботу далі,
 * або припинити її. Нові обробникі можна вставляти у довільне місце
 * ланцюга (всередину, middle), через що і сформована назва.
 * https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
 * посилання демонструє рисунки з "прямим" та "зворотним" ходом і
 * передачею управління методом "next()"
 * https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write
 * посилання демонструє зразок написання обробників (класів) Middleware
 * 
 */

/* HTTP
 * Обмін пакетами, є два типи пакетів - запит та відповідь
 * 
 * Request
  
POST /User/SignUp  HTTP/1.1                          | частина 1 - перший рядок
Host: localhost                                      | | 
Content-Type: application/x-www-form-urlencoded      | | частина 2 - заголовки
Content-Length: 52                                   | | 
Connection: close                                    | | 
                                                     | роздільник - порожній рядок
UserName=User&UserEmail=user@i.ua&UserLogin=user123  | частина 3 - тіло


Перший рядок
POST              | Метод запиту - перше слово (до пробілу) GET POST PUT DELETE PATCH OPTIONS TRACE HEAD CONNECT
/User/SignUp      | Path - частина URL
HTTP/1.1          | Протокол

Розрив (завершення) рядка - обов'язково \r\n

Заголовки -- ключ: значення; атрибути=значення
Content-Type: text/html; charset=utf-8 \r\n

Тіло - довільна послідовність до кінця пакету. За наявності тіла необхідним є
заголовок Content-Type, який визначає природу тіла



Response

HTTP/1.1 302 Found
Location: /User/Index
Connection: keep-alive     

Тіло опціональне - може бути / може не бути


302 - Status Code
Found - Reason Phrase

 * 
 */

/* Робота з файлами. Uploading, Downloading
 * 
 * 
 *          Uploading
 * Client   --------->  Server
 *          <---------
 *          Downloading
 */

/* Д.З. Модифікувати форму реєстрації користувача. Запропонувати 
 * вибір: використовувати в якості Slug свого логіну або ввести
 * його окремо (в такому разі додати валідацію поля на унікальність)
 * ** Зробити генератор Slug з імені (транслітерація + заміна пробілів)
 */
