using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuvoxSound.Data;
using NuvoxSound.Entities;
using Microsoft.Extensions.Configuration;

namespace NuvoxSound.Business
{
    public class UsuarioBusiness
    {
    private readonly UsuarioData _usuarioData;
        //Inyectamos la dependencia de UsuarioData a través del constructor
        public UsuarioBusiness(IConfiguration configuration)
        {
            _usuarioData = new UsuarioData(configuration);
        }
        //metodo para registrar (encriptar antes de guardar)
        public int RegistroUsuario(Usuario usuario, string passwordEnPlano)
        {
            //Encriptamos la contraseña antes de guardarla
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordEnPlano);
            //enviamos el objeto seguro a la bd
            return _usuarioData.RegistroUsuario(usuario);
        }
        //Metodo para login (validar hash)
        public Usuario Login(string correo, string passwordEnPlano)
        {
            //Obtenemos el usuario por correo 
            Usuario usuario = _usuarioData.ValidarUsuario(correo);
            if (usuario != null)
            {
                //BCrypt verifica el password en plano contra el hash almacenado
                bool passwordValido = BCrypt.Net.BCrypt.Verify(passwordEnPlano, usuario.PasswordHash);
                if (passwordValido)
                {
                    //limpiamos el hash antes de devolver el usuario
                    usuario.PasswordHash =string.Empty;
                    return usuario;
                }
            }
            //si el correo no existe o la contraseña no es válida, devolvemos null
            return null;
        }//Fin del metodo login
    }
}
