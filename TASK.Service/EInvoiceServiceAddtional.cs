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
    public class EInvoiceServiceAddtional
    {
        public static object _locker = new object();
        //lay danh sach cac hoa đơn bị exception gửi lại
        public static void Retry()
        {
            CreateInvoiceAddtional();
        }
        public static void CreateInvoiceAddtional()
        {
            var listInvoiceHermes = HermesInvoice.GetDataAddtional();
            if (listInvoiceHermes.Count > 0)
            {
                foreach (var invoice in listInvoiceHermes)
                {
                    string rp = "";
                    string jsonResult = JsonFomatInvoiceService.JsonInvoiceALSC(invoice);
                    LogJsonCreateInvoice(invoice.InvoiceIsn, jsonResult);
                    Common.HttpRequest rq = new Common.HttpRequest();
                    try
                    {
                        string url = AppSetting.URL_Appr;
                        rq.Url = url;
                        bool check = false;
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        // the code that you want to measure comes here
                        invoice.TimeSent = DateTime.Now;
                        int exceptionStatus = 0;
                            rp = rq.ExecuteAddtional(jsonResult, "POST", invoice.InvoiceIsn,"Einvoice" + invoice.InvoiceIsn, ref check, ref exceptionStatus);
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        if (check && exceptionStatus == 0)
                        {
                            JObject jObj = JObject.Parse(rp);                 // Parse the object graph
                            int seq = int.Parse(jObj["seq"].ToString());
                            string reference = jObj["id"].ToString();
                            string invoiceSearchLink = jObj["link"].ToString();
                            string searchCode = jObj["sec"].ToString();
                            int status = int.Parse(jObj["status"].ToString());
                            string idt = jObj["idt"].ToString();
                            invoice.Status = true;
                            invoice.TimeReponse = DateTime.Now;
                            invoice.ExecuteTime = elapsedMs.ToString();
                            invoice.Sequence = seq;
                            invoice.ReferenceNo = reference;
                            invoice.InvoiceStatus = CommonConstants.INVOICEAPROVE;
                            invoice.InvoiceDescription = CommonConstants.APPROVE;
                            invoice.InvoiceFieldForm = AppSetting.InvoiceFieldFormALSC;
                            invoice.InvoiceFieldType = AppSetting.InvoiceFieldTypeALSC;
                            invoice.Idt = idt;
                            if (invoice.ObjectType == "EXPORT AWB")
                            {
                                invoice.InvoiceFieldSerial = AppSetting.InvoiceFieldSerialALSC_EXPORT;
                            }
                            else
                            {
                                invoice.InvoiceFieldSerial = AppSetting.InvoiceFieldSerialALSC_IMPORT;
                            }
                            if (status == 2)
                            {
                                invoice.InvoiceStatus = CommonConstants.INVOICEWAITAPROVE;
                                invoice.InvoiceDescription = CommonConstants.WAITAPPROVE;
                            }
                            invoice.ExceptionStatus = 0;
                            invoice.EInvoiceSearchLink = invoiceSearchLink;
                            invoice.SearchCode = searchCode;
                            HermesInvoice.Update(invoice);
                            ResponseMessage message = new ResponseMessage();
                            message.ReturnCodeField = "OK";
                            message.KeyField = invoice.InvoiceIsn;
                            message.SHDonField = seq.ToString();
                            message.Created = DateTime.Now;
                            message.ApiType = "CREATE INVOICE";
                            ResponseMessage.Insert(message);
                        }
                        else if (check && exceptionStatus == 1)
                        {
                            JArray jsonArray = JArray.Parse(rp);
                            string sequence = jsonArray[0]["doc"]["seq"].ToString();
                            string idt = jsonArray[0]["doc"]["idt"].ToString();
                            int status = int.Parse(jsonArray[0]["doc"]["status"].ToString());
                            invoice.Status = true;
                            invoice.TimeReponse = DateTime.Now;
                            invoice.ExecuteTime = elapsedMs.ToString();
                            invoice.Sequence = int.Parse(sequence);
                            invoice.ReferenceNo = jsonArray[0]["doc"]["id"].ToString();
                            //invoice.EInvoiceSearchLink = jsonArray[0]["doc"]["link"].ToString();
                            invoice.SearchCode = jsonArray[0]["doc"]["sec"].ToString(); ;
                            invoice.InvoiceStatus = CommonConstants.INVOICEAPROVE;
                            invoice.InvoiceDescription = CommonConstants.APPROVE;
                            invoice.InvoiceFieldForm = AppSetting.InvoiceFieldFormALSC;
                            invoice.InvoiceFieldType = AppSetting.InvoiceFieldTypeALSC;
                            invoice.Idt = idt;
                            if (invoice.ObjectType == "EXPORT AWB")
                            {
                                invoice.InvoiceFieldSerial = AppSetting.InvoiceFieldSerialALSC_EXPORT;
                            }
                            else
                            {
                                invoice.InvoiceFieldSerial = AppSetting.InvoiceFieldSerialALSC_IMPORT;
                            }
                            if (status == 2)
                            {
                                invoice.InvoiceStatus = CommonConstants.INVOICEWAITAPROVE;
                                invoice.InvoiceDescription = CommonConstants.WAITAPPROVE;
                            }
                            invoice.ExceptionStatus = 0;
                            HermesInvoice.Update(invoice);
                            ResponseMessage message = new ResponseMessage();
                            message.ReturnCodeField = "OK";
                            message.KeyField = invoice.InvoiceIsn;
                            message.SHDonField = sequence;
                            message.Created = DateTime.Now;
                            message.ApiType = "CREATE INVOICE";
                            ResponseMessage.Insert(message);
                        }
                        else
                        {
                            Log.WriteLog(jsonResult, "ErrorInvoiceAddtional" + invoice.InvoiceIsn);
                            invoice.Status = false;
                            invoice.Retry = invoice.Retry + 1;
                            invoice.TimeReponse = DateTime.Now;
                            invoice.ExecuteTime = elapsedMs.ToString();
                            invoice.ExceptionStatus = exceptionStatus;
                            HermesInvoice.Update(invoice);
                            ResponseMessage message = new ResponseMessage();
                            message.ReturnCodeField = "Error";
                            message.KeyField = invoice.InvoiceIsn;
                            message.Created = DateTime.Now;
                            message.ErrorMessageField = rp;
                            ResponseMessage.Insert(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(ex, "ProcessEinvoice.txt");
                    }

                }
            }
        }
        public static void CancelInvoiceAddtional()
        {
            string rp = "";
            List<HermesInvoice> listInvoiceCancel = new List<HermesInvoice>();
            listInvoiceCancel = HermesInvoice.GetDataCancel();
            if (listInvoiceCancel.Count > 0)
            {
                foreach (var invoice in listInvoiceCancel)
                {
                    Account account = new Account();
                    account.username = AppSetting.UserNameEInvoiceALSC;
                    account.password = AppSetting.PassWordEInvoiceALSC;
                    adj adj = new adj();
                    adj.rdt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    adj.rea = "Sai thong tin hoa don";
                    adj.ref_ = invoice.InvoiceIsn;
                    inv inv = new inv();
                    if (invoice.ObjectType == "EXPORT AWB")
                    {
                        inv.seq = AppSetting.SeqCancel_EXPORT + invoice.Sequence.ToString().PadLeft(7, '0');
                    }
                    else
                    {
                        inv.seq = AppSetting.SeqCancel_IMPORT + invoice.Sequence.ToString().PadLeft(7, '0');
                    }

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
                        int exceptionStatus = 0;
                        // the code that you want to measure comes here
                        invoice.TimeSent = DateTime.Now;
                        //log trc khi huy
                        LogJsonCancelInvoice(invoice.InvoiceIsn, jsonInvoiceCancel);
                        rp = rq.Execute(jsonInvoiceCancel, "POST", "EinvoiceCacelAddtional" + invoice.InvoiceIsn, ref check, ref exceptionStatus);
                        if (check)
                        {
                            //update trang thai huy
                            invoice.InvoiceStatus = Common.CommonConstants.INVOICECANCEL;
                            invoice.InvoiceDescription = Common.CommonConstants.CANCEL;
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
                    catch
                    {
                    }
                }
            }
        }
     
        public static void LogJsonCreateInvoice(string invoiceIsn, string text)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.ToString("yyyy-MM");
                string day = DateTime.Now.Day.ToString();
                string folderPath = @"C://EInvoiceLogJson/" + year + "/" + month + "/" + day;
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
        public static string CreatePdf(string invoiceIsn, string objBase64)
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.ToString("yyyy-MM");
            string day = DateTime.Now.Day.ToString();
            string folderPath = @"C://EInvoice/" + year + "/" + month + "/" + day;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filename = invoiceIsn + ".pdf";
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
        public static void GetFolderFileByContent(string content, string invoiceisn)
        {
            string pathFile = string.Empty;
            try
            {
                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(content);
                string pathFolder = @"C://FolderSent/" + DateTime.Now.ToString("yyyy.MM.dd");
                Random random = new Random();
                string fileName = invoiceisn + "_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + "_" + random.Next(random.Next(Int32.MaxValue)) + ".xml";
                pathFile = pathFolder + "/" + fileName;
                if (!System.IO.Directory.Exists(pathFolder))
                    System.IO.Directory.CreateDirectory(pathFolder);
                xmlDoc.Save(pathFile);

            }
            catch (Exception)
            {

            }


        }
        public static string CreateError(string invoiceIsn, string objBase64)
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.ToString("yyyy-MM");
            string day = DateTime.Now.Day.ToString();
            string folderPath = @"C://EInvoice/" + year + "/" + month + "/" + day;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filename = invoiceIsn + ".pdf";
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
