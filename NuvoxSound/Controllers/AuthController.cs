using Microsoft.AspNetCore.Mvc;
using NuvoxSound.Business;
using NuvoxSound.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NuvoxSound.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioBusiness _usuarioBusiness;

        public AuthController(UsuarioBusiness usuarioBusiness)
        {
            _usuarioBusiness = usuarioBusiness;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                // Si ya está logueado, lo mandamos a su lugar correcto según su rol
                if (User.IsInRole("1"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                // Si es cliente, va a su librería
                return RedirectToAction("Index", "Cliente");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            var usuario = _usuarioBusiness.Login(correo, password);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombres),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, "NuvoxCookie");

                await HttpContext.SignInAsync("NuvoxCookie", new ClaimsPrincipal(claimsIdentity));

                // REDIRECCIÓN INTELIGENTE FINAL
                if (usuario.IdRol == 1) // 1 = Administrador
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else // Cualquier otro rol = Cliente
                {
                    return RedirectToAction("Index", "Cliente");
                }
            }

            ViewBag.Error = "Correo o contraseña incorrectos";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("NuvoxCookie");
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario usuario, string password)
        {
            // Asignamos rol de Cliente por defecto en el registro público
            usuario.IdRol = 2;
            usuario.Activo = true;

            int Resultado = _usuarioBusiness.RegistroUsuario(usuario, password);
            if (Resultado > 0)
            {
                ViewBag.Message = "Registro exitoso, por favor inicia sesión.";
                return View("Login");
            }
            ViewBag.Error = "Error al registrar el usuario.";
            return View(usuario);
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            
            TempData["SweetError"] = "No tienes permisos de administrador para ver esta sección.";
            return RedirectToAction("Login", "Auth");
        }
    }
}