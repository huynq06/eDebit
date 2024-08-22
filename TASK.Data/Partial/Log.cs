using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;
using Utils;

namespace TASK.Data
{
    public class Log
    {
        public static object _locker = new object();

        private static ConcurrentQueue<Log> _LogQueue = new ConcurrentQueue<Log>();

        public static int QueueCount()
        {
            return _LogQueue.Count;
        }

        private static List<Log> DeQueue(int batchSize)
        {
            List<Log> retList = new List<Log>();
            while (retList.Count < batchSize && !_LogQueue.IsEmpty)
            {
                Log dequeuElement = null;
                _LogQueue.TryDequeue(out dequeuElement);
                if (dequeuElement != null) retList.Add(dequeuElement);
            }
            return retList;
        }

        //public static void InsertLog(Exception ex, string logSource, string warningType)
        //{
        //    InsertLog(ex.Message, ex.StackTrace, (!string.IsNullOrWhiteSpace(logSource) ? logSource + " | " : "") + ex.Source, warningType);
        //}

        //public static void InsertLog(string logMessage, string logTrace, string logSource, string warningType)
        //{
        //    Log log = new Log();
        //    log.LogSource = logSource;
        //    log.LogMessage = logMessage;
        //    log.LogTime = DateTime.Now;
        //    log.LogTrace = logTrace;
        //    log.LogType = warningType;
        //    log.Status = true;
        //    _LogQueue.Enqueue(log);
        //}

        public static void BulkInsert()
        {
            List<Log> logs = DeQueue(AppSetting.BatchSize);
            if (logs.Count > 0)
            {
                try
                {
                    //Cố gắng dequeu thêm dữ liệu để xử lý
                    if (logs.Count < AppSetting.BatchSize)
                    {
                        DateTime now = DateTime.Now;
                        while ((logs.Count < AppSetting.BatchSize) && (DateTime.Now - now < TimeSpan.FromMilliseconds(AppSetting.UpdateDataInterval)))
                        {
                            logs.AddRange(DeQueue(AppSetting.BatchSize));
                        }
                    }
                    //DataAccess.BulkInsert<Log>(logs, "Log");
                }
                catch
                {
                    try
                    {
                        lock (_locker)
                        {
                            Xml.WriteListToFile<Log>(logs, AppSetting.LogBackupFileName);
                        }
                    }
                    catch
                    {

                    }
                }

                //// Insert sang Alert System
                //if (AppSetting.AlertSystemConnected)
                //{
                //    try
                //    {
                //        List<Warning_Message> warningMessages = new List<Warning_Message>();
                //        foreach (Log log in logs)
                //        {
                //            Warning_Message wm = new Warning_Message();
                //            wm.Message = AppSetting.ApplicationName + " | " + log.LogSource + " | " + log.LogMessage;
                //            if (wm.Message.Length > 1000) wm.Message = wm.Message.Remove(1000);
                //            wm.Message_Guid = Guid.NewGuid();
                //            wm.Message_Time = log.LogTime.HasValue ? log.LogTime.Value : DateTime.Now;
                //            wm.Message_Trace = log.LogTrace;
                //            wm.Status = false;
                //            wm.WarningSystem = AppSetting.SystemID;
                //            wm.WarningType = log.LogType;
                //            warningMessages.Add(wm);
                //        }
                //        DataAccess.BulkInsert<Warning_Message>(warningMessages, "Warning_Message", AppSetting.GetAlertSystemConnection());
                //    }
                //    catch
                //    {
                //    }
                //}
            }
        }

        public static void EnqueueXmlData()
        {
            try
            {
                string fileName = AppSetting.LogBackupFileName;
                if (System.IO.File.Exists(fileName))
                {
                    SqlConnection conn = AppSetting.GetConnection();
                    if (conn.State == ConnectionState.Open)
                    {
                        List<Log> listLog = Xml.ReadFromFileToList<Log>(fileName);
                        // Sau khi đọc file xml xong thì xóa file đi.
                        System.IO.File.Delete(fileName);

                        if (listLog != null && listLog.Count > 0)
                        {
                            foreach (Log item in listLog)
                            {
                                _LogQueue.Enqueue(item);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public static List<Log> Paging(DBContextDataContext context, int currentPage, int pageSize, ref int totalRecord, DateTime? fromTime, DateTime? toTime)
        //{
        //    var result = context.Logs.OrderByDescending(p => p.LogID).AsQueryable();
        //    if (fromTime != null)
        //        result = result.Where(p => p.LogTime >= fromTime);
        //    if (toTime != null)
        //        result = result.Where(p => p.LogTime < toTime.Value.AddDays(1));
        //    totalRecord = result.Count();
        //    return result
        //            .Skip(((currentPage - 1) < 0 ? 0 : (currentPage - 1)) * pageSize)
        //            .Take(pageSize)
        //            .ToList();
        //}

        public static void LogToText(string contents, string separateName)
        {
            try
            {
                contents += Environment.NewLine;
                File.AppendAllText(GetLogFilePath(DateTime.Now, separateName), contents);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void LogToText(Guid? smsGuid, string result, string separateName)
        {
            try
            {
                string line = string.Format("Guid = {0}; Result = {1}.", smsGuid ?? Guid.Empty, result ?? "") + Environment.NewLine;
                File.AppendAllText(GetLogFilePath(DateTime.Now, separateName), line);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void LogToTexts(string[] smsGuid, string[] result, string separateName)
        {
            try
            {
                using (StreamWriter stream = File.AppendText(GetLogFilePath(DateTime.Now, separateName)))
                {
                    for (int i = 0; i < smsGuid.Count(); i++)
                    {
                        string stt = "";
                        try
                        {
                            stt = result[i];
                        }
                        catch (Exception)
                        {
                            stt = "result array is out of range";
                        }
                        string line = string.Format("Guid = {0}; Result = {1}.", smsGuid[i] ?? Guid.Empty.ToString(), stt);
                        stream.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static string GetLogFilePath(DateTime dateTime, string separateName)
        {
            if (separateName == null) separateName = "";
            string year = dateTime.Year.ToString();
            string month = dateTime.ToString("yyyy-MM");
            string fileName = dateTime.ToString("yyyy-MM-dd") + "_" + separateName + ".txt";
            string folderPath = "";
            folderPath = Path.Combine(AppSetting.LogPath, year, month);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, fileName);
            return filePath;
        }
        public static void WriteLog(string logText)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-log.txt";
            WriteLog(logText, fileName);
        }

        public static void WriteLog(string logText, string fileName)
        {
            try
            {
                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.ToString("yyyy-MM");
                string day = DateTime.Now.Day.ToString();
                string folderPath = @"C://EInvoice/ErrorLog/" + year + "/" + month + "/" + day;
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

        public static void WriteLog(Exception ex)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "-log.txt";
            WriteLog(ex, fileName);
        }

        public static void WriteLog(Exception ex, string fileName)
        {
            WriteLog(ex.ToString(), fileName);
        }
    }
}
