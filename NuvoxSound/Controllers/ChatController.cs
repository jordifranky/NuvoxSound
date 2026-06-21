using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NuvoxSound.Business;
using NuvoxSound.Data; 

namespace NuvoxSound.Controllers
{
    public class ChatController : Controller
    {
        private readonly WatsonService _watsonService;
        private readonly ProductoData _productoData; 

        
        public ChatController(WatsonService watsonService, ProductoData productoData)
        {
            _watsonService = watsonService;
            _productoData = productoData;
        }

        [HttpPost]
        public IActionResult EnviarMensaje(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje))
            {
                return Json(new { respuesta = "Por favor, escribe un mensaje válido." });
            }

            // Recuperamos el ID de sesión de Watson guardado en la sesión de ASP.NET
            string sessionId = HttpContext.Session.GetString("WatsonSessionId") ?? "";

            // Enviamos el mensaje a la API de IBM
            string respuestaIA = _watsonService.EnviarMensaje(mensaje, sessionId);

            // =======================================================
            // 3. LA TRAMPA: Si Watson nos devuelve la alerta secreta...
            // =======================================================
            if (respuestaIA == "[BUSCAR_EN_BD]")
            {
                // C# entra en acción, busca en SQL Server y reemplaza el mensaje
                respuestaIA = _productoData.BuscarProductoParaChatbot(mensaje);
            }

            // Retornamos la respuesta final (ya sea de Watson o de SQL) en formato JSON
            return Json(new { respuesta = respuestaIA });
        }
    }
}