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
    }
}