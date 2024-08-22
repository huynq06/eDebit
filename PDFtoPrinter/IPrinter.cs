using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFtoPrinter
{
    public interface IPrinter
    {
        Task Print(PrintingOptions options, TimeSpan? timeout = null);
    }
}
