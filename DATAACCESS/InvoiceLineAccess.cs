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
    public class InvoiceLineAccess : DATABASE.OracleProvider
    {
        private InvoiceDetailViewModel GetProperties(OracleDataReader reader)
        {
            InvoiceDetailViewModel objInvoice = new InvoiceDetailViewModel();
            objInvoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
            objInvoice.Des = Convert.ToString(GetValueField(reader, "DES", string.Empty));
            objInvoice.TotalAmount = Convert.ToDouble(GetValueField(reader, "AMOUNT", 0));
            objInvoice.TotalVatAmount = Convert.ToDouble(GetValueField(reader, "VAT", 0));
            objInvoice.PERCENTAGE = Convert.ToDouble(GetValueField(reader, "PERCENTAGE", 0));
            objInvoice.Data = Convert.ToString(GetValueField(reader, "DATA", string.Empty));
            objInvoice.Title = Convert.ToString(GetValueField(reader, "TITLE", string.Empty));
            objInvoice.GroupIsn = Convert.ToString(GetValueField(reader, "GROUPISN", string.Empty));
            objInvoice.Weight = Convert.ToDouble(GetValueField(reader, "WEIGHT", 0));
            objInvoice.Flight = Convert.ToString(GetValueField(reader, "FLIGHT", string.Empty));
            objInvoice.SHC = Convert.ToString(GetValueField(reader, "SHC", string.Empty));
            objInvoice.Remark = Convert.ToString(GetValueField(reader, "REMARK", string.Empty));
            return objInvoice;
        }
        private InvoiceDetailViewModel GetDebitProperties(OracleDataReader reader)
        {
            InvoiceDetailViewModel objInvoice = new InvoiceDetailViewModel();
            objInvoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
            objInvoice.Des = Convert.ToString(GetValueField(reader, "DES", string.Empty));
            objInvoice.TotalAmount = Convert.ToDouble(GetValueField(reader, "AMOUNT", 0));
            objInvoice.TotalVatAmount = Convert.ToDouble(GetValueField(reader, "VAT", 0));
            objInvoice.PERCENTAGE = Convert.ToDouble(GetValueField(reader, "PERCENTAGE", 0));
            objInvoice.Unit = Convert.ToString(GetValueField(reader, "UNIT", string.Empty));
            objInvoice.Quantity = Convert.ToDouble(GetValueField(reader, "UNIT_QUANTITY", 0));
            //  objInvoice.Data = Convert.ToString(GetValueField(reader, "DATA", string.Empty));
            // objInvoice.Title = Convert.ToString(GetValueField(reader, "TITLE", string.Empty));
            objInvoice.MiCharged = Convert.ToBoolean(GetValueField(reader, "MIN", string.Empty));
            objInvoice.GroupIsn = Convert.ToString(GetValueField(reader, "GROUPISN", string.Empty));
            objInvoice.Weight = Convert.ToDouble(GetValueField(reader, "HAWB_WEIGHT", 0));
            objInvoice.CW_Weight = Convert.ToDouble(GetValueField(reader, "CW", 0));
            objInvoice.Received_Weight = Convert.ToDouble(GetValueField(reader, "RECEIVECD_WEIGHT", 0));
            objInvoice.Flight = Convert.ToString(GetValueField(reader, "FLIGHT", string.Empty));
            objInvoice.SHC = Convert.ToString(GetValueField(reader, "SHC", string.Empty));
            objInvoice.Remark = Convert.ToString(GetValueField(reader, "REMARK", string.Empty));
            return objInvoice;
        }
        public List<InvoiceDetailViewModel> GetAllInvoice(HermesInvoice invoice)
        {
            List<InvoiceDetailViewModel> Invoices = new List<InvoiceDetailViewModel>();
            try
            {
                    string sql = "select distinct invl.invl_invoice_isn as INVOICEISN,"
        + "invl.invl_description as DES,"
        + "invl.invl_amount as AMOUNT,"
        + "invl.invl_vat as VAT,"
        + "invl.invl_vat_percentage as PERCENTAGE,"
        + "invl.invl_tariff_grp_isn as GROUPISN,"
        + "trnb.trnb_backuptitles as TITLE,"
        + "trnb.trnb_backupdata as DATA, "
        + "iobd.iobd_weight as WEIGHT, "
        + "iobd.iobd_shc_list as SHC, "
        + "invl.invl_remark as REMARK, "
        + "iobd.iobd_flight_airline as FLIGHT "
        + "from invl_invoice_lines invl "
        + "JOIN TDIL_TRND_TO_INVL tdil on tdil.tdil_invoice_line_isn = invl.invl_invoice_line_isn "
        + "join invh_invoice_header invh on invl.invl_invoice_isn = invh.invh_invoice_isn "
        + "join iobd_invoice_object_dtl iobd on iobd.iobd_invoice_isn = invl.invl_invoice_isn "
        + "JOIN TRND_TRANSACTION_DETAIL trnd on trnd.trnd_transaction_detail_isn = tdil.tdil_transaction_detail_isn "
        + "JOIN TRNB_TRANSACTION_BACKUPDATA trnb on trnb.trnb_transaction_header_isn = trnd.trnd_transaction_header_isn "
        + "where invl.invl_invoice_isn = '" + invoice.InvoiceIsn + "'";
                            
                            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
                            {
                                while (reader.Read())
                                {
                                    Invoices.Add(GetProperties(reader));
                                }
                            }
                
            }
            catch (Exception ex)
            {

                Log.WriteLog(ex.ToString() + invoice.InvoiceIsn);
            }
            return Invoices;

        }
        public List<InvoiceDetailViewModel> GetAllDebit(HermesDebit debit)
        {
            List<InvoiceDetailViewModel> Invoices = new List<InvoiceDetailViewModel>();
            try
            {
                string sql = "select distinct invl.invl_invoice_isn as INVOICEISN, "+
        "invl.invl_description as DES, "+
        "invl.invl_amount as AMOUNT, "+
        "invl.invl_vat as VAT, "+
        "invl.invl_vat_percentage as PERCENTAGE, "+
        "invl.invl_tariff_grp_isn as GROUPISN, "+
        "iobd.iobd_chargeable_weight as CW, "+
        "iobd.iobd_weight_received as RECEIVECD_WEIGHT,  "+
        "iobd.iobd_shc_list as SHC,  "+
        "iobd.iobd_pieces as HAWB_PIECES, "+
        "iobd.iobd_weight as HAWB_WEIGHT, "+
        "iobd.iobd_object_isn, "+
        "iobd.iobd_hawb, "+
        "iobd.iobd_awb_prefix, "+
        "iobd.iobd_awb_serial, "+
        "iobd.iobd_split, "+
        "invl.invl_remark as REMARK, "+
       "iobd.iobd_flight_airline as FLIGHT, "+
       "invb.invb_unit_type, "+
       "invb.invb_unit_use, "+
       "invb.invb_measurement_unit as UNIT, " +
       "invb.invb_unit_quantity as UNIT_QUANTITY, "+
       "invb.invb_minimum_charged as MIN "+
        "from invl_invoice_lines invl "+
        "join invh_invoice_header invh on invl.invl_invoice_isn = invh.invh_invoice_isn "+
        "join iobd_invoice_object_dtl iobd on iobd.iobd_invoice_isn = invl.invl_invoice_isn "+
        "join INVB_INVOICE_BACKUP invb ON invb.invb_invoice_isn = invh.invh_invoice_isn and invb.invb_invoice_line = invl.invl_invoice_line_isn"
    + " where invl.invl_invoice_isn = '" + debit.InvoiceIsn + "'";

                using (OracleDataReader reader = GetScriptOracleDataReader(sql))
                {
                    while (reader.Read())
                    {
                        Invoices.Add(GetDebitProperties(reader));
                    }
                }

            }
            catch (Exception ex)
            {

                Log.WriteLog(ex.ToString() + debit.InvoiceIsn);
            }
            return Invoices;

        }
    }
}
