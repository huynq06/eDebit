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
    public class PrintAccess : DATABASE.OracleProvider
    {
        private InvoiceViewModel GetProperties(OracleDataReader reader)
        {
            InvoiceViewModel objInvoice = new InvoiceViewModel();
            objInvoice.No = Convert.ToString(GetValueField(reader, "INVOICE_NUMBER", string.Empty));
            objInvoice.Awb_Prefix = Convert.ToString(GetValueField(reader, "AWB_PREFIX", string.Empty));
            objInvoice.Awb_No = Convert.ToString(GetValueField(reader, "AWB_SERIRAL", string.Empty));
            objInvoice.Payment = Convert.ToString(GetValueField(reader, "PAYMENT", string.Empty));
            objInvoice.Hawb = Convert.ToString(GetValueField(reader, "HAWB", string.Empty));
            objInvoice.Amount_No_Vat = Convert.ToDouble(GetValueField(reader, "INVOICE_TOTAL_AMOUNT", 0));
            objInvoice.Amount_Vat = Convert.ToDouble(GetValueField(reader, "INVOICE_TOTAL_VAT", 0));
            objInvoice.Customer_Name = Convert.ToString(GetValueField(reader, "KUND_NAME", string.Empty));
            objInvoice.PersonName = Convert.ToString(GetValueField(reader, "PERSON_NAME", string.Empty));
            objInvoice.InvoiceDateTime = GetValueDateTimeField(reader, "INVOICEDATETIME", objInvoice.InvoiceDateTime);
            objInvoice.InvoiceIsn = Convert.ToString(GetValueField(reader, "INVOICEISN", string.Empty));
            objInvoice.InvoiceObjIns = Convert.ToString(GetValueField(reader, "INVOICEOBJISN", string.Empty));
            objInvoice.InvoiceRunIns = Convert.ToString(GetValueField(reader, "INVOICERUNISN", string.Empty));
            objInvoice.Amount_Total = objInvoice.Amount_No_Vat + objInvoice.Amount_Vat;
            objInvoice.Vat = (objInvoice.Amount_Vat * 100 / objInvoice.Amount_No_Vat).ToString("N0");
            objInvoice.InvoiceType = Convert.ToString(GetValueField(reader, "INVOICE_TYPE", string.Empty));
            objInvoice.Street = Convert.ToString(GetValueField(reader, "STREET", string.Empty)).TrimEnd(',');
            objInvoice.VatID = Convert.ToString(GetValueField(reader, "VATID", string.Empty));
            objInvoice.ObjType = Convert.ToString(GetValueField(reader, "OBJTYPE", string.Empty));
            objInvoice.InvoiceLink = Convert.ToString(GetValueField(reader, "INVOICELINK", string.Empty));
            objInvoice.PrinterNetWork = Convert.ToString(GetValueField(reader, "TRM_IMP", string.Empty)) == ""? "PXK_" +Convert.ToString(GetValueField(reader, "TRM", string.Empty)).Split(',')[0].Trim() : "PXK_" + Convert.ToString(GetValueField(reader, "TRM_IMP", string.Empty)).Split(',')[0].Trim();
            objInvoice.Email = Convert.ToString(GetValueField(reader, "EMAIL", string.Empty));
            objInvoice.EmailExtend = Convert.ToString(GetValueField(reader, "EMAILEXTEND", string.Empty));
            objInvoice.Note = Convert.ToString(GetValueField(reader, "NOTE", string.Empty));
            objInvoice.FullEmail = objInvoice.Email.Trim() + ";" + objInvoice.EmailExtend.Trim();
            return objInvoice;
        }
        public InvoiceViewModel GetAllInvoice(string invoiceIsn,ref bool check,InvoiceCheckQuery invoiceCheckQuery)
        {
            string sql = "select distinct ivp.INVP_PAYMENT_METHOD as PAYMENT,ih.Invh_Invoice_Number as INVOICE_NUMBER, "
                        + "iadr.iadr_street_1 ||','|| iadr.iadr_street_2||','||iadr.iadr_city||','||iadr.iadr_state_province as STREET,"
                        + "iadr.iadr_vat_id as VATID, "
                        + "ih.invh_invoice_isn as INVOICEISN, "
                        +"iod.IOBD_OBJECT_ISN as INVOICEOBJISN, "
                        +"ih.invh_invoice_run_isn as INVOICERUNISN, "
                        +"ivp.invp_datetime as INVOICEDATETIME, "
                        + "t.prnn_printdocument as INVOICELINK, "
                        + "ih.Invh_Invoice_Type as INVOICE_TYPE,"
                        + "ih.Invh_Invoice_Date as INVOICE_DATE,"
                        + "ih.Invh_Invoice_Total_Amount as INVOICE_TOTAL_AMOUNT,"
                        + "ih.Invh_INVOICE_TOTAL_VAT as INVOICE_TOTAL_VAT,"
                        + "iod.Iobd_Awb_Prefix as AWB_PREFIX,"
                        + "iod.Iobd_Awb_Serial as AWB_SERIRAL,"
                        + "iod.Iobd_HAWB as HAWB,"
                        + "iadr.iadr_email as EMAIL,"
                        + "iie.email as EMAILEXTEND,"
                        + "ivp.invp_description as NOTE,"
                        + "iod.IOBD_OBJECT_TYPE as OBJTYPE,"
                        + "iadr.iadr_name_1||' '||iadr.iadr_name_2||' '||iadr.iadr_name_3 as KUND_NAME, "
                        + "irun.IRUN_PERSON_NAME  as PERSON_NAME,"
                        + "t.prnn_printqueue PRINTER"
                        + " from INVH_INVOICE_HEADER ih inner  join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn"
                        + " inner  join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn"
                        + " inner  join iadr_invoice_addresses iadr on ih.invh_address_isn = iadr.iadr_address_isn"
                        + " inner join irun_invoice_run irun on irun.IRUN_INVOICE_RUN_ISN=ih.INVH_INVOICE_RUN_ISN"
                        + " left join prnn_printing_table t on ih.invh_invoice_isn = t.prnn_object_isn"
                        + " left join INVP_INVOICE_PAYMENTS ivp on ivp.INVP_INVOICE_ISN=ih.INVH_INVOICE_ISN"
                        + " left join iadr_invoice_email iie on iie.iadr_address_isn = ih.invh_address_isn"
                        + " where "
                         + "(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB')"
                        + " and  ih.INVH_CANCELLED=0 "
                        + " and  ih.Invh_Invoice_Type = 'CASH' "
                        + "and ih.invh_invoice_isn = '"+ invoiceIsn + "'"
                        + "and iod.iobd_invoice_isn = '" + invoiceIsn + "'"
                        + " and ((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE=il.INVL_INVOICE_LINE_ISN)=0)"
                        +" and((t.prnn_template_code not like '%E0%') or t.prnn_template_code is null) "
                        +" order by ih.Invh_Invoice_Number";
            InvoiceViewModel Invoice = new InvoiceViewModel();
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                if (reader.Read())
                {
                    InvoiceViewModel invoiceCheck = GetProperties(reader);
                    if (invoiceCheck.ObjType == "IMPORT AWB")
                    {
                        if (!string.IsNullOrEmpty(invoiceCheck.PrinterNetWork.Trim()))
                        {
                            Invoice = invoiceCheck;
                            check = true;
                        }
                        else if(invoiceCheckQuery.Retry > 10)
                        {
                            invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                            invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC304";
                            Invoice = invoiceCheck;
                        }
                        else
                        {
                            check = false;
                        }
                        //else
                        //{
                        //    invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                        //    invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC304";
                        //    Invoice = invoiceCheck;
                        //}
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(invoiceCheck.PrinterNetWork.Trim()))
                        {
                            Invoice = invoiceCheck;
                            check = true;
                        }
                        else if (invoiceCheckQuery.Retry > 5)
                        {
                            invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                            invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC114";
                            Invoice = invoiceCheck;
                        }
                        else
                        {
                            check = false;
                        }
                    }
                   
                    
                }
            }
            return Invoice;

        }
        public InvoiceViewModel GetAllDebit(string invoiceIsn, ref bool check, DebitCheckQuery invoiceCheckQuery)
        {
            string sql = "select distinct ivp.INVP_PAYMENT_METHOD as PAYMENT,ih.Invh_Invoice_Number as INVOICE_NUMBER, "
                        + "iadr.iadr_street_1 ||','|| iadr.iadr_street_2||','||iadr.iadr_city||','||iadr.iadr_state_province as STREET,"
                        + "iadr.iadr_vat_id as VATID, "
                        + "ih.invh_invoice_isn as INVOICEISN, "
                        + "iod.IOBD_OBJECT_ISN as INVOICEOBJISN, "
                        + "ih.invh_invoice_run_isn as INVOICERUNISN, "
                        + "ivp.invp_datetime as INVOICEDATETIME, "
                        + "t.prnn_printdocument as INVOICELINK, "
                        + "ih.Invh_Invoice_Type as INVOICE_TYPE,"
                        + "ih.Invh_Invoice_Date as INVOICE_DATE,"
                        + "ih.Invh_Invoice_Total_Amount as INVOICE_TOTAL_AMOUNT,"
                        + "ih.Invh_INVOICE_TOTAL_VAT as INVOICE_TOTAL_VAT,"
                        + "iod.Iobd_Awb_Prefix as AWB_PREFIX,"
                        + "iod.Iobd_Awb_Serial as AWB_SERIRAL,"
                        + "iod.Iobd_HAWB as HAWB,"
                        + "iadr.iadr_email as EMAIL,"
                        + "iie.email as EMAILEXTEND,"
                        + "ivp.invp_description as NOTE,"
                        + "iod.IOBD_OBJECT_TYPE as OBJTYPE,"
                        + "iadr.iadr_name_1||' '||iadr.iadr_name_2||' '||iadr.iadr_name_3 as KUND_NAME, "
                        + "irun.IRUN_PERSON_NAME  as PERSON_NAME, "
                        + "agen.agen_terminal TRM, "
                            + "agen2.agen_terminal TRM_IMP, "
                        + "t.prnn_printqueue PRINTER"
                        + " from INVH_INVOICE_HEADER ih inner  join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn"
                        + " inner  join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn"
                        + " inner  join iadr_invoice_addresses iadr on ih.invh_address_isn = iadr.iadr_address_isn"
                        + " inner join irun_invoice_run irun on irun.IRUN_INVOICE_RUN_ISN=ih.INVH_INVOICE_RUN_ISN"
                        + " left join prnn_printing_table t on ih.invh_invoice_isn = t.prnn_object_isn"
                        + " left join INVP_INVOICE_PAYMENTS ivp on ivp.INVP_INVOICE_ISN=ih.INVH_INVOICE_ISN"
                        + " left join iadr_invoice_email iie on iie.iadr_address_isn = ih.invh_address_isn"
                        + " left join labs on labs.labs_fwbm_serial_no = iod.iobd_object_isn and labs.labs_deleted = 0 "
                        + " left join agen on agen.agen_ident_no = labs.labs_ident_no and agen.agen_message_code = 'DEBIT NOTE PRODUCED' "
                             + " left join lagi on lagi.lagi_ident_no = iod.iobd_ident_no and lagi.lagi_deleted=0 "
                        + " left join agen agen2 on agen2.agen_ident_no = lagi.lagi_ident_no and agen2.agen_message_code = 'DEBIT NOTE PRODUCED' "
                        + " where "
                         + "(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB')"
                        + " and  ih.INVH_CANCELLED=0 "
                        + " and  ih.Invh_Invoice_Type = 'DEBIT NOTE' "
                        + "and ih.invh_invoice_isn = '" + invoiceIsn + "'"
                        + "and iod.iobd_invoice_isn = '" + invoiceIsn + "'"
                        + " and ((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE=il.INVL_INVOICE_LINE_ISN)=0)"
                        + " and((t.prnn_template_code not like '%E0%') or t.prnn_template_code is null) "
                        + " order by ih.Invh_Invoice_Number";
            InvoiceViewModel Invoice = new InvoiceViewModel();
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                if (reader.Read())
                {
                    InvoiceViewModel invoiceCheck = GetProperties(reader);
                    if (invoiceCheck.ObjType == "IMPORT AWB")
                    {
                        if (!string.IsNullOrEmpty(invoiceCheck.PrinterNetWork.Trim()))
                        {
                            Invoice = invoiceCheck;
                            check = true;
                        }
                        else if (invoiceCheckQuery.Retry > 10)
                        {
                            invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                            invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC304";
                            Invoice = invoiceCheck;
                        }
                        else
                        {
                            check = false;
                        }
                        //else
                        //{
                        //    invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                        //    invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC304";
                        //    Invoice = invoiceCheck;
                        //}
                    }
                    else
                    {
                        Invoice = invoiceCheck;
                        //if (!string.IsNullOrEmpty(invoiceCheck.PrinterNetWork.Trim()))
                        //{
                        //    Invoice = invoiceCheck;
                        //    check = true;
                        //}
                        //else if (invoiceCheckQuery.Retry > 5)
                        //{
                        //    invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                        //    invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC114";
                        //    Invoice = invoiceCheck;
                        //}
                        //else
                        //{
                        //    check = false;
                        //}
                    }


                }
            }
            return Invoice;

        }
        public InvoiceViewModel GetAllInvoiceTest(string invoiceIsn)
        {
            string sql = "select distinct ivp.INVP_PAYMENT_METHOD as PAYMENT,ih.Invh_Invoice_Number as INVOICE_NUMBER, "
                        + "iadr.iadr_street_1 ||','|| iadr.iadr_street_2||','||iadr.iadr_city||','||iadr.iadr_state_province as STREET,"
                        + "iadr.iadr_vat_id as VATID, "
                        + "ih.invh_invoice_isn as INVOICEISN, "
                        + "iod.IOBD_OBJECT_ISN as INVOICEOBJISN, "
                        + "ih.invh_invoice_run_isn as INVOICERUNISN, "
                        + "ivp.invp_datetime as INVOICEDATETIME, "
                        + "t.prnn_printdocument as INVOICELINK, "
                        + "ih.Invh_Invoice_Type as INVOICE_TYPE,"
                        + "ih.Invh_Invoice_Date as INVOICE_DATE,"
                        + "ih.Invh_Invoice_Total_Amount as INVOICE_TOTAL_AMOUNT,"
                        + "ih.Invh_INVOICE_TOTAL_VAT as INVOICE_TOTAL_VAT,"
                        + "iod.Iobd_Awb_Prefix as AWB_PREFIX,"
                        + "iod.Iobd_Awb_Serial as AWB_SERIRAL,"
                        + "iod.Iobd_HAWB as HAWB,"
                        + "iadr.iadr_email as EMAIL,"
                        + "iie.email as EMAILEXTEND,"
                        + "iod.IOBD_OBJECT_TYPE as OBJTYPE,"
                        + "iadr.iadr_name_1||' '||iadr.iadr_name_2||' '||iadr.iadr_name_3 as KUND_NAME, "
                        + "irun.IRUN_PERSON_NAME  as PERSON_NAME,"
                        + "t.prnn_printqueue PRINTER"
                        + " from INVH_INVOICE_HEADER ih inner  join INVL_INVOICE_LINES il on il.INVL_INVOICE_ISN = ih.invh_invoice_isn"
                        + " inner  join IOBD_INVOICE_OBJECT_DTL iod on iod.IOBD_OBJECT_ISN = il.Invl_Object_Isn"
                        + " inner  join iadr_invoice_addresses iadr on ih.invh_address_isn = iadr.iadr_address_isn"
                        + " inner join irun_invoice_run irun on irun.IRUN_INVOICE_RUN_ISN=ih.INVH_INVOICE_RUN_ISN"
                        + " left join prnn_printing_table t on ih.invh_invoice_isn = t.prnn_object_isn"
                        + " left join INVP_INVOICE_PAYMENTS ivp on ivp.INVP_INVOICE_ISN=ih.INVH_INVOICE_ISN"
                        + " left join iadr_invoice_email iie on iie.iadr_address_isn = ih.invh_address_isn"
                        + " where "
                         + "(iod.IOBD_OBJECT_TYPE = 'IMPORT AWB' or iod.IOBD_OBJECT_TYPE = 'EXPORT AWB')"
                        + " and  ih.INVH_CANCELLED=0 "
                        + " and  ih.Invh_Invoice_Type = 'CASH' "
                        + "and ih.invh_invoice_isn = '" + invoiceIsn + "'"
                        + " and ((select count(ilr.INVL_INVOICE_LINE_ISN) from INVL_INVOICE_LINES ilr where ilr.INVL_REFERENCE_INVOICE_LINE=il.INVL_INVOICE_LINE_ISN)=0)"
                        + " and((t.prnn_template_code not like '%E0%') or t.prnn_template_code is null) "
                        + " order by ih.Invh_Invoice_Number";
            InvoiceViewModel Invoice = new InvoiceViewModel();
            using (OracleDataReader reader = GetScriptOracleDataReader(sql))
            {
                if (reader.Read())
                {
                    InvoiceViewModel invoiceCheck = GetProperties(reader);
                    if (invoiceCheck.ObjType == "IMPORT AWB")
                    {
                        if (!string.IsNullOrEmpty(invoiceCheck.PrinterNetWork.Trim()))
                        {
                            Invoice = invoiceCheck;

                        }
                        else
                        {
                            invoiceCheck.InvoiceLink = @"\\VM-SHARE\Hermes5Share\HL\Forms\" + DateTime.Now.ToString("yyyyMMdd") + @"\Cash_Invoice_" + invoiceCheck.No + ".rtf";
                            invoiceCheck.PrinterNetWork = @"\\127.0.0.1\HD_PC304";
                            Invoice = invoiceCheck;
                        }
                    }
                    else
                    {

                        Invoice = invoiceCheck;
                    }


                }
            }
            return Invoice;

        }
    }
}
