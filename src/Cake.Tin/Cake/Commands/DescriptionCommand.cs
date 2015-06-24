// -----------------------------------------------------------------------
// <copyright file="DescriptionCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    using System;

    using Cake.Core.Scripting;
    using Cake.Scripting;

    /// <summary>
    /// A command that displays information about script tasks.
    /// </summary>
    internal sealed class DescriptionCommand : ICommand
    {
        #region Fields

        /// <summary>_host</summary>
        private readonly DescriptionScriptHost _host;

        /// <summary>_script runner</summary>
        private readonly IScriptRunner _scriptRunner;

        #endregion Fields

        #region Constructors

        public DescriptionCommand(IScriptRunner scriptRunner, DescriptionScriptHost host)
        {
            _scriptRunner = scriptRunner;
            _host = host;
        }

        #endregion Constructors

        #region Delegates

        // Delegate factory used by Autofac.
        public delegate DescriptionCommand Factory();

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