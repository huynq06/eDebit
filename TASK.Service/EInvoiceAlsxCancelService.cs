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

namespace TASK.Service
{
    public class EInvoiceAlsxCancelService
    {
        public static void CanCelInvoice()
        {
            DateTime dt = new DateTime(2022, 04, 10, 0, 0, 0);
            List<Invoice> listInvoiceCancel = Invoice.GetInvoiceCancel();
            if (listInvoiceCancel.Count > 0)
            {
                foreach(var invoice in listInvoiceCancel)
                {
                    string rp = "";
                    Account account = new Account();
                    account.username = AppSetting.UserNameEInvoiceALSx;
                    account.password = AppSetting.PassWordEInvoiceALSx;
                    adj adj = new adj();
                    adj.rdt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    adj.rea = "Sai thong tin hoa don";
                    adj.ref_ = invoice.InvoiceID;
                    inv inv = new inv();
                    inv.seq = invoice.InvoiceFieldForm + "-" + invoice.InvoiceFieldSerial + "-" + invoice.Sequence.ToString().PadLeft(7, '0');
                    if (invoice.InvoiceFieldSerial == "AB/20E")
                    {
                        inv.seq = invoice.InvoiceFieldForm + "-" + invoice.InvoiceFieldSerial + "-" + invoice.Sequence.ToString().PadLeft(8, '0');
                    }
                    //if (invoice.ID < 104619)
                    //{
                    //    inv.seq = "01GTKT0/001-AA/20E-" + invoice.Sequence.ToString().PadLeft(7, '0');
                    //}
                    //else if(invoice.Created > dt)
                    //{
                    //    inv.seq = "01GTKT0/001-AB/20E-" + invoice.Sequence.ToString().PadLeft(7, '0');
                    //}
                    //else
                    //{
                    //    inv.seq = AppSetting.SeqCancelALSx + invoice.Sequence.ToString().PadLeft(7, '0');
                    //}
                  
                    inv.adj = adj;
                    InvoiceCancel invoiceCancel = new InvoiceCancel();
                    invoiceCancel.user = account;
                    invoiceCancel.inv = inv;
                    string jsonResult = JsonConvert.SerializeObject(invoiceCancel);
                    string jsonInvoiceCancel = jsonResult.Replace("ref_", "ref");
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
                        rp = rq.Execute(jsonInvoiceCancel, "POST", "EinvoiceALSxCacel" + invoice.ID, ref check, ref exceptionStatus);
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
                        Log.WriteLog(ex.ToString() + invoice.ID, "InvoiceCancel.txt");
                    }
                }
            }
        }
    }
}
