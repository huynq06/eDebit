using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class Print
    {
        public static void Insert(List<Print> listPrint)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listPrint.Count > 0)
                {
                    dbConext.Prints.InsertAllOnSubmit(listPrint);
                }
                dbConext.SubmitChanges();
            }
        }
        public static List<Print> GetAll()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.Prints.ToList();
            }
        }
        public static void Insert(Print print)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {

                dbConext.Prints.InsertOnSubmit(print);
                dbConext.SubmitChanges();
            }
        }
    }
}
