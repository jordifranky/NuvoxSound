using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Entities;
using NuvoxSound.Data;
using NuvoxSound.Helpers;

namespace NuvoxSound.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ProductoData _productoData;
        private readonly VentaData _ventaData;

        public CarritoController(ProductoData productoData, VentaData ventaData)
        {
            _productoData = productoData;
            _ventaData = ventaData;
        }

        // Acción para ver la pantalla del carrito
        public IActionResult Index()
        {
            // Leemos el carrito actual desde la memoria de la sesión
            var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

            // Le enviamos la lista a la vista
            return View(carrito);
        }

        // 2. Acción para agregar un producto al carrito (Tu código intacto)
        public IActionResult Agregar(int id)
        {
            var producto = _productoData.ListarProductos().FirstOrDefault(p => p.IdPro == id);

            if (producto != null)
            {
                var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito") ?? new List<ItemCarrito>();

                var itemExistente = carrito.FirstOrDefault(p => p.Producto?.IdPro == id);

                if (itemExistente != null)
                {
                    itemExistente.Cantidad++;
                }
                else
                {
                    carrito.Add(new ItemCarrito { Producto = producto, Cantidad = 1 });
                }

                HttpContext.Session.SetObjectAsJson("Carrito", carrito);
            }

            return RedirectToAction("Index", "Producto");
        }
        //Acción para procesar la compra
        public IActionResult ProcesarPago()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<ItemCarrito>>("Carrito");
            //Si el carrito está vacío, redirigimos al usuario a la pantalla del carrito
            if (carrito == null || !carrito.Any())
                return RedirectToAction("Index");

            // Simulamos que el usuario logueado es el ID 1          
            int idUsuarioSimulado = 1;

            //llamamos el procedmiento almacenado
            bool exito = _ventaData.RegistrarVentaTransaccional(idUsuarioSimulado, carrito);

            if (exito)
            {
                //Si la BD guardo todo bien borramos la sesión del carrito
                HttpContext.Session.Remove("Carrito");
                // Redirigimos a una pagina de éxito
                return View("Confirmacion");

            }
            else
            {
                // Si hubo un error redirigimos a una pagina de error
                ViewBag.Error = "Ocurrió un error. Tu pago no fue procesado.";
                return View("ErrorCompra");
            }


        }
    }
}