// -----------------------------------------------------------------------
// <copyright file="VersionCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    using System.Diagnostics;

    using Cake.Core;
    using Cake.Tin;

    /// <summary>
    /// A command that shows version information.
    /// </summary>
    internal sealed class VersionCommand : ICommand
    {
        #region Fields

        /// <summary>_console</summary>
        private readonly IConsole _console;

        #endregion Fields

        #region Constructors

        public VersionCommand(IConsole console)
        {
            _console = console;
        }

        #endregion Constructors

        #region Delegates

        // Delegate factory used by Autofac.
        public delegate VersionCommand Factory();

        #endregion Delegates

        #region Methods

        public bool Execute(CakeOptions options)
        {
            _console.WriteLine();
            _console.WriteLine(@"             +##   #;;'");
            _console.WriteLine(@"             #;;#  .+;;;;+,");
            _console.WriteLine(@"             '+;;#;,+';;;;;'#.");
            _console.WriteLine(@"             ++'''';;;;;;;;;;# ;#;");
            _console.WriteLine(@"            ##';;;;++'+#;;;;;'.   `#:");
            _console.WriteLine(@"         ;#   '+'';;;;;;;;;'#`       #.");
            _console.WriteLine(@"      `#,        .'++;;;;;':..........#");
            _console.WriteLine(@"    '+      `.........';;;;':.........#");
            _console.WriteLine(@"   #..................+;;;;;':........#");
            _console.WriteLine(@"   #..................#';;;;;'+''''''.#");
            _console.WriteLine(@"   #.......,:;''''''''##';;;;;'+'''''#,");
            _console.WriteLine(@"   #''''''''''''''''''###';;;;;;+''''#");
            _console.WriteLine(@"   #''''''''''''''''''####';;;;;;#'''#");
            _console.WriteLine(@"   #''''''''''''''''''#####';;;;;;#''#");
            _console.WriteLine(@"   #''''''''''''''''''######';;;;;;#'#");
            _console.WriteLine(@"   #''''''''''''''''''#######';;;;;;##");
            _console.WriteLine(@"   #''''''''''''''''''########';;;;;;#");
            _console.WriteLine(@"   #''''''''''''++####+;#######';;;;;;#");
            _console.WriteLine(@"   #+####':,`             ,#####';;;;;;'");
            _console.WriteLine(@"                              +##'''''+.");
            _console.WriteLine(@"   ___      _          ___       _ _     _ ");
            _console.WriteLine(@"  / __\__ _| | _____  / __\_   _(_) | __| |");
            _console.WriteLine(@" / /  / _` | |/ / _ \/__\// | | | | |/ _` |");
            _console.WriteLine(@"/ /___ (_| |   <  __/ \/  \ |_| | | | (_| |");
            _console.WriteLine(@"\____/\__,_|_|\_\___\_____/\__,_|_|_|\__,_|");
            _console.WriteLine();
            _console.WriteLine(@"                             Version {0}", GetVersion());
            _console.WriteLine();

            return true;
        }

        private static string GetVersion()
        {
            var assembly = typeof(CakeTinBase).Assembly;
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }

        #endregion Methods
    }
}