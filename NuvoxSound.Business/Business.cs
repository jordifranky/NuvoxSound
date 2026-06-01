using NuvoxSound.Data;
using NuvoxSound.Entities;

namespace NuvoxSound.Business
{
    public class DashboardBusiness
    {
        private readonly DashboardData _dashboardData;

        public DashboardBusiness(DashboardData dashboardData)
        {
            _dashboardData = dashboardData;
        }

        public DashboardResumen ObtenerResumen()
        {
            return _dashboardData.ObtenerResumen();
        }
    }
}
