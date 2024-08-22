using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFtoPrinter
{
    public interface IProcessFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IProcess"/> with given arguments.
        /// </summary>
        /// <param name="executablePath">Path to a ".exe" file.</param>
        /// <param name="options">CLI arguments.</param>
        /// <returns>Corresponding <see cref="IProcess"/> instance.</returns>
        IProcess Create(string executablePath, PrintingOptions options);
    }
}
