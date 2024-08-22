using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TASK.Data;
using TASK.Settings;
using TASK.Model.ViewModel;
using TASK.Model.DBModel;
using DATAACCESS;


namespace TASK.Service
{
    public static class ServiceCheckIssueInvoiceHermers
    {
        public static List<InvoiceCheckQuery> _listKeyInvoice;
        public static void GetInvoiceIssueFromHermes()
        {
            _listKeyInvoice = new List<InvoiceCheckQuery>();
            ProcessData();
            try
            {
                using (TransactionScope scope = AppSetting.GetTransactionScope())
                {

                    if (_listKeyInvoice.Count > 0)
                    {
                        InvoiceCheckQuery.Insert(_listKeyInvoice);
                    }

                    scope.Complete();
                }
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
        }
        public static void ProcessData()
        {
            List<InvoiceCheckQuery> invoiceChecks = new CheckQueryInvoiceAccess().GetAllInvoice();
            if (invoiceChecks.Count > 0)
            {
                List<InvoiceCheckQuery> listInvoiceCheckQuery = new List<InvoiceCheckQuery>();
                listInvoiceCheckQuery = InvoiceCheckQuery.GetData();
                foreach (var invoiceCheck in invoiceChecks)
                {
                    if (!listInvoiceCheckQuery.Any(c => c.InvoiceIsn == invoiceCheck.InvoiceIsn))
                    {
                        invoiceCheck.Created = DateTime.Now;
                        invoiceCheck.Status = false;
                        invoiceCheck.Retry = 0;
                        _listKeyInvoice.Add(invoiceCheck);
                    }
                }
            }
        }
    }
}
