using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Configuration;
using NuvoxSound.Entities;

namespace NuvoxSound.Data
{
    public class VentaData
    {
        private readonly string cn;

        public VentaData(IConfiguration configuracion)
        {
            cn = configuracion.GetConnectionString("cn") ?? string.Empty;
        }

        public bool RegistrarVentaTransaccional(int idUsuario, List<ItemCarrito> carrito)
        {
            bool exito = false;

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                conexion.Open();
                using (SqlTransaction transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmdVenta = new SqlCommand("usp_RegistrarVenta", conexion, transaction);
                        cmdVenta.CommandType = CommandType.StoredProcedure;

                        cmdVenta.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmdVenta.Parameters.AddWithValue("@Total", carrito.Sum(x => x.SubTotal));

                        SqlParameter outIdVenta = new SqlParameter("@IdVenta", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmdVenta.Parameters.Add(outIdVenta);
                        cmdVenta.ExecuteNonQuery();

                        int idVentaGenerado = Convert.ToInt32(outIdVenta.Value);

                        foreach (var item in carrito)
                        {
                            SqlCommand cmdDetalle = new SqlCommand("usp_InsertarDetalleVenta", conexion, transaction);
                            cmdDetalle.CommandType = CommandType.StoredProcedure;

                            cmdDetalle.Parameters.AddWithValue("@IdVenta", idVentaGenerado);
                            cmdDetalle.Parameters.AddWithValue("@IdProducto", item.Producto?.IdPro);
                            cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", item.Producto?.Precio);
                            cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);

                            SqlParameter outIdDetalle = new SqlParameter("@IdDetalleNuevo", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmdDetalle.Parameters.Add(outIdDetalle);

                            cmdDetalle.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        exito = true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        exito = false;
                    }
                }
            }
            return exito;
        }
    }
}