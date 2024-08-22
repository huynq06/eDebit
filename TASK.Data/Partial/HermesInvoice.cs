using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;
using TASK.Model.ViewModel;
namespace TASK.Data
{
    public partial class HermesInvoice
    {

        public static List<HermesInvoice> GetAll()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.ToList();
            }
        }
        public static List<HermesInvoice> GetByDay()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c=>c.InvoiceDate.ToString()==DateTime.Now.Date.ToString("yyyy-MM-dd")).ToList();
            }
        }
        public static List<HermesInvoice> GetData()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.Status==false && c.Retry< 4 && c.InvoiceDate.ToString() == DateTime.Now.Date.ToString("yyyy-MM-dd")).OrderBy(c=>c.Created).ToList();
            }
        }
        public static List<HermesInvoice> GetDataCancel()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.Status == true && c.IsCancel == true && c.ExceptionStatus == 1 && c.InvoiceStatus != 3).OrderBy(c => c.Created).ToList();
            }
        }
        public static List<HermesInvoice> GetDataAddtional()
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.Status==false && c.ExceptionStatus == 1 && c.IsCancel != true && c.Retry >=4 && c.InvoiceCheckAddtional == true).OrderByDescending(c => c.Created).ToList();
            }
        }
        public static HermesInvoice GetByInvoiceIns(string invoiceIsn)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.SingleOrDefault(c => c.InvoiceIsn==invoiceIsn);
            }
        }
        public static HermesInvoice CheckExistAwb(string invoiceIsn, string awb, string hawb)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.SingleOrDefault(c => c.InvoiceIsn != invoiceIsn && c.AWB == awb && c.Hawb == hawb && c.Created > DateTime.Now.AddDays(-30));
            }
        }
        public static List<HermesInvoice> CheckExistListAwb(string invoiceIsn,string awb,string hawb)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.InvoiceIsn != invoiceIsn && c.AWB==awb && c.Hawb==hawb && c.Created > DateTime.Now.AddDays(-30)).ToList();
            }
        }
        public static void Insert(List<HermesInvoice> listInvoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listInvoice.Count > 0)
                {
                    dbConext.HermesInvoices.InsertAllOnSubmit(listInvoice);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Update(List<HermesInvoice> listInvoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listInvoice.Count > 0)
                {
                    foreach (var invoice in listInvoice)
                    {
                        var invoiceDB = dbConext.HermesInvoices.FirstOrDefault(c => c.ID == invoice.ID);
                        invoiceDB.Status = true;
                        invoiceDB.TimeReponse = invoice.TimeReponse;
                        invoiceDB.ExecuteTime = invoice.ExecuteTime;

                    }
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Update(HermesInvoice invoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {

                var invoiceDB = dbConext.HermesInvoices.FirstOrDefault(c => c.ID == invoice.ID);
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
        public static void UpdateCancel(HermesInvoice invoice)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {

                var invoiceDB = dbConext.HermesInvoices.FirstOrDefault(c => c.ID == invoice.ID);
                invoiceDB.IsCancel = invoice.IsCancel;
                invoiceDB.InvoiceStatus = invoice.InvoiceStatus;
                invoiceDB.InvoiceDescription = invoice.InvoiceDescription;
                invoiceDB.CancelDateTime = invoice.CancelDateTime;
                invoiceDB.ExceptionStatus = invoice.ExceptionStatus;
                invoiceDB.CancelReason = invoice.CancelReason;
                dbConext.SubmitChanges();
            }
        }
        public static List<HermesInvoice> GetInvoiceTempToCancel(InvoiceCancelViewModel invoiceCancel)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.AWB_Prefix == invoiceCancel.AWB_Prefix.Trim() && c.AWB_Serial.Trim() == invoiceCancel.AWB_Serial && c.Hawb.Trim() == invoiceCancel.Hawb&&c.ObjectType == invoiceCancel.ObjectType).ToList();
            }
        }
        public static List<HermesInvoice> GetInvoiceTempToCancelFromStartGolive(InvoiceCancelViewModel invoiceCancel,DateTime dt)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c =>c.Created > dt && c.AWB_Prefix == invoiceCancel.AWB_Prefix.Trim() && c.AWB_Serial.Trim() == invoiceCancel.AWB_Serial.PadLeft(8,'0') && c.Hawb.Trim() == invoiceCancel.Hawb && c.ObjectType == invoiceCancel.ObjectType).ToList();
            }
        }
        public static HermesInvoice GetInvoiceToCancel(InvoiceCancelViewModel invoiceCancel)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                return dbConext.HermesInvoices.Where(c => c.AWB_Prefix == invoiceCancel.AWB_Prefix.Trim() && c.AWB_Serial.Trim() == invoiceCancel.AWB_Serial && c.Hawb.Trim() == invoiceCancel.Hawb && c.InvoiceTotalNoVatAmount == invoiceCancel.Total_Amount && c.ObjectType == invoiceCancel.ObjectType).OrderBy(c=>c.Created).ToList()[0];
            }
        }
    }
}
