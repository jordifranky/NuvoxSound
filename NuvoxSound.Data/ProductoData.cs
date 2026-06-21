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
                       
                        cmd.CommandText = "usp_InsertarProducto";
                        cmd.Parameters.AddWithValue("@IdCategoria", obj.IdCate == 0 ? 1 : obj.IdCate);
                        cmd.Parameters.AddWithValue("@IdArtista", idArtistaDefecto);
                        cmd.Parameters.AddWithValue("@NombreProducto", obj.NomPro ?? "");
                        cmd.Parameters.AddWithValue("@Descripcion", obj.Descripcion ?? "");
                        cmd.Parameters.AddWithValue("@Precio", obj.Precio);
                        cmd.Parameters.AddWithValue("@RutaImagen", string.IsNullOrEmpty(obj.RutImag) ? "fa-solid fa-compact-disc" : obj.RutImag);
                        cmd.Parameters.AddWithValue("@RutaDemoAudio", DBNull.Value);
                        cmd.Parameters.AddWithValue("@RutaArchivo", DBNull.Value);

                        
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
        // =======================================================
        // OBTENER LISTA DE ARTISTAS DINÁMICA
        // =======================================================
       
        public object ObtenerArtistas()
        {
            var lista = new List<object>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                SqlCommand cmd = new SqlCommand("usp_ListarArtistas", conexion);
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
                                idArtista = Convert.ToInt32(dr["IdArtista"]),
                                nombreArtista = dr["NombreArtista"].ToString()
                            });
                        }
                    }
                }
                catch (Exception) { }
            }
            return lista;
        } //Fin del metodo ObtenerArtistas

        // =======================================================
        // GUARDAR PRODUCTO (VÍA STORED PROCEDURE)
        // =======================================================
        public string GuardarProductoSP(
            int idPro, string nomPro, int idCate, int idArtista, decimal precio,
            string rutImag, string descripcion, bool activo,
            string rutaImagen, string rutaAudio, string rutaZip, string enlaceDescarga)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cn))
                {
                    string nombreSP = (idPro == 0) ? "usp_InsertarProducto" : "usp_ActualizarProducto";

                    SqlCommand cmd = new SqlCommand(nombreSP, conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (idPro != 0) { cmd.Parameters.AddWithValue("@IdProducto", idPro); }

                    cmd.Parameters.AddWithValue("@IdCategoria", idCate);
                    cmd.Parameters.AddWithValue("@IdArtista", idArtista);
                    cmd.Parameters.AddWithValue("@NombreProducto", nomPro);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(descripcion) ? "" : descripcion);
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Activo", activo);

                    string imagenFinal = string.IsNullOrEmpty(rutaImagen) ? (string.IsNullOrEmpty(rutImag) ? null : rutImag) : rutaImagen;

                  
                    cmd.Parameters.AddWithValue("@RutaImagen", (object)imagenFinal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RutaDemoAudio", (object)rutaAudio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RutaArchivo", (object)rutaZip ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnlaceExterno", (object)enlaceDescarga ?? DBNull.Value);

                    if (idPro == 0)
                    {
                        SqlParameter outParam = new SqlParameter("@IdProductoNuevo", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outParam);
                    }

                    conexion.Open();
                    cmd.ExecuteNonQuery();

                    
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                
                return ex.Message;
            }
        } // fin del metodo GuardarProductoSP

        // =======================================================
        // GESTIÓN  CUPONES 
        // =======================================================
        public object ObtenerCupones()
        {
            var lista = new List<object>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_ListarCupones", conexion);
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
                                idCupon = Convert.ToInt32(dr["IdCupon"]),
                                codigo = dr["Codigo"].ToString(),
                                porcentaje = Convert.ToInt32(dr["Porcentaje"]),
                                fechaExpiracion = dr["FechaExpiracion"].ToString()
                            });
                        }
                    }
                }
                catch (Exception) { }
            }
            return lista;
        }

        public bool GuardarCupon(string codigo, int porcentaje, string fechaExpiracion)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cn))
                {
                    SqlCommand cmd = new SqlCommand("usp_InsertarCupon", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", codigo);
                    cmd.Parameters.AddWithValue("@Porcentaje", porcentaje);
                    cmd.Parameters.AddWithValue("@FechaExpiracion", fechaExpiracion);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }

        public bool EliminarCupon(int id)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(cn))
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarCupon", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCupon", id);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch { return false; }
        }
        // =======================================================
        // CÓDIGOS PARA CHATBOT (BÚSQUEDA DE PRODUCTOS POR TEXTO LIBRE)
        // =======================================================
        public string BuscarProductoParaChatbot(string busquedaCliente)
        {
           
            string palabraClave = busquedaCliente.Replace("busco", "").Replace("quiero", "").Trim();

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                SqlCommand cmd = new SqlCommand("usp_TiendaListar", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                
                cmd.Parameters.AddWithValue("@Busqueda", palabraClave);
                cmd.Parameters.AddWithValue("@Categoria", DBNull.Value);
                cmd.Parameters.AddWithValue("@IdArtista", DBNull.Value);

                conexion.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        
                        string nombre = dr["NombreProducto"].ToString();
                        string precio = dr["Precio"].ToString();
                        return $"¡Encontré algo perfecto para ti! Te recomiendo el pack '{nombre}' que está a ${precio}. ¿Te gustaría verlo en la tienda?";
                    }
                }
            }
            return "Lo siento, no encontré un pack exacto con ese nombre, pero puedes revisar nuestra sección de Categorías.";
        } //Fin del metodo BuscarProductoParaChatbot

        // ========================================================
        // 1. OBTENER PRODUCTOS POR CATEGORÍA 
        // ========================================================
        public IEnumerable<dynamic> ObtenerProductosPorCategoria(string categoria)
        {
            var lista = new List<dynamic>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                string query = @"
                    SELECT p.IdProducto, p.NombreProducto, p.Precio, ISNULL(a.NombreArtista, 'Nuvox Exclusive') AS NombreArtista 
                    FROM Producto p
                    LEFT JOIN Artista a ON p.IdArtista = a.IdArtista
                    LEFT JOIN Categoria c ON p.IdCategoria = c.IdCategoria
                    WHERE c.NombreCategoria LIKE '%' + @Categoria + '%' AND p.Activo = 1";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@Categoria", categoria);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new
                            {
                                idProducto = Convert.ToInt32(dr["IdProducto"]),
                                nombre = dr["NombreProducto"].ToString(),
                                precio = Convert.ToDouble(dr["Precio"]),
                                artista = dr["NombreArtista"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ERROR DE SQL: " + ex.Message);
                }
            }
            return lista;
        }//Fin del metodo obtenerProductosPorCategoria

        // ========================================================
        // 2. OBTENER PRODUCTOS POR ARTISTA 
        // ========================================================
        public IEnumerable<dynamic> ObtenerProductosPorArtista(int idArtista)
        {
            var lista = new List<dynamic>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                // Tablas en singular: Producto, Artista
                string query = @"
                    SELECT p.IdProducto, p.NombreProducto, p.Precio, ISNULL(a.NombreArtista, 'Nuvox Exclusive') AS NombreArtista 
                    FROM Producto p
                    LEFT JOIN Artista a ON p.IdArtista = a.IdArtista
                    WHERE p.IdArtista = @IdArtista AND p.Activo = 1";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@IdArtista", idArtista);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new
                            {
                                idProducto = Convert.ToInt32(dr["IdProducto"]),
                                nombre = dr["NombreProducto"].ToString(),
                                precio = Convert.ToDouble(dr["Precio"]),
                                artista = dr["NombreArtista"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ERROR DE SQL: " + ex.Message);
                }
            }
            return lista;
        }//Fin del metodo obtenerProductosPorArtista

        // ========================================================
        // 3. OBTENER DETALLE DE UN PRODUCTO INDIVIDUAL
        // ========================================================
        public dynamic ObtenerDetalleProducto(int idProducto)
        {
            using (SqlConnection conexion = new SqlConnection(cn))
            {
               
                string query = @"
                    SELECT p.IdProducto, p.NombreProducto, p.Descripcion, p.Precio, 
                           ISNULL(a.NombreArtista, 'Nuvox Exclusive') AS NombreArtista 
                    FROM Producto p
                    LEFT JOIN Artista a ON p.IdArtista = a.IdArtista
                    WHERE p.IdProducto = @IdProducto";

                SqlCommand cmd = new SqlCommand(query, conexion);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            double precioReal = Convert.ToDouble(dr["Precio"]);

                           
                            double precioTachado = precioReal + (precioReal * 0.30);

                            return new
                            {
                                idProducto = Convert.ToInt32(dr["IdProducto"]),
                                nombre = dr["NombreProducto"].ToString(),

                                descripcion = string.IsNullOrEmpty(dr["Descripcion"].ToString()) ?
                                              "Un paquete esencial con sonidos puros para tu próxima producción Trance." :
                                              dr["Descripcion"].ToString(),
                                precio = precioReal,
                                precioAntiguo = precioTachado,
                                artista = dr["NombreArtista"].ToString()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ERROR DE SQL: " + ex.Message);
                }
            }
            return null;
        }//Fin del metodo obtenerDetalleProducto
    }


}