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
    public class JsonFomatAlsx
    {
        public static string JsonInvoiceALSX(Invoice invoice)
        {
            string jsonResult = "";
            List<InvoiceDetail> listInvoiceDetail = new List<InvoiceDetail>();
            listInvoiceDetail = InvoiceDetail.GetByInvoiceID(invoice.InvoiceID);
            if (listInvoiceDetail.Count > 0)
            {
                Account account = new Account();
                account.username = AppSetting.UserNameEInvoiceALSx;
                account.password = AppSetting.PassWordEInvoiceALSx;
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
                    objItem.vrt = "10";
                    objItem.c0 = invoice.AWB + "/" + invoice.Hawb;
                    listItem.Add(objItem);
                }
                InvoiceFields invoiceField = new InvoiceFields();
                invoiceField.sid = invoice.InvoiceID;
                invoiceField.idt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                invoiceField.type = AppSetting.InvoiceFieldTypeALSx;
                invoiceField.form = AppSetting.InvoiceFieldFormALSx;
                invoiceField.serial = AppSetting.InvoiceFieldSerialALSx_EXPORT;
                invoiceField.aun = 2;
                invoiceField.bname = invoice.Customer;
                invoiceField.buyer = invoice.Customer;
                if (string.IsNullOrEmpty(invoice.TaxCode.Trim()))
                {
                    invoiceField.btax = "";
                }
                else
                {
                    if (invoice.TaxCode.Length > 10)
                    {
                        invoiceField.btax = invoice.TaxCode.Trim().Replace("-", "").Replace(" ", "").Insert(10, "-");
                    }
                    else
                    {
                        invoiceField.btax = invoice.TaxCode;
                    }

                }
                invoiceField.baddr = invoice.Address;
                invoiceField.btel = "";
                invoiceField.bmail = "";
                invoiceField.paym = invoice.Payment == "CASH" ? "TM" : "CK";
                invoiceField.curr = "VND";
                invoiceField.exrt = 1;
                invoiceField.bacc = "";
                invoiceField.bbank = "";
                invoiceField.note = "Test API eInvoice";
                invoiceField.sumv = (double)invoice.InvoiceTotalNoVatAmount;
                invoiceField.sum = (double)invoice.InvoiceTotalNoVatAmount;
                invoiceField.vatv = (double)Math.Round(invoiceField.sumv/10,0, MidpointRounding.AwayFromZero);
                invoiceField.vat = (double)Math.Round(invoiceField.sumv/10, 0, MidpointRounding.AwayFromZero);
                invoiceField.totalv = invoiceField.sum + invoiceField.vatv;
                invoiceField.word = Utils.ConvertNumber.ChuyenSo(invoiceField.totalv.ToString()) + " đồng";
                invoiceField.discount = "";
                invoiceField.total = (double)(invoice.InvoiceTotalAmount);
                invoiceField.items = listItem;
                invoiceField.stax = invoice.TaxNo;
                InvoiceJson invoiceJson = new InvoiceJson();
                invoiceJson.user = account;
                invoiceJson.inv = invoiceField;
                jsonResult = JsonConvert.SerializeObject(invoiceJson);
            }

            return jsonResult;
        }
    }
}
