using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFtoPrinter
{
    public class Process :
        System.Diagnostics.Process,
        IProcess
    {
        /// <inheritdoc/>
        public Task<bool> WaitForExitAsync(TimeSpan timeout)
        {
            return Task.Factory.StartNew(
                () => this.WaitForExit((int)timeout.TotalMilliseconds));
        }
    }
}
