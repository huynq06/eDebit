using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;
namespace TASK.Data
{
    public partial class DebitCheckQuery
    {
        public static List<DebitCheckQuery> GetData()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.DebitCheckQueries.Where(c => c.Created > DateTime.Now.AddMinutes(-120)).ToList();
            }
        }
        public static List<DebitCheckQuery> GetDataProcess()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.DebitCheckQueries.Where(c => c.Status == false).ToList();
            }
        }
        public static List<DebitCheckQuery> GetDataCancel(DateTime dt)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.DebitCheckQueries.Where(c => c.Created > dt).ToList();
            }
        }
        public static void Insert(DebitCheckQuery invoice)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                dbConext.DebitCheckQueries.InsertOnSubmit(invoice);
                dbConext.SubmitChanges();
            }
        }
     
        public static void Insert(List<DebitCheckQuery> listInvoice)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                if (listInvoice.Count > 0)
                {
                    foreach (var item in listInvoice)
                    {
                        DebitCheckQuery invoiceDB = dbConext.DebitCheckQueries.FirstOrDefault(c => c.InvoiceIsn == item.InvoiceIsn);
                        if (invoiceDB == null)
                        {
                            dbConext.DebitCheckQueries.InsertOnSubmit(item);
                        }
                        // dbConext.DebitCheckQueries.InsertAllOnSubmit(listInvoice);
                    }

                }
                dbConext.SubmitChanges();
            }
        }
      
        public static void Update(DebitCheckQuery invoice)
        {
            try
            {
                using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
                {
                    var invoiceDB = dbConext.DebitCheckQueries.FirstOrDefault(c => c.ID == invoice.ID);
                    invoiceDB.Status = invoice.Status;
                    invoiceDB.Retry = invoice.Retry;
                    dbConext.SubmitChanges();
                }
            }
            catch (Exception ex)
            {

                Log.WriteLog(ex.ToString() + invoice.InvoiceIsn);
            }

        }
    }
}
