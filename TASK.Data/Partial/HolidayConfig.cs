using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class HolidayConfig
    {
        public static List<HolidayConfig> GetData()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HolidayConfigs.Where(c => c.Created.Value.Year == DateTime.Now.Year).ToList();
            }
        }
    }
}
