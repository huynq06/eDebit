using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class PrintConfig
    {
        public static PrintConfig GetByPrintNetwork(string printNetWork)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.PrintConfigs.SingleOrDefault(c=>c.PrintUser == printNetWork);
            }
        }
    }
}
