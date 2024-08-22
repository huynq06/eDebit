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
    public class InvoiceImpCancelAccess : DATABASE.OracleProvider
    {
        private InvoiceCancelViewModel GetProperties(OracleDataReader reader)
        {
            InvoiceCancelViewModel objInvoice = new InvoiceCancelViewModel();
            objInvoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
            objInvoice.AWB_Prefix = Convert.ToString(GetValueField(reader, "AWB_PREFIX", string.Empty)).Trim();
            objInvoice.AWB_Serial = Convert.ToString(GetValueField(reader, "AWB_SERIRAL", string.Empty)).Trim();
            objInvoice.Hawb = Convert.ToString(GetValueField(reader, "HAWB", string.Empty)).Trim();
            objInvoice.Total_Amount = Convert.ToDecimal(GetValueField(reader, "INVOICE_TOTAL_AMOUNT", 0));
            objInvoice.Reason = Convert.ToString(GetValueField(reader, "REASON", string.Empty));
            objInvoice.ObjectType = Convert.ToString(GetValueField(reader, "OBJTYPE", string.Empty));
            return objInvoice;
        }
        public List<InvoiceCancelCheck> GetAllInvoiceCancelChecK(DateTime? date)
        {
            List<InvoiceCancelCheck> Invoices = new List<InvoiceCancelCheck>();
            string sql = "select distinct ih.invh_invoice_isn as INVOICEISN " +
                        "from " +
                        "INVH_INVOICE_HEADER ih inner join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn " +
                        "inner join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn " +
                        "where il.INVL_REFERENCE_INVOICE_LINE != il.INVL_INVOICE_LINE_ISN " +
                        "and(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB') " +
                        "and ih.INVH_CANCELLED = 0 " +
                        "and UPPER(ih.INVH_INVOICE_TYPE) = 'CREDITNOTE' " +
                        "and ih.invh_printed_date > sysdate - 60 / 1440 " +
                        "and((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE = il.INVL_INVOICE_LINE_ISN) = 0) " +
                        " and to_date('" + date.Value.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')= ih.Invh_Invoice_Date ";
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                while (reader.Read())
                {
                    InvoiceCancelCheck invoice = new InvoiceCancelCheck();
                    invoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
                    Invoices.Add(invoice);
                }
            }
            return Invoices;
        }
        public InvoiceCancelViewModel GetAllInvoice(string invoiceIsn)
        {
            //string sql = "select distinct ih.invh_invoice_isn as INVOICEISN "
            //            + " from INVH_INVOICE_HEADER ih inner  join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn"
            //            + " inner  join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn"
            //            + " inner join irun_invoice_run irun on irun.IRUN_INVOICE_RUN_ISN=ih.INVH_INVOICE_RUN_ISN"
            //            + " where il.INVL_REFERENCE_INVOICE_LINE!=il.INVL_INVOICE_LINE_ISN and "
            //            + "(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB') "
            //            + "and  ( ih.INVH_CANCELLED!=0 or ((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE=il.INVL_INVOICE_LINE_ISN)!=0) )"
            //            + " and  ih.Invh_Invoice_Type = 'CASH'"
            //            //+"and ih.invh_printed_date > sysdate - 1000 / 1440 "
            //            + " and to_date('" + date.Value.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')= ih.Invh_Invoice_Date ";
            string sql = "select distinct ih.invh_invoice_isn as INVOICEISN, " +
                        "ih.invh_reason as REASON, " +
                        "ih.Invh_Invoice_Total_Amount as INVOICE_TOTAL_AMOUNT, " +
                        "iod.Iobd_Awb_Prefix as AWB_PREFIX, " +
                        "iod.Iobd_Awb_Serial as AWB_SERIRAL, " +
                        "iod.Iobd_HAWB as HAWB, " +
                        "iod.IOBD_OBJECT_TYPE as OBJTYPE " +
                         "from " +
                         "INVH_INVOICE_HEADER ih inner join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn " +
                         "inner join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn " +
                         "where il.INVL_REFERENCE_INVOICE_LINE != il.INVL_INVOICE_LINE_ISN " +
                         "and(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB') " +
                         "and ih.INVH_CANCELLED = 0 " +
                         "and UPPER(ih.INVH_INVOICE_TYPE) = 'CREDITNOTE' " +
                         "and((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE = il.INVL_INVOICE_LINE_ISN) = 0) " +
                         "and ih.invh_invoice_isn = '" + invoiceIsn + "'";

           InvoiceCancelViewModel listInvoiceCancel = new InvoiceCancelViewModel();
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                if (reader.Read())
                {

                    listInvoiceCancel = GetProperties(reader);
                }
            }
            return listInvoiceCancel;

        }
        public bool CheckInvoiceCancelAddtional(DateTime dateCheck,string invoiceIsn)
        {
            bool check = false;
            string sql = "select distinct ih.invh_invoice_isn as INVOICEISN "
                        + " from INVH_INVOICE_HEADER ih inner  join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn"
                        + " inner  join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn"
                        + " inner join irun_invoice_run irun on irun.IRUN_INVOICE_RUN_ISN=ih.INVH_INVOICE_RUN_ISN"
                        + " where il.INVL_REFERENCE_INVOICE_LINE!=il.INVL_INVOICE_LINE_ISN and "
                        + "(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB') "
                        + "and  ( ih.INVH_CANCELLED!=0 or ((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE=il.INVL_INVOICE_LINE_ISN)!=0) )"
                        + " and  ih.Invh_Invoice_Type = 'CASH'"
                       + " and  ih.invh_invoice_isn = '"+ invoiceIsn + "'" 
                        //+"and ih.invh_printed_date > sysdate - 1000 / 1440 "
                        + " and to_date('" + dateCheck.ToString("yyyy-MM-dd") + "', 'YYYY-MM-DD')= ih.Invh_Invoice_Date ";
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                if (reader.Read())
                {

                    if (!string.IsNullOrEmpty(Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty))))
                        check = true;
                }
            }
            return check;
        }
    }
}
