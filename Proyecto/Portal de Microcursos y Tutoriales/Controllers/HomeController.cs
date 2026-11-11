using Microsoft.AspNetCore.Mvc;
using Portal_de_Microcursos_y_Tutoriales.Models;
using System.Diagnostics;

namespace Portal_de_Microcursos_y_Tutoriales.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
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
