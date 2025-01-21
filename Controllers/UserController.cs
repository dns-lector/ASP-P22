using ASP_P22.Data;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController(
        DataContext dataContext, 
        IKdfService kdfService,
        IRandomService randomService,
        IConfiguration configuration) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IRandomService _randomService = randomService;
        private readonly IConfiguration _configuration = configuration;

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
                        Email = formModel.UserEmail
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

        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(),
                kdf.GetSection("DkLength").Get<uint>()
            );
        }

        public IActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            // return View("Index");  // Украй не рекомендується переходити на 
            // представлення після прийняття даних форми
            
            HttpContext.Session.SetString(
                "formModel",
                JsonSerializer.Serialize(formModel)
            );
            return RedirectToAction("Index");
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
