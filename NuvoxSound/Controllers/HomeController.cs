using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using NuvoxSound.Data; // Referencia a tu capa de datos
using NuvoxSound.Entities; // Referencia a tus entidades

namespace NuvoxSound.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. Declaramos la variable de tu capa de datos
        private readonly ProductoData _productoData;

        // 2. Inyectamos la dependencia en el constructor junto con el logger
        public HomeController(ILogger<HomeController> logger, ProductoData productoData)
        {
            _logger = logger;
            _productoData = productoData;
        }

        public IActionResult Index()
        {
            // 3. Traemos la lista de la base de datos
            
            var listaProductos = _productoData.ListarProductos();

            
            return View(listaProductos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}