﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;
using TASK.Service;
namespace TASK.Business.staticThread
{
    public class ThreadCreatInvoiceManagement : ThreadBase
    {
        public object _locker = new object();
        public ThreadCreatInvoiceManagement()
            : base()
        {
        }

        protected override int DUETIME
        {
            get
            {
                return 1000;
            }
        }
        protected override void DoWork()
        {
            try
            {
                Service.PrintInvoiceHermesService.GetDataFromHermes();
            }
            catch (Exception ex)
            {
                //Log.InsertLog(ex, "Issue", "Issue");
                Log.WriteLog(ex, "EinvoiceAlc");
            }
        }
    }
}
