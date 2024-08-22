using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;

namespace TASK.Data
{
    public partial class BasePdf
    {
        public static void InsertAll(List<BasePdf> listPdf)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {
                if (listPdf.Count > 0)
                {
                    dbConext.BasePdfs.InsertAllOnSubmit(listPdf);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(BasePdf pdf)
        {
            using (DBContextDataContext dbConext = new DBContextDataContext(AppSetting.ConnectionStringeInvoice))
            {

                dbConext.BasePdfs.InsertOnSubmit(pdf);
                dbConext.SubmitChanges();
            }
        }
    }
}
