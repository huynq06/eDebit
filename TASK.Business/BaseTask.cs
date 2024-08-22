using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluggableModulesInterface;
using TASK.Settings;
namespace TASK.Business
{
    public abstract class BaseTask : WorkingBaseTask
    {
        #region constructor
        public BaseTask()
            : base()
        {
        }
        #endregion constructor

        protected override void InsertLog(Exception ex)
        {
            //Log.InsertLog(ex, ObjectType, AppSetting.WarningType.application_error);
        }

        protected override void InsertLog(Exception ex, string logType)
        {
            //Log.InsertLog(ex, ObjectType, logType);
        }
    }
}
