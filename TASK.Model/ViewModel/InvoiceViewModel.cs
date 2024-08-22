using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASK.Model.ViewModel
{
    public class InvoiceViewModel
    {
        public string No { get; set; }
        public string Awb_Prefix { get; set; }
        public string Awb_No { get; set; }
        public string Hawb { get; set; }
        public double Amount_No_Vat { get; set; }
        public double Amount_Vat { get; set; }
        public string Vat { get; set; }
        public double Amount_Total { get; set; }
        public string Customer_Name { get; set; }
        public string Payment { get; set; }
        public string PersonName { get; set; }
        public string InvoiceIsn { set; get; }
        public string InvoiceObjIns { set; get; }
        public string InvoiceRunIns { set; get; }
        public DateTime? InvoiceDateTime { set; get; }
        public string InvoiceType { set; get; }
        public string Street { set; get; }
        public string VatID { set; get; }
        public string ObjType { set; get; }
        public string InvoiceLink { set; get; }
        public string PrinterNetWork { set; get; }
        public string Email { set; get; }
        public string Note { set; get; }
        public string EmailExtend { set; get; }
        public string FullEmail { set; get; }
    }
}
