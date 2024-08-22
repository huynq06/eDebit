using PluggableModulesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASK.Business.staticThread
{
    public class ThreadEinvoiceAlsxManagement
    {
        #region singleton pattern

        List<WorkingBaseTimer> ThreadList;

        private ThreadEinvoiceAlsxManagement()
        {
            ThreadList = new List<WorkingBaseTimer>();
            WakeupTimer wakeup = new WakeupTimer();
            ThreadList.Add(wakeup);
            //ThreadList.Add(new EinvoiceAlsxThread());
            ThreadList.Add(new ThreadCancelEinvoiceAlsx());
           // ThreadList.Add(new EinvoiceAlsxAddtionalThread());
        }

        private static object _lock = new object();
        private static ThreadEinvoiceAlsxManagement _instance;
        public static ThreadEinvoiceAlsxManagement Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new ThreadEinvoiceAlsxManagement();
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
