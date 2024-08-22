using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Business.staticThread;
using Microsoft.Office.Interop.Word;
using PDFtoPrinter;
using System.IO;
using TASK.Data;

namespace ALSC.eInvoiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CHUONG TRINH QUAN LY HOA DON DIEN TU, VUI LONG DUNG TAT!!!");
            int IntervalParameter = 5;

            // .Range(0, 1440 / IntervalParameter) - see Zohar Peled's comment -  
            // is brief, but less readable
            List<string> query = Enumerable
              .Range(0, (int)(new TimeSpan(24, 0, 0).TotalMinutes / IntervalParameter))
              .Select(i => DateTime.Today
                 .AddMinutes(i * (double)IntervalParameter) // AddHours is redundant
                 .ToString("HH:mm"))                        // Let's provide HH:mm format 
              .ToList();
            foreach(var item in query)
            {
                Console.WriteLine("Time" + item);
            }
            // string input = "TANG 04,TOA NHA D1,O DAT CT2,KHU DO THI MOI KIM VAN-KIM LU,PHUONG DAI KIM,QUAN HOANG MAI,TP.HA NOI,";
            //string dateInoute = "31-08-20 10:01";
            //DateTime dt = DateTime.ParseExact(dateInoute, "dd-MM-yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            //DateTime StartChargeDate = dt.Date.AddDays(3);
            ////Console.WriteLine(StartChargeDate.ToString("dd/MM/yyy HH:mm"));
            //string FinishDate = "12-09-20 23:55";
            //DateTime toDate = DateTime.ParseExact(FinishDate, "dd-MM-yy HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            //DateTime DateOut = toDate.Date.AddDays(1);
            ////Console.WriteLine(toDate.Date.AddDays(1).AddMinutes(-1));
            //int weekendDays = 0;
            //List<HolidayConfig> listHoliday = HolidayConfig.GetData();
            //for (DateTime date = StartChargeDate; date.Date <= DateOut.AddMinutes(-1).Date; date = date.AddDays(1))
            //{
            //    if ((date.DayOfWeek == DayOfWeek.Saturday) || (date.DayOfWeek == DayOfWeek.Sunday))
            //    {
            //        weekendDays++;
            //    }
            //    if (listHoliday.Any(c => c.DateHoliday.Value.Date == date.Date))
            //    {
            //        if ((date.DayOfWeek != DayOfWeek.Saturday) || (date.DayOfWeek != DayOfWeek.Sunday))
            //        {
            //            weekendDays++;
            //        }
            //    }
            //}
            ////Console.WriteLine(weekendDays);
            //double storeTime = (DateOut - StartChargeDate).TotalDays - weekendDays;
            //Console.WriteLine(storeTime);
            //double a = 90.01;
            //double b = 89.99;
            //double c = Math.Round(a);
            //double d = Math.Round(b);
            //Console.WriteLine("d : {0}", d);
            //Console.WriteLine("c : {0}", c);


            //Console.ReadLine();
            ThreadManagement.Instance.Start();
            //------ ThreadEinvoiceAlsxTT78Management.Instance.Start();
            //var wrapper = new PDFtoPrinterPrinter();
            //string filePath = @"D:\file.pdf";
            //FileInfo fileInfo = new FileInfo(filePath);
            //if (fileInfo.Exists)
            //{
            //    try
            //    {
            //        wrapper
            //         .Print(new PrintingOptions("IT_TEST", @"D:\file.pdf"))
            //         .Wait();
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.ToString());
            //        throw;
            //    }



            //}
            Console.ReadLine();
        }
    }
}
