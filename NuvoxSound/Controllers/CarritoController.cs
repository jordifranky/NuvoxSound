using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NuvoxSound.Entities;
using NuvoxSound.Data;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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

        // ========================================================
        // MANEJO DE MEMORIA (DICCIONARIO SEGURO)
        // ========================================================
        private Dictionary<int, int> ObtenerIdsSesion()
        {
            var data = HttpContext.Session.GetString("CarritoIDs");
            if (string.IsNullOrEmpty(data)) return new Dictionary<int, int>();
            return JsonSerializer.Deserialize<Dictionary<int, int>>(data) ?? new Dictionary<int, int>();
        }

        private void GuardarIdsSesion(Dictionary<int, int> carritoIds)
        {
            HttpContext.Session.SetString("CarritoIDs", JsonSerializer.Serialize(carritoIds));
        }

        // =============================================
        // VER EL CARRITO (CON LIMPIEZA DE FANTASMAS)
        // =============================================
        [HttpGet]
        public IActionResult Index()
        {
            var cantidades = ObtenerIdsSesion();
            var carritoReal = new List<ItemCarrito>();
            var todosLosProductos = _productoData.ListarProductos();

            bool huboFantasmas = false;

            // Cruzamos la memoria con tu Base de Datos SQL
            foreach (var kvp in cantidades)
            {
                var prod = todosLosProductos.FirstOrDefault(p => p.IdPro == kvp.Key);
                if (prod != null)
                {
                    carritoReal.Add(new ItemCarrito { Producto = prod, Cantidad = kvp.Value });
                }
                else
                {
                    huboFantasmas = true; // Detectamos un ID erróneo (Ej: ID 0)
                }
            }

            // Si detectó errores viejos, reescribe la memoria solo con los datos reales
            if (huboFantasmas)
            {
                var memoriaLimpia = carritoReal.ToDictionary(x => x.Producto.IdPro, x => x.Cantidad);
                GuardarIdsSesion(memoriaLimpia);
            }

            return View(carritoReal);
        }

        // =============================================
        // AGREGAR PRODUCTO (CON ESCUDO PROTECTOR)
        // =============================================
        [HttpPost]
        public IActionResult Agregar(int idProducto) // <--- ¡AQUÍ ESTÁ EL CAMBIO CLAVE!
        {
            // ESCUDO: Verificamos que el producto SÍ exista en la BD antes de guardarlo
            var productoReal = _productoData.ListarProductos().FirstOrDefault(p => p.IdPro == idProducto);

            if (productoReal == null)
            {
                // Si el ID vino mal desde el HTML (como un 0), rebotamos la acción sin guardar nada
                return RedirectToAction("Index", "Home");
            }

            var cantidades = ObtenerIdsSesion();

            if (cantidades.ContainsKey(productoReal.IdPro))
            {
                cantidades[productoReal.IdPro]++;
            }
            else
            {
                cantidades[productoReal.IdPro] = 1;
            }

            GuardarIdsSesion(cantidades);
            return RedirectToAction("Index");
        }

        // =============================================
        // ELIMINAR Y PAGAR
        // =============================================
        [HttpPost]
        public IActionResult Eliminar(int idProducto) // <--- ¡AQUÍ ESTÁ EL OTRO CAMBIO CLAVE!
        {
            var cantidades = ObtenerIdsSesion();
            if (cantidades.ContainsKey(idProducto))
            {
                cantidades.Remove(idProducto);
                GuardarIdsSesion(cantidades);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
       
        public IActionResult ProcesarPago()
        {
            var cantidades = ObtenerIdsSesion();
            if (!cantidades.Any()) return RedirectToAction("Index");

            if (!User.Identity!.IsAuthenticated)
            {
                TempData["SweetError"] = "Debes iniciar sesión para completar tu pedido.";
                return RedirectToAction("Login", "Auth");
            }

            var carritoReal = new List<ItemCarrito>();
            var todos = _productoData.ListarProductos();
            foreach (var kvp in cantidades)
            {
                var p = todos.FirstOrDefault(x => x.IdPro == kvp.Key);
                if (p != null) carritoReal.Add(new ItemCarrito { Producto = p, Cantidad = kvp.Value });
            }

            // ===================================================================
            //REPARACIÓN MÁSTER: EXTRACCIÓN ROBUSTA DEL ID DE USUARIO ROBUSTA
            // ===================================================================
            // Buscamos el ID del usuario intentando por múltiples vías para asegurar que no falle
            var claimId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // ESCUDO DE SEGURIDAD: Si por algún motivo extremo la sesión se corrompe y viene nulo,
            // frenamos la transacción en seco en lugar de asignarle el ID 1 por defecto.
            if (string.IsNullOrEmpty(claimId))
            {
                ViewBag.Error = "Error de autenticación: No se pudo verificar tu ID de usuario. Por favor, cierra sesión e ingresa nuevamente.";
                return View("ErrorCompra");
            }

            // Convertimos de forma segura ya que sabemos que el string tiene el ID real (Ej: "3")
            int idUsuarioReal = Convert.ToInt32(claimId);

            // Registramos la venta con tu ID verdadero
            bool exito = _ventaData.RegistrarVentaTransaccional(idUsuarioReal, carritoReal);

            if (exito)
            {
                HttpContext.Session.Remove("CarritoIDs");
                return RedirectToAction("Confirmacion");
            }
            else
            {
                ViewBag.Error = "Transacción rechazada en el servidor.";
                return View("ErrorCompra");
            }
        }
        // =============================================
        // PANTALLA DE CHECKOUT (PASARELA DE PAGO)
        // =============================================
        [HttpGet]
        public IActionResult Checkout()
        {
            var cantidades = ObtenerIdsSesion();
            if (!cantidades.Any()) return RedirectToAction("Index"); 

            var carritoReal = new List<ItemCarrito>();
            var todos = _productoData.ListarProductos();
            foreach (var kvp in cantidades)
            {
                var p = todos.FirstOrDefault(x => x.IdPro == kvp.Key);
                if (p != null) carritoReal.Add(new ItemCarrito { Producto = p, Cantidad = kvp.Value });
            }

            return View(carritoReal);
        }

        [HttpGet]
        public IActionResult Confirmacion() { return View(); }

        [HttpGet]
        public IActionResult ErrorCompra() { return View(); }
    }
}