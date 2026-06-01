using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Data;

namespace NuvoxSound.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ProductoData _productoData;

        // ASP.NET inyecta automáticamente tu ProductoData aquí
        public ProductoController(ProductoData productoData)
        {
            _productoData = productoData;
        }

        public IActionResult Index()
        {
            // Ejecutamos tu método y guardamos la lista
            var listaProductos = _productoData.ListarProductos();

            // Mandamos la lista a la vista
            return View(listaProductos);
        }
    }
}