using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASK.Model.ViewModel
{
    public class InvoiceDetailViewModel
    {
        public string InvoiceLineIsn { set; get; }
        public string InvoiceIsn { set; get; }
        public string LineNo { set; get; }
        public string Des { set; get; }
        public double TotalAmount { set; get; }
        public double TotalVatAmount { set; get; }
        public double PERCENTAGE { set; get; }
        public string invl_object_isn { set; get; }
        public string invl_object_type { set; get; }
        public string Unit { set; get; }
        public double Quantity { set; get; }
        public double UnitPrice { set; get; }
        public bool MiCharged { set; get; }
        public string Data { set; get; }
        public string GroupIsn { set; get; }
        public string Title { set; get; }
        public double Weight { set; get; }
        public double CW_Weight { set; get; }
        public double Received_Weight { set; get; }
        public string Flight { set; get; }
        public string SHC { set; get; }
        public string Remark { set; get; }
        public double StorageTimeLog { set; get; }
    }
}
