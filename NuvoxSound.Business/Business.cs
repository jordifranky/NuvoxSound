using NuvoxSound.Data;
using NuvoxSound.Entities;

namespace NuvoxSound.Business
{
    public class DashboardBusiness
    {
        
        private readonly NuvoxSound.Data.Data _dashboardData;

        public DashboardBusiness(NuvoxSound.Data.Data dashboardData)
        {
            _dashboardData = dashboardData;
        }

        public DashboardResumen ObtenerResumen()
        {
            return _dashboardData.ObtenerResumen();
        }
    }
}
