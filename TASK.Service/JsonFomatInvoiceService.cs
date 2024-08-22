using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;
using TASK.Model.DBModel;
using TASK.Settings;

namespace TASK.Service
{
    public static class JsonFomatInvoiceService
    {
        public static string JsonInvoiceALSC(HermesInvoice invoice)
        {
            string jsonResult = "";
            List<HermesInvoiceDetail> listInvoiceDetail = new List<HermesInvoiceDetail>();
            listInvoiceDetail = HermesInvoiceDetail.GetByInvoiceIsn(invoice.InvoiceIsn);
            if (listInvoiceDetail.Count > 0)
            {
                Account account = new Account();
                account.username = AppSetting.UserNameEInvoiceALSC;
                account.password = AppSetting.PassWordEInvoiceALSC;
                invoice.TimeSent = DateTime.Now;
                int line = 0;
                List<item> listItem = new List<item>();
                foreach (var invoiceDetail in listInvoiceDetail)
                {
                    line++;
                    item objItem = new item();
                    objItem.line = line;
                    objItem.name = invoiceDetail.Item;
                    objItem.quantity = invoiceDetail.Quantity.Value;
                    objItem.price = invoiceDetail.UnitPrice.Value;
                    objItem.amount = invoiceDetail.Amount.Value;
                    objItem.unit = invoiceDetail.Unit;
                    objItem.type = "";
                    objItem.vrt = invoiceDetail.TaxRate;
                    objItem.c0 = invoice.Hawb.Trim().Length>0 ? invoice.AWB + "/ " + invoice.Hawb : invoice.AWB + "/";
                    listItem.Add(objItem);
                }
                InvoiceFields invoiceField = new InvoiceFields();
                invoiceField.sid = invoice.InvoiceIsn;
                invoiceField.idt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                invoiceField.type = AppSetting.InvoiceFieldTypeALSC;
                invoiceField.form = AppSetting.InvoiceFieldFormALSC;
                if (invoice.ObjectType == "EXPORT AWB")
                {
                    invoiceField.serial = AppSetting.InvoiceFieldSerialALSC_EXPORT;
                }
                else
                {
                    invoiceField.serial = AppSetting.InvoiceFieldSerialALSC_IMPORT;
                }
                invoiceField.aun = 2;
               
              
                //khách hàng cá nhân
                if (string.IsNullOrEmpty(invoice.TaxCode.Trim()))
                {
                    invoiceField.buyer = invoice.KundName;
                    invoiceField.bname = invoice.KundName;
                    invoiceField.btax = "";
                }
                //khách hàng doanh nghiệp không cần buyer
                else
                {
                    invoiceField.bname = invoice.KundName;
                    invoiceField.buyer = "";
                    if (invoice.TaxCode.Length > 10)
                    {
                        invoiceField.btax = invoice.TaxCode.Trim().Replace("-", "").Replace(" ", "").Insert(10, "-");
                    }
                    else
                    {
                        invoiceField.btax = invoice.TaxCode;
                    }

                }
                invoiceField.baddr = invoice.Address.Trim().TrimEnd(',').Trim().TrimEnd(',').TrimStart(',');
                invoiceField.btel = "";
                invoiceField.bmail = string.IsNullOrEmpty(invoice.Email.Trim()) ? "hddt.als@als.com.vn" : invoice.Email.Replace(" ", "").Replace(',', ';').Trim() + ";hddt.als@als.com.vn";
                //invoiceField.bmail = "hddt.als@als.com.vn";
                invoiceField.paym = "TM/CK";
                invoiceField.curr = "VND";
                invoiceField.exrt = 1;
                invoiceField.bacc = "";
                invoiceField.bbank = "";
                invoiceField.note = "Cần kiểm tra, đối chiếu khi lập, giao nhận hóa đơn";
                invoiceField.sumv = (double)invoice.InvoiceTotalNoVatAmount;
                invoiceField.sum = (double)invoice.InvoiceTotalNoVatAmount;
                invoiceField.vatv = (double)invoice.InvoiceTotalVatAmount;
                invoiceField.vat = (double)invoice.InvoiceTotalVatAmount;
                invoiceField.totalv = (double)(invoice.InvoiceTotalAmount);
                invoiceField.word = "";
                invoiceField.discount = "";
                invoiceField.total = (double)(invoice.InvoiceTotalAmount);
                invoiceField.items = listItem;
                invoiceField.stax = AppSetting.StaxALSC;

                //thong tu 78
                //invoiceField.sign = -1;
                invoiceField.type_ref = 1;
                //invoiceField.listnum = "";
                //invoiceField.listdt = "";
                invoiceField.sendtype = 1;
                InvoiceJson invoiceJson = new InvoiceJson();
                invoiceJson.user = account;
                invoiceJson.inv = invoiceField;
                jsonResult = JsonConvert.SerializeObject(invoiceJson);
            }

            return jsonResult;
        }
    }
}
