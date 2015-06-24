// -----------------------------------------------------------------------
// <copyright file="ArgumentOptions.cs" company="Mark Walker">
//     Copyright (c) 2015, Mark Walker and contributors. Based on Cake - Copyright (c) 2014, Patrik Svensson and contributors.
// </copyright>
// -----------------------------------------------------------------------
namespace Cake.Tin.Enums
{
    using System;

    #region Enumerations

    /// <summary>
    /// Argument Options
    /// </summary>
    [Flags]
    public enum ArgumentOptions
    {
        /// <summary>Allow arguments from the command line</summary>
        CommandLine = 1,

        /// <summary>Allow arguments from Environmental Variables</summary>
        EnvironmentalVariables = 2,
    }

    #endregion Enumerations
}