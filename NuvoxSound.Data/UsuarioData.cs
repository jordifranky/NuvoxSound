using System;
using System.Data;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Configuration;
using NuvoxSound.Entities;

namespace NuvoxSound.Data
{
    public  class UsuarioData
    {
        private readonly string cn;

        public UsuarioData(IConfiguration configuration)
        {
            cn = configuration.GetConnectionString("cn") ?? string.Empty;
        }
        //metodo para obtener el usuario por correo y capa negocios valide el hash
        public Usuario ValidarUsuario(string correo)
        {
            Usuario usuario = null;
            using(var conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_ValidarUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Correo", correo);

                try
                {
                    conexion.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            usuario = new Usuario()
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Nombres = dr["Nombres"].ToString() ?? string.Empty,
                                Apellidos = dr["Apellidos"].ToString() ?? string.Empty,
                                NombreRol = dr["NombreRol"].ToString() ?? string.Empty,
                                PasswordHash = dr["PasswordHash"].ToString() ?? string.Empty
                            };
                        }
                    }
                }
                catch (Exception) 
                { 
                    return null;
                }
            }
            return usuario;
        }
        //metodo para registrar un nuevo usuario
        public int RegistroUsuario(Usuario usuario)
        { 
            int idGenerado =0;
            using ( var conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_RegistrarUsuario", conexion);
                cmd.CommandType= CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdRol", usuario.IdRol);
                cmd.Parameters.AddWithValue("@Nombres", usuario.Nombres);
                cmd.Parameters.AddWithValue("@Apellidos", usuario.Apellidos);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);

                //parametro de salida
                SqlParameter outputIdParam = new SqlParameter("@IdUsuarioNuevo", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outputIdParam);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    idGenerado = Convert.ToInt32(outputIdParam.Value);

                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return idGenerado;
        } //Fin del metodo RegistroUsuario
    }
}
