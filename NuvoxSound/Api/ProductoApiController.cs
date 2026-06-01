using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Data;
using NuvoxSound.Security;

namespace NuvoxSound.Api
{
    // Esta ruta define cómo se accederá a tu API en el navegador (ej: tusitio.com/api/productoapi)
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ProductoApiController : ControllerBase
    {
        private readonly ProductoData _productoData;

        // Inyectamos tu capa de datos real conectada a SQL Server
        public ProductoApiController(ProductoData productoData)
        {
            _productoData = productoData;
        }

        // Endpoint GET para obtener todos los productos
        [HttpGet]
        public IActionResult ObtenerProductos()
        {
            try
            {
                var lista = _productoData.ListarProductos();

                if (lista == null || !lista.Any())
                {
                    return NotFound(new { mensaje = "No se encontraron Sample Packs." });
                }

                // El método Ok() devuelve los datos con un código HTTP 200 (Éxito) en formato JSON
                return Ok(lista);
            }
            catch (Exception ex)
            {
                // Manejo de errores profesional devolviendo un HTTP 500
                return StatusCode(500, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}