using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting; // 🔥 VITAL: Para acceder a la carpeta wwwroot
using NuvoxSound.Data;
using System;
using System.IO; // 🔥 VITAL: Para guardar archivos reales en el disco
using System.Linq;
using System.Threading.Tasks;

namespace NuvoxSound.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly UsuarioData _usuarioData;
        private readonly ProductoData _productoData;
        private readonly VentaData _ventaData;
        private readonly IWebHostEnvironment _env; 

        // Agregamos IWebHostEnvironment al constructor
        public DashboardController(UsuarioData usuarioData, ProductoData productoData, VentaData ventaData, IWebHostEnvironment env)
        {
            _usuarioData = usuarioData;
            _productoData = productoData;
            _ventaData = ventaData;
            _env = env;
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
                return View(new NuvoxSound.Entities.DashboardResumen { TotalUsuarios = 0, TotalProductos = 0, TotalVentas = 0 });
            }
        }

        // =======================================================
        // 2. ENDPOINTS GET (LECTURA DE DATOS PARA TABLAS)
        // =======================================================

        [HttpGet]
        public IActionResult ObtenerUsuarios()
        {
            try { return Json(new { success = true, data = _usuarioData.ListarUsuarios() }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public IActionResult ObtenerProductos()
        {
            try { return Json(new { success = true, data = _productoData.ListarProductos() }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public IActionResult ObtenerReporteVentas(string inicio, string fin)
        {
            try { return Json(new { success = true, data = _ventaData.ObtenerReporte(inicio, fin) }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public IActionResult ObtenerResumenCategorias()
        {
            try { return Json(new { success = true, data = _productoData.ObtenerResumenCategorias() }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public IActionResult ObtenerArtistas()
        {
            try { return Json(new { success = true, data = _productoData.ObtenerArtistas() }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public IActionResult ObtenerCupones()
        {
            try { return Json(new { success = true, data = _productoData.ObtenerCupones() }); }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
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
                if (resultado) return Json(new { success = true, message = "Estado actualizado correctamente." });
                else return Json(new { success = false, message = "No se pudo actualizar el estado en la BD." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> GuardarProducto(
             int IdPro, string? NomPro, int IdCate, int IdArtista, decimal Precio,
             string? RutImag, string? Descripcion, bool Activo, string? EnlaceDescarga,
             IFormFile? ImagenPortada, IFormFile? DemoAudio, IFormFile? ArchivoZip)
        {
            try
            {
                string? rutaImagen = null;
                string? rutaAudio = null;
                string? rutaZip = null;

                // Capturamos la ruta principal de tu wwwroot
                string webRootPath = _env.WebRootPath;

                //  1. GUARDADO FÍSICO DE LA IMAGEN
                if (ImagenPortada != null && ImagenPortada.Length > 0)
                {
                    string folderPath = Path.Combine(webRootPath, "uploads", "img");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + "_" + ImagenPortada.FileName;
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await ImagenPortada.CopyToAsync(fileStream);
                    }
                    rutaImagen = "/uploads/img/" + fileName;
                }

                //2.GUARDADO FÍSICO DEL AUDIO
                if (DemoAudio != null && DemoAudio.Length > 0)
                {
                    string folderPath = Path.Combine(webRootPath, "uploads", "audio");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + "_" + DemoAudio.FileName;
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await DemoAudio.CopyToAsync(fileStream);
                    }
                    rutaAudio = "/uploads/audio/" + fileName;
                }

                //3. GUARDADO FÍSICO DEL ZIP/RAR
                if (ArchivoZip != null && ArchivoZip.Length > 0)
                {
                    string folderPath = Path.Combine(webRootPath, "uploads", "packs");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + "_" + ArchivoZip.FileName;
                    string fullPath = Path.Combine(folderPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await ArchivoZip.CopyToAsync(fileStream);
                    }
                    rutaZip = "/uploads/packs/" + fileName;
                }

                // Finalmente mandamos las rutas a la BD a través del Procedimiento Almacenado
                string mensajeResultado = _productoData.GuardarProductoSP(
                    IdPro, NomPro ?? "", IdCate, IdArtista, Precio, RutImag ?? "",
                    Descripcion ?? "", Activo, rutaImagen, rutaAudio, rutaZip, EnlaceDescarga);

                if (mensajeResultado == "OK")
                    return Json(new { success = true, message = "Producto guardado con éxito." });
                else
                    return Json(new { success = false, message = mensajeResultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error en C#: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EliminarProducto(int id)
        {
            try
            {
                bool resultado = _productoData.Eliminar(id);
                if (resultado) return Json(new { success = true, message = "Producto eliminado de la tienda." });
                else return Json(new { success = false, message = "No se pudo eliminar el producto en la BD." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult GuardarCupon(string Codigo, int Porcentaje, string FechaExpiracion)
        {
            try
            {
                bool resultado = _productoData.GuardarCupon(Codigo, Porcentaje, FechaExpiracion);
                if (resultado) return Json(new { success = true, message = "Cupón guardado con éxito." });
                return Json(new { success = false, message = "No se pudo guardar el cupón (posible código duplicado)." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult EliminarCupon(int id)
        {
            try
            {
                bool resultado = _productoData.EliminarCupon(id);
                if (resultado) return Json(new { success = true, message = "Cupón eliminado." });
                return Json(new { success = false, message = "Error al eliminar el cupón." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}