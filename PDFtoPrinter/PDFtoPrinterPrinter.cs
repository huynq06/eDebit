﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoPrinter
{
    public class PDFtoPrinterPrinter : IPrinter
    {
        private const string UtilName = "PDFtoPrinter.exe";
        private static readonly string UtilPath = GetUtilPath(UtilName);
        private static readonly TimeSpan PrintTimeout = new TimeSpan(0, 1, 0);

        private readonly SemaphoreSlim semaphore;
        private readonly IProcessFactory processFactory;

        /// <summary>
        /// Creates new <see cref="PDFtoPrinterPrinter"/> instance without concurrent printing.
        /// <param name="processFactory"><see cref="IProcessFactory"/> instance.</param>
        /// </summary>
        public PDFtoPrinterPrinter(
            IProcessFactory processFactory = null)
            : this(1, processFactory)
        {
        }

        /// <summary>
        /// Creates new <see cref="PDFtoPrinterPrinter"/> instance with concurrent printing.
        /// </summary>
        /// <param name="maxConcurrentPrintings">Max count of cuncurrent printings.</param>
        /// <param name="processFactory"><see cref="IProcessFactory"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thows an exception if <paramref name="maxConcurrentPrintings"/> less or equals 0.
        /// </exception>
        public PDFtoPrinterPrinter(
            int maxConcurrentPrintings,
            IProcessFactory processFactory = null)
        {
            if (maxConcurrentPrintings <= 0)
            {
                throw new ArgumentException(
                    ErrorMessages.ValueGreterZero,
                    nameof(maxConcurrentPrintings));
            }

            this.semaphore = new SemaphoreSlim(maxConcurrentPrintings);
            this.processFactory = processFactory ?? new SystemProcessFactory();
        }

        [Obsolete("Please use \"Task Print(PrintingOptions options, TimeSpan? timeout = null)\" method instead.")]
        public Task Print(
            string filePath, string printerName, TimeSpan? timeout = null)
        {
            return this.Print(
                new PrintingOptions(printerName, filePath),
                timeout);
        }

        /// <inheritdoc/>
        public async Task Print(PrintingOptions options, TimeSpan? timeout = null)
        {
            await this.semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using (IProcess proc = this.processFactory.Create(UtilPath, options))
                {
                    proc.Start();
                    bool result = await proc
                        .WaitForExitAsync(timeout ?? PrintTimeout)
                        .ConfigureAwait(false);
                    if (!result)
                    {
                        proc.Kill();
                    }
                }
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        private static string GetUtilPath(string utilName)
        {
            return Path.Combine(
                Path.GetDirectoryName(
                    (System.Reflection.Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location),
                utilName);
        }
    }
}
