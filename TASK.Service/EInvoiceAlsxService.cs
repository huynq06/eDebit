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
    public class EInvoiceAlsxService
    {
        public static object _locker = new object();
        public static List<Invoice> _listInvoieToUpdate;
      //  public static List<Print> _listPrintToInsert;
        public static List<ResponseMessage> _listMessage;
        public static void EConnect()
        {
            _listMessage = new List<ResponseMessage>();
            processData();
        }
        public static void processData()
        {
            List<Invoice> listInvoice = new List<Invoice>();
            listInvoice = Invoice.GetData();
            //List<InvoiceDetail> listInvoiceDetail = InvoiceDetail.GetAll();
            if (listInvoice.Count > 0)
            {
                foreach (var invoice in listInvoice)
                {
                    string rp = "";
                    string jsonResult = JsonFomatAlsx.JsonInvoiceALSX(invoice);
                    LogSentMessageAlsx logMsg = new LogSentMessageAlsx();
                    logMsg.InvoiceID = invoice.InvoiceID;
                    logMsg.LogMessage = jsonResult;
                    logMsg.MessageType = "CREATE INVOICE";
                    logMsg.Created = DateTime.Now;
                    LogSentMessageAlsx.Insert(logMsg);
                   // LogJsonCreateInvoice(invoice.InvoiceIsn, jsonResult);
                    Common.HttpRequest rq = new Common.HttpRequest();
                    try
                    {
                        string url = AppSetting.URL_Appr;
                        rq.Url = url;
                        bool check = false;
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        // the code that you want to measure comes here
                        int exceptionStatus = 0;
                        rp = rq.Execute(jsonResult, "POST", "EinvoiceAlsx" + invoice.InvoiceID, ref check,ref exceptionStatus);
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        if (check)
                        {
                            JObject jObj = JObject.Parse(rp);                 // Parse the object graph
                            int seq = int.Parse(jObj["seq"].ToString());
                            string reference = jObj["id"].ToString();
                            invoice.Status = 1;
                            invoice.Sequence = seq;
                            invoice.Reference = reference;
                            invoice.InvoiceStatus = 1;
                            invoice.InvoiceDescription = "PHÊ DUYỆT";
                            invoice.ExceptionStatus = 0;
                            Invoice.Update(invoice);
                            ResponseMessageAlsx message = new ResponseMessageAlsx();
                            message.MsgReturn = "OK";
                            message.InvoiceID = invoice.InvoiceID;
                            message.Created = DateTime.Now;
                            message.MesageType = "CREATE INVOICE";
                            message.InvoiceStatus = 1;
                            ResponseMessageAlsx.Insert(message);
                            //lay base 64 de in cho khach
                            string rpPDF = "";
                            Common.HttpRequest rqPdf = new Common.HttpRequest();
                            rqPdf.Credentials = new Credentials()
                            {
                                UserName = AppSetting.UserNameEInvoiceALSx,
                                Password = AppSetting.PassWordEInvoiceALSC
                            };
                            string urlPdf = AppSetting.URL_SearchPDFALSX + "&sid=" + invoice.InvoiceID + "&type=pdf";
                            rqPdf.Url = urlPdf;
                            bool checkPDF = false;
                            int i = 0;
                            int exceptionStatusPDF = 0;
                            do
                            {
                                rpPDF = rqPdf.Execute(null, "GET", "EinvoiceAlsxPDF" + invoice.InvoiceID, ref checkPDF,ref exceptionStatusPDF);
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
                                //save DB
                                InvoicePDF pdf = new InvoicePDF();
                                pdf.InvoiceID = invoice.InvoiceID;
                                pdf.PDF = objBase64;
                                pdf.Created = DateTime.Now;
                                InvoicePDF.Insert(pdf);
                                ResponseMessageAlsx messagePDF = new ResponseMessageAlsx();
                                messagePDF.MsgReturn = "OK PDF";
                                messagePDF.InvoiceID = invoice.InvoiceID;
                                messagePDF.Created = DateTime.Now;
                                messagePDF.MesageType = "CREATE PDF";
                                messagePDF.InvoiceStatus = 1;
                                //ghi ra file de in
                            }
                            else
                            {
                                ResponseMessageAlsx messagePDF = new ResponseMessageAlsx();
                                messagePDF.MsgReturn = rpPDF;
                                messagePDF.InvoiceID = invoice.InvoiceID;
                                messagePDF.Created = DateTime.Now;
                                messagePDF.MesageType = "CREATE PDF";
                                messagePDF.InvoiceStatus = 0;
                                ResponseMessageAlsx.Insert(messagePDF);
                            }
                        }
                        else
                        {
                            WriteLog(jsonResult, "ErrorInvoice" + invoice.ID);
                            invoice.Status = 0;
                            invoice.InvoiceStatus = 0;
                            invoice.InvoiceDescription = rp;
                            invoice.Retry = invoice.Retry + 1;
                            invoice.ExceptionStatus = exceptionStatus;
                            Invoice.Update(invoice);
                            ResponseMessageAlsx message = new ResponseMessageAlsx();
                            message.MsgReturn = rp;
                            message.InvoiceID = invoice.InvoiceID;
                            message.Created = DateTime.Now;
                            message.MesageType = "CREATE INVOICE";
                            message.InvoiceStatus = 0;
                            ResponseMessageAlsx.Insert(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.ToString(), "ProcessEinvoiceAlsx.txt");
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
