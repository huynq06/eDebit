using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    class EinvoiceAdditionalThread : ThreadBase
    {
        public object _locker = new object();
        public EinvoiceAdditionalThread()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 300000;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.EInvoiceServiceAddtional.Retry();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EinvoiceAlc");
            }
        }
    }
}
