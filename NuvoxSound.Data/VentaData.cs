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

        // =======================================================
        // 1. REGISTRO DE VENTAS
        // =======================================================
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

                        SqlCommand cmdConfirmar = new SqlCommand("usp_ConfirmarVenta", conexion, transaction);
                        cmdConfirmar.CommandType = CommandType.StoredProcedure;
                        cmdConfirmar.Parameters.AddWithValue("@IdVenta", idVentaGenerado);
                        cmdConfirmar.ExecuteNonQuery();

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

        // =======================================================
        // 2. LECTURA DE LIBRERÍA (Para el Panel del Cliente)
        // =======================================================
        public List<Producto> ListarMiLibreria(int idUsuario)
        {
            var lista = new List<Producto>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_Cliente_MiLibreria", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            
                            lista.Add(new Producto
                            {
                                IdPro = Convert.ToInt32(dr["IdPro"]),
                                NomPro = dr["NomPro"].ToString() ?? string.Empty,
                                NomCat = dr["NomCat"].ToString() ?? string.Empty,
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                RutImag = dr["RutImag"].ToString() ?? string.Empty,
                                DescargasRestantes = dr["DescargasRestantes"] != DBNull.Value ? Convert.ToInt32(dr["DescargasRestantes"]) : 0
                            });
                        }
                    }
                }
                catch (Exception) { }
            }
            return lista;
        }

        // =======================================================
        // 3. LECTURA DE COMPRAS (Historial de transacciones)
        // =======================================================
        public List<Venta> ListarMisCompras(int idUsuario)
        {
            var lista = new List<Venta>();
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_Cliente_MisCompras", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Venta
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                Total = Convert.ToDecimal(dr["Total"]),
                                FechaVenta = Convert.ToDateTime(dr["FechaVenta"]),
                                Estado = dr["Estado"].ToString() ?? string.Empty
                            });
                        }
                    }
                }
                catch (Exception) { /* Falla silenciosa */ }
            }
            return lista;
        }

        // =======================================================
        // 4. REGISTRAR DESCENSO DE LÍMITE DE DESCARGAS
        // =======================================================
        public bool RegistrarDescarga(int idUsuario, int idProducto)
        {
            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_RegistrarDescarga", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdProducto", idProducto);

                try
                {
                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0; 
                }
                catch (Exception) { return false; }
            }
        }
        public BoletaFisica ObtenerBoletaFisica(int idVenta, int idUsuario)
        {
            BoletaFisica boleta = null!;

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerDetalleBoleta", conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        // 1. Leemos los datos generales
                        if (dr.Read())
                        {
                            boleta = new BoletaFisica
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                FechaVenta = Convert.ToDateTime(dr["FechaVenta"]),
                                Estado = dr["Estado"].ToString() ?? string.Empty,
                                Total = Convert.ToDecimal(dr["Total"]),
                                ClienteNombres = dr["Nombres"].ToString() + " " + dr["Apellidos"].ToString(),
                                ClienteCorreo = dr["Correo"].ToString() ?? string.Empty,
                                Detalles = new List<BoletaDetalle>()
                            };
                        }

                        // 2. Saltamos a la siguiente tabla y leemos los productos
                        if (boleta != null && dr.NextResult())
                        {
                            while (dr.Read())
                            {
                                boleta.Detalles.Add(new BoletaDetalle
                                {
                                    NombreProducto = dr["NombreProducto"].ToString() ?? string.Empty,
                                    Descripcion = dr["Descripcion"].ToString() ?? string.Empty,
                                    PrecioUnitario = Convert.ToDecimal(dr["PrecioUnitario"]),
                                    Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                    SubTotal = Convert.ToDecimal(dr["SubTotal"])
                                });
                            }
                        }
                    }
                }
                catch (Exception) { return null!; }
            }
            return boleta;
        }

        // =======================================================
        // 5. REPORTE DE VENTAS (Para el Panel Admin)
        // =======================================================
     
        public object ObtenerReporte(string inicio, string fin)
        {
            var lista = new List<object>();

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                
                SqlCommand cmd = new SqlCommand("usp_ReporteVentas", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                // Manejo seguro de fechas para SQL Server (evita errores si el input viene vacío)
                object paramInicio = DBNull.Value;
                object paramFin = DBNull.Value;

                if (!string.IsNullOrEmpty(inicio) && DateTime.TryParse(inicio, out DateTime fechaI))
                {
                    paramInicio = fechaI;
                }

                if (!string.IsNullOrEmpty(fin) && DateTime.TryParse(fin, out DateTime fechaF))
                {
                    // Le sumamos casi 24h al último día para que incluya las ventas hechas por la noche
                    paramFin = fechaF.AddHours(23).AddMinutes(59).AddSeconds(59);
                }

                cmd.Parameters.AddWithValue("@FechaInicio", paramInicio);
                cmd.Parameters.AddWithValue("@FechaFin", paramFin);

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            
                            lista.Add(new
                            {
                                idVenta = Convert.ToInt32(dr["IdVenta"]),
                                fecha = Convert.ToDateTime(dr["FechaVenta"]).ToString("dd/MM/yyyy HH:mm"),
                                cliente = dr["Cliente"].ToString(),
                                email = dr["EmailCliente"].ToString(),
                                total = Convert.ToDecimal(dr["Total"]),
                                estado = dr["Estado"].ToString()
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    
                }
            }
            return lista;
        }
        // =======================================================
        // 6. OBTENER TOTALES REALES DEL DASHBOARD
        // =======================================================
        public DashboardResumen ObtenerResumenNativo()
        {
            DashboardResumen resumen = new DashboardResumen { TotalUsuarios = 0, TotalProductos = 0, TotalVentas = 0 };

            using (SqlConnection conexion = new SqlConnection(cn))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerResumenDashboard", conexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            
                            resumen.TotalUsuarios = dr["TotalUsuarios"] != DBNull.Value ? Convert.ToInt32(dr["TotalUsuarios"]) : 0;
                            resumen.TotalProductos = dr["TotalProductos"] != DBNull.Value ? Convert.ToInt32(dr["TotalProductos"]) : 0;
                            resumen.TotalVentas = dr["TotalVentas"] != DBNull.Value ? Convert.ToDecimal(dr["TotalVentas"]) : 0;
                        }
                    }
                }
                catch (Exception)
                {
                    
                    return resumen;
                }
            }
            return resumen;
        }
    }
}