using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TASK.Business.staticThread;

namespace Alsx.CanceleInvoice78
{
    [RunInstaller(true)]
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ThreadEinvoiceAlsxTT78Management.Instance.Start();
        }

        protected override void OnStop()
        {
            ThreadEinvoiceAlsxTT78Management.Instance.Stop();
        }
    }
}
