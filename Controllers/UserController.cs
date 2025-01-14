using ASP_P22.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_P22.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                ViewData["formModel"] = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );


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
    }
}
/* Зробити сторінку з формою, яка дозволяє ввести коментар та оцінку (1-5)
 * Реалізувати передачу даних з цієї форми, вивести їх на сторінці.
 */
