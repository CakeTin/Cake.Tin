// -----------------------------------------------------------------------
// <copyright file="ErrorCommandDecorator.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Commands
{
    /// <summary>
    /// A command that decorates another command but always return failure.
    /// </summary>
    internal sealed class ErrorCommandDecorator : ICommand
    {
        #region Fields

        /// <summary>_command</summary>
        private readonly ICommand _command;

        #endregion Fields

        #region Constructors

        public ErrorCommandDecorator(ICommand command)
        {
            _command = command;
        }

        #endregion Constructors

        #region Methods

        public bool Execute(CakeOptions options)
        {
            _command.Execute(options);
            return false;
        }

        #endregion Methods
    }
}