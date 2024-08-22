using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;


namespace TASK.Data
{
    public partial class HermesDebitDetail
    {
        public static List<HermesDebitDetail> GetAll()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebitDetails.ToList();
            }
        }
        public static void Insert(List<HermesDebitDetail> listInvoiceDetail)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                if (listInvoiceDetail.Count > 0)
                {
                    dbConext.HermesDebitDetails.InsertAllOnSubmit(listInvoiceDetail);
                }
                dbConext.SubmitChanges();
            }
        }
        public static List<HermesDebitDetail> GetByInvoiceIsn(string invoiceIsn)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebitDetails.Where(c => c.InvoiceIns == invoiceIsn).ToList();
            }
        }
        public static IEnumerable<HermesDebitDetail> GetMultiByInvoiceIsn(string invoiceIsn)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebitDetails.Where(c => c.InvoiceIns == invoiceIsn);
            }
        }
    }
}
