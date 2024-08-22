using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;
using TASK.Model.ViewModel;
namespace TASK.Data
{
    public partial class HermesDebit
    {
        public static List<HermesDebit> GetAll()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.ToList();
            }
        }
        public static List<HermesDebit> GetByDay()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.Where(c => c.InvoiceDate.ToString() == DateTime.Now.Date.ToString("yyyy-MM-dd")).ToList();
            }
        }
        public static List<HermesDebit> GetData()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.Where(c => c.Status == false && c.Retry < 4 && c.InvoiceDate.ToString() == DateTime.Now.Date.ToString("yyyy-MM-dd")).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<HermesDebit> GetDataCancel()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.Where(c => c.Status == true && c.IsCancel == true && c.ExceptionStatus == 1 && c.InvoiceStatus != 3).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<HermesDebit> GetDataAddtional()
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.Where(c => c.Status == false && c.ExceptionStatus == 1 && c.IsCancel != true && c.Retry >= 4 && c.InvoiceCheckAddtional == true).OrderByDescending(c => c.Created).ToList();
            }
        }
        public static HermesDebit GetByInvoiceIns(string invoiceIsn)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.SingleOrDefault(c => c.InvoiceIsn == invoiceIsn);
            }
        }
        public static HermesDebit CheckExistAwb(string invoiceIsn, string awb, string hawb)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.SingleOrDefault(c => c.InvoiceIsn != invoiceIsn && c.AWB == awb && c.Hawb == hawb && c.Created > DateTime.Now.AddDays(-30));
            }
        }
        public static List<HermesDebit> CheckExistListAwb(string invoiceIsn, string awb, string hawb)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                return dbConext.HermesDebits.Where(c => c.InvoiceIsn != invoiceIsn && c.AWB == awb && c.Hawb == hawb && c.Created > DateTime.Now.AddDays(-30)).ToList();
            }
        }
        public static void Insert(List<HermesDebit> listInvoice)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                if (listInvoice.Count > 0)
                {
                    dbConext.HermesDebits.InsertAllOnSubmit(listInvoice);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Update(List<HermesDebit> listInvoice)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {
                if (listInvoice.Count > 0)
                {
                    foreach (var invoice in listInvoice)
                    {
                        var invoiceDB = dbConext.HermesDebits.FirstOrDefault(c => c.ID == invoice.ID);
                        invoiceDB.Status = true;
                        invoiceDB.TimeReponse = invoice.TimeReponse;
                        invoiceDB.ExecuteTime = invoice.ExecuteTime;

                    }
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Update(HermesDebit invoice)
        {
            using (DebitContextDataContext dbConext = new DebitContextDataContext(AppSetting.ConnectionStringeDebit))
            {

                var invoiceDB = dbConext.HermesDebits.FirstOrDefault(c => c.ID == invoice.ID);
                invoiceDB.Status = invoice.Status;
                invoiceDB.Retry = invoice.Retry;
                invoiceDB.TimeReponse = invoice.TimeReponse;
                invoiceDB.ExecuteTime = invoice.ExecuteTime;
                invoiceDB.Sequence = invoice.Sequence;
                invoiceDB.ReferenceNo = invoice.ReferenceNo;
                invoiceDB.InvoiceStatus = invoice.InvoiceStatus;
                invoiceDB.InvoiceDescription = invoice.InvoiceDescription;
                invoiceDB.ExceptionStatus = invoice.ExceptionStatus;
                invoiceDB.SearchCode = invoice.SearchCode;
                invoiceDB.EInvoiceSearchLink = invoice.EInvoiceSearchLink;
                invoiceDB.InvoiceFieldSerial = invoice.InvoiceFieldSerial;
                invoiceDB.InvoiceFieldForm = invoice.InvoiceFieldForm;
                invoiceDB.InvoiceFieldType = invoice.InvoiceFieldType;
                invoiceDB.Idt = invoice.Idt;
                dbConext.SubmitChanges();
            }
        }
    }
}
