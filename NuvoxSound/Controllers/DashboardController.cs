using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NuvoxSound.Data;
using System;
using System.Linq;

namespace NuvoxSound.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UsuarioData _usuarioData;
        private readonly ProductoData _productoData;
        private readonly VentaData _ventaData;

        public DashboardController(UsuarioData usuarioData, ProductoData productoData, VentaData ventaData)
        {
            _usuarioData = usuarioData;
            _productoData = productoData;
            _ventaData = ventaData;
        }

        // =======================================================
        // 1. VISTA PRINCIPAL (CARGA DE MÉTRICAS EN EL HEADER)
        // =======================================================
        public IActionResult Index()
        {
            try
            {
                
                var resumen = _ventaData.ObtenerResumenNativo();

                return View(resumen);
            }
            catch (Exception)
            {
                // Si la BD falla, mandamos el modelo en 0 para que la vista no se caiga
                return View(new NuvoxSound.Entities.DashboardResumen { TotalUsuarios = 0, TotalProductos = 0, TotalVentas = 0 });
            }
        }

        // =======================================================
        // 2. ENDPOINTS GET (LECTURA DE DATOS PARA TABLAS)
        // =======================================================

        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            try
            {
                var listaUsuarios = _usuarioData.ListarUsuarios();
                return Json(new { success = true, data = listaUsuarios });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObtenerProductos()
        {
            try
            {
                var listaProductos = _productoData.ListarProductos();
                return Json(new { success = true, data = listaProductos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObtenerReporteVentas(string inicio, string fin)
        {
            try
            {
                var listaVentas = _ventaData.ObtenerReporte(inicio, fin);
                return Json(new { success = true, data = listaVentas });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObtenerCupones()
        {
            try
            {
                return Json(new { success = true, data = new string[] { } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ObtenerResumenCategorias()
        {
            try
            {
                return Json(new { success = true, data = _productoData.ObtenerResumenCategorias() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =======================================================
        // 3. ENDPOINTS POST (ESCRITURA, EDICIÓN Y ELIMINACIÓN)
        // =======================================================

        [HttpPost]
        public IActionResult CambiarEstadoUsuario(int id, bool estado)
        {
            try
            {
                bool resultado = _usuarioData.ActualizarEstado(id, estado);

                if (resultado)
                    return Json(new { success = true, message = "Estado actualizado correctamente." });
                else
                    return Json(new { success = false, message = "No se pudo actualizar el estado en la BD." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GuardarProducto(NuvoxSound.Entities.Producto obj)
        {
            try
            {
                bool resultado = _productoData.Guardar(obj);

                if (resultado)
                    return Json(new { success = true, message = "Producto guardado con éxito." });
                else
                    return Json(new { success = false, message = "No se pudo guardar el producto en la BD." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EliminarProducto(int id)
        {
            try
            {
                bool resultado = _productoData.Eliminar(id);

                if (resultado)
                    return Json(new { success = true, message = "Producto eliminado de la tienda." });
                else
                    return Json(new { success = false, message = "No se pudo eliminar el producto en la BD." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GuardarCupon()
        {
            try
            {
                return Json(new { success = true, message = "Cupón activado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EliminarCupon(int id)
        {
            try
            {
                return Json(new { success = true, message = "Cupón eliminado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}