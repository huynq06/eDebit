using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class InvoiceCancelCheck
    {
        public static List<InvoiceCancelCheck> GetData()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.InvoiceCancelChecks.Where(c => c.Created > DateTime.Now.AddMinutes(-120)).ToList();
            }
        }
        public static void Insert(InvoiceCancelCheck invoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                dbConext.InvoiceCancelChecks.InsertOnSubmit(invoice);
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(List<InvoiceCancelCheck> listInvoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listInvoice.Count > 0)
                {
                    dbConext.InvoiceCancelChecks.InsertAllOnSubmit(listInvoice);
                }
                dbConext.SubmitChanges();
            }
        }
    }
}
