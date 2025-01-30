using System.Diagnostics;
using System.Text;
using ASP_P22.Data;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using ASP_P22.Services.Random;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P22.Controllers
{
    public class HomeController : Controller
    {
        // В контролерах (як і в інших об'єктах) інжекція створюється
        // через конструктор. Приклад зазвичай є в базовому коді щодо
        // ILogger<HomeController> _logger;
        private readonly ILogger<HomeController> _logger;
        private readonly IRandomService _randomService;
        private readonly IHashService _hashService;
        private readonly DataContext _dataContext;

        public HomeController(
            ILogger<HomeController> logger,
            IRandomService randomService,
            IHashService hashService,
            DataContext dataContext)
        {
            _logger = logger;
            _randomService = randomService;
            _hashService = hashService;
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            ViewData["path"] = Directory.GetCurrentDirectory();

            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }
        
        public IActionResult Razor()
        {
            return View();
        }
        
        public IActionResult IoC()
        {
            // ViewData - об'єкт для передачі даних від контролера до представлення
            ViewData["_randomService"] = _randomService;
            ViewData["hash123"] = _hashService.Digest("123");
            return View();
        }

        public IActionResult Db()
        {
            ViewData["db-info"] = $"Users: {_dataContext.Users.Count()}, Accesses: {_dataContext.UsersAccess.Count()}";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
