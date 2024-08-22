﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;

namespace TASK.Business.staticThread
{
    public class ThreadCancelEinvoiceAlsx : ThreadBase
    {
        public ThreadCancelEinvoiceAlsx()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 500;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.EInvoiceAlsxCancelService.CanCelInvoice();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "CancelEInvoiceAlsxConnectService");
            }
        }
    }
}
