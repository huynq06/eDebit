using PluggableModulesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Business.staticThread;

namespace TASK.Business.staticThread
{
    public class ThreadEinvoiceAlscSendDataManagement
    {
        #region singleton pattern

        List<WorkingBaseTimer> ThreadList;

        private ThreadEinvoiceAlscSendDataManagement()
        {
            ThreadList = new List<WorkingBaseTimer>();
            WakeupTimer wakeup = new WakeupTimer();
            ThreadList.Add(wakeup);
            ThreadList.Add(new EInvoiceConnectThreadManagement());
            ThreadList.Add(new InvoiceImpCancelThreadManagement());
            ThreadList.Add(new EinvoiceAdditionalThread());

        }

        private static object _lock = new object();
        private static ThreadEinvoiceAlscSendDataManagement _instance;
        public static ThreadEinvoiceAlscSendDataManagement Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = new ThreadEinvoiceAlscSendDataManagement();
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
