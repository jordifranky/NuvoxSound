using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Data;

namespace NuvoxSound.Controllers
{
    public class TiendaController : Controller
    {
        private readonly ProductoData _productoData;

        public TiendaController(ProductoData productoData)
        {
            _productoData = productoData;
        }

        // 1. RUTA PARA CATEGORÍAS 
        [HttpGet("Tienda/Categoria/{nombreCategoria}")]
        public IActionResult Categoria(string nombreCategoria)
        {
            ViewBag.TituloPantalla = nombreCategoria.ToUpper();
            ViewBag.Subtitulo = $"Explora nuestra colección premium de {nombreCategoria}";

            
            var productos = _productoData.ObtenerProductosPorCategoria(nombreCategoria);
            return View("Catalogo", productos);
        }

        // 2. RUTA PARA ARTISTAS 
        [HttpGet("Tienda/Artista/{idArtista}")]
        public IActionResult Artista(int idArtista)
        {
            ViewBag.TituloPantalla = "COLECCIÓN DEL ARTISTA";
            ViewBag.Subtitulo = "Tracks y herramientas de producción exclusivas";

            
            var productos = _productoData.ObtenerProductosPorArtista(idArtista);
            return View("Catalogo", productos);
        }

        // 3. RUTA PARA EL PRODUCTO INDIVIDUAL 
        [HttpGet("Tienda/Detalle/{idProducto}")]
        public IActionResult Detalle(int idProducto)
        {
            
            var producto = _productoData.ObtenerDetalleProducto(idProducto);
            return View(producto);
        }
    }
}