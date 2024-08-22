using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class EinvoiceAlsxAddtionalThreadTT78 : ThreadBase
    {
        public object _locker = new object();
        public EinvoiceAlsxAddtionalThreadTT78()
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
                Service.EInvoiceAlsxAdditionalServiceTT78.Retry();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EinvoiceAlsxTT78");
            }
        }
    }
}
