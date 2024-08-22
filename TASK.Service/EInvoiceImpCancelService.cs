using DATAACCESS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;
using TASK.Model.DBModel;
using TASK.Model.DBModel.TT78;
using TASK.Model.ViewModel;
using TASK.Settings;
namespace TASK.Service
{
    public static class EInvoiceImpCancelService
    {
        public static void CancelInvoice()
        {
            DateTime dt = new DateTime(2022, 01, 01, 0, 0, 0);
            List<InvoiceCancelCheck> listInvoiceCheckQuery = new List<InvoiceCancelCheck>();
            listInvoiceCheckQuery = new InvoiceImpCancelAccess().GetAllInvoiceCancelChecK(DateTime.Now);
            // lay danh sach hoa don Huy
            // List<InvoiceCancelViewModel> listInvoiceInsCancel =  new InvoiceImpCancelAccess().GetAllInvoice(DateTime.Now);
            if (listInvoiceCheckQuery.Count > 0)
            {
                var listInvoiceCheckQueryDB = InvoiceCancelCheck.GetData();
                foreach (var invoiceCheckQuery in listInvoiceCheckQuery)
                {
                    if (!listInvoiceCheckQueryDB.Any(c => c.InvoiceIsn == invoiceCheckQuery.InvoiceIsn))
                    {
                        try
                        {
                            InvoiceCancelViewModel objCancel = new InvoiceImpCancelAccess().GetAllInvoice(invoiceCheckQuery.InvoiceIsn);
                            InvoiceCancelCheck invoiceCheck = new InvoiceCancelCheck();
                            invoiceCheck.InvoiceIsn = invoiceCheckQuery.InvoiceIsn;
                            invoiceCheck.Created = DateTime.Now;
                            InvoiceCancelCheck.Insert(invoiceCheck);
                            List<HermesInvoice> invoiceTemp = new List<HermesInvoice>();
                            invoiceTemp = HermesInvoice.GetInvoiceTempToCancelFromStartGolive(objCancel, dt);
                          //  invoiceTemp = HermesInvoice.GetInvoiceTempToCancel(objCancel);
                            if (invoiceTemp.Count > 0)
                            {
                                var listInvoiceSelect = invoiceTemp.Where(c => c.InvoiceTotalNoVatAmount == objCancel.Total_Amount).OrderBy(c => c.Created).ToList();
                                if (listInvoiceSelect.Count > 0)
                                {
                                    foreach(var invoice in listInvoiceSelect)
                                    {
                                        if (invoice != null && invoice.IsCancel == false && invoice.InvoiceStatus != 3)
                                        {
                                            bool check = new InvoiceImpCancelAccess().CheckInvoiceCancelAddtional(invoice.Created.Value, invoice.InvoiceIsn);
                                            if (check)
                                            {
                                                if(invoice.InvoiceFieldSerial.StartsWith("A"))
                                                {
                                                    ExecuteCancelInvoice(objCancel.Reason, invoice);
                                                }
                                               else
                                                {
                                                    ExecuteCancelInvoiceTT78(objCancel.Reason, invoice);
                                                }
                                            }
                                        }
                                    }
                                   
                                   
                                }
                                else
                                {
                                    foreach (var item in invoiceTemp)
                                    {
                                        bool check = new InvoiceImpCancelAccess().CheckInvoiceCancelAddtional(item.Created.Value, item.InvoiceIsn);
                                        if (check && item.IsCancel == false && item.InvoiceStatus != 3)
                                        {
                                            if (item.InvoiceFieldSerial.StartsWith("A"))
                                            {
                                                ExecuteCancelInvoice(objCancel.Reason, item);
                                            }
                                            else
                                            {
                                                ExecuteCancelInvoiceTT78(objCancel.Reason, item);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {

                            Log.WriteLog(ex.ToString() + invoiceCheckQuery.InvoiceIsn, "GetInvoiceCancel.txt");
                        }
                    }
                }

            }
            
        }

        public static void CancelInvoiceTest()
        {
            DateTime dt = new DateTime(2020, 11, 01, 0, 0, 0);
            List<InvoiceCancelCheck> listInvoiceCheckQuery = new List<InvoiceCancelCheck>();
            listInvoiceCheckQuery = new InvoiceImpCancelAccess().GetAllInvoiceCancelChecK(DateTime.Now);
            string invoiceCheckQuery = "30040705028517";
            // lay danh sach hoa don Huy
            // List<InvoiceCancelViewModel> listInvoiceInsCancel =  new InvoiceImpCancelAccess().GetAllInvoice(DateTime.Now);
            InvoiceCancelViewModel objCancel = new InvoiceImpCancelAccess().GetAllInvoice(invoiceCheckQuery);
            InvoiceCancelCheck invoiceCheck = new InvoiceCancelCheck();
            invoiceCheck.InvoiceIsn = invoiceCheckQuery;
            invoiceCheck.Created = DateTime.Now;
            //   InvoiceCancelCheck.Insert(invoiceCheck);
            List<HermesInvoice> invoiceTemp = new List<HermesInvoice>();
            invoiceTemp = HermesInvoice.GetInvoiceTempToCancelFromStartGolive(objCancel, dt);
            //  invoiceTemp = HermesInvoice.GetInvoiceTempToCancel(objCancel);
            if (invoiceTemp.Count > 0)
            {
                var listInvoiceSelect = invoiceTemp.Where(c => c.InvoiceTotalNoVatAmount == objCancel.Total_Amount).OrderBy(c => c.Created).ToList();
                if (listInvoiceSelect.Count > 0)
                {
                    foreach (var invoice in listInvoiceSelect)
                    {
                        if (invoice != null && invoice.IsCancel == false && invoice.InvoiceStatus != 3)
                        {
                            bool check = new InvoiceImpCancelAccess().CheckInvoiceCancelAddtional(invoice.Created.Value, invoice.InvoiceIsn);
                            if (check)
                            {
                                //ExecuteCancelInvoice(objCancel.Reason, invoice);
                            }
                        }
                    }


                }
                else
                {
                    foreach (var item in invoiceTemp)
                    {
                        bool check = new InvoiceImpCancelAccess().CheckInvoiceCancelAddtional(item.Created.Value, item.InvoiceIsn);
                        if (check && item.IsCancel == false && item.InvoiceStatus != 3)
                        {
                            //ExecuteCancelInvoice(objCancel.Reason, item);
                            break;
                        }
                    }
                }

            }
        }
        public static void ExecuteCancelInvoice(string reason, HermesInvoice invoice)
        {
            string rp = "";
            Account account = new Account();
            account.username = AppSetting.UserNameEInvoiceALSC;
            account.password = AppSetting.PassWordEInvoiceALSC;
            adj adj = new adj();
            adj.rdt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            adj.rea = reason;
            adj.ref_ = invoice.InvoiceIsn;
            inv inv = new inv();
            inv.seq = invoice.InvoiceFieldForm + "-" + invoice.InvoiceFieldSerial+"-" + invoice.Sequence.ToString().PadLeft(7, '0');
            inv.adj = adj;
            InvoiceCancel invoiceCancel = new InvoiceCancel();
            invoiceCancel.user = account;
            invoiceCancel.inv = inv;
            string jsonResult = JsonConvert.SerializeObject(invoiceCancel);
            string jsonInvoiceCancel = jsonResult.Replace("ref_", "ref");
            Common.HttpRequest rq = new Common.HttpRequest();
            invoice.IsCancel = true;
            try
            {
                string url = AppSetting.URL_Cancel;
                rq.Url = url;
                bool check = false;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int exceptionStatus = 0;
                // the code that you want to measure comes here
                invoice.TimeSent = DateTime.Now;
                //log trc khi huy
                LogJsonCancelInvoice(invoice.InvoiceIsn, jsonInvoiceCancel);
                rp = rq.Execute(jsonInvoiceCancel, "POST", "EinvoiceCacel" + invoice.InvoiceIsn, ref check, ref exceptionStatus);
                if (check)
                {
                    //update trang thai huy
                    invoice.InvoiceStatus = Common.CommonConstants.INVOICECANCEL;
                    invoice.InvoiceDescription = Common.CommonConstants.CANCEL;
                    invoice.CancelReason = reason;
                    invoice.CancelDateTime = DateTime.Now;
                    invoice.ExceptionStatus = 0;
                    HermesInvoice.UpdateCancel(invoice);
                    ResponseMessage message = new ResponseMessage();
                    message.ReturnCodeField = "OK";
                    message.KeyField = invoice.InvoiceIsn;
                    message.SHDonField = invoice.Sequence.ToString();
                    message.ApiType = "CANCEL INVOICE";
                    message.Created = DateTime.Now;
                    ResponseMessage.Insert(message);
                }
                else
                {
                    invoice.ExceptionStatus = exceptionStatus;
                    invoice.CancelReason = reason;
                    HermesInvoice.UpdateCancel(invoice);
                    ResponseMessage message = new ResponseMessage();
                    message.ReturnCodeField = "ErrorCancel";
                    message.KeyField = invoice.InvoiceIsn;
                    message.SHDonField = invoice.Sequence.ToString();
                    message.ApiType = "CANCEL INVOICE";
                    message.Created = DateTime.Now;
                    ResponseMessage.Insert(message);
                }
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString() + invoice.InvoiceIsn, "InvoiceCancel.txt");
            }
        }

        public static void ExecuteCancelInvoiceTT78(string reason, HermesInvoice invoice)
        {
            string rp = "";
            Account account = new Account();
            account.username = AppSetting.UserNameEInvoiceALSC;
            account.password = AppSetting.PassWordEInvoiceALSC;
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
            wr.stax = AppSetting.StaxALSC;
            wr.noti_taxtype = "1";
            wr.noti_taxnum = "";
            wr.noti_taxdt = "";
            wr.budget_relationid = "";
            wr.place = "Hà Nội";
            wr.items = listItem;
            invoiceCancel.wrongnotice = wr;
            string jsonResult = JsonConvert.SerializeObject(invoiceCancel);
            Common.HttpRequest rq = new Common.HttpRequest();
            invoice.IsCancel = true;
            try
            {
                string url = AppSetting.URL_CancelTT78;
                rq.Url = url;
                bool check = false;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int exceptionStatus = 0;
                // the code that you want to measure comes here
                invoice.TimeSent = DateTime.Now;
                //log trc khi huy
                LogJsonCancelInvoice(invoice.InvoiceIsn, jsonResult);
                rp = rq.Execute(jsonResult, "POST", "EinvoiceCacel" + invoice.InvoiceIsn, ref check, ref exceptionStatus);
                if (check)
                {
                    //update trang thai huy
                    invoice.InvoiceStatus = Common.CommonConstants.INVOICECANCEL;
                    invoice.InvoiceDescription = Common.CommonConstants.CANCEL;
                    invoice.CancelReason = reason;
                    invoice.CancelDateTime = DateTime.Now;
                    invoice.ExceptionStatus = 0;
                    HermesInvoice.UpdateCancel(invoice);
                    ResponseMessage message = new ResponseMessage();
                    message.ReturnCodeField = "OK";
                    message.KeyField = invoice.InvoiceIsn;
                    message.SHDonField = invoice.Sequence.ToString();
                    message.ApiType = "CANCEL INVOICE";
                    message.Created = DateTime.Now;
                    ResponseMessage.Insert(message);
                }
                else
                {
                    invoice.ExceptionStatus = exceptionStatus;
                    invoice.CancelReason = reason;
                    HermesInvoice.UpdateCancel(invoice);
                    ResponseMessage message = new ResponseMessage();
                    message.ReturnCodeField = "ErrorCancel";
                    message.KeyField = invoice.InvoiceIsn;
                    message.SHDonField = invoice.Sequence.ToString();
                    message.ApiType = "CANCEL INVOICE";
                    message.Created = DateTime.Now;
                    ResponseMessage.Insert(message);
                }
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString() + invoice.InvoiceIsn, "InvoiceCancel.txt");
            }
        }
        public static object _locker = new object();
        public static void LogJsonCancelInvoice(string invoiceIsn, string text)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.ToString("yyyy-MM");
                string day = DateTime.Now.Day.ToString();
                string folderPath = @"C://EInvoice/Cancel/" + year + "/" + month + "/" + day;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filename = invoiceIsn + ".txt";
                string filePath = Path.Combine(folderPath, filename);

                lock (_locker)
                {
                    using (Stream s = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (StreamWriter w = new StreamWriter(s))
                        {
                            w.WriteLine(text);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
