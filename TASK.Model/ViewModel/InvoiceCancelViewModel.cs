using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASK.Model.ViewModel
{
    public class InvoiceCancelViewModel
    {
        public string InvoiceIsn { set; get; }
        public string AWB_Prefix { set; get; }
        public string AWB_Serial { set; get; }
        public decimal Total_Amount { set; get; }
        public string Hawb { set; get; }
        public string Reason { set; get; }
        public string ObjectType { set; get; }
    }
}
