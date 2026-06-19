using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NuvoxSound.Data;
using NuvoxSound.Entities;
using System.Linq;

namespace NuvoxSound.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly VentaData _ventaData;

        public ClienteController(VentaData ventaData)
        {
            _ventaData = ventaData;
        }

        // =======================================================
        // 1. CARGA PRINCIPAL DEL PANEL (Librería + Historial SPA + Perfil)
        // =======================================================
        [HttpGet]
        public IActionResult Index()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId)) return RedirectToAction("Login", "Auth");

            int idUsuario = int.Parse(claimId);

            // Cargamos la librería
            var misProductos = _ventaData.ListarMiLibreria(idUsuario);

            // Cargamos las ventas para la misma vista
            var misVentas = _ventaData.ListarMisCompras(idUsuario);
            ViewBag.MisCompras = misVentas;
            //Cargamos los datos del Perfil desde la sesión de seguridad
            ViewBag.NombreUsuario = User.Identity?.Name;
            ViewBag.CorreoUsuario = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            // DATOS AMPLIADOS 
            ViewBag.Alias = User.Claims.FirstOrDefault(c => c.Type == "Alias")?.Value ?? "";
            ViewBag.Telefono = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value ?? "";
            ViewBag.Pais = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Country)?.Value ?? "";

            return View(misProductos);
        }

        // =======================================================
        // 2. DESCARGAR PRODUCTO (Con límite de 10 descargas)
        // =======================================================
        [HttpGet]
        public IActionResult DescargarProducto(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId)) return RedirectToAction("Login", "Auth");

            int idUsuario = int.Parse(claimId);

            bool puedeDescargar = _ventaData.RegistrarDescarga(idUsuario, id);

            if (!puedeDescargar)
            {
                TempData["SweetError"] = "Has superado el límite de 10 descargas para este producto.";
                return RedirectToAction("Index");
            }

            try
            {
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes("Gracias por comprar en Nuvox Records. Tu descarga se ha realizado con éxito.");
                string fileName = $"Nuvox_Pack_{id}.txt";
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index");
            }
        }

        // =======================================================
        // 3. GENERAR BOLETA FÍSICA (Diseño para PDF)
        // =======================================================
        [HttpGet]
        public IActionResult VerBoleta(int idVenta)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId)) return RedirectToAction("Login", "Auth");

            int idUsuario = int.Parse(claimId);

            var boleta = _ventaData.ObtenerBoletaFisica(idVenta, idUsuario);

            if (boleta == null)
            {
                TempData["SweetError"] = "Boleta no encontrada o no tienes permisos para verla.";
                return RedirectToAction("Index");
            }

            return View(boleta);
        }
    }
}