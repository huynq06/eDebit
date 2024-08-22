using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Settings;


namespace TASK.Data
{
    public partial class InvoicePDF
    {
        public static void InsertAll(List<InvoicePDF> listPdf)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {
                if (listPdf.Count > 0)
                {
                    dbConext.InvoicePDFs.InsertAllOnSubmit(listPdf);
                }
                dbConext.SubmitChanges();
            }
        }
        public static void Insert(InvoicePDF pdf)
        {
            using (AlsxDbContextDataContext dbConext = new AlsxDbContextDataContext(AppSetting.ConnectionStringeInvoiceAlsx))
            {

                dbConext.InvoicePDFs.InsertOnSubmit(pdf);
                dbConext.SubmitChanges();
            }
        }
    }
}
