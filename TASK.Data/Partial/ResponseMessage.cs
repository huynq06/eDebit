using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class ResponseMessage
    {
        public static void InsertAll(List<ResponseMessage> listMessage)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listMessage.Count > 0)
                {
                    dbConext.ResponseMessages.InsertAllOnSubmit(listMessage);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(ResponseMessage message)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {

                dbConext.ResponseMessages.InsertOnSubmit(message);
                dbConext.SubmitChanges();
            }
        }
    }
}
