// -----------------------------------------------------------------------
// <copyright file="CakeBuildLog.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Diagnostics
{
    using System;
    using System.Collections.Generic;

    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Diagnostics.Formatting;

    internal sealed class CakeBuildLog : IVerbosityAwareLog
    {
        #region Fields

        /// <summary>_console</summary>
        private readonly IConsole _console;

        /// <summary>_lock</summary>
        private readonly object _lock;

        /// <summary>_palettes</summary>
        private readonly IDictionary<LogLevel, ConsolePalette> _palettes;

        #endregion Fields

        #region Constructors

        public CakeBuildLog(IConsole console, Verbosity verbosity = Verbosity.Normal)
        {
            _console = console;
            _lock = new object();
            _palettes = CreatePalette();
            Verbosity = verbosity;
        }

        #endregion Constructors

        #region Properties

        public Verbosity Verbosity { get; private set; }

        #endregion Properties

        #region Methods

        public void SetVerbosity(Verbosity verbosity)
        {
            Verbosity = verbosity;
        }

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            if (verbosity > Verbosity)
            {
                return;
            }
            lock (_lock)
            {
                try
                {
                    var palette = _palettes[level];
                    var tokens = FormatParser.Parse(format);
                    foreach (var token in tokens)
                    {
                        SetPalette(token, palette);
                        _console.Write("{0}", token.Render(args));
                    }
                }
                finally
                {
                    _console.ResetColor();
                    _console.WriteLine();
                }
            }
        }

        private IDictionary<LogLevel, ConsolePalette> CreatePalette()
        {
            var background = _console.BackgroundColor;
            var palette = new Dictionary<LogLevel, ConsolePalette>
            {
                { LogLevel.Error, new ConsolePalette(ConsoleColor.DarkRed, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.White) },
                { LogLevel.Warning, new ConsolePalette(background, ConsoleColor.Yellow, background, ConsoleColor.Yellow) },
                { LogLevel.Information, new ConsolePalette(background, ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.White) },
                { LogLevel.Verbose, new ConsolePalette(background, ConsoleColor.Gray, background, ConsoleColor.White) },
                { LogLevel.Debug, new ConsolePalette(background, ConsoleColor.DarkGray, background, ConsoleColor.Gray) }
            };
            return palette;
        }

        private void SetPalette(FormatToken token, ConsolePalette palette)
        {
            var property = token as PropertyToken;
            if (property != null)
            {
                _console.BackgroundColor = palette.ArgumentBackground;
                _console.ForegroundColor = palette.ArgumentForeground;
            }
            else
            {
                _console.BackgroundColor = palette.Background;
                _console.ForegroundColor = palette.Foreground;
            }
        }

        #endregion Methods
    }
}