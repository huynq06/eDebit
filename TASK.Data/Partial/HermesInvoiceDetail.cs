using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class HermesInvoiceDetail
    {
        public static List<HermesInvoiceDetail> GetAll()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoiceDetails.ToList();
            }
        }
        public static void Insert(List<HermesInvoiceDetail> listInvoiceDetail)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listInvoiceDetail.Count > 0)
                {
                    dbConext.HermesInvoiceDetails.InsertAllOnSubmit(listInvoiceDetail);
                }
                dbConext.SubmitChanges();
            }
        }
        public static List<HermesInvoiceDetail> GetByInvoiceIsn(string invoiceIsn)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoiceDetails.Where(c=>c.InvoiceIns == invoiceIsn).ToList();
            }
        }
        public static IEnumerable<HermesInvoiceDetail> GetMultiByInvoiceIsn(string invoiceIsn)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoiceDetails.Where(c => c.InvoiceIns == invoiceIsn);
            }
        }
    }
}
