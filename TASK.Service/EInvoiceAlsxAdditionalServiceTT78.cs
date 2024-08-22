using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;
using TASK.Data;
using TASK.Model.DBModel;
using TASK.Service.vn.cinvoice.api;
using TASK.Settings;
using System.Threading;
using TASK.Model.DBModel.TT78;

namespace TASK.Service
{
    public class EInvoiceAlsxAdditionalServiceTT78
    {
        public static object _locker = new object();
        public static void Retry()
        {
            CancelInvoiceAddtional();
        }
        public static void CancelInvoiceAddtional()
        {
            List<Invoice> listInvoiceCancel = Invoice.GetInvoiceCancelAddtional();
            if (listInvoiceCancel.Count > 0)
            {
                foreach (var invoice in listInvoiceCancel)
                {
                    string rp = "";
                    Account account = new Account();
                    account.username = AppSetting.UserNameEInvoiceALSx;
                    account.password = AppSetting.PassWordEInvoiceALSx;
                    List<item78> listItem = new List<item78>();
                    item78 item = new item78();
                    item.form = AppSetting.InvoiceFieldFormALSxTT78;
                    item.serial = AppSetting.InvoiceFieldSerialALSx_MDTT78;
                    if (invoice.Code == "ALSW_GL")
                    {
                        item.serial = AppSetting.InvoiceFieldSerialALSx_GLTT78;
                    }
                    item.seq = invoice.Sequence.ToString().PadLeft(8, '0');
                    item.idt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    item.type_ref = 1;
                    item.noti_type = "1";
                    item.rea = "Sai thong tin hoa don";
                    listItem.Add(item);
                    InvoiceCancel78 invoiceCancel = new InvoiceCancel78();
                    invoiceCancel.user = account;
                    invoiceCancel.lang = "";
                    wrongnotice wr = new wrongnotice();
                    wr.stax = AppSetting.StaxALSW;
                    wr.noti_taxtype = "1";
                    wr.noti_taxnum = "";
                    wr.noti_taxdt = "";
                    wr.budget_relationid = "";
                    wr.place = "Hà Nội";
                    wr.items = listItem;
                    invoiceCancel.wrongnotice = wr;
                    string jsonResult = JsonConvert.SerializeObject(invoiceCancel);
                    //string jsonInvoiceCancel = jsonResult.Replace("ref_", "ref");
                    Common.HttpRequest rq = new Common.HttpRequest();
                    try
                    {
                        string url = AppSetting.URL_Cancel;
                        rq.Url = url;
                        bool check = false;
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        LogSentMessageAlsx logMsg = new LogSentMessageAlsx();
                        logMsg.InvoiceID = invoice.InvoiceID;
                        logMsg.LogMessage = jsonResult;
                        logMsg.MessageType = "CANCEL INVOICE";
                        logMsg.Created = DateTime.Now;
                        LogSentMessageAlsx.Insert(logMsg);
                        int exceptionStatus = 0;
                        rp = rq.Execute(jsonResult, "POST", "EinvoiceALSxCacelAddtionalTT78" + invoice.ID, ref check, ref exceptionStatus);
                        if (check)
                        {
                            //update trang thai huy
                            invoice.ExecuteCancel = true;
                            invoice.InvoiceStatus = 2;
                            invoice.InvoiceDescription = "HỦY";
                            invoice.ExceptionStatus = 0;
                            invoice.CancelDateTime = DateTime.Now;
                            Invoice.UpdateCancel(invoice);

                            ResponseMessageAlsx message = new ResponseMessageAlsx();
                            message.MesageType = "CANCELOK";
                            message.InvoiceID = invoice.InvoiceID;
                            message.MsgReturn = rp;
                            message.MesageType = "CANCEL INVOICE";
                            message.InvoiceStatus = 1;
                            message.Created = DateTime.Now;
                            ResponseMessageAlsx.Insert(message);
                        }
                        else
                        {
                            invoice.CancelRetry = invoice.CancelRetry + 1;
                            Invoice.UpdateCancel(invoice);
                            ResponseMessageAlsx message = new ResponseMessageAlsx();
                            message.InvoiceID = invoice.InvoiceID;
                            message.MsgReturn = rp;
                            message.MesageType = "CANCEL INVOICE";
                            message.InvoiceStatus = 0;
                            message.Created = DateTime.Now;
                            ResponseMessageAlsx.Insert(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(ex.ToString() + invoice.ID, "InvoiceALSXCancelTT78.txt");
                    }
                }
            }
        }
        public static string CreateError(string InvoiceID, string objBase64)
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.ToString("yyyy-MM");
            string day = DateTime.Now.Day.ToString();
            string folderPath = @"C://EInvoice/" + year + "/" + month + "/" + day;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filename = InvoiceID + ".pdf";
            string filePath = Path.Combine(folderPath, filename);
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                byte[] bytes = Convert.FromBase64String(objBase64);
                System.IO.FileStream stream =
             new FileStream(filePath, FileMode.CreateNew);
                System.IO.BinaryWriter writer =
                    new BinaryWriter(stream);
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();
            }
            return filePath;

        }
        public static void WriteLog(string logText, string fileName)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.ToString("yyyy-MM");
                string day = DateTime.Now.Day.ToString();
                string folderPath = @"C://EInvoiceAlsx/ErrorLog/" + year + "/" + month + "/" + day;
                logText = string.Format("========={0}=========\n{1}\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), logText);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-log.txt";
                string fullPath = Path.Combine(folderPath, fileName);
                lock (_locker)
                {
                    using (Stream s = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter w = new StreamWriter(s))
                        {
                            w.WriteLine(logText);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
