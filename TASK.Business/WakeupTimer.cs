using PluggableModulesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business
{
    public class WakeupTimer : WorkingBaseTimer
    {
        public List<ThreadBase> Target { get; set; }

        public WakeupTimer()
            : base()
        {
            Target = new List<ThreadBase>();
        }

        protected override void DoWork()
        {
            try
            {
                foreach (ThreadBase item in Target)
                {
                    if (item.WakeupCondition)
                        item.WakeUp();
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    HandleException(e);
                }
            }
        }

        protected override void InsertLog(Exception ex)
        {
            Log.WriteLog(ex);
        }

        protected override void InsertLog(Exception ex, string description)
        {
            Log.WriteLog(ex, description);
        }
    }
}
