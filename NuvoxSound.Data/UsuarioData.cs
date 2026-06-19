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
                                PasswordHash = dr["PasswordHash"].ToString() ?? string.Empty,
                                IdRol = Convert.ToInt32(dr["IdRol"])
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

        // Método para listar a todos los usuarios en el panel de administrador
        public List<Usuario> ListarUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();

            using (var conexion = new SqlConnection(cn))
            {
               
                SqlCommand cmd = new SqlCommand("usp_ListarUsuarios", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    conexion.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Usuario()
                            {
                                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                                Nombres = dr["Nombres"].ToString() ?? string.Empty,
                                Apellidos = dr["Apellidos"].ToString() ?? string.Empty,
                                Correo = dr["Correo"].ToString() ?? string.Empty,
                               
                                Activo = dr["Activo"] != DBNull.Value ? Convert.ToBoolean(dr["Activo"]) : true
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    return lista; 
                }
            }
            return lista;
        } //fin del metodo ListarUsuarios

        // Método para activar o bloquear el acceso de un usuario
        public bool ActualizarEstado(int id, bool estado)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                SqlCommand cmd = new SqlCommand("usp_ActualizarEstadoUsuario", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", id);
                cmd.Parameters.AddWithValue("@Activo", estado);

                try
                {
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception)
                {
                    respuesta = false;
                }
            }
            return respuesta;
        }//fin del metodo ActualizarEstado
    }
}
