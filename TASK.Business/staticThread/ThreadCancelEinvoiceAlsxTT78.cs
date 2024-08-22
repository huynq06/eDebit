using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class ThreadCancelEinvoiceAlsxTT78 : ThreadBase
    {
        public ThreadCancelEinvoiceAlsxTT78()
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
                Service.EInvoiceAlsxTT78CancelService.CanCelTT78Invoice();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "CancelEInvoiceAlsxConnectService");
            }
        }
    }
}
