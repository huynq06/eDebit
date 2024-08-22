using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluggableModulesInterface;
using System.Threading;
using TASK.Business;
using TASK.Settings;

namespace TASK.Business
{
    public class BaseTimer : WorkingBaseTimer
    {
        #region properties
        protected string ObjectType { get; private set; }
        #endregion properties
        #region constructor
        public BaseTimer()
            : base()
        {
            ObjectType = this.GetType().FullName;
        }

        public BaseTimer(bool required_wakeup)
            : base(required_wakeup)
        {
            ObjectType = this.GetType().FullName;
        }

        public BaseTimer(TimerCallback callback)
            : base(callback)
        {
            ObjectType = this.GetType().FullName;
        }
        #endregion

        protected override void InsertLog(Exception ex)
        {
            //Log.InsertLog(ex, ObjectType, AppSetting.WarningType.application_error);
        }

        protected override void InsertLog(Exception ex, string warningType)
        {
            //Log.InsertLog(ex, ObjectType, warningType);
        }
    }
}
