using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class PrintEInvoiceManagement : ThreadBase
    {
        public object _locker = new object();
        public PrintEInvoiceManagement()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 60000;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.PrintEInvoiceService.PrintInvoice();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "Issue");
            }
        }
    }
}
