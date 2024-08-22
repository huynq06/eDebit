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
    public static class EInvoiceConnectService
    {
        public static object _locker = new object();
        //Lay day sach Hoa don can tao EInvoice
        public static List<HermesInvoice> _listInvoieHermesToUpdate;
        public static List<Print> _listPrintToInsert;
        public static List<ResponseMessage> _listMessage;
        public static void EConnect()
        {
            _listInvoieHermesToUpdate = new List<HermesInvoice>();
            _listPrintToInsert = new List<Print>();
            _listMessage = new List<ResponseMessage>();
            processData();
        }
        public static void processData()
        {
            List<HermesInvoice> listInvoiceHermes = new List<HermesInvoice>();
            listInvoiceHermes = HermesInvoice.GetData();
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
                        rp = rq.Execute(jsonResult, "POST", "Einvoice" + invoice.InvoiceIsn, ref check, ref exceptionStatus);
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        if (check)
                        {
                            LogJsonResponseInvoice(invoice.InvoiceIsn, rp);
                            JObject jObj = JObject.Parse(rp);
                            // Parse the object graph
                            int seq = int.Parse(jObj["seq"].ToString());
                            string reference = jObj["id"].ToString();
                            string invoiceSearchLink = jObj["link"].ToString();
                            int status = int.Parse(jObj["status"].ToString());
                            string searchCode = jObj["sec"].ToString();
                            string idt = jObj["idt"].ToString();
                            invoice.Status = true;
                            invoice.TimeReponse = DateTime.Now;
                            invoice.ExecuteTime = Math.Round((double)elapsedMs / 1000, 2).ToString();
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
                            //lay base 64 de in cho khach
                            //ktra status = 3 moi lay base
                            if (status == 3)
                            {
                                string rpPDF = "";
                                Common.HttpRequest rqPdf = new Common.HttpRequest();
                                rqPdf.Credentials = new Credentials()
                                {
                                    UserName = AppSetting.UserNameEInvoiceALSC,
                                    Password = AppSetting.PassWordEInvoiceALSC
                                };
                                string urlPdf = AppSetting.URL_SearchPDF + "&sid=" + invoice.InvoiceIsn + "&type=pdf";
                                rqPdf.Url = urlPdf;
                                bool checkPDF = false;
                                int i = 0;
                                int exceptionStatusPdf = 0;
                                do
                                {
                                    rpPDF = rqPdf.Execute(null, "GET", "EinvoicePDF" + invoice.InvoiceIsn, ref checkPDF, ref exceptionStatusPdf);
                                    if (checkPDF)
                                    {
                                        break;
                                    }

                                    else
                                    {
                                        i++;
                                        Thread.Sleep(1000);
                                    }
                                } while (i < 2);


                                if (checkPDF)
                                {
                                    var jArray = JArray.Parse(rpPDF);
                                    string obj = jArray[0]["pdf"].ToString();
                                    string objBase64 = obj.Replace("data:application/pdf;base64,", "");
                                    try
                                    {
                                        List<Print> prints = new List<Print>();
                                        Print printcopy1 = new Print();
                                        Print printcopy2 = new Print();
                                        PrintConfig prinConfig = new PrintConfig();
                                        prinConfig = PrintConfig.GetByPrintNetwork(invoice.PrinterNetworkName);
                                        if (prinConfig != null)
                                        {
                                            printcopy1.Status = false;
                                            printcopy1.PrintIns = invoice.InvoiceIsn;
                                            printcopy1.PrintLink = CreatePdf(invoice.InvoiceIsn, objBase64, 1);
                                            printcopy1.PrintShareLink = printcopy1.PrintLink.Replace("C:/", "Y:/");
                                            printcopy1.PrintUser = prinConfig.PrintName;
                                            printcopy1.GroupName = prinConfig.GroupName.Value;
                                            printcopy1.ThreadInstance = prinConfig.ID;
                                            printcopy1.Created = DateTime.Now;
                                            printcopy1.Copy = 1;
                                            prints.Add(printcopy1);
                                            printcopy2.Status = false;
                                            printcopy2.PrintIns = invoice.InvoiceIsn;
                                            printcopy2.PrintLink = CreatePdf(invoice.InvoiceIsn, objBase64, 2);
                                            printcopy2.PrintShareLink = printcopy2.PrintLink.Replace("C:/", "Y:/");
                                            printcopy2.PrintUser = prinConfig.PrintName;
                                            printcopy2.GroupName = prinConfig.GroupName.Value;
                                            printcopy2.ThreadInstance = prinConfig.ID;
                                            printcopy2.Created = DateTime.Now;
                                            printcopy2.Copy = 2;
                                            prints.Add(printcopy2);
                                            Print.Insert(prints);
                                        }
                                        else
                                        {
                                            printcopy1.Status = false;
                                            printcopy1.PrintIns = invoice.InvoiceIsn;
                                            printcopy1.PrintLink = CreatePdf(invoice.InvoiceIsn, objBase64, 1);
                                            printcopy1.PrintShareLink = printcopy1.PrintLink.Replace("C:/", "Y:/");
                                            printcopy1.PrintUser = "HD_PC113";
                                            printcopy1.GroupName = 11;
                                            printcopy1.ThreadInstance = 11;
                                            printcopy1.Created = DateTime.Now;
                                            printcopy1.Copy = 1;
                                            prints.Add(printcopy1);
                                            printcopy2.Status = false;
                                            printcopy2.PrintIns = invoice.InvoiceIsn;
                                            printcopy2.PrintLink = CreatePdf(invoice.InvoiceIsn, objBase64, 2);
                                            printcopy2.PrintShareLink = printcopy2.PrintLink.Replace("C:/", "Y:/");
                                            printcopy2.PrintUser = "HD_PC114";
                                            printcopy2.GroupName = 11;
                                            printcopy2.ThreadInstance = 11;
                                            printcopy2.Created = DateTime.Now;
                                            printcopy2.Copy = 2;
                                            prints.Add(printcopy2);
                                            Print.Insert(prints);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLog(ex, string.Format("{0}-{1}.txt", invoice.InvoiceIsn + "Print", DateTime.Now.ToString("yyyy-MM-dd")));
                                    }
                                }
                                else
                                {
                                    ResponseMessage messagePDF = new ResponseMessage();
                                    messagePDF.ReturnCodeField = "ErrorPDF";
                                    messagePDF.KeyField = invoice.InvoiceIsn;
                                    messagePDF.Created = DateTime.Now;
                                    message.ErrorMessageField = rpPDF;
                                    message.KHHDonField = "PDF";
                                    ResponseMessage.Insert(message);
                                }
                            }
                        }
                        else
                        {
                            Log.WriteLog(jsonResult, "ErrorInvoice" + invoice.InvoiceIsn);
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
        public static void LogJsonResponseInvoice(string invoiceIsn, string text)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.ToString("yyyy-MM");
                string day = DateTime.Now.Day.ToString();
                string folderPath = @"C://EInvoiceLogResponseJson/" + year + "/" + month + "/" + day;
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
        public static string CreatePdf(string invoiceIsn, string objBase64,int index)
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.ToString("yyyy-MM");
            string day = DateTime.Now.Day.ToString();
            string folderPath = @"C://EInvoice/" + year + "/" + month + "/" + day;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filename = invoiceIsn + "_" + index + ".pdf";
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
        public static void GetFolderFileByContent(string content,string invoiceisn)
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
    }
}
