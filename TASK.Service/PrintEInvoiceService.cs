using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Data;
using PDFtoPrinter;
namespace TASK.Service
{
    public static class PrintEInvoiceService
    {

        public static void PrintInvoice()
        {
            List<Print> filesPrint = Print.GetAll();
            var allowedCocurrentPrintings = filesPrint.Count;
            var wrapper = new PDFtoPrinterPrinter(allowedCocurrentPrintings);
            foreach (var print in filesPrint)
            {

                FileInfo fileInfo = new FileInfo(print.PrintShareLink);
                if (fileInfo.Exists)
                {
                    try
                    {

                        wrapper
                         .Print(new PrintingOptions(print.PrintUser, print.PrintShareLink)).Wait();
                       
                        print.Status = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        throw;
                    }
                }
            }
        }
    }
}
