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
    public class ServiceCheckIssueDebitHermers
    {
        public static List<DebitCheckQuery> _listKeyDebit;
        public static void GetDebitIssueFromHermes()
        {
            _listKeyDebit = new List<DebitCheckQuery>();
            ProcessData();
            try
            {
                using (TransactionScope scope = AppSetting.GetTransactionScope())
                {

                    if (_listKeyDebit.Count > 0)
                    {
                        DebitCheckQuery.Insert(_listKeyDebit);
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
            List<DebitCheckQuery> DebitChecks = new CheckQueryDebitAccess().GetAllDebit();
            if (DebitChecks.Count > 0)
            {
                List<DebitCheckQuery> listDebitCheckQuery = new List<DebitCheckQuery>();
                listDebitCheckQuery = DebitCheckQuery.GetData();
                foreach (var DebitCheck in DebitChecks)
                {
                    if (!listDebitCheckQuery.Any(c => c.InvoiceIsn == DebitCheck.InvoiceIsn))
                    {
                        DebitCheck.Created = DateTime.Now;
                        DebitCheck.Status = false;
                        DebitCheck.Retry = 0;
                        _listKeyDebit.Add(DebitCheck);
                    }
                }
            }
        }
    }
}
