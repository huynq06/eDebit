using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class EInvoiceConnectThreadManagement : ThreadBase
    {
        public object _locker = new object();
        public EInvoiceConnectThreadManagement()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 500;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.EInvoiceConnectService.EConnect();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EInvoiceConnectService");
            }
        }
    }
}
