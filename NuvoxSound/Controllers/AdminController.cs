using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace NuvoxSound.Controllers
{
    [Authorize(Roles = "Administrador")] 
    public class AdminController : Controller
    {
        
        [HttpGet]
        public IActionResult Usuarios()
        {
            
            return View();
        }
    }
}