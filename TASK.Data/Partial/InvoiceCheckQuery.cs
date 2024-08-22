using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class InvoiceCheckQuery
    {
        public static List<InvoiceCheckQuery> GetData()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.InvoiceCheckQueries.Where(c=>c.Created > DateTime.Now.AddMinutes(-120)).ToList();
            }
        }
        public static List<InvoiceCheckQuery> GetDataProcess()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.InvoiceCheckQueries.Where(c => c.Status==false).ToList();
            }
        }
        public static List<InvoiceCheckQuery> GetDataCancel(DateTime dt)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.InvoiceCheckQueries.Where(c => c.Created > dt).ToList();
            }
        }
        public static void Insert(InvoiceCheckQuery invoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                dbConext.InvoiceCheckQueries.InsertOnSubmit(invoice);
                dbConext.SubmitChanges();
            }
        }
        //public static void Insert(List<InvoiceCheckQuery> listInvoice)
        //{
        //    using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
        //    {
        //        if (listInvoice.Count > 0)
        //        {
        //            dbConext.InvoiceCheckQueries.InsertAllOnSubmit(listInvoice);
        //        }
        //        dbConext.SubmitChanges();
        //    }
        //}
        public static void Insert(List<InvoiceCheckQuery> listInvoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listInvoice.Count > 0)
                {
                    foreach (var item in listInvoice)
                    {
                        InvoiceCheckQuery invoiceDB = dbConext.InvoiceCheckQueries.FirstOrDefault(c => c.InvoiceIsn == item.InvoiceIsn);
                        if (invoiceDB == null)
                        {
                            dbConext.InvoiceCheckQueries.InsertOnSubmit(item);
                        }
                        // dbConext.InvoiceCheckQueries.InsertAllOnSubmit(listInvoice);
                    }

                }
                dbConext.SubmitChanges();
            }
        }
        //public static void InsertV2(List<InvoiceCheckQuery> listInvoice)
        //{
        //    using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
        //    {
        //        if (listInvoice.Count > 0)
        //        {
        //            foreach (var item in listInvoice)
        //            {
        //                InvoiceCheckQuery invoiceDB = dbConext.InvoiceCheckQueries.FirstOrDefault(c => c.InvoiceIsn == item.InvoiceIsn);
        //                if (invoiceDB == null)
        //                {
        //                    dbConext.InvoiceCheckQueries.InsertOnSubmit(invoiceDB);
        //                }
        //                // dbConext.InvoiceCheckQueries.InsertAllOnSubmit(listInvoice);
        //            }

        //        }
        //        dbConext.SubmitChanges();
        //    }
        //}
        public static void Update(InvoiceCheckQuery invoice)
        {
            try
            {
                using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
                {
                    var invoiceDB = dbConext.InvoiceCheckQueries.FirstOrDefault(c => c.ID == invoice.ID);
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
