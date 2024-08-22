using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;


namespace TASK.Data
{
    public partial class Invoice
    {
        public static List<Invoice> GetData()
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.Invoices.Where(c => c.Status == 0 && c.Created.Value.Date == DateTime.Now.Date && c.Retry < 4).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<Invoice> GetDataAddtional()
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.Invoices.Where(c => c.Status == 0 && c.Created.Value.Date == DateTime.Now.Date && c.ExceptionStatus == 1 && c.Retry >= 4).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<Invoice> GetInvoiceCancel()
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.Invoices.Where(c => c.Status == 1 && c.Canceled == true && c.CancelRetry < 4 && c.ExecuteCancel==false).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<Invoice> GetInvoiceCancelAddtional()
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                return dbConext.Invoices.Where(c => c.Status == 1 && c.Canceled == true && c.ExceptionStatus ==1 && c.CancelRetry >= 4 && c.ExecuteCancel == false).OrderBy(c => c.Created).ToList();
            }
        }
        public static void Update(List<Invoice> listInvoice)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                if (listInvoice.Count > 0)
                {
                    foreach (var invoice in listInvoice)
                    {
                        var invoiceDB = dbConext.Invoices.FirstOrDefault(c => c.ID == invoice.ID);
                        invoiceDB.Status = 1;
                    }
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Update(Invoice invoice)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {

                var invoiceDB = dbConext.Invoices.FirstOrDefault(c => c.ID == invoice.ID);
                invoiceDB.Status = invoice.Status;
                invoiceDB.Retry = invoice.Retry;
                invoiceDB.Sequence = invoice.Sequence;
                invoiceDB.Reference = invoice.Reference;
                invoiceDB.InvoiceStatus = invoice.InvoiceStatus;
                invoiceDB.InvoiceDescription = invoice.InvoiceDescription;
                invoiceDB.ExceptionStatus = invoice.ExceptionStatus;
                dbConext.SubmitChanges();
            }
        }
        public static void UpdateCancel(Invoice invoice)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {

                var invoiceDB = dbConext.Invoices.FirstOrDefault(c => c.ID == invoice.ID);
                invoiceDB.ExecuteCancel = invoice.ExecuteCancel;
                invoiceDB.CancelRetry = invoice.CancelRetry;
                invoiceDB.InvoiceStatus = invoice.InvoiceStatus;
                invoiceDB.InvoiceDescription = invoice.InvoiceDescription;
                invoiceDB.ExceptionStatus = invoice.ExceptionStatus;
                invoiceDB.CancelDateTime = invoice.CancelDateTime;
                dbConext.SubmitChanges();
            }
        }
    }
}
