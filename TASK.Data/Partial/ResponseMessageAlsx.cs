using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class ResponseMessageAlsx
    {
        public static void InsertAll(List<ResponseMessageAlsx> listMessage)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                if (listMessage.Count > 0)
                {
                    dbConext.ResponseMessageAlsxes.InsertAllOnSubmit(listMessage);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(ResponseMessageAlsx message)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {

                dbConext.ResponseMessageAlsxes.InsertOnSubmit(message);
                dbConext.SubmitChanges();
            }
        }
    }
}
