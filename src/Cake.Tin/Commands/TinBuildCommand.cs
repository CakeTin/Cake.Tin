// -----------------------------------------------------------------------
// <copyright file="TinBuildCommand.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin.Commands
{
    using Cake.Commands;

    internal class TinBuildCommand : ICommand
    {
        #region Fields

        /// <summary>Cake tin base</summary>
        private readonly CakeTinBase cakeTinBase;

        #endregion Fields

        #region Constructors

        public TinBuildCommand(CakeTinBase cakeTinBase)
        {
            this.cakeTinBase = cakeTinBase;
        }

        #endregion Constructors

        #region Methods

        public bool Execute(CakeOptions options)
        {
            this.cakeTinBase.CreateAndExecuteBuild();
              return true;
        }

        #endregion Methods
    }
}