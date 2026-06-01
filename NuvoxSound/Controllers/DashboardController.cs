using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Business;

namespace NuvoxSound.Controllers
{
    // CANDADO DE SEGURIDAD ROBUSTO: Solo permite el acceso a usuarios con Rol "1"
    [Authorize(Roles = "1")]
    public class DashboardController : Controller
    {
        private readonly DashboardBusiness _dashboardBusiness;

        public DashboardController(DashboardBusiness dashboardBusiness)
        {
            _dashboardBusiness = dashboardBusiness;
        }

        public IActionResult Index()
        {
            var resumen = _dashboardBusiness.ObtenerResumen();
            return View(resumen);
        }
    }
}