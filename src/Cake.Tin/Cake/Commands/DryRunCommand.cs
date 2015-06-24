// -----------------------------------------------------------------------
// <copyright file="DryRunCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    using System;

    using Cake.Core.Scripting;
    using Cake.Scripting;

    /// <summary>
    /// A command that dry runs a build script.
    /// </summary>
    internal sealed class DryRunCommand : ICommand
    {
        #region Fields

        /// <summary>_host</summary>
        private readonly DryRunScriptHost _host;

        /// <summary>_script runner</summary>
        private readonly IScriptRunner _scriptRunner;

        #endregion Fields

        #region Constructors

        public DryRunCommand(IScriptRunner scriptRunner, DryRunScriptHost host)
        {
            _scriptRunner = scriptRunner;
            _host = host;
        }

        #endregion Constructors

        #region Delegates

        // Delegate factory used by Autofac.
        public delegate DryRunCommand Factory();

        #endregion Delegates

        #region Methods

        public bool Execute(CakeOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            _scriptRunner.Run(_host, options.Script, options.Arguments);
            return true;
        }

        #endregion Methods
    }
}