using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NuvoxSound.Entities;

namespace NuvoxSound.Data
{
    public class ProductoData
    {
        private readonly string cn;

        public ProductoData(IConfiguration configuracion)
        {
            cn = configuracion.GetConnectionString("cn") ?? string.Empty;
        }

        public List<Producto> ListarProductos()
        {
            List<Producto> lista = new List<Producto>();

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_ListarProductos", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Producto
                            {
                                IdPro = Convert.ToInt32(dr["IdProducto"]),
                                IdCate = Convert.ToInt32(dr["IdCategoria"]),
                                NomCat = dr["NombreCategoria"]?.ToString(),
                                NomPro = dr["NombreProducto"]?.ToString(),
                                Descripcion = dr["Descripcion"]?.ToString(),
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                RutImag = dr["RutaImagen"]?.ToString(),
                                Activo = Convert.ToBoolean(dr["Activo"])
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    return lista; // Retornamos la lista vacía en lugar de quebrar el programa
                }
            }
            return lista;
        }
        // Método para insertar (si IdPro es 0) o actualizar un producto
        public bool Guardar(Producto obj)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conexion;
                cmd.CommandType = CommandType.StoredProcedure;

              
                int idArtistaDefecto = 1;

                try
                {
                    conexion.Open();
                    if (obj.IdPro == 0)
                    {
                        // 1. INSERTAR NUEVO PRODUCTO
                        cmd.CommandText = "usp_InsertarProducto";
                        cmd.Parameters.AddWithValue("@IdCategoria", obj.IdCate == 0 ? 1 : obj.IdCate);
                        cmd.Parameters.AddWithValue("@IdArtista", idArtistaDefecto);
                        cmd.Parameters.AddWithValue("@NombreProducto", obj.NomPro ?? "");
                        cmd.Parameters.AddWithValue("@Descripcion", obj.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@Precio", obj.Precio);
                        cmd.Parameters.AddWithValue("@RutaImagen", string.IsNullOrEmpty(obj.RutImag) ? "fa-solid fa-compact-disc" : obj.RutImag);
                        cmd.Parameters.AddWithValue("@RutaDemoAudio", DBNull.Value);
                        cmd.Parameters.AddWithValue("@RutaArchivo", DBNull.Value);

                        // Tu SP exige este parámetro de salida
                        SqlParameter outputId = new SqlParameter("@IdProductoNuevo", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(outputId);

                        cmd.ExecuteNonQuery();
                        respuesta = true;
                    }
                    else
                    {
                        // 2. ACTUALIZAR PRODUCTO EXISTENTE
                        cmd.CommandText = "usp_ActualizarProducto";
                        cmd.Parameters.AddWithValue("@IdProducto", obj.IdPro);
                        cmd.Parameters.AddWithValue("@IdCategoria", obj.IdCate == 0 ? 1 : obj.IdCate);
                        cmd.Parameters.AddWithValue("@IdArtista", idArtistaDefecto);
                        cmd.Parameters.AddWithValue("@NombreProducto", obj.NomPro ?? "");
                        cmd.Parameters.AddWithValue("@Descripcion", obj.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@Precio", obj.Precio);
                        cmd.Parameters.AddWithValue("@RutaImagen", string.IsNullOrEmpty(obj.RutImag) ? "fa-solid fa-compact-disc" : obj.RutImag);
                        cmd.Parameters.AddWithValue("@RutaDemoAudio", DBNull.Value);
                        cmd.Parameters.AddWithValue("@RutaArchivo", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Activo", obj.Activo);

                        cmd.ExecuteNonQuery();
                        respuesta = true;
                    }
                }
                catch (Exception)
                {
                    respuesta = false;
                }
            }
            return respuesta;
        }

        // Método para eliminar un producto
        public bool Eliminar(int id)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                SqlCommand cmd = new SqlCommand("usp_EliminarProducto", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdProducto", id);

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
        }
        // Método nativo para contar productos por categoría
        public List<object> ObtenerResumenCategorias()
        {
            var lista = new List<object>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_Admin_ResumenCategorias", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new
                            {
                                nomCat = dr["NomCat"].ToString(),
                                count = Convert.ToInt32(dr["Cantidad"])
                            });
                        }
                    }
                }
                catch (Exception) { }
            }
            return lista;
        }
    }
}