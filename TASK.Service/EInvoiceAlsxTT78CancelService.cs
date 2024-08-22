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
    public class EInvoiceAlsxTT78CancelService
    {
        public static void CanCelTT78Invoice()
        {
            List<Invoice> listInvoiceCancel = Invoice.GetInvoiceCancel();
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
                    item.form = invoice.InvoiceFieldForm;
                    item.serial = invoice.InvoiceFieldSerial;
                    //if (invoice.Code == "ALSW_GL")
                    //{
                    //    item.serial = AppSetting.InvoiceFieldSerialALSx_GLTT78;
                    //}
                    item.seq = invoice.Sequence.ToString().PadLeft(8, '0');
                    item.idt = invoice.Idt;
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
                        rp = rq.Execute(jsonResult, "POST", "EinvoiceALSxCacelTT78" + invoice.ID, ref check, ref exceptionStatus);
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
                        Log.WriteLog(ex.ToString() + invoice.ID, "InvoiceCancelTT78.txt");
                    }
                }
            }
        }
    }
}
