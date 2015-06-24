// -----------------------------------------------------------------------
// <copyright file="HelpCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    using System;

    using Cake.Core;
    using Cake.Core.Diagnostics;

    /// <summary>
    /// A command that displays help information.
    /// </summary>
    internal sealed class HelpCommand : ICommand
    {
        #region Fields

        /// <summary>_console</summary>
        private readonly IConsole _console;

        #endregion Fields

        #region Constructors

        public HelpCommand(IConsole console)
        {
            _console = console;
        }

        #endregion Constructors

        #region Delegates

        // Delegate factory used by Autofac.
        public delegate HelpCommand Factory();

        #endregion Delegates

        #region Methods

        public bool Execute(CakeOptions options)
        {
            _console.WriteLine();
            _console.WriteLine("Usage: Cake.exe [build-script] [-verbosity=value]");
            _console.WriteLine("                [-showdescription] [-dryrun] [..]");
            _console.WriteLine();
            _console.WriteLine("Example: Cake.exe");
            _console.WriteLine("Example: Cake.exe build.cake -verbosity=quiet");
            _console.WriteLine("Example: Cake.exe build.cake -showdescription");
            _console.WriteLine();
            _console.WriteLine("Options:");
            _console.WriteLine("    -verbosity=value    Specifies the amount of information to be displayed.");
            _console.WriteLine("                        ({0})",
                string.Join(", ", Enum.GetNames(typeof(Verbosity))));
            _console.WriteLine("    -showdescription    Shows description about tasks.");
            _console.WriteLine("    -dryrun             Performs a dry run.");
            _console.WriteLine("    -version            Displays version information.");
            _console.WriteLine("    -help               Displays usage information.");
            _console.WriteLine("    -experimental       Uses the nightly builds of Roslyn script engine.");
            _console.WriteLine();

            return true;
        }

        #endregion Methods
    }
}