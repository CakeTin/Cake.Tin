// -----------------------------------------------------------------------
// <copyright file="TinBuildCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------

using Cake.Core.Diagnostics;

namespace Cake.Tin.Commands
{
    using System;

    using Cake.Commands;
    using Cake.Common.Diagnostics;

    /// <summary>
    /// The cake tin build command
    /// </summary>
    internal class TinBuildCommand : ICommand
    {
        #region Fields

        /// <summary>Cake tin base</summary>
        private readonly CakeTinBase cakeTinBase;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TinBuildCommand"/> class.
        /// </summary>
        /// <param name="cakeTinBase">The cake tin base.</param>
        public TinBuildCommand(CakeTinBase cakeTinBase)
        {
            this.cakeTinBase = cakeTinBase;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Executes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>True if successful</returns>
        public bool Execute(CakeOptions options)
        {
            try
            {
                this.cakeTinBase.CreateAndExecuteBuild();
                return true;
            }
            catch (Exception ex)
            {
              this.cakeTinBase.Error(
                this.cakeTinBase.Log.Verbosity == Verbosity.Diagnostic 
                  ? ex.ToString()
                  : ex.Message);
              return false;
            }
        }

        #endregion Methods
    }
}