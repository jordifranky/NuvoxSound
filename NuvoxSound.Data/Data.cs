using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NuvoxSound.Entities;

namespace NuvoxSound.Data
{
    public class Data 
    {
        private readonly string _cadenaConexion;

        public Data(IConfiguration configuration)
        {
            //Usamos "cn" que es la clave real de tu appsettings.json
            _cadenaConexion = configuration.GetConnectionString("cn") ?? string.Empty;
        }

        public DashboardResumen ObtenerResumen()
        {
            DashboardResumen resumen = new DashboardResumen();

            using (SqlConnection conexion = new SqlConnection(_cadenaConexion))
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
                            // Escudo protector por si algún campo viene vacío en SQL
                            resumen.TotalUsuarios = dr["TotalUsuarios"] != DBNull.Value ? Convert.ToInt32(dr["TotalUsuarios"]) : 0;
                            resumen.TotalProductos = dr["TotalProductos"] != DBNull.Value ? Convert.ToInt32(dr["TotalProductos"]) : 0;

                            // SUM en SQL devuelve decimal, lo leemos de forma segura
                            resumen.TotalVentas = dr["TotalVentas"] != DBNull.Value ? Convert.ToDecimal(dr["TotalVentas"]) : 0.00m;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Esto te pintará el error real en la consola de Visual Studio si algo falla
                    Console.WriteLine("Error crítico en Dashboard: " + ex.Message);
                }
            }
            return resumen;
        }
    }
}