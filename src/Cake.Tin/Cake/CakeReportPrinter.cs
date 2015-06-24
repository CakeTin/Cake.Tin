// -----------------------------------------------------------------------
// <copyright file="CakeReportPrinter.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Cake.Core;

    internal sealed class CakeReportPrinter : ICakeReportPrinter
    {
        #region Fields

        /// <summary>_console</summary>
        private readonly IConsole _console;

        #endregion Fields

        #region Constructors

        public CakeReportPrinter(IConsole console)
        {
            _console = console;
        }

        #endregion Constructors

        #region Methods

        public void Write(CakeReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }

            try
            {
                _console.ForegroundColor = ConsoleColor.Green;

                // Write header.
                _console.WriteLine();
                _console.WriteLine("{0,-30}{1,-20}", "Task", "Duration");
                _console.WriteLine(new string('-', 50));

                // Write task status.
                foreach (var item in report)
                {
                    _console.WriteLine("{0,-30}{1,-20}", item.TaskName, FormatTime(item.Duration));
                }

                // Write footer.
                _console.WriteLine(new string('-', 50));
                _console.WriteLine("{0,-30}{1,-20}", "Total:", FormatTime(GetTotalTime(report)));
            }
            finally
            {
                _console.ResetColor();
            }
        }

        private static string FormatTime(TimeSpan time)
        {
            return time.ToString("c", CultureInfo.InvariantCulture);
        }

        private static TimeSpan GetTotalTime(IEnumerable<CakeReportEntry> entries)
        {
            return entries.Select(i => i.Duration)
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }

        #endregion Methods
    }
}