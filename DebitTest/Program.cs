using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK.Business.staticThread;
namespace DebitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Print Managerment ");
            ThreadManagement.Instance.Start();
            Console.ReadLine();
        }
    }
}
