using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Business;

namespace NuvoxSound.Controllers
{
    public class ChatController : Controller
    {
        private readonly WatsonService _watsonService;

        public ChatController(WatsonService watsonService)
        {
            _watsonService = watsonService;
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

            // Retornamos la respuesta en formato JSON para que JavaScript la procese
            return Json(new { respuesta = respuestaIA });
        }
    }
}