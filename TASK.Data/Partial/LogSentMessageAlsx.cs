using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class LogSentMessageAlsx
    {
        public static void InsertAll(List<LogSentMessageAlsx> listMessage)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                if (listMessage.Count > 0)
                {
                    dbConext.LogSentMessageAlsxes.InsertAllOnSubmit(listMessage);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(LogSentMessageAlsx message)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {

                dbConext.LogSentMessageAlsxes.InsertOnSubmit(message);
                dbConext.SubmitChanges();
            }
        }
    }
}
