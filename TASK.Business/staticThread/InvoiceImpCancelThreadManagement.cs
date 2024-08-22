using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class InvoiceImpCancelThreadManagement : ThreadBase
    {
        public object _locker = new object();
        public InvoiceImpCancelThreadManagement()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 20000;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.EInvoiceImpCancelService.CancelInvoice();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EInvoiceConnectService");
            }
        }
    }
}
