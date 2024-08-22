using PluggableModulesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Business.staticThread;

namespace TASK.Business.staticThread
{
    public class ThreadEinvoiceAlsxTT78Management
    {
        #region singleton pattern

        List<WorkingBaseTimer> ThreadList;

        private ThreadEinvoiceAlsxTT78Management()
        {
            ThreadList = new List<WorkingBaseTimer>();
            WakeupTimer wakeup = new WakeupTimer();
            ThreadList.Add(wakeup);
            //ThreadList.Add(new EinvoiceAlsxThread());
            ThreadList.Add(new ThreadCancelEinvoiceAlsxTT78());
            //ThreadList.Add(new EinvoiceAlsxAddtionalThreadTT78());
        }

        private static object _lock = new object();
        private static ThreadEinvoiceAlsxTT78Management _instance;
        public static ThreadEinvoiceAlsxTT78Management Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new ThreadEinvoiceAlsxTT78Management();
                    }
                }
                return _instance;
            }
        }

        #endregion singleton pattern

        public bool Running
        {
            get
            {
                return ThreadList.Any(p => p.Started);
            }
        }

        public void Start()
        {
            try
            {
                foreach (var item in ThreadList)
                {
                    if (!item.Started) item.Start();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                foreach (var item in ThreadList)
                {
                    item.Wait();
                    item.Stop();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Exit()
        {
            Stop();
            //xử lý dữ liệu còn tồn đọng trong queueu
            //Đẩy lại dữ liệu trong queue của mo_queue_waiting vào db
            //tudn mo_queue_waiting.ReInsertFromQueueToDB();

            //Gửi nốt tin nhắn mt đang chờ trong queue
            //tudn mt_queue_log.SendAllMt();
        }

        /// <summary>
        /// Xử lý dữ liệu còn nằm trên file xml, đẩy vào csdl.
        /// </summary>
        public void ProcessXmlData()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ExportXMLToSQL()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
