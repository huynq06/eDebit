using PluggableModulesInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;


namespace TASK.Business
{
    public abstract class ThreadBase : WorkingBaseTimer
    {
        private WakeupTimer wakeup_timer;

        public ThreadBase() :
            base()
        {

        }

        public ThreadBase(WakeupTimer wakeup_timer)
            : base(true)
        {
            this.wakeup_timer = wakeup_timer;
            wakeup_timer.Target.Add(this);
        }

        public virtual bool WakeupCondition
        {
            get { return false; }
        }

        protected override void InsertLog(Exception ex)
        {
            Log.WriteLog(ex);
        }

        protected override void InsertLog(Exception ex, string description)
        {
            Log.WriteLog(ex, description);
        }

        #region Dispose
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (wakeup_timer != null)
            {
                wakeup_timer.Target.Remove(this);
            }
            base.Dispose(disposing);
        }

        ~ThreadBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
