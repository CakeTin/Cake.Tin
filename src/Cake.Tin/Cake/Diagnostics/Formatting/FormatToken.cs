// -----------------------------------------------------------------------
// <copyright file="FormatToken.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Diagnostics.Formatting
{
    internal abstract class FormatToken
    {
        #region Methods

        public abstract string Render(object[] args);

        #endregion Methods
    }
}