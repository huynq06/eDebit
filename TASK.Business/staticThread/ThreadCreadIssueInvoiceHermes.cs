using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;
using TASK.Service;

namespace TASK.Business.staticThread
{
    public class ThreadCreadIssueInvoiceHermes : ThreadBase
    {
        public object _locker = new object();
        public ThreadCreadIssueInvoiceHermes()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 1000;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.ServiceCheckIssueInvoiceHermers.GetInvoiceIssueFromHermes();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EinvoiceAlc");
            }
        }
    }
}
