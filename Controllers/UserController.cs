using ASP_P22.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var formModel = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );                
                var (validationStatus, errors) = ValidateUserSignUpFormModel(formModel);
                ViewData["formModel"] = formModel;
                ViewData["validationStatus"] = validationStatus;
                ViewData["errors"] = errors;
                HttpContext.Session.Remove("formModel");
            }
            return View();
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
