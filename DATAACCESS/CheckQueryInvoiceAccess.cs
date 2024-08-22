using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Model.ViewModel;
using TASK.Data;

namespace DATAACCESS
{
    public class CheckQueryInvoiceAccess : DATABASE.OracleProvider
    {
        public List<InvoiceCheckQuery> GetAllInvoice()
        {
            List<InvoiceCheckQuery> Invoices = new List<InvoiceCheckQuery>();
            try
            {
                string sql = "select ih.invh_invoice_isn as INVOICEISN,ih.invh_printed_date "
                            +"from INVH_INVOICE_HEADER ih "
                            +"where ih.INVH_CANCELLED = 0 "
                            +"and ih.Invh_Invoice_Type = 'CASH' "
                            +"and ih.invh_printed_date > sysdate - 180 / 1440";

                using (OracleDataReader reader = GetScriptOracleDataReader(sql))
                {
                    while (reader.Read())
                    {
                        InvoiceCheckQuery invoice = new InvoiceCheckQuery();
                        invoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
                        Invoices.Add(invoice);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString() + "InvoiceCheckQuery");
            }
            return Invoices;

        }
    }
}
