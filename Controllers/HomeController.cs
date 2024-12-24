using System.Diagnostics;
using System.Text;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using ASP_P22.Services.Random;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P22.Controllers
{
    public class HomeController : Controller
    {
        // � ����������� (�� � � ����� ��'�����) �������� �����������
        // ����� �����������. ������� �������� � � �������� ��� ����
        // ILogger<HomeController> _logger;
        private readonly ILogger<HomeController> _logger;
        private readonly IRandomService _randomService;
        private readonly IHashService _hashService;

        public HomeController(
            ILogger<HomeController> logger,
            IRandomService randomService,
            IHashService hashService)
        {
            _logger = logger;
            _randomService = randomService;
            _hashService = hashService;
        }

        public IActionResult Index()
        {
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
            // ViewData - ��'��� ��� �������� ����� �� ���������� �� �������������
            ViewData["_randomService"] = _randomService;
            ViewData["hash123"] = _hashService.Digest("123");
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
