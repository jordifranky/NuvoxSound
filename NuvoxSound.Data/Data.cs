using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NuvoxSound.Entities;

namespace NuvoxSound.Data
{
    public class DashboardData
    {
        private readonly string _cadenaConexion;

        // Inyectamos la configuración para leer la cadena de conexión de appsettings.json
        public DashboardData(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("CadenaSQL")!;
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
                            resumen.TotalUsuarios = Convert.ToInt32(dr["TotalUsuarios"]);
                            resumen.TotalProductos = Convert.ToInt32(dr["TotalProductos"]);
                            resumen.TotalVentas = Convert.ToDecimal(dr["TotalVentas"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener datos del Dashboard: " + ex.Message);
                }
            }
            return resumen;
        }
    }
}