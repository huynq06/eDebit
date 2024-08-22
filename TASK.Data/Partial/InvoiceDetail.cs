using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;


namespace TASK.Data
{
    public partial class InvoiceDetail
    {
        public static List<InvoiceDetail> GetByInvoiceID(string invoiceID)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.InvoiceDetails.Where(c => c.InvoiceID == invoiceID).ToList();
            }
        }
        public static List<InvoiceDetail> GetAll()
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.InvoiceDetails.ToList();
            }
        }
    }
}
